using MauiApp1.Views.Auth;


namespace MauiApp1
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
            Routing.RegisterRoute("ListaColaboradoresPage", typeof(MauiApp1.Views.Dashboards.ListaColaboradores.ListaColaboradoresPage));
            Routing.RegisterRoute("AdicionarUsuarioPage", typeof(MauiApp1.Views.Administrativa.AdicionarUsuarioPage));
        }
    }
}
