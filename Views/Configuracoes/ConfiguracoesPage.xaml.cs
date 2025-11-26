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
    public string NomeUsuario { get; set; }
    public string EmailUsuario { get; set; }



    // *************************** Evento: Bot�o Voltar ***************************
    private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PainelGestaoPage");
    }
    // *************************** Fim Evento: Botão Voltar ***************************



    // *************************** Evento: Salvar Configura��es ***************************
    private async void OnSalvarClicked(object sender, EventArgs e)
    {
        //           
        //              bool pushNotif = OnPushNotificationsToggled.IsEnabled;
        //    
        //           
        //    
        //              string mensagem = $"Configura��es salvas!\n\n" +
        //                               $"Push Notifications: {(pushNotif ? "Ativado" : "Desativado")}\n" +
        //                               $"Tema Escuro: {(TemaEscuroEnabled ? "Ativado" : "Desativado")}";
        //    
        //              await DisplayAlert("Sucesso", mensagem, "OK");
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



    // *************************** Construtor ***************************
    public ConfiguracoesPage()
    {
        InitializeComponent();
        // 1. Permite o uso de notificações locais 
        LocalNotificationCenter.Current.RequestNotificationPermission();
        BindingContext = this; // Conecta a UI (XAML) com este c�digo

        // 2. Carrega os dados salvos do Preferences
        NomeUsuario = Preferences.Get("user_name", "Usu�rio"); // "Usu�rio" � um valor padr�o
        EmailUsuario = Preferences.Get("user_email", "email@exemplo.com"); // "email..." � um valor padr�o


        // 3. Define o BindingContext da p�gina para ELA MESMA.
        // Agora o XAML {Binding NomeUsuario} vai encontrar a propriedade p�blica acima.
        this.BindingContext = this;

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
            "Ao usar este aplicativo, voc� concorda com nossos termos e pol�ticas de privacidade.",
            "Li e aceito"
        );
    }
    // *************************** Fim Evento: Termos de Uso ***************************



    // *************************** Evento: Notifica��es em Push ***************************
    private async void OnPushNotificationsToggled(object sender, ToggledEventArgs e)
    {
        bool isEnabled = e.Value;
        Preferences.Set("PushNotificationsEnabled", isEnabled);

        // 4. Se o bot�o foi LIGADO, envia a notifica��o
        if (isEnabled)
        {
            // Cria a notifica��o
            var request = new NotificationRequest
            {
                NotificationId = 1337, // Um ID �nico para esta notifica��o
                Title = "Notifica��es Ativadas!",
                Description = "Voc� agora receber� alertas e promo��es.",
                BadgeNumber = 1, // N�mero que aparece no �cone do app
                Schedule = new NotificationRequestSchedule
                {
                    // Dispara a notifica��o 1 segundo ap�s ligar o switch
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