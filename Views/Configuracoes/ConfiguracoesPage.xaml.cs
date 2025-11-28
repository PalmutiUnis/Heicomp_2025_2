using System.ComponentModel;
using System.Runtime.CompilerServices;
using Plugin.LocalNotification;
using Microsoft.Maui.Storage;

namespace MauiApp1.Views.Configuracoes
{
    // Adicionamos a interface INotifyPropertyChanged para o Binding funcionar 100%
    public partial class ConfiguracoesPage : ContentPage, INotifyPropertyChanged
    {
        // Campos privados
        private string _userName;
        private string _userEmail;
        private bool _temaEscuroEnabled;

        // -----------------------------------------------------------
        // 1. PROPRIEDADES PÚBLICAS (Com o nome EXATO do XAML)
        // -----------------------------------------------------------
        public string userName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(); // Avisa a tela que mudou!
            }
        }

        public string userEmail
        {
            get => _userEmail;
            set
            {
                _userEmail = value;
                OnPropertyChanged(); // Avisa a tela que mudou!
            }
        }

        public bool TemaEscuroEnabled
        {
            get => _temaEscuroEnabled;
            set
            {
                if (_temaEscuroEnabled != value)
                {
                    _temaEscuroEnabled = value;
                    OnPropertyChanged();
                    AplicarTema(value);
                }
            }
        }

        public ConfiguracoesPage()
        {
            InitializeComponent();
            LocalNotificationCenter.Current.RequestNotificationPermission();

            // 2. Define o BindingContext ANTES de carregar
            this.BindingContext = this;

            // 3. Carrega os dados
            CarregarDadosUsuario();

            TemaEscuroEnabled = Preferences.Get("TemaEscuro", false);
        }

        private void CarregarDadosUsuario()
        {
            // Usa as MESMAS chaves do Login ("user_name" e "user_email")
            userName = Preferences.Get("user_name", "Usuário Visitante");
            userEmail = Preferences.Get("user_email", "sem.email@unis.com");
        }

        // ... SEUS OUTROS MÉTODOS (Botões, Switches) MANTIDOS IGUAIS ...

        private async void BotaoVoltarPainelGestao(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//PainelGestaoPage");
        }

        private async void OnSalvarClicked(object sender, EventArgs e)
        {
            // Exemplo de leitura direta do componente (se tiver x:Name no XAML)
            // Se não tiver x:Name, use as preferências salvas
            bool isPushOn = Preferences.Get("PushNotificationsEnabled", false);

            string statusPush = isPushOn ? "Ativado" : "Desativado";
            string statusTema = TemaEscuroEnabled ? "Ativado" : "Desativado";

            await DisplayAlert("Sucesso", $"Configurações Salvas:\nPush: {statusPush}\nTema: {statusTema}", "OK");
        }

        private async void OnAjudaClicked(object sender, EventArgs e) => await DisplayAlert("Ajuda", "Contato: suporte@heicomp.com", "OK");

        private async void OnTermosClicked(object sender, EventArgs e) => await DisplayAlert("Termos", "Termos de uso...", "OK");

        private async void OnPushNotificationsToggled(object sender, ToggledEventArgs e)
        {
            bool isEnabled = e.Value;
            Preferences.Set("PushNotificationsEnabled", isEnabled);

            if (isEnabled)
            {
                var request = new NotificationRequest
                {
                    NotificationId = 1337, // Um ID unico para esta notificacao
                    Title = "Notificações Ativadas!",
                    Description = "Você agora receberá alertas e promoções.",
                    BadgeNumber = 1, // N�mero que aparece no icone do app
                    Schedule = new NotificationRequestSchedule
                    {
                        // Dispara a notificacao 1 segundo apos ligar o switch
                        NotifyTime = DateTime.Now.AddMilliseconds(300)
                    }
                };
                await LocalNotificationCenter.Current.Show(request);
            }
        }

        private void AplicarTema(bool temaEscuro)
        {
            try
            {
                AppTheme theme = temaEscuro ? AppTheme.Dark : AppTheme.Light;
                Application.Current.UserAppTheme = theme;
                Preferences.Set("TemaEscuro", temaEscuro);
            }
            catch { }
        }

        // Implementação Padrão do INotifyPropertyChanged
        public new event PropertyChangedEventHandler PropertyChanged;
        protected new void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}