using MauiApp1.Views.Auth;


namespace MauiApp1
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
            Routing.RegisterRoute("ListaColaboradoresPage", typeof(Heicomp_2025_2.Views.Dashboards.ListaColaboradores.ListaColaboradoresPage));
        }
    }
}
