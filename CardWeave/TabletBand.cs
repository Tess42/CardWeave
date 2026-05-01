using System.Text.Json.Serialization;
using System.Windows.Media;

namespace CardWeave
{
    /// <summary>
    /// Represents a full band composed of multiple tablets.
    /// </summary>
    public class TabletBand
    {
        /// <summary> List of tablets forming the band. </summary>
        public List<Tablet> Tablets { get; set; } = new List<Tablet>();
        public int TabletCount => Tablets.Count;

        /// <summary> Number of rows in the visualized pattern. </summary>
        public int RowCount { get; set; } = 0;

        /// <summary> Number of thread holes per tablet. </summary>
        public int ThreadCount { get; set; } = 4;

        /// <summary> Number of rows after which the pattern repeats (useful in guide). </summary>
        public int PatternRepeat { get; set; } = 0;

        /// <summary> Number of rows already woven. </summary>
        public int RowsCompleted { get; set; } = 0;

        /// <summary> Path to the currently opened file, if any. </summary>
        [JsonIgnore]
        public string? CurrentFilePath { get; set; } = null;

        public TabletBand() { }

        /// <summary>
        /// Creates a deep copy of the entire band, including all tablets.
        /// </summary>
        public TabletBand Clone()
        {
            return new TabletBand
            {
                RowCount = this.RowCount,
                ThreadCount = this.ThreadCount,
                PatternRepeat = this.PatternRepeat,
                RowsCompleted = this.RowsCompleted,
                Tablets = this.Tablets.Select(t => t.Clone()).ToList()
            };
        }

        /// <summary>
        /// Changes the number of rows in the pattern and updates all tablets accordingly.
        /// </summary>
        public void ChangeRowCount(int value)
        {
            if (RowCount != value && value >= 0 && value <= WeavingConstants.MaxRows)
            {
                foreach (Tablet tablet in Tablets)
                {
                    if (value > RowCount)
                    {
                        tablet.Rotations.AddRange(Enumerable.Repeat(RotationDirection.Forward, value - RowCount));
                    }
                    else if (value < RowCount)
                    {
                        tablet.Rotations.RemoveRange(value, RowCount - value);
                    }
                }

                RowCount = value;
            }
        }

        /// <summary>
        /// Changes the number of tablets in the band by adding new tablets or removing existing ones.
        /// </summary>
        public void ChangeTabletCount(int value)
        {
            if (TabletCount != value && value >= 0 && value <= WeavingConstants.MaxTablets)
            {
                if (value > TabletCount)
                {
                    Tablets.AddRange(Enumerable.Range(0, value - TabletCount).Select(i => new Tablet(RowCount, ThreadCount)));
                }
                else if (value < TabletCount)
                {
                    Tablets.RemoveRange(value, TabletCount - value);
                }
            }
        }

        /// <summary>
        /// Changes the number of thread holes for all tablets.
        /// </summary>
        public void ChangeThreadCount(int value)
        {
            if (ThreadCount != value && value >= WeavingConstants.MinThreadCount && value <= WeavingConstants.MaxThreadCount)
            {
                foreach (Tablet tablet in Tablets)
                {
                    tablet.ChangeThreadCount(value);
                }

                ThreadCount = value;
            }
        }

        public void ChangePatternRepeat(int value)
        {
            if (value > 0 && value <= RowCount)
            {
                PatternRepeat = value;
            }
        }

        public void ChangeRowsCompleted(int value)
        {
            if (value >= 0)
            {
                RowsCompleted = value;
            }
        }

        /// <summary>
        /// Inserts a new tablet at given index.
        /// </summary>
        public void AddTablet(int index)
        {
            if (index > 0 && index <= (TabletCount + 1) && TabletCount < WeavingConstants.MaxTablets)
            {
                Tablets.Insert(index - 1, new Tablet(RowCount, ThreadCount));
            }
        }

        /// <summary>
        /// Removes tablet at given index.
        /// </summary>
        public void RemoveTablet(int index)
        {
            if (index > 0 && index <= TabletCount && TabletCount > 0)
            {
                Tablets.RemoveAt(index - 1);
            }
        }

        public void ReverseAllTurning()
        {
            foreach (var tablet in Tablets)
            {
                tablet.ReverseTurning();
            }
        }

        public void SetAllRotationTo(RotationDirection newRotation)
        {
            foreach (var tablet in Tablets)
            {
                tablet.SetAllRotationTo(newRotation);
            }
        }

        public void AlternateForwardBackward(int count)
        {
            foreach (var tablet in Tablets)
            {
                tablet.AlternateForwardBackward(count);
            }
        }

        public void ReverseAllThreading()
        {
            foreach (var tablet in Tablets)
            {
                tablet.ChangeThreading();
            }
        }

        public void SetAllThreadingTo(ThreadingDirection newThreading)
        {
            foreach (var tablet in Tablets)
            {
                tablet.Threading = newThreading;
            }
        }

        public void SetHalvedThreading()
        {
            for (int i = 0; i < TabletCount; i++)
            {
                if (i < TabletCount / 2)
                {
                    Tablets[i].Threading = ThreadingDirection.S;
                } else
                {
                    Tablets[i].Threading = ThreadingDirection.Z;
                }
            }
        }

        public void AlternateSZThreading()
        {
            for (int i = 0; i < TabletCount; i++)
            {
                if (i % 2 == 0)
                {
                    Tablets[i].Threading = ThreadingDirection.S;
                }
                else
                {
                    Tablets[i].Threading = ThreadingDirection.Z;
                }
            }
        }

        public void ShuffleColors(Dictionary<Color, Color> colorDict)
        {
            foreach (var tablet in Tablets)
            {
                tablet.ShuffleColors(colorDict);
            }
        }
    }
}
