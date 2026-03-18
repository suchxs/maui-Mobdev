using Microsoft.Extensions.Logging;
#if IOS || MACCATALYST
using Microsoft.Maui.Handlers;
using UIKit;
#endif

namespace listView_Corsega;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if IOS || MACCATALYST
        // Remove the native UITextField rounded border so only the outer MAUI Border is visible.
        EntryHandler.Mapper.AppendToMapping("NoNativeEntryBorder", (handler, view) =>
        {
            handler.PlatformView.BorderStyle = UITextBorderStyle.None;
            handler.PlatformView.BackgroundColor = UIColor.Clear;
        });
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}