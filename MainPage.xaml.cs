using EasyWebsiteManager.Models;
using EasyWebsiteManager.Services;
using EasyWebsiteManager.ViewModels;
using EasyWebsiteManager.Views;

namespace EasyWebsiteManager;

public partial class MainPage : ContentPage
{
    private WebsiteCategory? _deletedCategory;
    private WebsiteItem? _deletedWebsite;
    private WebsiteCategory? _deletedWholeCategory;

    private CancellationTokenSource? _undoCancellation;
    private bool _dataLoaded;

#if WINDOWS
    // Keep a surviving category at the same physical screen position.
    // This is deliberately Windows-only; Android is not involved.
    private sealed record WindowsScrollAnchor(Guid CategoryId, double ScreenY);

    private Microsoft.UI.Xaml.Controls.ListViewBase? GetWindowsCategoryList()
        => CategoryListView.Handler?.PlatformView as Microsoft.UI.Xaml.Controls.ListViewBase;

    private static T? FindWindowsDescendant<T>(Microsoft.UI.Xaml.DependencyObject root)
        where T : Microsoft.UI.Xaml.DependencyObject
    {
        if (root is T matching) return matching;
        for (int i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var found = FindWindowsDescendant<T>(
                Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(root, i));
            if (found != null) return found;
        }
        return null;
    }

    private WindowsScrollAnchor? CaptureWindowsAnchor(WebsiteCategory changedCategory)
    {
        if (BindingContext is not MainViewModel vm) return null;
        var list = GetWindowsCategoryList();
        if (list == null) return null;
        var scroller = FindWindowsDescendant<Microsoft.UI.Xaml.Controls.ScrollViewer>(list);
        if (scroller == null) return null;

        // Prefer the first visible category that will SURVIVE the deletion.
        // ContainerFromIndex only returns realized (on-screen) items.
        WindowsScrollAnchor? best = null;
        double bestY = double.MaxValue;
        for (int i = 0; i < vm.FilteredCategories.Count; i++)
        {
            var category = vm.FilteredCategories[i];
            if (category.Id == changedCategory.Id) continue;
            if (list.ContainerFromIndex(i) is not Microsoft.UI.Xaml.FrameworkElement container)
                continue;
            var y = container.TransformToVisual(scroller)
                .TransformPoint(new Windows.Foundation.Point(0, 0)).Y;
            if (y + container.ActualHeight <= 0 || y >= scroller.ViewportHeight)
                continue;
            if (y < bestY)
            {
                bestY = y;
                best = new WindowsScrollAnchor(category.Id, y);
            }
        }
        return best;
    }

    private void RestoreWindowsAnchor(WindowsScrollAnchor? anchor)
    {
        if (anchor == null) return;
        // Wait for native CollectionView layout to process the removal/insertion.
        CategoryListView.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(80), () =>
        {
            if (BindingContext is not MainViewModel vm) return;
            var list = GetWindowsCategoryList();
            if (list == null) return;
            var scroller = FindWindowsDescendant<Microsoft.UI.Xaml.Controls.ScrollViewer>(list);
            if (scroller == null) return;
            var index = vm.FilteredCategories.ToList().FindIndex(c => c.Id == anchor.CategoryId);
            if (index < 0) return;

            // A reset can unrealize the anchor. Bring it into view first, then
            // restore its precise vertical position in the following layout pass.
            if (list.ContainerFromIndex(index) is not Microsoft.UI.Xaml.FrameworkElement)
                list.ScrollIntoView(vm.FilteredCategories[index]);

            CategoryListView.Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(80), () =>
            {
                var currentList = GetWindowsCategoryList();
                if (currentList == null) return;
                var currentScroller = FindWindowsDescendant<Microsoft.UI.Xaml.Controls.ScrollViewer>(currentList);
                if (currentScroller == null) return;
                var currentIndex = vm.FilteredCategories.ToList().FindIndex(c => c.Id == anchor.CategoryId);
                if (currentIndex < 0 || currentList.ContainerFromIndex(currentIndex) is not Microsoft.UI.Xaml.FrameworkElement container)
                    return;
                var currentY = container.TransformToVisual(currentScroller)
                    .TransformPoint(new Windows.Foundation.Point(0, 0)).Y;
                var desiredOffset = Math.Clamp(currentScroller.VerticalOffset + currentY - anchor.ScreenY,
                    0, currentScroller.ScrollableHeight);
                currentScroller.ChangeView(null, desiredOffset, null, true);
            });
        });
    }
#endif

    public MainPage()
    {
        InitializeComponent();

        var vm = new MainViewModel();
        BindingContext = vm;

        CategoryListView.ChildAdded += CategoryListView_ChildAdded;

#if ANDROID
        if (Application.Current is not null)
            Application.Current.RequestedThemeChanged += SearchCursor_RequestedThemeChanged;
#endif
    }

    // Keep the Android text insertion cursor visible in both themes.
    // This changes only the native cursor tint, not search behavior.
    private void SearchEntry_HandlerChanged(object? sender, EventArgs e)
    {
#if ANDROID
        UpdateAndroidSearchCursor();
#endif
    }

#if ANDROID
    private void SearchCursor_RequestedThemeChanged(object? sender, AppThemeChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(UpdateAndroidSearchCursor);
    }

    private void UpdateAndroidSearchCursor()
    {
        if (SearchEntry?.Handler?.PlatformView is not Android.Widget.EditText editText)
            return;

        // Android 10+ supports tinting the actual cursor drawable.
        if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.Q)
            return;
        // Remove Android's native search-field underline.
        editText.BackgroundTintList =
            Android.Content.Res.ColorStateList.ValueOf(
                Android.Graphics.Color.Transparent);

        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var color = dark ? Android.Graphics.Color.White : Android.Graphics.Color.Black;
        editText.TextCursorDrawable?.SetTint(color);
    }
#endif

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_dataLoaded)
            return;

        _dataLoaded = true;

        if (BindingContext is not MainViewModel vm)
            return;

        var data = await StorageService.LoadAsync();

        if (data != null)
        {
            vm.LoadAppData(data);

            // Persist any one-time migrations.
            await SaveCurrentDataAsync();
        }
    }

    // ---------------------------------------------------------
    // IMPORT
    // ---------------------------------------------------------

    private async Task ImportDataAsync()
    {
        try
        {
            var result =
                await FilePicker.Default.PickAsync(
                    new PickOptions
                    {
                        PickerTitle =
                            "Import EasyWebsiteManager Backup"
                    });

            if (result == null)
                return;

            // Read through the FileResult stream instead of
            // relying on FullPath. This is important on Android,
            // where files may come from document providers,
            // Downloads, USB storage, cloud storage, etc.
            await using var stream =
                await result.OpenReadAsync();

            using var reader =
                new StreamReader(stream);

            var json =
                await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(json))
            {
                await DisplayAlertAsync(
                    "Import Failed",
                    "This is not a valid EasyWebsiteManager backup.",
                    "OK");

                return;
            }

            AppData? importedData;

            try
            {
                importedData =
                    System.Text.Json.JsonSerializer.Deserialize<AppData>(
                        json,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
            }
            catch
            {
                importedData = null;
            }

            if (importedData == null ||
                importedData.Categories == null ||
                importedData.Settings == null)
            {
                await DisplayAlertAsync(
                    "Import Failed",
                    "This is not a valid EasyWebsiteManager backup.",
                    "OK");

                return;
            }

            var confirmed =
                await DisplayAlertAsync(
                    "Import Data",
                    "Import this EasyWebsiteManager backup?",
                    "Import",
                    "Cancel");

            if (!confirmed)
                return;

            if (BindingContext is not MainViewModel vm)
                return;

            vm.LoadAppData(importedData);

            await SaveCurrentDataAsync();

            await DisplayAlertAsync(
                "Import Complete",
                "Your data was imported successfully.",
                "OK");
        }
        catch
        {
            await DisplayAlertAsync(
                "Import Failed",
                "This is not a valid EasyWebsiteManager backup.",
                "OK");
        }
    }

    private async Task SaveCurrentDataAsync()
    {
        if (BindingContext is not MainViewModel vm)
            return;

        var data = vm.CreateAppData();

        await StorageService.SaveAsync(data);
    }

    // ---------------------------------------------------------
    // MAIN + BUTTON
    // ---------------------------------------------------------

    private async void AddButton_Clicked(
        object? sender,
        EventArgs e)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        var addPage =
            new AddWebsitePage(vm.Categories);

        addPage.WebsiteSaved +=
            async (category, website, url) =>
            {
                vm.AddWebsite(
                    category,
                    website,
                    url);

                await SaveCurrentDataAsync();
            };

        await Navigation.PushModalAsync(addPage);
    }

    // ---------------------------------------------------------
    // SETTINGS
    // ---------------------------------------------------------

    private async void SettingsButton_Clicked(
        object? sender,
        EventArgs e)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        var settingsPage =
            new SettingsPage(vm.Settings);

        settingsPage.SettingsChanged += async () =>
        {
            await SaveCurrentDataAsync();
        };

        settingsPage.ExportDataRequested += async () =>
        {
            await ExportDataAsync();
        };

        settingsPage.ImportDataRequested += async () =>
        {
            await ImportDataAsync();
        };

        await Navigation.PushModalAsync(settingsPage);
    }

    // ---------------------------------------------------------
    // HOME
    // ---------------------------------------------------------

    private void HomeButton_Clicked(
        object? sender,
        EventArgs e)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        vm.SearchText = string.Empty;
    }

    // ---------------------------------------------------------
    // CATEGORY VIEW EVENT WIRING
    // ---------------------------------------------------------

    private void CategoryListView_ChildAdded(
        object? sender,
        ElementEventArgs e)
    {
        if (e.Element is not CategoryView categoryView)
            return;

        if (BindingContext is not MainViewModel vm)
            return;

        categoryView.Settings = vm.Settings;

        categoryView.WebsiteDeleted -=
            CategoryView_WebsiteDeleted;

        categoryView.WebsiteDeleted +=
            CategoryView_WebsiteDeleted;

        categoryView.WebsiteNoteChanged -=
            CategoryView_WebsiteNoteChanged;

        categoryView.WebsiteNoteChanged +=
            CategoryView_WebsiteNoteChanged;

        categoryView.WebsiteUpdated -=
            CategoryView_WebsiteUpdated;

        categoryView.WebsiteUpdated +=
            CategoryView_WebsiteUpdated;

        categoryView.CategoryEditRequested -=
            CategoryView_CategoryEditRequested;

        categoryView.CategoryEditRequested +=
            CategoryView_CategoryEditRequested;

        categoryView.CategoryDeleteRequested -=
            CategoryView_CategoryDeleteRequested;

        categoryView.CategoryDeleteRequested +=
            CategoryView_CategoryDeleteRequested;
    }

    // ---------------------------------------------------------
    // WEBSITE DELETE
    // ---------------------------------------------------------

    private async void CategoryView_WebsiteDeleted(
        WebsiteCategory category,
        WebsiteItem website)
    {
        try
        {
            // Search results can use a temporary category. Always remove
            // from the original category held by the ViewModel.
            if (BindingContext is not MainViewModel vm)
                return;

            var originalCategory = vm.Categories.FirstOrDefault(c =>
                ReferenceEquals(c, category) || c.Id == category.Id);
            if (originalCategory == null)
                return;

            if (!originalCategory.Websites.Contains(website))
            {
                // Already removed from the original collection by CategoryView.
            }
            else
            {
                originalCategory.Websites.Remove(website);
            }

            category = originalCategory;
            await SaveCurrentDataAsync();

            // Refresh any temporary search-result categories.
            if (!string.IsNullOrWhiteSpace(vm.SearchText))
            {
                var search = vm.SearchText;
                vm.SearchText = string.Empty;
                vm.SearchText = search;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Website deletion save failed: {ex}");
            // Restore the item in memory if persistence failed.
            if (!category.Websites.Contains(website))
            {
                var insertAt = category.Websites.Count(item =>
                    string.Compare(item.Name, website.Name,
                        StringComparison.CurrentCultureIgnoreCase) <= 0);
                category.Websites.Insert(insertAt, website);
            }

            await DisplayAlertAsync("Delete Failed",
                "The deletion could not be saved. The website has been restored.",
                "OK");
            return;
        }

        _deletedCategory = category;
        _deletedWebsite = website;
        _deletedWholeCategory = null;
        UndoMessageLabel.Text = "Website deleted";
        UndoBanner.IsVisible = true;
        StartUndoCountdown();
    }

    private void StartUndoCountdown()
    {
        _undoCancellation?.Cancel();
        _undoCancellation?.Dispose();
        var cancellation = new CancellationTokenSource();
        _undoCancellation = cancellation;
        _ = ExpireUndoAsync(cancellation);
    }

    private async Task ExpireUndoAsync(CancellationTokenSource cancellation)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5), cancellation.Token);
            if (!ReferenceEquals(_undoCancellation, cancellation))
                return;

            UndoBanner.IsVisible = false;
            _deletedCategory = null;
            _deletedWebsite = null;
            _deletedWholeCategory = null;
        }
        catch (OperationCanceledException)
        {
            // A newer deletion or Undo replaced this countdown.
        }
        finally
        {
            if (ReferenceEquals(_undoCancellation, cancellation))
            {
                _undoCancellation = null;
                cancellation.Dispose();
            }
        }
    }

    // ---------------------------------------------------------
    // WEBSITE NOTE
    // ---------------------------------------------------------

    private async void CategoryView_WebsiteNoteChanged(
        WebsiteItem website)
    {
        await SaveCurrentDataAsync();
    }

    // ---------------------------------------------------------
    // WEBSITE EDIT
    // ---------------------------------------------------------

    private async void CategoryView_WebsiteUpdated(
        WebsiteItem website)
    {
        await SaveCurrentDataAsync();
    }

    // ---------------------------------------------------------
    // CATEGORY EDIT
    // ---------------------------------------------------------

    private async void CategoryView_CategoryEditRequested(
        WebsiteCategory category)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        // Remember the category name and its displayed position before editing.
        // A colour-only edit must not rebuild the CollectionView's ItemsSource.
        var originalName = category.Name;
#if WINDOWS
        var originalDisplayIndex = vm.FilteredCategories.IndexOf(category);
        if (originalDisplayIndex < 0)
            originalDisplayIndex = vm.FilteredCategories
                .ToList().FindIndex(c => c.Id == category.Id);
#endif

        var editPage =
            new EditCategoryPage(category);

        editPage.CategoryUpdated +=
            async updatedCategory =>
            {
                if (!string.Equals(originalName, updatedCategory.Name,
                        StringComparison.Ordinal))
                {
                    // Renamed categories must remain alphabetically sorted.
                    vm.ResortCategories();

#if WINDOWS
                    // ResortCategories currently rebuilds the displayed list.
                    // Return to the prior viewport rather than the first item.
                    if (originalDisplayIndex >= 0 && vm.FilteredCategories.Count > 0)
                    {
                        var restoreIndex = Math.Min(originalDisplayIndex,
                            vm.FilteredCategories.Count - 1);
                        CategoryListView.Dispatcher.Dispatch(() =>
                            CategoryListView.ScrollTo(restoreIndex,
                                position: ScrollToPosition.Start, animate: false));
                    }
#endif
                }

                // A colour-only edit updates the observable model directly;
                // no collection rebuild or scroll reset is necessary.
                await SaveCurrentDataAsync();
            };

        // Add Website directly to the category being edited.
        editPage.AddWebsiteRequested +=
            async selectedCategory =>
            {
                var addPage =
                    new AddWebsitePage(
                        vm.Categories,
                        selectedCategory);

                addPage.WebsiteSaved +=
                    async (categoryName, websiteName, url) =>
                    {
                        vm.AddWebsite(
                            categoryName,
                            websiteName,
                            url);

                        await SaveCurrentDataAsync();
                    };

                await Navigation.PushModalAsync(addPage);
            };

        await Navigation.PushModalAsync(editPage);
    }

    // ---------------------------------------------------------
    // CATEGORY DELETE
    // ---------------------------------------------------------

    private async void CategoryView_CategoryDeleteRequested(
        WebsiteCategory category)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        if (vm.Settings.ConfirmDelete)
        {
            if (Window?.Page is not Page page)
                return;

            var confirmed =
                await DialogService.ConfirmAsync(
                    page,
                    "Delete Category",
                    $"Delete {category.Name} and all websites inside it?");

            if (!confirmed)
                return;
        }

#if WINDOWS
        var scrollAnchor = CaptureWindowsAnchor(category);
#endif
        if (!vm.DeleteCategory(category))
            return;
#if WINDOWS
        RestoreWindowsAnchor(scrollAnchor);
#endif

        _deletedWholeCategory = category;

        _deletedCategory = null;
        _deletedWebsite = null;

        await SaveCurrentDataAsync();

        UndoMessageLabel.Text = "Category deleted";

        UndoBanner.IsVisible = true;

        StartUndoCountdown();
    }

    // ---------------------------------------------------------
    // UNDO DELETE
    // ---------------------------------------------------------

    private async void UndoDelete_Clicked(
        object? sender,
        EventArgs e)
    {
        _undoCancellation?.Cancel();

        if (_deletedWholeCategory != null)
        {
            if (BindingContext is MainViewModel vm)
            {
#if WINDOWS
                // Use a surviving visible category as the reference for Undo too.
                var scrollAnchor = CaptureWindowsAnchor(_deletedWholeCategory);
#endif
                vm.RestoreCategory(
                    _deletedWholeCategory);
#if WINDOWS
                RestoreWindowsAnchor(scrollAnchor);
#endif
                await SaveCurrentDataAsync();
            }

            _deletedWholeCategory = null;

            UndoBanner.IsVisible = false;

            return;
        }

        if (_deletedCategory == null ||
            _deletedWebsite == null)
        {
            return;
        }

        // Restore one item without clearing/recreating bound Android buttons.
        if (!_deletedCategory.Websites.Contains(_deletedWebsite))
        {
            var insertAt = _deletedCategory.Websites.Count(item =>
                string.Compare(item.Name, _deletedWebsite.Name,
                    StringComparison.CurrentCultureIgnoreCase) <= 0);
            _deletedCategory.Websites.Insert(insertAt, _deletedWebsite);
        }

        await SaveCurrentDataAsync();

        UndoBanner.IsVisible = false;

        _deletedCategory = null;
        _deletedWebsite = null;
    }

    // ---------------------------------------------------------
    // EXPORT
    // ---------------------------------------------------------

    private async Task ExportDataAsync()
    {
        if (BindingContext is not MainViewModel vm)
            return;

#if WINDOWS

        try
        {
            var data = vm.CreateAppData();

            var picker =
                new Windows.Storage.Pickers.FileSavePicker();

            picker.SuggestedFileName =
                "EasyWebsiteManagerBackup";

            picker.FileTypeChoices.Add(
                "JSON Backup",
                new List<string> { ".json" });

            var window =
                Application.Current?.Windows.FirstOrDefault();

            if (window?.Handler?.PlatformView
                is not Microsoft.UI.Xaml.Window nativeWindow)
            {
                return;
            }

            var hwnd =
                WinRT.Interop.WindowNative.GetWindowHandle(
                    nativeWindow);

            WinRT.Interop.InitializeWithWindow.Initialize(
                picker,
                hwnd);

            var file =
                await picker.PickSaveFileAsync();

            if (file == null)
                return;

            await StorageService.ExportAsync(
                data,
                file.Path);

            await DisplayAlertAsync(
                "Export Complete",
                "Your EasyWebsiteManager backup was saved successfully.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Export Failed",
                ex.Message,
                "OK");
        }

#elif ANDROID

        string? temporaryFilePath = null;

        try
        {
            var data = vm.CreateAppData();

            temporaryFilePath =
                Path.Combine(
                    FileSystem.CacheDirectory,
                    "EasyWebsiteManagerBackup.json");

            // Create the JSON using the same export routine
            // used by the Windows version.
            await StorageService.ExportAsync(
                data,
                temporaryFilePath);

            if (Platform.CurrentActivity
                is not MainActivity activity)
            {
                await DisplayAlertAsync(
                    "Export Failed",
                    "Android file saving is unavailable.",
                    "OK");

                return;
            }

            var destinationUri =
                await activity.CreateBackupFileAsync(
                    "EasyWebsiteManagerBackup.json");

            // User canceled the Android file picker.
            if (destinationUri == null)
                return;

            var contentResolver =
                activity.ContentResolver;

            if (contentResolver == null)
            {
                await DisplayAlertAsync(
                    "Export Failed",
                    "Android file access is unavailable.",
                    "OK");

                return;
            }

            await using var sourceStream =
                File.OpenRead(
                    temporaryFilePath);

            // Explicitly open the selected Android document for
            // write + truncate. This prevents stale bytes from a
            // previous, longer JSON file remaining at the end.
            await using (
                var destinationStream =
                    contentResolver.OpenOutputStream(
                        destinationUri,
                        "wt"))
            {
                if (destinationStream == null)
                {
                    await DisplayAlertAsync(
                        "Export Failed",
                        "The selected file could not be opened.",
                        "OK");

                    return;
                }

                await sourceStream.CopyToAsync(
                    destinationStream);

                await destinationStream.FlushAsync();
            }

            // The destination stream has been flushed and closed
            // before reporting that the export is complete.
            await DisplayAlertAsync(
                "Export Complete",
                "Your EasyWebsiteManager backup was saved successfully.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Export Failed",
                ex.Message,
                "OK");
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(
                    temporaryFilePath) &&
                File.Exists(temporaryFilePath))
            {
                try
                {
                    File.Delete(
                        temporaryFilePath);
                }
                catch
                {
                    // Failure to remove a temporary cache
                    // file should not make the export fail.
                }
            }
        }

#else

        await DisplayAlertAsync(
            "Export",
            "Export is not currently available on this platform.",
            "OK");

#endif
    }
}