using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CardWeave
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

        /// <summary>
        /// Reverts the band to its previous state using the undo stack
        /// and replaces the current band with the restored version.
        /// </summary>
        private void Undo_Click(object? sender = null, RoutedEventArgs? e = null)
        {
            TabletBand band = UndoRedoManager.Undo(Band.Clone());
            ReplaceBand(band);
        }

        /// <summary>
        /// Restores the band to the next state in the redo stack
        /// and updates the current band accordingly.
        /// </summary>
        private void Redo_Click(object? sender = null, RoutedEventArgs? e = null)
        {
            TabletBand band = UndoRedoManager.Redo(Band.Clone());
            ReplaceBand(band);
        }

        // ───────────────────────────────────────────────
        // Toolbar – View
        // ───────────────────────────────────────────────

        /// <summary>
        /// Switches between front and back side view of the band.
        /// </summary>
        private void BandSide_Click(object sender, RoutedEventArgs e)
        {
            BackSide = !BackSide;
            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
        }

        /// <summary>
        /// Shows or hides column (tablet) labeling above the pattern.
        /// </summary>
        private void ColumnLabeling_Click(object sender, RoutedEventArgs e)
        {
            ColumnLabeling = !ColumnLabeling;
            GridRenderer.GenerateTextRow(Band, TabletNumberRow, j => (j + 1).ToString(), ColumnLabeling);
        }

        /// <summary>
        /// Shows or hides row labeling on the left side of the pattern.
        /// </summary>
        private void RowLabeling_Click(object sender, RoutedEventArgs e)
        {
            RowLabeling = !RowLabeling;
            GridRenderer.GenerateTextColumn(Band, RowNumberGrid, i => (i + 1).ToString(), Band.RowCount, true, RowLabeling);
        }

        /// <summary>
        /// Shows or hides the hole labeling (A, B, C, D) on the left.
        /// </summary>
        private void HoleLabeling_Click(object sender, RoutedEventArgs e)
        {
            HoleLabeling = !HoleLabeling;
            GridRenderer.GenerateTextColumn(Band, HoleLabelGrid, i => ((char)('A' + i)).ToString(), Band.ThreadCount, false, HoleLabeling);
        }

        /// <summary>
        /// Shows or hides hole labels inside each grid cell.
        /// </summary>
        private void GridHoleLabeling_Click(object sender, RoutedEventArgs e)
        {
            GridHoleLabeling = !GridHoleLabeling;
            GridRenderer.GenerateGrid(Band, ColorPickingGrid, (band, grid, count) => GridRenderer.AddSquares(band, grid, count, 0, GridHoleLabeling, Square_MouseLeftButtonDown, Square_MouseRightButtonDown), Band.ThreadCount);
            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
        }

        // ───────────────────────────────────────────────
        // Toolbar – Turning
        // ───────────────────────────────────────────────

        /// <summary>
        /// Reverses turning direction (F ↔ B) for all tablets in the band.
        /// </summary>
        private void ReverseAllTurning_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone());
            Band.ReverseAllTurning();

            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
        }

        /// <summary>
        /// Sets turning direction of all tablets to Forward.
        /// </summary>
        private void SetAllToForward_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone());
            Band.SetAllRotationTo(RotationDirection.Forward);

            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
        }

        /// <summary>
        /// Sets turning direction of all tablets to Backward.
        /// </summary>
        private void SetAllToBackward_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone());
            Band.SetAllRotationTo(RotationDirection.Backward);

            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
        }

        /// <summary>
        /// Applies alternating turning pattern: 1 row forward, 1 row backward.
        /// </summary>
        private void AlternateForwardBackward_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone());
            Band.AlternateForwardBackward(1);

            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
        }

        /// <summary>
        /// Applies alternating turning pattern: 2 rows forward, 2 rows backward.
        /// </summary>
        private void Alternate2Forward2Backward_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone());
            Band.AlternateForwardBackward(2);

            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
        }

        /// <summary>
        /// Applies alternating turning pattern: 4 rows forward, 4 rows backward.
        /// </summary>
        private void Alternate4Forward4Backward_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone());
            Band.AlternateForwardBackward(4);

            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
        }

        // ───────────────────────────────────────────────
        // Toolbar – Threading
        // ───────────────────────────────────────────────

        /// <summary>
        /// Reverses threading direction (S ↔ Z) for all tablets in the band.
        /// </summary>
        private void ReverseAllThreading_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone());
            Band.ReverseAllThreading();

            GridRenderer.GenerateTextRow(Band, TabletDirectionRow, j => Band.Tablets[j].Threading == ThreadingDirection.S ? "S" : "Z", true, Threading_MouseLeftButtonDown);
            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
        }

        /// <summary>
        /// Sets threading direction of all tablets to S-threading.
        /// </summary>
        private void SetAllThreadingToS_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone());
            Band.SetAllThreadingTo(ThreadingDirection.S);

            GridRenderer.GenerateTextRow(Band, TabletDirectionRow, j => Band.Tablets[j].Threading == ThreadingDirection.S ? "S" : "Z", true, Threading_MouseLeftButtonDown);
            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
        }

        /// <summary>
        /// Sets threading direction of all tablets to Z-threading.
        /// </summary>
        private void SetAllThreadingToZ_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone());
            Band.SetAllThreadingTo(ThreadingDirection.Z);

            GridRenderer.GenerateTextRow(Band, TabletDirectionRow, j => Band.Tablets[j].Threading == ThreadingDirection.S ? "S" : "Z", true, Threading_MouseLeftButtonDown);
            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
        }

        /// <summary>
        /// Applies halved threading: first half S-threading, second half Z-threading.
        /// </summary>
        private void SetHalvedThreading_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone());
            Band.SetHalvedThreading();

            GridRenderer.GenerateTextRow(Band, TabletDirectionRow, j => Band.Tablets[j].Threading == ThreadingDirection.S ? "S" : "Z", true, Threading_MouseLeftButtonDown);
            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
        }

        /// <summary>
        /// Applies alternating threading pattern: S, Z, S, Z, ...
        /// </summary>
        private void AlternateSZThreading_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone());
            Band.AlternateSZThreading();

            GridRenderer.GenerateTextRow(Band, TabletDirectionRow, j => Band.Tablets[j].Threading == ThreadingDirection.S ? "S" : "Z", true, Threading_MouseLeftButtonDown);
            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
        }

        // ───────────────────────────────────────────────
        // Toolbar – Color
        // ───────────────────────────────────────────────

        /// <summary>
        /// Sets the band background color to the currently picked color.
        /// </summary>
        private void BackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            BandBackground = CurrentColor;
            BandVisualizationGrid.Background = new SolidColorBrush(CurrentColor);
        }

        /// <summary>
        /// Sets all thread colors in the band to the currently picked color.
        /// </summary>
        private void AllToPickedColor_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone());
            ColorManager.SetAllThreadsToColor(Band, CurrentColor);

            GridRenderer.GenerateGrid(Band, ColorPickingGrid, (band, grid, count) => GridRenderer.AddSquares(band, grid, count, 0, GridHoleLabeling, Square_MouseLeftButtonDown, Square_MouseRightButtonDown), Band.ThreadCount);
            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
            GridRenderer.GenerateBandPalette(BandPaletteGrid, ColorManager, FindResource, ColorPaletteSlot_LeftClick, ColorPaletteSlot_RightClick);
        }

        /// <summary>
        /// Randomly shuffles all colors used in the band.
        /// </summary>
        public void ShuffleBandColors_Click(object sender, RoutedEventArgs e)
        {
            UndoRedoManager.SaveState(Band.Clone()); 
            ColorManager.ShuffleBandColors(Band);

            GridRenderer.GenerateGrid(Band, ColorPickingGrid, (band, grid, count) => GridRenderer.AddSquares(band, grid, count, 0, GridHoleLabeling, Square_MouseLeftButtonDown, Square_MouseRightButtonDown), Band.ThreadCount);
            GridRenderer.GenerateGrid(Band, BandVisualizationGrid, (band, grid, _) => GridRenderer.AddHexagons(band, grid, BandBackground, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown), Band.RowCount);
            GridRenderer.GenerateBandPalette(BandPaletteGrid, ColorManager, FindResource, ColorPaletteSlot_LeftClick, ColorPaletteSlot_RightClick);
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

        // ───────────────────────────────────────────────
        // Toolbar – Help
        // ───────────────────────────────────────────────

        /// <summary>
        /// Opens the Help window with user guide and documentation.
        /// </summary>
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow
            {
                Owner = this
            };

            helpWindow.ShowDialog();
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
