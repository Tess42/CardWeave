using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CardWeave
{
    /// <summary>
    /// Provides helper methods for rendering tablet-weaving visual elements into WPF grids.
    /// This includes generating grid layouts, text labels, band color palette, square thread cells
    /// and hexagon pattern cells, as well as computing thread positions and element orientations.
    /// </summary>
    public static class GridRenderer
    {
        private const int SquareSize = 20;
        
        /// <summary>
        /// Removes all children and layout definitions from the grid.
        /// </summary>
        public static void EraseGrid(Grid grid)
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();
        }

        /// <summary>
        /// Generates a single horizontal row of text labels, one per tablet.
        /// The text for each column is provided by <paramref name="getText"/>.
        /// Optionally adds a click handler to each label.
        /// </summary>
        public static void GenerateTextRow(TabletBand band, Grid grid, Func<int, string> getText, bool visibility, MouseButtonEventHandler? clickHandler = null)
        {
            EraseGrid(grid);

            if (band.TabletCount == 0 || !visibility) return;

            grid.Width = SquareSize * band.TabletCount;
            grid.Height = SquareSize;
            grid.HorizontalAlignment = HorizontalAlignment.Center;
            grid.VerticalAlignment = VerticalAlignment.Top;

            for (int j = 0; j < band.TabletCount; j++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SquareSize) });

                TextBlock textBlock = new TextBlock
                {
                    Text = getText(j),
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                if (clickHandler != null)
                {
                    textBlock.Cursor = Cursors.Hand;
                    textBlock.MouseLeftButtonDown += clickHandler;
                }

                Grid.SetColumn(textBlock, j);
                grid.Children.Add(textBlock);
            }
        }

        /// <summary>
        /// Generates a vertical column of text labels, one per row.
        /// The text for each row is provided by <paramref name="getText"/>.
        /// Optionally aligns the text to the right.
        /// </summary>
        public static void GenerateTextColumn(TabletBand band, Grid grid, Func<int, string> getText, int numberOfRows, bool rightAlignment, bool visibility)
        {
            EraseGrid(grid);

            if (numberOfRows == 0 || band.TabletCount == 0 || !visibility) return;

            grid.Width = SquareSize;
            grid.Height = SquareSize * numberOfRows;
            grid.HorizontalAlignment = HorizontalAlignment.Left;
            grid.VerticalAlignment = VerticalAlignment.Top;

            for (int i = 0; i < numberOfRows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(SquareSize) });

                TextBlock textBlock = new TextBlock
                {
                    Text = getText(i),
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = rightAlignment ? HorizontalAlignment.Right : HorizontalAlignment.Left
                };

                Grid.SetRow(textBlock, i);
                grid.Children.Add(textBlock);
            }
        }

        /// <summary>
        /// Generates a grid layout for visual elements (such as squares or hexagons),
        /// with a number of rows defined by <paramref name="rows"/> and a number of
        /// columns based on <see cref="TabletBand.TabletCount"/>.
        /// The provided callback <paramref name="addElementsToGrid"/> is responsible
        /// for populating the grid with the actual visual elements.
        /// </summary>
        public static void GenerateGrid(TabletBand band, Grid grid, Action<TabletBand, Grid, int> addElementsToGrid, int rows)
        {
            EraseGrid(grid);

            grid.Width = SquareSize * band.TabletCount;

            if (band.TabletCount == 0)
            {
                grid.Height = 0;
                return;
            }

            grid.Height = SquareSize * rows;
            grid.HorizontalAlignment = HorizontalAlignment.Center;
            grid.VerticalAlignment = VerticalAlignment.Top;

            for (int i = 0; i < rows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(SquareSize) });
            }

            for (int j = 0; j < band.TabletCount; j++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(SquareSize) });
            }

            addElementsToGrid(band, grid, rows);
        }

        /// <summary>
        /// Calculates the thread index for a tablet at the specified starting row.
        /// The index is determined by applying all rotations from row 0 up to <paramref name="from"/>.
        /// </summary>
        private static int GetThreadIndex(TabletBand band, int from, Tablet tablet)
        {
            int threadIndex = 0;

            for (int i = 0; i < from; i++)
            {
                if (tablet.Rotations[i] == RotationDirection.Backward)
                {
                    threadIndex = (band.ThreadCount + threadIndex - 1) % band.ThreadCount;
                }

                if (tablet.Rotations[i] == RotationDirection.Forward)
                {
                    threadIndex = (threadIndex + 1) % band.ThreadCount;
                }
            }

            return threadIndex;
        }

        /// <summary>
        /// Adds square elements to the specified <paramref name="grid"/> based on the threading
        /// configuration of the <paramref name="band"/>. Each square represents a thread color,
        /// starting from the given <paramref name="from"/> index and spanning <paramref name="count"/> rows.
        /// Optional mouse handlers can be attached to each square.
        /// </summary>
        public static void AddSquares(TabletBand band, Grid grid, int count, int from, bool holeLabel, MouseButtonEventHandler? leftClickHandler = null, MouseButtonEventHandler? rightClickHandler = null)
        {
            for (int tabletIndex = 0; tabletIndex < band.TabletCount; tabletIndex++)
            {
                int threadIndex = GetThreadIndex(band, from, band.Tablets[tabletIndex]);
                
                for (int row = 0; row < count; row++)
                {
                    int index = (threadIndex + row) % band.ThreadCount;
                    var color = band.Tablets[tabletIndex].ThreadColors[index];

                    var square = CreateSquare(color, leftClickHandler, rightClickHandler);

                    Grid.SetRow(square, row);
                    Grid.SetColumn(square, tabletIndex);
                    grid.Children.Add(square);

                    if (holeLabel)
                    {
                        var label = CreateHoleLabel(index, color);

                        Grid.SetRow(label, row);
                        Grid.SetColumn(label, tabletIndex);
                        grid.Children.Add(label);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a square UI element of size <see cref="SquareSize"/> filled with the specified color.
        /// The square has a black border and a small margin to visually separate it from other cells.
        /// Optional mouse handlers may be attached if both are provided.
        /// </summary>
        private static Rectangle CreateSquare(Color color, MouseButtonEventHandler? leftClickHandler, MouseButtonEventHandler? rightClickHandler)
        {
            var square = new Rectangle
            {
                Width = SquareSize,
                Height = SquareSize,
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 1,
                Margin = new Thickness(0.3),
            };

            if (leftClickHandler != null && rightClickHandler != null)
            {
                square.MouseLeftButtonDown += leftClickHandler;
                square.MouseRightButtonDown += rightClickHandler;
                square.Cursor = Cursors.Hand;
            }

            return square;
        }

        /// <summary>
        /// Creates a text label representing a thread letter (A, B, C...) based
        /// on the given index and adjusts the text color for optimal contrast
        /// against the provided background color.
        /// </summary>
        private static TextBlock CreateHoleLabel(int index, Color color)
        {
            char letter = (char)('A' + index);

            var label = new TextBlock
            {
                Text = letter.ToString(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Normal,
                Foreground = GetTextColorForBackground(color),
                IsHitTestVisible = false
            };

            return label;
        }

        /// <summary>
        /// Determines whether black or white text provides better contrast
        /// against the specified background color.
        /// </summary>
        private static Brush GetTextColorForBackground(Color bg)
        {
            double luminance = 0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B;
            return luminance < 128 ? Brushes.White : Brushes.Black;
        }

        /// <summary>
        /// Generates a grid of hexagon elements representing the tablet band pattern.
        /// Each hexagon corresponds to one thread segment and is positioned according
        /// to the band structure, rotation rules, and optional backside mirroring.
        /// </summary>
        public static void AddHexagons(TabletBand band, Grid grid, Color bandBackground, bool backSide, bool holeLabel, MouseButtonEventHandler clickHandler)
        {
            grid.Background = new SolidColorBrush(bandBackground);

            for (int tabletIndex = 0; tabletIndex < band.TabletCount; tabletIndex++)
            {
                int threadIndex = 0;
                var tablet = band.Tablets[tabletIndex];
                int columnIndex = backSide ? (band.TabletCount - 1 - tabletIndex) : tabletIndex;

                for (int row = 0; row < band.RowCount; row++)
                {
                    var rotation = tablet.Rotations[row];

                    if (rotation == RotationDirection.Backward)
                    {
                        threadIndex = (band.ThreadCount + threadIndex - 1) % band.ThreadCount;
                    }

                    var fillColor = backSide
                        ? tablet.ThreadColors[(threadIndex + tablet.ThreadCount / 2) % tablet.ThreadCount]
                        : tablet.ThreadColors[threadIndex];

                    var hexagon = CreateHexagon(tablet, tabletIndex, fillColor, rotation, clickHandler);

                    Grid.SetRow(hexagon, row);
                    Grid.SetColumn(hexagon, columnIndex);

                    grid.Children.Add(hexagon);

                    if (holeLabel)
                    {
                        var label = CreateHoleLabel(threadIndex, fillColor);

                        Grid.SetRow(label, row);
                        Grid.SetColumn(label, columnIndex);
                        grid.Children.Add(label);
                    }

                    if (rotation == RotationDirection.Forward)
                    {
                        threadIndex = (threadIndex + 1) % band.ThreadCount;
                    }
                }
            }
        }

        /// <summary>
        /// Generates a grid showing band preview of last 10 completed rows in guide.
        /// </summary>
        public static void AddGuideHexagons(TabletBand band, Grid grid, int count, int from, Color[,] patternColors)
        {
            for (int tabletIndex = 0; tabletIndex < band.TabletCount; tabletIndex++)
            {
                var tablet = band.Tablets[tabletIndex];

                for (int row = 0; row < count; row++)
                {
                    var rotation = tablet.Rotations[(from + row) % band.PatternRepeat];
                    var fillColor = patternColors[(from + row) % band.PatternRepeat, tabletIndex];

                    var hexagon = CreateHexagon(tablet, tabletIndex, fillColor, rotation);

                    Grid.SetRow(hexagon, row);
                    Grid.SetColumn(hexagon, tabletIndex);

                    grid.Children.Add(hexagon);
                }
            }
        }

        /// <summary>
        /// Creates a single hexagon element representing one thread segment of a tablet.
        /// The hexagon's color, orientation and interactivity are determined by the
        /// tablet configuration, thread index, rotation and rendering options.
        /// </summary>
        private static Polygon CreateHexagon(Tablet tablet, int tabletIndex, Color fillColor, RotationDirection rotation, MouseButtonEventHandler? clickHandler = null)
        {
            var hexagon = new Polygon
            {
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 1,
                Fill = new SolidColorBrush(fillColor),
                Tag = tabletIndex
            };

            if (clickHandler != null)
            {
                hexagon.MouseLeftButtonDown += clickHandler;
                hexagon.Cursor = Cursors.Hand;
            }

            bool flipped = IsHexagonFlipped(tablet, rotation);
            hexagon.Points = GetHexagonPoints(flipped);

            return hexagon;
        }

        /// <summary>
        /// Determines whether the hexagon should be rendered in a flipped orientation
        /// based on the tablet's threading direction and the current rotation.
        /// </summary>
        private static bool IsHexagonFlipped(Tablet tablet, RotationDirection rotation)
        {
            return (tablet.Threading == ThreadingDirection.S && rotation == RotationDirection.Forward)
                || (tablet.Threading == ThreadingDirection.Z && rotation == RotationDirection.Backward);
        }

        /// <summary>
        /// Returns the point collection defining a hexagon shape of the given size.
        /// The orientation of the hexagon depends on the <paramref name="flipped"/> flag.
        /// </summary>
        private static PointCollection GetHexagonPoints(bool flipped)
        {
            if (flipped)
            {
                return new PointCollection
                {
                    new Point(0, 0),
                    new Point(SquareSize / 2, 0),
                    new Point(SquareSize, SquareSize / 2),
                    new Point(SquareSize, SquareSize),
                    new Point(SquareSize / 2, SquareSize),
                    new Point(0, SquareSize / 2)
                };
            }

            return new PointCollection
            {
                new Point(SquareSize / 2, 0),
                new Point(SquareSize, 0),
                new Point(SquareSize, SquareSize / 2),
                new Point(SquareSize / 2, SquareSize),
                new Point(0, SquareSize),
                new Point(0, SquareSize / 2)
            };
        }

        /// <summary>
        /// Generates the band color palette inside a UniformGrid.
        /// Creates one slot per used color, applies styles and attaches click handlers.
        /// </summary>
        public static void GenerateBandPalette(UniformGrid grid, BandColorManager colorManager, Func<string, object> findResource, MouseButtonEventHandler leftClick, MouseButtonEventHandler rightClick)
        {
            grid.Children.Clear();

            var colors = colorManager.ColorUsage.Keys;
            if (colors.Count == 0) return;

            grid.Rows = 1;
            grid.Columns = colors.Count;

            double slotWidth = grid.ActualWidth / colors.Count;

            foreach (var color in colors)
            {
                var border = new Border
                {
                    Style = (Style)findResource("ColorSlot"),
                    Background = new SolidColorBrush(color),
                    Width = slotWidth - 4,
                    ToolTip = new ToolTip
                    {
                        Content = colorManager.ColorUsage[color].ToString(),
                        Style = (Style)findResource("ColorSlotTooltip")
                    }
                };

                border.MouseLeftButtonDown += leftClick;
                border.MouseRightButtonDown += rightClick;

                grid.Children.Add(border);
            }
        }

    }
}
