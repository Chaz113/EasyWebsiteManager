using EasyWebsiteManager.ViewModels;

namespace EasyWebsiteManager;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel();
    }

    private async void AddButton_Clicked(object sender, EventArgs e)
    {
        await DisplayAlert(
            "Add Website",
            "Our Add Website window is coming next.",
            "OK");
    }
}