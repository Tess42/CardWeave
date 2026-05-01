using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CardWeave
{
    public partial class MainWindow
    {
        // ───────────────────────────────────────────────
        // Band settings
        // ───────────────────────────────────────────────

        /// <summary>
        /// Sets new number of holes per tablet when the textbox loses focus.
        /// Restores previous value if input is invalid.
        /// </summary>
        private void NumberOfHolesTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (int.TryParse(NumberOfHolesTextBox.Text, out int value) && value != Band.ThreadCount && value >= WeavingConstants.MinThreadCount && value <= WeavingConstants.MaxThreadCount)
            {
                UndoRedoManager.SaveState(Band.Clone());

                Band.ChangeThreadCount(value);
                ColorManager.RebuildColorUsage(Band);
                RedrawVisualization();
            }
            else
            {
                NumberOfHolesTextBox.Text = Band.ThreadCount.ToString();
            }
        }

        /// <summary>
        /// Sets new number of tablets when the textbox loses focus.
        /// Restores previous value if input is invalid.
        /// </summary>
        private void NumberOfTabletsTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (int.TryParse(NumberOfTabletsTextBox.Text, out int value) && value != Band.TabletCount && value >= 0 && value <= WeavingConstants.MaxTablets)
            {
                UndoRedoManager.SaveState(Band.Clone());

                Band.ChangeTabletCount(value);
                ColorManager.RebuildColorUsage(Band);
                RedrawVisualization();
            }
            else
            {
                NumberOfTabletsTextBox.Text = Band.TabletCount.ToString();
            }
        }

        /// <summary>
        /// Sets new number of rows when the textbox loses focus.
        /// Restores previous value if input is invalid.
        /// </summary>
        private void NumberOfRowsTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (int.TryParse(NumberOfRowsTextBox.Text, out int value) && value != Band.RowCount && value >= 0 && value <= WeavingConstants.MaxRows)
            {
                UndoRedoManager.SaveState(Band.Clone());

                Band.ChangeRowCount(value);
                RedrawVisualization();
            }
            else
            {
                NumberOfRowsTextBox.Text = Band.RowCount.ToString();
            }
        }

        // ───────────────────────────────────────────────
        // Modify tablets
        // ───────────────────────────────────────────────

        /// <summary>
        /// Adds a new tablet at the specified index when the textbox loses focus.
        /// Clears the textbox after processing.
        /// </summary>
        private void AddTabletTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (int.TryParse(AddTabletTextBox.Text, out int value))
            {
                UndoRedoManager.SaveState(Band.Clone());

                Band.AddTablet(value);
                ColorManager.RebuildColorUsage(Band);
                RedrawVisualization();
                NumberOfTabletsTextBox.Text = Band.TabletCount.ToString();
            }

            AddTabletTextBox.Text = "";
        }

        /// <summary>
        /// Removes a tablet at the specified index when the textbox loses focus.
        /// Clears the textbox after processing.
        /// </summary>
        private void RemoveTabletTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (int.TryParse(RemoveTabletTextBox.Text, out int value))
            {
                UndoRedoManager.SaveState(Band.Clone());

                Band.RemoveTablet(value);
                ColorManager.RebuildColorUsage(Band);
                RedrawVisualization();
                NumberOfTabletsTextBox.Text = Band.TabletCount.ToString();
            }

            RemoveTabletTextBox.Text = "";
        }

        // ───────────────────────────────────────────────
        // Color selection and palette
        // ───────────────────────────────────────────────

        /// <summary>
        /// Selects a color from a color slot and sets it as the current color.
        /// </summary>
        private void ColorSlot_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border b && b.Background is SolidColorBrush brush)
            {
                CurrentColor = brush.Color;
            }
        }

        /// <summary>
        /// Opens a color picker dialog and adds the selected color to the palette.
        /// Updates the current color and refreshes palette UI.
        /// </summary>
        private void AddColor_Click(object sender, MouseButtonEventArgs e)
        {
            var dialog = new ColorPickerWindow()
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                CurrentColor = dialog.SelectedColor;
                ColorManager.AddColorToSlots(dialog.SelectedColor);
                ColorManager.ApplyColorsToSlots(ColorSlotsPanel);
            }
        }

        /// <summary>
        /// Replaces all occurrences of the clicked palette color in the band with the current color.
        /// Updates the palette slot and redraws the visualization.
        /// </summary>
        private void ColorPaletteSlot_RightClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right && sender is Border b && b.Background is SolidColorBrush brush)
            {
                UndoRedoManager.SaveState(Band.Clone());

                ColorManager.ReplaceColorInBand(Band, brush.Color, CurrentColor);

                b.Background = new SolidColorBrush(CurrentColor);
                RedrawVisualization();
            }
        }

        /// <summary>
        /// Selects a palette color as the current color when left-clicked.
        /// </summary>
        private void ColorPaletteSlot_LeftClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && sender is Border b && b.Background is SolidColorBrush brush)
            {
                CurrentColor = brush.Color;
            }
        }
    }
}
