using CommunityToolkit.Maui;
using MauiApp1.Services;
using MauiApp1.Services.Admin;
using MauiApp1.ViewModels.Dashboards;
using MauiApp1.ViewModels.Administrativa;
using MauiApp1.Views.Auth;
using MauiApp1.Views.Administrativa;
using MauiApp1.Views.Dashboards;
using MauiApp1.Views.Dashboards.ListaColaboradores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using Plugin.LocalNotification;
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

            // Páginas com DI
            builder.Services.AddTransient<ColaboradoresPage>();
            builder.Services.AddTransient<ListaColaboradoresPage>();

            // ===========================================================
            // 📌 Módulo Diversidade
            // ===========================================================

            builder.Services.AddTransient<DiversidadeService>();
            builder.Services.AddTransient<DiversidadeViewModel>();
            builder.Services.AddTransient<DiversidadePage>();

            // ===========================================================
            // 📌 Módulo Administrativo
            // ===========================================================

            // Serviços
            builder.Services.AddSingleton<AdminService>();

            // ViewModels
            builder.Services.AddTransient<AreaAdministrativaViewModel>();
            builder.Services.AddTransient<AdicionarUsuarioViewModel>();

            // Páginas
            builder.Services.AddTransient<AreaAdministrativaPage>();
            builder.Services.AddTransient<AdicionarUsuarioPage>();


            // ===========================================================
            // 📌 Módulo Gráficos Detalhados
            // ===========================================================

            //Conexão com banco de dados e serviços para gráficos detalhados
            builder.Services.AddTransient<GraficosDetalhadosServices>();
            //VM para gráficos detalhados
            builder.Services.AddTransient<GraficosDetalhadosViewModel>();
            //Página para gráficos detalhados
            builder.Services.AddTransient<GraficosDetalhadosPage>();

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
