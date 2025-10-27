namespace MauiApp1; // ← MUDE para o nome do seu projeto!

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Carrega o tema salvo
        CarregarTema();

        MainPage = new AppShell();
    }

    private void CarregarTema()
    {
        try
        {
            if (Preferences.ContainsKey("TemaEscuro"))
            {
                bool temaEscuro = Preferences.Get("TemaEscuro", false);

                if (temaEscuro)
                {
                    UserAppTheme = AppTheme.Dark;
                }
                else
                {
                    UserAppTheme = AppTheme.Light;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro: {ex.Message}");
            UserAppTheme = AppTheme.Light;
        }
    }
}