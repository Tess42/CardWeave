namespace CardWeave
{
    /// <summary>
    /// Provides undo/redo functionality for TabletBand states.
    /// Stores a limited history of previous states and allows
    /// stepping backward and forward through changes.
    /// </summary>
    public static class UndoRedoManager
    {
        /// <summary> List of previously saved band states for undo operations. </summary>
        private static readonly List<TabletBand> undoList = new();

        /// <summary> List of states for redo operations after an undo. </summary>
        private static readonly List<TabletBand> redoList = new();

        /// <summary> Maximum number of undo steps stored. </summary>
        private const int MaxUndoSteps = 16;

        /// <summary>
        /// Saves the current band state to the undo list.
        /// Clears the redo list and trims history if needed.
        /// </summary>
        public static void SaveState(TabletBand state)
        {
            undoList.Add(state);

            if (undoList.Count > MaxUndoSteps)
            {
                undoList.RemoveAt(0);
            }

            redoList.Clear();
        }

        /// <summary>
        /// Restores the previous band state.
        /// Moves the current state to the redo list.
        /// Returns the restored state or the current one if undo is not possible.
        /// </summary>
        public static TabletBand Undo(TabletBand current)
        {
            if (undoList.Count == 0)
            {
                return current;
            }

            redoList.Add(current);

            var previous = undoList[^1];
            undoList.RemoveAt(undoList.Count - 1);

            return previous;
        }

        /// <summary>
        /// Restores the next band state from the redo list.
        /// Moves the current state to the undo list.
        /// Returns the restored state or the current one if redo is not possible.
        /// </summary>
        public static TabletBand Redo(TabletBand current)
        {
            if (redoList.Count == 0)
            {
                return current;
            }

            undoList.Add(current);

            var next = redoList[^1];
            redoList.RemoveAt(redoList.Count - 1);

            return next;
        }
    }
}
