using EasyWebsiteManager.ViewModels;

namespace EasyWebsiteManager;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel();
    }

    private async void AddButton_Clicked(object? sender, EventArgs e)
    {
        var category = await DisplayPromptAsync(
            "Category",
            "Enter category name:");

        if (string.IsNullOrWhiteSpace(category))
            return;

        var website = await DisplayPromptAsync(
            "Website",
            "Enter website name:");

        if (string.IsNullOrWhiteSpace(website))
            return;

        var url = await DisplayPromptAsync(
            "URL",
            "Enter website address:");

        if (string.IsNullOrWhiteSpace(url))
            return;

        if (BindingContext is MainViewModel vm)
        {
            vm.AddWebsite(category, website, url);
        }
    }
}