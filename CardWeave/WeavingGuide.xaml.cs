using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CardWeave
{
    /// <summary>
    /// Window providing a step‑by‑step weaving guide.
    /// Displays tablet rotations for current row,
    /// thread colors on top, and recent rows to help with physical weaving.
    /// </summary>
    public partial class WeavingGuide : Window
    {
        /// <summary> The band being woven. </summary>
        private TabletBand Band { get; set; }

        /// <summary> Unsaved progress in number of rows already woven. </summary>
        private int RowsCompleted { get; set; }

        /// <summary> Unsaved changes in number of rows after which the pattern repeats. </summary>
        private int PatternRepeat { get; set; }

        /// <summary> Precomputed pattern colors for each row and tablet. </summary>
        private Color[,] PatternColors { get; set; }

        /// <summary> List of possible pattern repeat lengths. </summary>
        private List<int> PossibleRepeats { get; set; } = new List<int>();

        /// <summary>
        /// Initializes the weaving guide with the given band and pattern colors.
        /// Sets initial repeat and completed rows.
        /// </summary> 
        public WeavingGuide(TabletBand band, Color[,] patternColors)
        {
            InitializeComponent();
            Band = band;
            PatternColors = patternColors;

            if (Band.PatternRepeat == 0 || Band.PatternRepeat > Band.RowCount)
            {
                Band.PatternRepeat = Band.RowCount;
            }

            Loaded += WeavingGuide_Loaded;
        }

        /// <summary>
        /// Loads possible repeat values and draws the initial visualization.
        /// </summary>
        private void WeavingGuide_Loaded(object sender, RoutedEventArgs e)
        {
            FillPossibleRepeats();

            RepeatComboBox.ItemsSource = PossibleRepeats;
            RepeatComboBox.SelectedItem = Band.PatternRepeat;
            RowsCompletedTextBox.Text = Band.RowsCompleted.ToString();

            RowsCompleted = Band.RowsCompleted;
            PatternRepeat = Band.PatternRepeat;

            RedrawVisualization();
        }

        /// <summary>
        /// Redraws all parts of the weaving guide visualization.
        /// </summary>
        private void RedrawVisualization()
        {
            GenerateTabletNumberRow();
            GenerateRotationRow();
            GenerateColorsOnTop();
            GenerateBandPreview();
        }

        /// <summary>
        /// Generates the row showing tablet numbers.
        /// </summary>
        private void GenerateTabletNumberRow()
        {
            GridRenderer.GenerateTextRow(Band, TabletNumberRow, j => (j + 1).ToString(), true);
        }

        /// <summary>
        /// Generates the row showing how to turn each tablet for the current row.
        /// </summary>
        private void GenerateRotationRow()
        {
            GridRenderer.GenerateTextRow(
                Band,
                TurningRow,
                j => Band.Tablets[j].Rotations[RowsCompleted % PatternRepeat] == RotationDirection.Forward ? "F" : "B",
                true
            );
        }

        /// <summary>
        /// Generates a preview of the last few woven rows (up to 10).
        /// </summary>
        private void GenerateBandPreview()
        {
            int from = 0;
            if (RowsCompleted > 10)
            {
                from = (RowsCompleted + PatternRepeat - 10) % PatternRepeat;
            }

            int rows = Math.Min(RowsCompleted, 10);

            GridRenderer.GenerateGrid(Band, BandPreview, (band, grid, count) => GridRenderer.AddGuideHexagons(band, grid, count, from, PatternColors), rows);
        }

        /// <summary>
        /// Generates the thread colors on top for the current row.
        /// </summary>
        private void GenerateColorsOnTop()
        {
            int from = RowsCompleted % PatternRepeat;

            GridRenderer.GenerateGrid(Band, ColorsOnTop, (band, grid, count) => GridRenderer.AddSquares(band, grid, count, from, true), 2);
        }

        /// <summary>
        /// Computes all possible pattern repeat lengths based on the first row repetition.
        /// </summary>
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

        /// <summary>
        /// Updates pattern repeat when the user selects a new value.
        /// </summary>
        private void RepeatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RepeatComboBox.SelectedItem is int value)
            {
                if (value != PatternRepeat)
                {
                    PatternRepeat = value;
                    RedrawVisualization();
                }
            }
        }

        /// <summary>
        /// Validates and updates the number of completed rows when the textbox loses focus.
        /// </summary>
        private void RowsCompletedTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (int.TryParse(RowsCompletedTextBox.Text, out int value) && value != RowsCompleted && value >= 0 && value < 100000)
            {
                RowsCompleted = value;
                RedrawVisualization();
            }
            else
            {
                RowsCompletedTextBox.Text = RowsCompleted.ToString();
            }
        }

        /// <summary>
        /// Advances to the next row in the weaving guide.
        /// </summary>
        private void Next_Click(object? sender = null, RoutedEventArgs? e = null)
        {
            if (PatternRepeat > 0)
            {
                RowsCompleted++;
                RedrawVisualization();
                RowsCompletedTextBox.Text = RowsCompleted.ToString();
            }
        }

        /// <summary>
        /// Moves back to the previous row in the weaving guide.
        /// </summary>
        private void Previous_Click(object? sender = null, RoutedEventArgs? e = null)
        {
            if (PatternRepeat > 0 && RowsCompleted > 0)
            {
                RowsCompleted--;
                RedrawVisualization();
                RowsCompletedTextBox.Text = RowsCompleted.ToString();
            }
        }

        /// <summary>
        /// Clears focus when clicking on empty space.
        /// </summary>
        private void EmptySpace_MouseDown(object sender, MouseButtonEventArgs e)
        {
            FocusManager.SetFocusedElement(this, this);
            Keyboard.Focus(this);
        }

        /// <summary>
        /// Removes focus from a textbox when Enter is pressed.
        /// </summary>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FocusManager.SetFocusedElement(this, this);
                Keyboard.Focus(this);
            }
        }

        /// <summary>
        /// Handles keyboard shortcuts: Space/Right for next row, Left for previous row.
        /// </summary>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
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

        /// <summary>
        /// Asks the user whether to save progress when closing the window.
        /// Resets or keeps progress based on the dialog result.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Band.RowsCompleted == RowsCompleted && Band.PatternRepeat == PatternRepeat)
            {
                return;
            }

            var dialog = new SaveProgressDialog
            {
                Owner = this
            };

            dialog.ShowDialog();

            if (dialog.Result == MessageBoxResult.Yes)
            {
                Band.ChangeRowsCompleted(RowsCompleted);
                Band.PatternRepeat = PatternRepeat;
            }
            else if (dialog.Result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }
}
