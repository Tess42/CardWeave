using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CardWeave
{
    public partial class MainWindow
    {
        /// <summary>
        /// Changes threading direction to opposite for the clicked tablet column.
        /// Updates the threading row and the full band visualization.
        /// </summary>
        private void Threading_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                UndoRedoManager.SaveState(Band.Clone());

                int column = Grid.GetColumn(textBlock);
                Band.Tablets[column].ChangeThreading();

                textBlock.Text = Band.Tablets[column].Threading == ThreadingDirection.S ? "S" : "Z";
                GridRenderer.UpdateHexagonColumn(Band, BandVisualizationGrid, column, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown);
            }
        }

        /// <summary>
        /// Changes the color of a thread to the currently selected color.
        /// Updates the band palette and the full band visualization.
        /// </summary>
        private void Square_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Rectangle square)
            {
                UndoRedoManager.SaveState(Band.Clone());

                ColorManager.RemoveUsedColor(((SolidColorBrush)square.Fill).Color);
                ColorManager.AddUsedColor(CurrentColor);

                square.Fill = new SolidColorBrush(CurrentColor);

                int row = Grid.GetRow(square);
                int column = Grid.GetColumn(square);

                Band.Tablets[column].ChangeThreadColor(row, CurrentColor);

                GridRenderer.GenerateBandPalette(BandPaletteGrid, ColorManager, FindResource, ColorPaletteSlot_LeftClick, ColorPaletteSlot_RightClick);
                GridRenderer.UpdateHexagonColumn(Band, BandVisualizationGrid, column, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown);
            }
        }

        /// <summary>
        /// Resets the color of a thread to white on right-click.
        /// Updates the band palette and the full band visualization.
        /// </summary>
        private void Square_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Rectangle square)
            {
                UndoRedoManager.SaveState(Band.Clone());

                ColorManager.RemoveUsedColor(((SolidColorBrush)square.Fill).Color);
                ColorManager.AddUsedColor(Colors.White);

                square.Fill = new SolidColorBrush(Colors.White);

                int row = Grid.GetRow(square);
                int column = Grid.GetColumn(square);

                Band.Tablets[column].ChangeThreadColor(row, Colors.White);

                GridRenderer.GenerateBandPalette(BandPaletteGrid, ColorManager, FindResource, ColorPaletteSlot_LeftClick, ColorPaletteSlot_RightClick);
                GridRenderer.UpdateHexagonColumn(Band, BandVisualizationGrid, column, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown);
            }
        }

        /// <summary>
        /// Changes rotation direction to opposite for a clicked hexagon cell.
        /// Updates the full band visualization.
        /// </summary>
        private void Hexagon_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Polygon hexagon)
            {
                UndoRedoManager.SaveState(Band.Clone());

                int column = (int)hexagon.Tag; ;
                int row = Grid.GetRow(hexagon);
                if (Band.Tablets[column].Rotations[row] == RotationDirection.Forward)
                {
                    Band.Tablets[column].Rotations[row] = RotationDirection.Backward;
                }
                else
                {
                    Band.Tablets[column].Rotations[row] = RotationDirection.Forward;
                }

                GridRenderer.UpdateHexagonColumn(Band, BandVisualizationGrid, column, BackSide, GridHoleLabeling, Hexagon_MouseLeftButtonDown);
            }
        }

        // ───────────────────────────────────────────────
        // Scroll synchronization
        // ───────────────────────────────────────────────

        /// <summary>
        /// Handles scroll changes in the band visualization grid and synchronizes
        /// vertical and horizontal offsets of all related grids.
        /// </summary>
        private void BandScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                SyncVerticalScroll(e.VerticalOffset);
            }

            if (e.HorizontalChange != 0)
            {
                SyncHorizontalScroll(e.HorizontalOffset);
            }
        }

        /// <summary>
        /// Synchronizes vertical scrolling of row number grid with given offset.
        /// </summary>
        private void SyncVerticalScroll(double offset)
        {
            RowNumberScroll.ScrollToVerticalOffset(offset);
        }

        /// <summary>
        /// Synchronizes horizontal scrolling of tablet direction grid,
        /// color picking grid and tablet number grid with given offset.
        /// </summary>
        private void SyncHorizontalScroll(double offset)
        {
            TabletDirectionScroll.ScrollToHorizontalOffset(offset);
            ColorPickingGridScroll.ScrollToHorizontalOffset(offset);
            TabletNumberRowScroll.ScrollToHorizontalOffset(offset);
        }
    }
}
