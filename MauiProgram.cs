using CommunityToolkit.Maui;
using MauiApp1.Services;
using MauiApp1.ViewModels.Dashboards;
using MauiApp1.Views.Auth;
using MauiApp1.Views.Dashboards;
using MauiApp1.Views.Dashboards.ListaColaboradores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using Plugin.LocalNotification;
// ADICIONADO: Necessário para os gráficos funcionarem
using SkiaSharp.Views.Maui.Controls.Hosting;
using Microcharts.Maui;

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseSkiaSharp()
                .UseMicrocharts()
                .UseLocalNotification()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ===========================================================
            // 📌 Carrega o appsettings.json
            // ===========================================================
            using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").Result;
            builder.Configuration.AddJsonStream(stream);

            // ===========================================================
            // 📌 Registrar Serviços
            // ===========================================================

            // Factory de conexão
            builder.Services.AddSingleton<IMySqlConnectionFactory, MySqlConnectionFactory>();

            // Módulo Turnover (já existia)
            builder.Services.AddSingleton<TurnoverRepository>();

            // Serviços de Cargos
            builder.Services.AddTransient<ICargosService, CargosService>();
            builder.Services.AddTransient<CargosViewModel>();

            // Páginas globais
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<PainelGestaoPage>();
            builder.Services.AddTransient<CargosPage>();

            // ===========================================================
            // 📌 Módulo Colaboradores
            // ===========================================================

            builder.Services.AddTransient<ColaboradoresService>();

            // ViewModels
            builder.Services.AddTransient<ColaboradoresViewModel>();
            builder.Services.AddTransient<ListaColaboradoresViewModel>();

            // Páginas que recebem VM via DI
            builder.Services.AddTransient<ColaboradoresPage>();
            builder.Services.AddTransient<ListaColaboradoresPage>();

            // ===========================================================
            // 📌 Módulo Diversidade (CORRIGIDO)
            // ===========================================================

            // 1. Serviço
            builder.Services.AddTransient<DiversidadeService>();

            // 2. ViewModel
            builder.Services.AddTransient<DiversidadeViewModel>();

            // 3. Page
            builder.Services.AddTransient<DiversidadePage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

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
