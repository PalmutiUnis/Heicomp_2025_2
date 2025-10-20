namespace Heicomp_2025_2.Views.Cards;

public partial class CardPequeno : ContentView
{    // Propriedades que voc� pode passar pelo XAML da p�gina
    public static readonly BindableProperty TituloProperty =
        BindableProperty.Create(nameof(Titulo), typeof(string), typeof(CardPequeno), string.Empty);

    public static readonly BindableProperty NumeroProperty =
        BindableProperty.Create(nameof(Numero), typeof(string), typeof(CardPequeno), string.Empty);

    public static readonly BindableProperty IndicadorProperty =
        BindableProperty.Create(nameof(Indicador), typeof(string), typeof(CardPequeno), string.Empty);

    public static readonly BindableProperty IconeProperty =
        BindableProperty.Create(nameof(Icone), typeof(string), typeof(CardPequeno), string.Empty);

    // Propriedades p�blicas para facilitar o binding
    public string Titulo { get => (string)GetValue(TituloProperty); set => SetValue(TituloProperty, value); }
    public string Numero { get => (string)GetValue(NumeroProperty); set => SetValue(NumeroProperty, value); }
    public string Indicador { get => (string)GetValue(IndicadorProperty); set => SetValue(IndicadorProperty, value); }
    public string Icone { get => (string)GetValue(IconeProperty); set => SetValue(IconeProperty, value); }

    public CardPequeno()
    {
        InitializeComponent();

    }

}