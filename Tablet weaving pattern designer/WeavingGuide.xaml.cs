using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Tablet_weaving_pattern_designer
{
    public partial class WeavingGuide : Window
    {
        private TabletBand Band { get; set; }
        private Color[,] PatternColors { get; set; }
        private List<int> PossibleRepeats { get; set; } = new List<int>();

        public WeavingGuide(TabletBand band, Color[,] patternColors)
        {
            InitializeComponent();
            Band = band;
            PatternColors = patternColors;

            if (Band.PatternRepeat == 0)
            {
                Band.PatternRepeat = Band.RowCount;
            }
            RepeatComboBox.SelectedItem = Band.PatternRepeat;
            RowsCompletedTextBox.Text = Band.RowsCompleted.ToString();

            Loaded += WeavingGuide_Loaded;
        }

        private void WeavingGuide_Loaded(object sender, RoutedEventArgs e)
        {
            FillPossibleRepeats();
            RepeatComboBox.ItemsSource = PossibleRepeats; 
            RedrawVisualization();
        }

        private void RedrawVisualization()
        {
            GenerateTabletNumberRow();
            GenerateRotationRow();
            GenerateColorsOnTop();
            GenerateBandPreview();
        }

        private void GenerateTabletNumberRow()
        {
            GridRenderer.GenerateTextRow(Band, TabletNumberRow, j => (j + 1).ToString(), true);
        }

        private void GenerateRotationRow()
        {
            GridRenderer.GenerateTextRow(
                Band,
                TurningRow,
                j => Band.Tablets[j].Rotations[Band.RowsCompleted % Band.PatternRepeat] == RotationDirection.Forward ? "F" : "B",
                true
            );
        }

        private void GenerateBandPreview()
        {
            int from = 0;
            if (Band.RowsCompleted > 10)
            {
                from = (Band.RowsCompleted + Band.PatternRepeat - 10) % Band.PatternRepeat;
            }

            int rows = Math.Min(Band.RowsCompleted, 10);

            GridRenderer.GenerateGrid(Band, BandPreview, (band, grid, count) => GridRenderer.AddGuideHexagons(band, grid, count, from, PatternColors), rows);
        }

        private void GenerateColorsOnTop()
        {
            int from = Band.RowsCompleted % Band.PatternRepeat;

            GridRenderer.GenerateGrid(Band, ColorsOnTop, (band, grid, count) => GridRenderer.AddSquares(band, grid, count, from, true), 2);
        }

        // Repeats
        private void FillPossibleRepeats()
        {
            for (int row = 1; row < Band.RowCount; row++)
            {
                bool equal = true;

                for (int tabletIndex  = 0; tabletIndex < Band.TabletCount; tabletIndex++)
                {
                    if (PatternColors[row, tabletIndex] != PatternColors[0, tabletIndex] || Band.Tablets[tabletIndex].Rotations[row] != Band.Tablets[tabletIndex].Rotations[0])
                    {
                        equal = false; 
                        break;
                    }
                }

                if (equal)
                {
                    PossibleRepeats.Add(row);
                }
            }

            if (PossibleRepeats.Count == 0 || PossibleRepeats[^1] != Band.RowCount)
            {
                PossibleRepeats.Add(Band.RowCount);
            }
        }

        private void RepeatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RepeatComboBox.SelectedItem is int value)
            {
                if (value != Band.PatternRepeat)
                {
                    Band.ChangePatternRepeat(value);
                    RedrawVisualization();
                }
            }
        }

        // Rows completed
        private void RowsCompletedTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (int.TryParse(RowsCompletedTextBox.Text, out int value) && value != Band.RowsCompleted && value >= 0 && value < 100000)
            {
                Band.ChangeRowsCompleted(value);
                RedrawVisualization();
            }
            else
            {
                RowsCompletedTextBox.Text = Band.RowsCompleted.ToString();
            }
        }

        private void Next_Click(object? sender = null, RoutedEventArgs? e = null)
        {
            if (Band.PatternRepeat > 0)
            {
                Band.ChangeRowsCompleted(Band.RowsCompleted + 1);
                RedrawVisualization();
                RowsCompletedTextBox.Text = Band.RowsCompleted.ToString();
            }
        }

        private void Previous_Click(object? sender = null, RoutedEventArgs? e = null)
        {
            if (Band.RowsCompleted > 0)
            {
                Band.ChangeRowsCompleted(Band.RowsCompleted - 1);
            }
            RedrawVisualization();
            RowsCompletedTextBox.Text = Band.RowsCompleted.ToString();
        }

        // Clear focus after clicking outside of textbox
        private void EmptySpace_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.Focus(this);
        }

        // Clear focus after pressing ENTER
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FocusManager.SetFocusedElement(this, this);
                Keyboard.Focus(this);
            }
        }

        // Keyboard shortcuts
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space || e.Key == Key.Right)
            {
                Next_Click();
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                Previous_Click();
                e.Handled = true;
            }
        }

        // Save progress
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var dialog = new SaveProgressDialog
            {
                Owner = this
            };

            dialog.ShowDialog();

            if (dialog.Result == MessageBoxResult.No)
            {
                Band.RowsCompleted = 0;
                Band.PatternRepeat = Band.RowCount;
            }
            else if (dialog.Result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }
}
