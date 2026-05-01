using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CardWeave
{
    /// <summary>
    /// Manages color usage, color statistics and color operations for band.
    /// </summary>
    public class BandColorManager
    {
        /// <summary> Tracks how many times each color is used in the band. </summary>
        public Dictionary<Color, int> ColorUsage { get; private set; } = new();

        /// <summary> Recently used colors available in color slots. </summary>
        public List<Color> ColorSlots { get; private set; } = new()
        {
            Colors.Gray,
            Colors.MediumOrchid,
            Colors.CornflowerBlue,
            Colors.LightSalmon,
            Colors.ForestGreen,
            Colors.Olive,
            Colors.Orange,
            Colors.YellowGreen,

            Colors.IndianRed,
            Colors.DarkRed,
            Colors.Goldenrod,
            Colors.MidnightBlue,
            Colors.MediumTurquoise,
            Colors.White,
            Colors.Black
        };

        /// <summary>
        /// Rebuilds color usage statistics from all tablets in the band.
        /// </summary>
        public void RebuildColorUsage(TabletBand band)
        {
            ColorUsage.Clear();

            foreach (var tablet in band.Tablets)
            {
                foreach (var color in tablet.ThreadColors)
                {
                    AddUsedColor(color);
                }
            }
        }

        /// <summary>
        /// Increases usage count for the given color.
        /// </summary>
        public void AddUsedColor(Color color)
        {
            if (ColorUsage.ContainsKey(color))
            {
                ColorUsage[color]++;
            }
            else
            {
                ColorUsage[color] = 1;
            }
        }

        /// <summary>
        /// Decreases usage count for the given color and removes it if unused.
        /// </summary>
        public void RemoveUsedColor(Color color)
        {
            if (ColorUsage.ContainsKey(color))
            {
                ColorUsage[color]--;

                if (ColorUsage[color] == 0)
                {
                    ColorUsage.Remove(color);
                }
            }
        }

        /// <summary>
        /// Shifts color slots left and inserts a new color at the end.
        /// </summary>
        public void AddColorToSlots(Color newColor)
        {
            for (int i = 0; i < ColorSlots.Count - 1; i++)
            {
                ColorSlots[i] = ColorSlots[i + 1];
            }

            ColorSlots[^1] = newColor;
        }

        /// <summary>
        /// Applies color to UI elements inside the given panel.
        /// </summary>
        public void ApplyColorsToSlots(Panel panel)
        {
            var borders = panel.Children
                .OfType<Border>()
                .Take(ColorSlots.Count)
                .ToList();

            for (int i = 0; i < borders.Count; i++)
            {
                borders[i].Background = new SolidColorBrush(ColorSlots[i]);
            }
        }

        /// <summary>
        /// Replaces all occurrences of one color with another across the entire band and updates statistics.
        /// </summary>
        public void ReplaceColorInBand(TabletBand band, Color oldColor, Color newColor)
        {
            foreach (var tablet in band.Tablets)
            {
                tablet.ReplaceColor(oldColor, newColor);
            }

            if (ColorUsage.TryGetValue(oldColor, out int count))
            {
                ColorUsage.Remove(oldColor);

                if (ColorUsage.ContainsKey(newColor))
                {
                    ColorUsage[newColor] += count;
                }
                else
                {
                    ColorUsage[newColor] = count;
                }
            }
        }

        /// <summary>
        /// Sets all threads in the band to a single color and rebuilds usage statistics.
        /// </summary>
        public void SetAllThreadsToColor(TabletBand band, Color color)
        {
            if (band.TabletCount == 0 || band.RowCount == 0)
            {
                return;
            }
            
            foreach (var tablet in band.Tablets)
            {
                for (int i = 0; i < tablet.ThreadCount; i++)
                {
                    tablet.ChangeThreadColor(i, color);
                }
            }

            ColorUsage.Clear();
            ColorUsage[color] = band.TabletCount * band.ThreadCount;
        }

        /// <summary>
        /// Randomly shuffles all colors in the band and updates usage statistics.
        /// </summary>
        public void ShuffleBandColors(TabletBand band)
        {
            if (band.TabletCount == 0 || band.RowCount == 0 || ColorUsage.Count < 2)
            {
                return;
            }

            var colorDict = ColorUsage.Keys
                .Zip(Shuffle(), (oldC, newC) => (oldC, newC))
                .ToDictionary(x => x.oldC, x => x.newC);

            band.ShuffleColors(colorDict);
            RebuildColorUsage(band);
        }

        /// <summary>
        /// Randomly shuffles the list of colors ensuring the result differs from the original order.
        /// </summary>
        private static readonly Random rnd = new();

        private List<Color> Shuffle()
        {
            var originalColors = ColorUsage.Keys.ToList();
            var newColors = originalColors.ToList();

            do
            {
                for (int i = newColors.Count - 1; i > 0; i--)
                {
                    int j = rnd.Next(i + 1);
                    (newColors[i], newColors[j]) = (newColors[j], newColors[i]);
                }
            } while (newColors.SequenceEqual(originalColors));

            return newColors;
        }
    }
}

