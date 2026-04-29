using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Tablet_weaving_pattern_designer
{
    public partial class MainWindow
    {
        /// <summary>
        /// Opens a context menu for a toolbar button.
        /// Loads preset menu items dynamically when needed.
        /// </summary>
        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.ContextMenu == PresetsMenu)
                {
                    LoadPresetsMenu();
                }

                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // ───────────────────────────────────────────────
        // Toolbar – File
        // ───────────────────────────────────────────────

        /// <summary>
        /// Creates a new empty band and stores the previous state for undo.
        /// </summary>
        private void NewBand_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone());

            ReplaceBand(new TabletBand());
        }

        /// <summary>
        /// Saves the band to a new file selected by the user.
        /// </summary>
        private async void SaveBandAs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? path = await BandFileService.SaveBandAsAsync(Band);

                if (path != null)
                {
                    Band.CurrentFilePath = path;
                }
            }
            catch
            {
                ShowErrorMessage("Saving failed.");
            }
        }

        /// <summary>
        /// Saves the band to its current file or to a new file selected by the user if none exists.
        /// </summary>
        private async void SaveBand_Click(object? sender = null, RoutedEventArgs? e = null)
        {
            try
            {
                Band.CurrentFilePath = await BandFileService.SaveBandAsync(Band, Band.CurrentFilePath);
            }
            catch
            {
                ShowErrorMessage("Saving failed.");
            }
        }

        /// <summary>
        /// Loads a band from file and stores the previous state for undo.
        /// </summary>
        private async void OpenBand_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UndoRedoManager.SaveState(Band.Clone());

                var result = await BandFileService.LoadBandAsync();

                if (result != null)
                {
                    ReplaceBand(result);
                }
            }
            catch
            {
                ShowErrorMessage("Loading failed.");
            }
        }

        /// <summary>
        /// Exports the pattern visualization as an image file.
        /// </summary>
        private async void ExportPatternAsImage_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "PNG Image (*.png)|*.png|JPEG Image (*.jpg)|*.jpg",
                Title = "Export as image"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await BandFileService.ExportPatternAsImageAsync(PatternVisualization, dialog.FileName);
                }
                catch
                {
                    ShowErrorMessage("Export failed.");
                }
            }
        }

        // ───────────────────────────────────────────────
        // Toolbar – Edit (Undo/Redo)
        // ───────────────────────────────────────────────

        private void Undo_Click(object? sender = null, RoutedEventArgs? e = null)
        {
            TabletBand band = UndoRedoManager.Undo(Band.Clone());
            ReplaceBand(band);
        }

        private void Redo_Click(object? sender = null, RoutedEventArgs? e = null)
        {
            TabletBand band = UndoRedoManager.Redo(Band.Clone());
            ReplaceBand(band);
        }

        // ───────────────────────────────────────────────
        // Toolbar – View
        // ───────────────────────────────────────────────

        /// <summary>
        /// Changes a boolean visualization setting to opposite and redraws the pattern.
        /// </summary>
        private void ChancheVisibility(bool currentValue, Action<bool> setter)
        {
            setter(!currentValue);
            RedrawVisualization();
        }

        private void BandSide_Click(object sender, RoutedEventArgs e)
        {
            ChancheVisibility(BackSide, v => BackSide = v);
        }

        private void ColumnLabeling_Click(object sender, RoutedEventArgs e)
        {
            ChancheVisibility(ColumnLabeling, v => ColumnLabeling = v);
        }

        private void RowLabeling_Click(object sender, RoutedEventArgs e)
        {
            ChancheVisibility(RowLabeling, v => RowLabeling = v);
        }

        private void HoleLabeling_Click(object sender, RoutedEventArgs e)
        {
            ChancheVisibility(HoleLabeling, v => HoleLabeling = v);
        }

        private void GridHoleLabeling_Click(object sender, RoutedEventArgs e)
        {
            ChancheVisibility(GridHoleLabeling, v => GridHoleLabeling = v);
        }

        /// <summary>
        /// Applies a modification to the band, stores undo state, and redraws the visualization.
        /// </summary>
        private void ApplyBandChange(Action<TabletBand> action)
        {
            UndoRedoManager.SaveState(Band.Clone());
            action(Band);
            RedrawVisualization();
        }

        // ───────────────────────────────────────────────
        // Toolbar – Turning
        // ───────────────────────────────────────────────

        private void ReverseAllTurning_Click(object sender, RoutedEventArgs e)
        {
            ApplyBandChange(b => b.ReverseAllTurning());
        }

        private void SetAllToForward_Click(object sender, RoutedEventArgs e)
        {
            ApplyBandChange(b => b.SetAllRotationTo(RotationDirection.Forward));
        }

        private void SetAllToBackward_Click(object sender, RoutedEventArgs e)
        {
            ApplyBandChange(b => b.SetAllRotationTo(RotationDirection.Backward));
        }

        private void AlternateForwardBackward_Click(object sender, RoutedEventArgs e)
        {
            ApplyBandChange(b => b.AlternateForwardBackward(1));
        }

        private void Alternate2Forward2Backward_Click(object sender, RoutedEventArgs e)
        {
            ApplyBandChange(b => b.AlternateForwardBackward(2));
        }

        private void Alternate4Forward4Backward_Click(object sender, RoutedEventArgs e)
        {
            ApplyBandChange(b => b.AlternateForwardBackward(4));
        }

        // ───────────────────────────────────────────────
        // Toolbar – Threading
        // ───────────────────────────────────────────────

        private void ReverseAllThreading_Click(object sender, RoutedEventArgs e)
        {
            ApplyBandChange(b => b.ReverseAllThreading());
        }

        private void SetAllThreadingToS_Click(object sender, RoutedEventArgs e)
        {
            ApplyBandChange(b => b.SetAllThreadingTo(ThreadingDirection.S));
        }

        private void SetAllThreadingToZ_Click(object sender, RoutedEventArgs e)
        {
            ApplyBandChange(b => b.SetAllThreadingTo(ThreadingDirection.Z));
        }

        private void SetHalvedThreading_Click(object sender, RoutedEventArgs e)
        {
            ApplyBandChange(b => b.SetHalvedThreading());
        }

        private void AlternateSZThreading_Click(object sender, RoutedEventArgs e)
        {
            ApplyBandChange(b => b.AlternateSZThreading());
        }

        // ───────────────────────────────────────────────
        // Toolbar – Color
        // ───────────────────────────────────────────────

        private void BackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            BandBackground = CurrentColor;
            RedrawVisualization();
        }

        private void AllToPickedColor_Click(object sender, RoutedEventArgs e)
        {
            ApplyBandChange(b => ColorManager.SetAllThreadsToColor(b, CurrentColor));
        }

        public void ShuffleBandColors_Click(object sender, RoutedEventArgs e)
        {
            ApplyBandChange(b => ColorManager.ShuffleBandColors(b));
        }

        // ───────────────────────────────────────────────
        // Toolbar – Presets
        // ───────────────────────────────────────────────

        /// <summary>
        /// Loads preset files and populates the presets context menu.
        /// </summary>
        private void LoadPresetsMenu()
        {
            if (PresetsMenu == null) return;

            PresetsMenu.Items.Clear();

            var files = BandFileService.GetPresetFiles();

            foreach (var file in files)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(file);

                var item = new MenuItem
                {
                    Header = name,
                    Style = (Style)FindResource("MenuItem")
                };

                item.Click += async (s, e) => await LoadPresetFromFile(file);

                PresetsMenu.Items.Add(item);
            }
        }

        /// <summary>
        /// Loads a preset band from JSON file and replaces the current band.
        /// </summary>
        private async Task LoadPresetFromFile(string filePath)
        {
            try
            {
                UndoRedoManager.SaveState(Band.Clone());

                string json = await File.ReadAllTextAsync(filePath);
                var loaded = JsonSerializer.Deserialize<TabletBand>(json);

                if (loaded != null)
                {
                    ReplaceBand(loaded);
                    Band.CurrentFilePath = null;
                }
            }
            catch
            {
                ShowErrorMessage("Preset loading failed.");
            }
        }

        // ───────────────────────────────────────────────
        // Toolbar – Weaving guide
        // ───────────────────────────────────────────────

        /// <summary>
        /// Creates a color matrix representing the current band visualization.
        /// </summary>
        public static Color[,] GetColorMatrix(Grid grid)
        {
            int rows = grid.RowDefinitions.Count;
            int cols = grid.ColumnDefinitions.Count;

            var matrix = new Color[rows, cols];

            foreach (var child in grid.Children)
            {
                if (child is Polygon hex)
                {
                    int row = Grid.GetRow(hex);
                    int col = Grid.GetColumn(hex);

                    var brush = hex.Fill as SolidColorBrush;
                    matrix[row, col] = brush?.Color ?? Colors.Transparent;
                }
            }

            return matrix;
        }

        /// <summary>
        /// Opens the weaving guide window if the band contains at least one tablet and one row.
        /// </summary>
        private void StartGuide_Click(object sender, RoutedEventArgs e)
        {
            if (Band.TabletCount == 0 || Band.RowCount == 0)
            {
                ShowErrorMessage("The band must contain at least one tablet and one row to open the guide.");
                return;
            }

            Color[,] colors = GetColorMatrix(BandVisualizationGrid);
            
            var guide = new WeavingGuide(Band, colors)
            {
                Owner = this
            };

            guide.ShowDialog();
        }
    }
}
