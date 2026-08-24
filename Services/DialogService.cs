namespace EasyWebsiteManager.Services;

public static class DialogService
{
    public static async Task ShowMessageAsync(
        Page page,
        string title,
        string message)
    {
        await page.DisplayAlertAsync(
            title,
            message,
            "OK");
    }

    public static async Task<bool> ConfirmAsync(
        Page page,
        string title,
        string message,
        string acceptText = "Delete",
        string cancelText = "Cancel")
    {
        return await page.DisplayAlertAsync(
            title,
            message,
            acceptText,
            cancelText);
    }
}