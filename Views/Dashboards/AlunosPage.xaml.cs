namespace Heicomp_2025_2.Views.Dashboards;

public partial class AlunosPage : ContentPage
{
	// Construtor padrão simples; sem teste de conexão
	public AlunosPage()
	{
		InitializeComponent();
	}

	private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("//PainelGestaoPage");
	}
}