namespace Heicomp_2025_2.Views.Configuracoes;

public partial class ConfiguracoesPage : ContentPage
{
	public ConfiguracoesPage()
	{
		InitializeComponent();
	}

    private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PainelGestaoPage");
    }
}