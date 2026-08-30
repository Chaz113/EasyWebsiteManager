using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace EasyWebsiteManager;

[Activity(
    Label = "Easy Website Manager",
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    Exported = true,
    Icon = "@mipmap/appicon",
    RoundIcon = "@mipmap/appicon_round",
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges =
        ConfigChanges.ScreenSize |
        ConfigChanges.Orientation |
        ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private const int CreateBackupRequestCode = 2001;

    private TaskCompletionSource<Android.Net.Uri?>?
        _createBackupTaskCompletionSource;

    public Task<Android.Net.Uri?> CreateBackupFileAsync(
        string suggestedFileName)
    {
        _createBackupTaskCompletionSource =
            new TaskCompletionSource<Android.Net.Uri?>();

        var intent =
            new Intent(Intent.ActionCreateDocument);

        intent.AddCategory(Intent.CategoryOpenable);
        intent.SetType("application/json");

        intent.PutExtra(
            Intent.ExtraTitle,
            suggestedFileName);

#pragma warning disable CS0618
        StartActivityForResult(
            intent,
            CreateBackupRequestCode);
#pragma warning restore CS0618

        return _createBackupTaskCompletionSource.Task;
    }

#pragma warning disable CS0672, CS0618
    protected override void OnActivityResult(
        int requestCode,
        Result resultCode,
        Intent? data)
    {
        base.OnActivityResult(
            requestCode,
            resultCode,
            data);

        if (requestCode != CreateBackupRequestCode)
            return;

        var resultUri =
            resultCode == Result.Ok
                ? data?.Data
                : null;

        _createBackupTaskCompletionSource?
            .TrySetResult(resultUri);

        _createBackupTaskCompletionSource = null;
    }
#pragma warning restore CS0672, CS0618
}