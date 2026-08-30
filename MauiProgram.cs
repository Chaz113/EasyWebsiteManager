using Microsoft.Extensions.Logging;

#if ANDROID
using Android.Content.Res;
using Microsoft.Maui.Handlers;
#endif

namespace EasyWebsiteManager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont(
                        "OpenSans-Regular.ttf",
                        "OpenSansRegular");

                    fonts.AddFont(
                        "OpenSans-Semibold.ttf",
                        "OpenSansSemibold");
                });

#if ANDROID
            SwitchHandler.Mapper.AppendToMapping(
                "EasyWebsiteManagerAndroidSwitch",
                (handler, view) =>
                {
                    var platformSwitch =
                        handler.PlatformView;

                    var states = new[]
                    {
                        new[]
                        {
                            Android.Resource.Attribute.StateChecked
                        },
                        new[]
                        {
                            -Android.Resource.Attribute.StateChecked
                        }
                    };

                    var trackColors = new[]
                    {
                        unchecked((int)0xFF3478F6), // ON blue
                        unchecked((int)0xFF667085)  // OFF grey
                    };

                    var thumbColors = new[]
                    {
                        unchecked((int)0xFFFFFFFF), // ON white
                        unchecked((int)0xFFFFFFFF)  // OFF white
                    };

                    platformSwitch.TrackTintList =
                        new ColorStateList(
                            states,
                            trackColors);

                    platformSwitch.ThumbTintList =
                        new ColorStateList(
                            states,
                            thumbColors);
                });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}