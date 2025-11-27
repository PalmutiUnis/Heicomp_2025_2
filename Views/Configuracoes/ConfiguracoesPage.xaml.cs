namespace MauiApp1.Views.Configuracoes;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Plugin.LocalNotification;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

public partial class ConfiguracoesPage : ContentPage
{

    // 1. Propriedades públicas para o XAML
    // O XAML vai se ligar a "NomeUsuario", "EmailUsuario" e "InitialsUsuario"
    public string userName { get; set; }
    public string userEmail { get; set; }



    // *************************** Evento: Botao Voltar ***************************
    private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PainelGestaoPage");
    }
    // *************************** Fim Evento: Botão Voltar ***************************



    // *************************** Evento: Salvar Configura��es ***************************
    private async void OnSalvarClicked(object sender, EventArgs e)
    {
        // 1. Tentar ler os Switches
        bool isPushOn = Preferences.Get("PushNotificationsEnabled", false); // Usando Preferences como proxy de estado
        bool isTemaOn = Preferences.Get("TemaEscuro", false);

        // NOTE: Se quiser ler diretamente do Switch na tela, você precisa dar x:Name="PushSwitch"
        // e usar: bool isPushOn = PushSwitch.IsToggled;

        string statusPush = isPushOn ? "Ativado" : "Desativado";
        string statusTema = isTemaOn ? "Ativado" : "Desativado";

        string mensagem = $"Configurações salvas!\n\n" +
                         $"Push Notifications: {statusPush}\n" +
                         $"Tema Escuro: {statusTema}";

        await DisplayAlert("Sucesso", mensagem, "OK");
    }
    // *************************** Fim Evento: Salvar Configurações ***************************



    // *************************** Propriedade: Tema Escuro ***************************
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
    // *************************** Fim Propriedade: Tema Escuro ***************************



    // *************************** Propriedade: Carregar dados do usuário ***************************
    private void CarregarDadosUsuario()
    {
        // Lê os valores salvos no LoginPage. Se não existirem, usa valores padrão.
        userName = Preferences.Get("userName", "Usuário Desconhecido");
        userEmail = Preferences.Get("userEmail", "sem.email@unis.com");
    }
    // *************************** Fim Propriedade: Carregar dados do usuário ***************************



    // *************************** Construtor ***************************
    public ConfiguracoesPage()
    {
        InitializeComponent();
        // 1. Permite o uso de notificações locais 
        LocalNotificationCenter.Current.RequestNotificationPermission();
        BindingContext = this; // Conecta a UI (XAML) com este c�digo

        //2. Carrega os dados do usuário
        CarregarDadosUsuario();

        // 3. Define o BindingContext da p�gina para ELA MESMA.
        this.BindingContext = this;

        //4. Carrega a preferência do tema
        TemaEscuroEnabled = Preferences.Get("TemaEscuro", false);

    }
    // *************************** Fim Construtor ***************************



    // *************************** Evento: Ajuda e Suporte ***************************
    private async void OnAjudaClicked(object sender, EventArgs e)
    {
        await DisplayAlert(
            "Ajuda e Suporte",
            "Entre em contato:\n\nEmail: suporte@heicomp.com\nTelefone: (35) 1234-5678",
            "OK"
        );
    }
    // *************************** Fim Evento: Ajuda e Suporte ***************************



    // *************************** Evento: Termos de Uso ***************************
    private async void OnTermosClicked(object sender, EventArgs e)
    {
        await DisplayAlert(
            "Termos de Uso",
            "Ao usar este aplicativo, você concorda com nossos termos e políticas de privacidade.",
            "Li e aceito"
        );
    }
    // *************************** Fim Evento: Termos de Uso ***************************



    // *************************** Evento: Notificacoes em Push ***************************
    private async void OnPushNotificationsToggled(object sender, ToggledEventArgs e)
    {
        bool isEnabled = e.Value;
        Preferences.Set("PushNotificationsEnabled", isEnabled);

        // 4. Se o botao foi LIGADO, envia a notificacao
        if (isEnabled)
        {
            // Cria a notificacao
            var request = new NotificationRequest
            {
                NotificationId = 1337, // Um ID unico para esta notificacao
                Title = "Notificações Ativadas!",
                Description = "Você agora receberá alertas e promoções.",
                BadgeNumber = 1, // N�mero que aparece no icone do app
                Schedule = new NotificationRequestSchedule
                {
                    // Dispara a notificacao 1 segundo apos ligar o switch
                    NotifyTime = DateTime.Now.AddSeconds(0.3)
                }
            };

            // Envia a notifica��o
            await LocalNotificationCenter.Current.Show(request);
        }
    }
    // *************************** Fim Evento: Notificações em Push ***************************



    // *************************** Evento: Alterar tema da app ***************************
    private void AplicarTema(bool temaEscuro)
    {
        try
        {
            AppTheme themeRequested = temaEscuro ? AppTheme.Dark : AppTheme.Light;

            if (Application.Current.UserAppTheme == themeRequested)
                return;

            Application.Current.UserAppTheme = themeRequested;

            // Salva a prefer�ncia
            Preferences.Set("TemaEscuro", temaEscuro);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao aplicar tema: {ex.Message}");
        }
    }
}