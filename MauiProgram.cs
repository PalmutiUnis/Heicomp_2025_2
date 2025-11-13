using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Microsoft.Extensions.Configuration;
using MauiApp1.Services;
using Microsoft.Maui.Storage;
using Heicomp_2025_2.Views.Auth;
using Microcharts.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMicrocharts()
                .UseSkiaSharp()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            try
            {
                using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").Result;
                using var reader = new StreamReader(stream);
                var json = reader.ReadToEnd();
                var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;
                var configBuilder = new ConfigurationBuilder();
                if (root.TryGetProperty("MySql", out var mySqlSection))
                {
                    foreach (var prop in mySqlSection.EnumerateObject())
                    {
                        builder.Configuration[prop.Name.StartsWith("MySql:") ? prop.Name : $"MySql:{prop.Name}"] = prop.Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load appsettings.json: {ex.Message}");
            }

            builder.Services.AddSingleton<IMySqlConnectionFactory, MySqlConnectionFactory>();
            builder.Services.AddSingleton<TurnoverRepository>();
            builder.Services.AddTransient<ICargosService, CargosService>();
            builder.Services.AddTransient<IDiversidadeService, DiversidadeService>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<Heicomp_2025_2.Views.Dashboards.PainelGestaoPage>();
            builder.Services.AddTransient<Heicomp_2025_2.ViewModels.Dashboards.PainelGestaoViewModel>();
            builder.Services.AddTransient<Heicomp_2025_2.ViewModels.Dashboards.RotatividadeViewModel>();
            builder.Services.AddTransient<Heicomp_2025_2.Views.Dashboards.RotatividadePage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Cria o app
            var app = builder.Build();
          
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
