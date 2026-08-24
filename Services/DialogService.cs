namespace EasyWebsiteManager.Services;

public static class DialogService
{
    public static async Task ShowMessageAsync(
        Page page,
        string title,
        string message)
    {
        await page.DisplayAlertAsync(title, message, "OK");
    }
}