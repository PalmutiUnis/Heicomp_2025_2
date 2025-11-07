using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using CommunityToolkit.Maui; // <-- 1. ADICIONE ESTE 'using'
namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLocalNotification()
                .UseMauiCommunityToolkit() // <-- 2. ADICIONE ESTA LINHA AQUI
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Cria o app
            var app = builder.Build();

            // 🔥 Captura exceções não tratadas (inclusive XAML)
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("🔥 Unhandled Exception: " + e.ExceptionObject.ToString());
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("🔥 Task Exception: " + e.Exception.ToString());
            };


            // Handler para remover espaço lateral do Shell.TitleView no ANDROID

            #if ANDROID
                Microsoft.Maui.Handlers.ToolbarHandler.Mapper.AppendToMapping("CustomNavigationView", (handler, view) =>
                        {
                            handler.PlatformView.ContentInsetStartWithNavigation = 0;
                            handler.PlatformView.SetContentInsetsAbsolute(0, 0);
                        });
            #endif

            return app;
        }
    }
}
