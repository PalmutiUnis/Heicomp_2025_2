namespace Heicomp_2025_2.Views.Configuracoes;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public partial class ConfiguracoesPage : ContentPage, INotifyPropertyChanged
{
    private bool _temaEscuroEnabled;

    public bool TemaEscuroEnabled
    {
        get => _temaEscuroEnabled;
        set
        {
            if (_temaEscuroEnabled != value)
            {
                _temaEscuroEnabled = value;
                OnPropertyChanged(); // Notifica a UI que o valor mudou
                AplicarTema(value); // Aplica o tema
            }
        }
    }

    public ConfiguracoesPage()
    {
        InitializeComponent();
        // Carrega a preferência salva e define o estado inicial do Switch
        TemaEscuroEnabled = Preferences.Get("TemaEscuro", false);
        BindingContext = this; // Conecta a UI (XAML) com este código
    }

    // Evento: Botão Voltar
    private async void OnVoltarClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Voltar", "Voltando para a tela anterior...", "OK");
        // await Navigation.PopAsync(); // Descomente quando tiver navegação configurada
    }

    // Evento: Salvar Alterações
    private async void OnSalvarClicked(object sender, EventArgs e)
    {
        // Captura os valores dos outros switches
        bool pushNotif = SwitchPushNotif.IsToggled;
        bool emailNotif = SwitchEmailNotif.IsToggled;
        
        // A lógica do tema escuro não é mais necessária aqui, pois é aplicada instantaneamente.

        string mensagem = $"Configurações salvas!\n\n" +
                         $"Push Notifications: {(pushNotif ? "Ativado" : "Desativado")}\n" +
                         $"Email Notifications: {(emailNotif ? "Ativado" : "Desativado")}\n" +
                         $"Tema Escuro: {(TemaEscuroEnabled ? "Ativado" : "Desativado")}";

        await DisplayAlert("Sucesso", mensagem, "OK");
    }

    // Evento: Idioma
    private async void OnIdiomaClicked(object sender, EventArgs e)
    {
        string resultado = await DisplayActionSheet(
            "Selecione o Idioma",
            "Cancelar",
            null,
            "Português",
            "English",
            "Español"
        );

        if (resultado != null && resultado != "Cancelar")
        {
            await DisplayAlert("Idioma", $"Idioma selecionado: {resultado}", "OK");
        }
    }

    // Evento: Ajuda e Suporte
    private async void OnAjudaClicked(object sender, EventArgs e)
    {
        await DisplayAlert(
            "Ajuda e Suporte",
            "Entre em contato:\n\nEmail: suporte@heicomp.com\nTelefone: (35) 1234-5678",
            "OK"
        );
    }

    // Evento: Termos de Uso
    private async void OnTermosClicked(object sender, EventArgs e)
    {
        await DisplayAlert(
            "Termos de Uso",
            "Ao usar este aplicativo, você concorda com nossos termos e políticas de privacidade.",
            "Li e aceito"
        );
    }

    private void AplicarTema(bool temaEscuro)
    {
        try
        {
            AppTheme themeRequested = temaEscuro ? AppTheme.Dark : AppTheme.Light;

            if (Application.Current.UserAppTheme == themeRequested)
                return;

            Application.Current.UserAppTheme = themeRequested;

            // Salva a preferência
            Preferences.Set("TemaEscuro", temaEscuro);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao aplicar tema: {ex.Message}");
        }
    }

    #region INotifyPropertyChanged Implementation
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}