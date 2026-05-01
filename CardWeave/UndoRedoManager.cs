namespace CardWeave
{
    public static class UndoRedoManager
    {
        private static readonly List<TabletBand> undoList = new();
        private static readonly List<TabletBand> redoList = new();
        private const int MaxUndoSteps = 8;

        public static bool CanUndo => undoList.Count > 0;
        public static bool CanRedo => redoList.Count > 0;

        public static void SaveState(TabletBand state)
        {
            undoList.Add(state);

            if (undoList.Count > MaxUndoSteps)
            {
                undoList.RemoveAt(0);
            }

            redoList.Clear();
        }

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
