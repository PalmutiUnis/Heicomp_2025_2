namespace Heicomp_2025_2.Views.Cards;
public partial class CardGrande : ContentView
{
    // Propriedades que você pode passar pelo XAML da página
    public static readonly BindableProperty TituloProperty =
        BindableProperty.Create(nameof(Titulo), typeof(string), typeof(CardGrande), string.Empty);

    public static readonly BindableProperty NumeroProperty =
        BindableProperty.Create(nameof(Numero), typeof(string), typeof(CardGrande), string.Empty);

    public static readonly BindableProperty IndicadorProperty =
        BindableProperty.Create(nameof(Indicador), typeof(string), typeof(CardGrande), string.Empty);

    public static readonly BindableProperty IconeProperty =
        BindableProperty.Create(nameof(Icone), typeof(string), typeof(CardGrande), string.Empty);

    // Propriedades públicas para facilitar o binding
    public string Titulo { get => (string)GetValue(TituloProperty); set => SetValue(TituloProperty, value); }
    public string Numero { get => (string)GetValue(NumeroProperty); set => SetValue(NumeroProperty, value); }
    public string Indicador { get => (string)GetValue(IndicadorProperty); set => SetValue(IndicadorProperty, value); }
    public string Icone { get => (string)GetValue(IconeProperty); set => SetValue(IconeProperty, value); }

    public CardGrande()
    {
        InitializeComponent();  // Inicializa o XAML do card
        BindingContext = this;  // Conecta o XAML do card às propriedades do code-behind
    }
}

