using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tablet_weaving_pattern_designer
{
    /// <summary>
    /// Main window logic, global state and UI initialization.
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary> Current band. </summary>
        public TabletBand Band { get; set; }

        /// <summary> Visibility of tablet number labels. </summary>
        public bool ColumnLabeling { get; set; } = true;

        /// <summary> Visibility of row number labels. </summary>
        public bool RowLabeling { get; set; } = true;

        /// <summary> Visibility of hole label column. </summary>
        public bool HoleLabeling { get; set; } = true;

        /// <summary> Visibility of hole labels in grid. </summary>
        public bool GridHoleLabeling { get; set; } = false;

        /// <summary> Background color of visualized band. </summary>
        public Color BandBackground { get; set; } = Colors.White;

        /// <summary> Tracks color usage and manages band color palette. </summary>
        private BandColorManager ColorManager { get; } = new();

        /// <summary> Indicates whether the backside of the band is shown. </summary>
        private bool _backSide = false;
        public bool BackSide
        {
            get => _backSide;
            set 
            {
                BackSideLabel.Visibility = value && Band.TabletCount != 0 && Band.RowCount != 0
                    ? Visibility.Visible 
                    : Visibility.Collapsed;

                _backSide = value;
            }
        }

        /// <summary> Currently selected color in the UI. </summary>
        private Color _currentColor = Colors.White;

        public Color CurrentColor
        {
            get => _currentColor;
            set
            {
                if (_currentColor != value)
                {
                    _currentColor = value;
                    OnPropertyChanged(nameof(CurrentColor));
                }
            }
        }

        /// <summary>
        /// Initializes window, band model, bindings and preset menu.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Band = new TabletBand();
            NumberOfHolesTextBox.Text = Band.ThreadCount.ToString();
            NumberOfTabletsTextBox.Text = Band.TabletCount.ToString();
            NumberOfRowsTextBox.Text = Band.RowCount.ToString();
            Dispatcher.InvokeAsync(() => ColorManager.ApplyColorsToSlots(ColorSlotsPanel));
            Dispatcher.InvokeAsync(() => LoadPresetsMenu());
        }

        /// <summary>
        /// Regenerates all visualization grids, labels and color palette.
        /// </summary>
        private void RedrawVisualization()
        {
            GridRenderer.GenerateTextRow(Band, TabletDirectionRow, j => Band.Tablets[j].Threading == ThreadingDirection.S ? "S" : "Z", true, Threading_MouseLeftButtonDown);
            GridRenderer.GenerateGrid(Band, ColorPickingGrid, (band, grid, count) => GridRenderer.AddSquares(band, grid, count, 0, GridHoleLabeling, Square_MouseLeftButtonDown, Square_MouseRightButtonDown), Band.ThreadCount);
            GridRenderer.GenerateTextColumn(Band, HoleLabelGrid, i => ((char)('A' + i)).ToString(), Band.ThreadCount, false, HoleLabeling);
            GridRenderer.GenerateTextRow(Band, TabletNumberRow, j => (j + 1).ToString(), ColumnLabeling);
            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling,Hexagon_MouseLeftButtonDown), Band.RowCount);
            GridRenderer.GenerateTextColumn(Band, RowNumberGrid, i => (i + 1).ToString(), Band.RowCount, true, RowLabeling);
            GridRenderer.GenerateBandPalette(BandPaletteGrid, ColorManager, FindResource, ColorPaletteSlot_LeftClick, ColorPaletteSlot_RightClick);
        }

        /// <summary>
        /// Replaces the current band with a new one and refreshes UI.
        /// </summary>
        private void ReplaceBand(TabletBand newBand)
        {
            Band = newBand;
            BackSide = false;

            NumberOfHolesTextBox.Text = Band.ThreadCount.ToString();
            NumberOfTabletsTextBox.Text = Band.TabletCount.ToString();
            NumberOfRowsTextBox.Text = Band.RowCount.ToString();

            ColorManager.RebuildColorUsage(Band);
            RedrawVisualization();
        }

        /// <summary>
        /// Displays a temporary animated error message.
        /// </summary>
        private async void ShowErrorMessage(string message)
        {
            Border errorMessage = new Border
            {
                Style = (Style)FindResource("ErrorMessage"),
                Child = new TextBlock{ Text = message }
            };

            ErrorMessage.Children.Add(errorMessage);

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            errorMessage.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(3000);

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, e) => ErrorMessage.Children.Remove(errorMessage);
            errorMessage.BeginAnimation(OpacityProperty, fadeOut);
        }

        /// <summary>
        /// Asks user whether to save progress before closing the window.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var dialog = new SaveProgressDialog
            {
                Owner = this
            };

            dialog.ShowDialog();

            if (dialog.Result == MessageBoxResult.Yes)
            {
                SaveBand_Click();
            }
            else if (dialog.Result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Handles global keyboard shortcuts (Undo, Redo, Save).
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Z)
                {
                    Undo_Click();
                    e.Handled = true;
                }
                else if (e.Key == Key.Y)
                {
                    Redo_Click();
                    e.Handled = true;
                }
                else if (e.Key == Key.S)
                {
                    SaveBand_Click();
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Clears focus when clicking outside of textboxes.
        /// </summary>
        private void EmptySpace_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.Focus(this);
        }

        /// <summary>
        /// Clears focus when pressing ENTER inside a textbox.
        /// </summary>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FocusManager.SetFocusedElement(this, this);
                Keyboard.Focus(this);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
