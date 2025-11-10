using Heicomp_2025_2.Views.Auth;

namespace MauiApp1
{
    public partial class App : Application
    {
        // Construtor padrão usado quando o MAUI cria a aplicação antes do container estar pronto
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new LoginPage());
        }

        // Construtor alternativo usado quando DI já pode fornecer a LoginPage
        public App(LoginPage loginPage)
        {
            InitializeComponent();
            MainPage = new NavigationPage(loginPage);
        }
    }
}
