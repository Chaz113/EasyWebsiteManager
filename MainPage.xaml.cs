using EasyWebsiteManager.ViewModels;
using EasyWebsiteManager.Views;

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
        if (BindingContext is not MainViewModel vm)
            return;

        var addPage = new AddWebsitePage(vm.Categories);

        addPage.WebsiteSaved += (category, website, url) =>
        {
            vm.AddWebsite(category, website, url);
        };

        await Navigation.PushModalAsync(addPage);
    }
}