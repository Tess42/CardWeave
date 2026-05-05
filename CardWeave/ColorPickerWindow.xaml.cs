using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CardWeave
{
    /// <summary>
    /// Window providing an HSV-based color picker for selecting custom colors.
    /// </summary>
    public partial class ColorPickerWindow : Window, INotifyPropertyChanged
    {
        /// <summary> Current hue. </summary>
        public double Hue { get; set; } = 1.0;

        /// <summary> Current value. </summary>
        public double Value { get; set; } = 1.0;

        /// <summary> Current saturation. </summary>
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

        /// <summary>
        /// Brush representing the currently selected color (used for UI preview).
        /// </summary>
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

        /// <summary> The currently selected RGB color. </summary>
        public Color SelectedColor { get; private set; }

        /// <summary>
        /// Initializes the color picker window and sets up data binding.
        /// </summary>
        public ColorPickerWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// Handles mouse click in the color area and picks a color at the clicked position.
        /// </summary>
        private void ColorArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PickColor(e.GetPosition(ColorArea));
        }

        /// <summary>
        /// Handles mouse drag in the color area to continuously update the selected color.
        /// </summary>
        private void ColorArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                PickColor(e.GetPosition(ColorArea));
        }

        /// <summary>
        /// Converts a point inside the color area into HSV values and updates the color.
        /// </summary>
        private void PickColor(Point p)
        {
            double w = ColorArea.ActualWidth;
            double h = ColorArea.ActualHeight;

            Hue = (p.X / w) * 360.0;
            Value = 1.0 - (p.Y / h);

            UpdateColor();
        }

        /// <summary>
        /// Recalculates the selected color from HSV values and updates the preview brush.
        /// </summary>
        private void UpdateColor()
        {
            SelectedColor = ColorFromHSV();
            SelectedColorBrush = new SolidColorBrush(SelectedColor);
        }

        /// <summary>
        /// Converts the current HSV values to an RGB color.
        /// </summary>
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

        /// <summary>
        /// Confirms the selected color and closes the window.
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Raises the PropertyChanged event for UI updates.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}