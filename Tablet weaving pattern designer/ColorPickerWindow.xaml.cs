using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Tablet_weaving_pattern_designer
{
    public partial class ColorPickerWindow : Window, INotifyPropertyChanged
    {
        public double Hue { get; set; } = 1.0;
        public double Value { get; set; } = 1.0;

        private double _saturation = 1.0;

        public double Saturation
        {
            get => _saturation;
            set
            {
                _saturation = value;
                UpdateColor();
                OnPropertyChanged(nameof(Saturation));
            }
        }

        private Brush _selectedColorBrush = Brushes.White;

        public Brush SelectedColorBrush
        {
            get => _selectedColorBrush;
            set
            {
                _selectedColorBrush = value;
                OnPropertyChanged(nameof(SelectedColorBrush));
            }
        }

        public Color SelectedColor { get; private set; }

        public ColorPickerWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void ColorArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PickColor(e.GetPosition(ColorArea));
        }

        private void ColorArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                PickColor(e.GetPosition(ColorArea));
        }

        private void PickColor(Point p)
        {
            double w = ColorArea.ActualWidth;
            double h = ColorArea.ActualHeight;

            Hue = (p.X / w) * 360.0;
            Value = 1.0 - (p.Y / h);

            UpdateColor();
        }

        private void UpdateColor()
        {
            SelectedColor = ColorFromHSV();
            SelectedColorBrush = new SolidColorBrush(SelectedColor);
        }

        private Color ColorFromHSV()
        {
            double c = Value * Saturation;
            double x = c * (1 - Math.Abs((Hue / 60 % 2) - 1));
            double m = Value - c;

            double r, g, b;

            if (Hue < 60)
                (r, g, b) = (c, x, 0);
            else if (Hue < 120)
                (r, g, b) = (x, c, 0);
            else if (Hue < 180)
                (r, g, b) = (0, c, x);
            else if (Hue < 240)
                (r, g, b) = (0, x, c);
            else if (Hue < 300)
                (r, g, b) = (x, 0, c);
            else
                (r, g, b) = (c, 0, x);

            return Color.FromRgb(
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((b + m) * 255)
            );
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}