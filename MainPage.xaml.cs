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
    private async Task ImportDataAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(
                new PickOptions
                {
                    PickerTitle = "Import EasyWebsiteManager Backup"
                });

            if (result == null)
                return;

            var json = await File.ReadAllTextAsync(
                result.FullPath);

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

    private void HomeButton_Clicked(
        object? sender,
        EventArgs e)
    {
        if (BindingContext is not MainViewModel vm)
            return;

        vm.SearchText = string.Empty;
    }

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

    private async void CategoryView_WebsiteNoteChanged(
        WebsiteItem website)
    {
        await SaveCurrentDataAsync();
    }

    private async void CategoryView_WebsiteUpdated(
        WebsiteItem website)
    {
        await SaveCurrentDataAsync();
    }

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

        await Navigation.PushModalAsync(editPage);
    }

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

    private async Task ExportDataAsync()
    {
#if WINDOWS
        if (BindingContext is not MainViewModel vm)
            return;

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
#else
        await DisplayAlertAsync(
            "Export",
            "Export is currently implemented for Windows.",
            "OK");
#endif
    }
}