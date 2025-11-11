using Microsoft.Maui.Graphics;

namespace Heicomp_2025_2.Controls
{
    public class CircularGaugeView : GraphicsView
    {
        public static readonly BindableProperty ProgressProperty =
            BindableProperty.Create(nameof(Progress), typeof(double), typeof(CircularGaugeView), 0.0,
                propertyChanged: OnProgressChanged);

        public static readonly BindableProperty ProgressColorProperty =
            BindableProperty.Create(nameof(ProgressColor), typeof(Color), typeof(CircularGaugeView), Colors.Blue,
                propertyChanged: OnProgressChanged);

        public static readonly BindableProperty TrackColorProperty =
            BindableProperty.Create(nameof(TrackColor), typeof(Color), typeof(CircularGaugeView), Colors.LightGray,
                propertyChanged: OnProgressChanged);

        public static readonly BindableProperty StrokeWidthProperty =
            BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(CircularGaugeView), 20f,
                propertyChanged: OnProgressChanged);

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public Color ProgressColor
        {
            get => (Color)GetValue(ProgressColorProperty);
            set => SetValue(ProgressColorProperty, value);
        }

        public Color TrackColor
        {
            get => (Color)GetValue(TrackColorProperty);
            set => SetValue(TrackColorProperty, value);
        }

        public float StrokeWidth
        {
            get => (float)GetValue(StrokeWidthProperty);
            set => SetValue(StrokeWidthProperty, value);
        }

        public CircularGaugeView()
        {
            Drawable = new CircularGaugeDrawable(this);
        }

        private static void OnProgressChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is CircularGaugeView view)
            {
                view.Invalidate();
            }
        }

        private class CircularGaugeDrawable : IDrawable
        {
            private readonly CircularGaugeView _view;

            public CircularGaugeDrawable(CircularGaugeView view)
            {
                _view = view;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                var centerX = dirtyRect.Width / 2;
                var centerY = dirtyRect.Height / 2;
                var radius = Math.Min(dirtyRect.Width, dirtyRect.Height) / 2 - _view.StrokeWidth;

                // Desenhar círculo de fundo (track)
                canvas.StrokeColor = _view.TrackColor;
                canvas.StrokeSize = _view.StrokeWidth;
                canvas.DrawCircle(centerX, centerY, radius);

                // Desenhar arco de progresso
                if (_view.Progress > 0)
                {
                    canvas.StrokeColor = _view.ProgressColor;
                    canvas.StrokeSize = _view.StrokeWidth;
                    canvas.StrokeLineCap = LineCap.Round;

                    // Calcular o ângulo baseado no progresso (0-100% = 0-360 graus)
                    var sweepAngle = (float)(_view.Progress * 3.6); // 100% = 360 graus

                    // Começar do topo (-90 graus)
                    var startAngle = -90f;

                    canvas.DrawArc(
                        centerX - radius,
                        centerY - radius,
                        radius * 2,
                        radius * 2,
                        startAngle,
                        sweepAngle,
                        clockwise: true,
                        closed: false
                    );
                }
            }
        }
    }
}
