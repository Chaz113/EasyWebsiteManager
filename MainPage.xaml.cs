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

    public MainPage()
    {
        InitializeComponent();

        var vm = new MainViewModel();
        BindingContext = vm;

        CategoryListView.ChildAdded += CategoryListView_ChildAdded;
    }

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
        _deletedCategory = category;
        _deletedWebsite = website;
        _deletedWholeCategory = null;

        await SaveCurrentDataAsync();

        UndoMessageLabel.Text =
            $"{website.Name} deleted";

        UndoBanner.IsVisible = true;

        _undoCancellation?.Cancel();

        _undoCancellation =
            new CancellationTokenSource();

        try
        {
            await Task.Delay(
                TimeSpan.FromSeconds(5),
                _undoCancellation.Token);

            UndoBanner.IsVisible = false;

            _deletedCategory = null;
            _deletedWebsite = null;
        }
        catch (TaskCanceledException)
        {
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

        var editPage =
            new EditCategoryPage(category);

        editPage.CategoryUpdated +=
            async updatedCategory =>
            {
                vm.ResortCategories();

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

        if (!vm.DeleteCategory(category))
            return;

        _deletedWholeCategory = category;

        _deletedCategory = null;
        _deletedWebsite = null;

        await SaveCurrentDataAsync();

        UndoMessageLabel.Text =
            $"{category.Name} category deleted";

        UndoBanner.IsVisible = true;

        _undoCancellation?.Cancel();

        _undoCancellation =
            new CancellationTokenSource();

        try
        {
            await Task.Delay(
                TimeSpan.FromSeconds(5),
                _undoCancellation.Token);

            UndoBanner.IsVisible = false;

            _deletedWholeCategory = null;
        }
        catch (TaskCanceledException)
        {
        }
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
                vm.RestoreCategory(
                    _deletedWholeCategory);

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

        _deletedCategory.Websites.Add(
            _deletedWebsite);

        var sorted =
            _deletedCategory.Websites
                .OrderBy(w => w.Name)
                .ToList();

        _deletedCategory.Websites.Clear();

        foreach (var website in sorted)
        {
            _deletedCategory.Websites.Add(
                website);
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