using System.Windows.Media;

namespace CardWeave
{
    /// <summary>
    /// Represents a single weaving tablet.
    /// </summary>
    public class Tablet
    {
        /// <summary> Colors of individual threads in the tablet's holes. </summary>
        public List<Color> ThreadColors { get; set; } = new List<Color>();

        /// <summary> Number of thread holes in the tablet. </summary>
        public int ThreadCount => ThreadColors.Count;

        /// <summary> Threading direction of the tablet (S or Z). </summary>
        public ThreadingDirection Threading { get; set; }

        /// <summary> Rotation direction for each row of the pattern. </summary>
        public List<RotationDirection> Rotations { get; set; } = new List<RotationDirection>();

        public Tablet() { }

        /// <summary>
        /// Creates a tablet with the given number of rows and thread holes.
        /// Initializes rotations to Forward and thread colors to white.
        /// </summary>
        public Tablet(int rowCount, int threadCount)
        {
            Rotations = Enumerable.Repeat(RotationDirection.Forward, rowCount).ToList();
            Threading = ThreadingDirection.S;
            ChangeThreadCount(threadCount);
        }

        /// <summary>
        /// Creates a deep copy of this tablet.
        /// </summary>
        public Tablet Clone()
        {
            return new Tablet
            {
                Threading = this.Threading,
                ThreadColors = new List<Color>(this.ThreadColors),
                Rotations = new List<RotationDirection>(this.Rotations)
            };
        }

        /// <summary>
        /// Changes the number of thread holes. Adds white threads or removes excess threads.
        /// </summary>
        public void ChangeThreadCount(int value)
        {
            if (value < 0) return;
            
            if (value > ThreadCount)
            {
                ThreadColors.AddRange(Enumerable.Repeat(Colors.White, value - ThreadCount));
            }
            else if (value < ThreadCount)
            {
                ThreadColors.RemoveRange(value, ThreadCount - value);
            }
        }

        /// <summary>
        /// Changes the color of thread in a specific hole to given color.
        /// </summary>
        public void ChangeThreadColor(int index, Color newColor)
        {
            if (index >= 0 && index < ThreadCount)
            {
                ThreadColors[index] = newColor;
            }
        }

        /// <summary>
        /// Changes threading direction to opposite.
        /// </summary>
        public void ChangeThreading()
        {
            Threading = Threading == ThreadingDirection.S 
                ? ThreadingDirection.Z 
                : ThreadingDirection.S;
        }

        /// <summary>
        /// Changes rotation direction to opposite for a specific row.
        /// </summary>
        public void ChangeRotation(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < Rotations.Count)
            {
                Rotations[rowIndex] = Rotations[rowIndex] == RotationDirection.Forward 
                    ? RotationDirection.Backward 
                    : RotationDirection.Forward;
            }
        }

        /// <summary>
        /// Reverses rotation direction for all rows.
        /// </summary>
        public void ReverseTurning()
        {
            for (int i = 0; i < Rotations.Count; i++)
            {
                ChangeRotation(i);
            }
        }

        /// <summary>
        /// Sets the rotation direction for all rows.
        /// </summary>
        public void SetAllRotationTo(RotationDirection newRotation)
        {
            for (int i = 0; i < Rotations.Count; i++) 
            {
                Rotations[i] = newRotation;
            }
        }

        /// <summary>
        /// Alternates rotation direction in blocks of the given size.
        /// </summary>
        public void AlternateForwardBackward(int count)
        {
            if (count <= 0) return;
            
            for (int i = 0; i < Rotations.Count; i++)
            {
                Rotations[i] = (i / count) % 2 == 0 
                    ? RotationDirection.Forward 
                    : RotationDirection.Backward;
            }
        }

        /// <summary>
        /// Replaces all occurrences of one color with another.
        /// </summary>
        public void ReplaceColor(Color oldColor, Color newColor)
        {
            for (int i = 0; i < ThreadColors.Count; i++)
            {
                if (ThreadColors[i] == oldColor)
                {
                    ThreadColors[i] = newColor;
                }
            }
        }

        /// <summary>
        /// Replaces thread colors according to a mapping dictionary.
        /// </summary>
        public void ShuffleColors(Dictionary<Color, Color> colorDict)
        {
            for (int i = 0; i < ThreadColors.Count; i++)
            {
                if (colorDict.TryGetValue(ThreadColors[i], out var newColor))
                {
                    ThreadColors[i] = newColor;
                }
            }
        }
    }
}
