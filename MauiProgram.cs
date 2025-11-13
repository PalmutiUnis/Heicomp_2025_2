using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Microsoft.Extensions.Configuration;
using MauiApp1.Services;
using Microsoft.Maui.Storage;
using Heicomp_2025_2.Views.Auth;
using Heicomp_2025_2.Services;
using Heicomp_2025_2.ViewModels.Dashboards;

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseMauiCommunityToolkit() // Agora reconhecido
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ✅ Carrega corretamente o appsettings.json
            try
            {
                using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").Result;
                builder.Configuration.AddJsonStream(stream);
                System.Diagnostics.Debug.WriteLine("✅ appsettings.json carregado com sucesso!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Falha ao carregar appsettings.json: {ex.Message}");
            }

            // Register MySQL connection factory & repository
            builder.Services.AddSingleton<IMySqlConnectionFactory, MySqlConnectionFactory>();
            builder.Services.AddSingleton<TurnoverRepository>();

            // Register CargosService via interface
            builder.Services.AddTransient<ICargosService, CargosService>();
            builder.Services.AddTransient<CargosViewModel>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<MauiApp1.Views.Dashboards.PainelGestaoPage>();
            builder.Services.AddTransient<CargosPage>();

            // ✅ Adições específicas para o módulo de Colaboradores
            builder.Services.AddSingleton<ColaboradoresService>();            // camada de acesso ao banco
            builder.Services.AddTransient<ColaboradoresViewModel>();          // ViewModel principal (dashboard)
            builder.Services.AddTransient<ListaColaboradoresViewModel>();     // ViewModel da lista completa

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

#if ANDROID
            // Handler para remover espaço lateral do Shell.TitleView no ANDROID
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
