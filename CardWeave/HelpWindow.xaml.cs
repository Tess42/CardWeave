using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CardWeave
{
    /// <summary>
    /// Window displaying help topics and their corresponding texts.
    /// </summary>
    public partial class HelpWindow : Window
    {
        /// <summary> Dictionary mapping topic titles to text content. </summary>
        private readonly Dictionary<string, string> helpContent = new();

        /// <summary> Currently selected topic button (highlighted in bold). </summary>
        private Button? currentSelectedButton = null;

        /// <summary>
        /// Initializes the help window, loads text content and selects the default topic.
        /// </summary>
        public HelpWindow()
        {
            InitializeComponent();
            InitializeHelpContent();
            Loaded += (s, e) => LoadDefaultTopic();
        }

        /// <summary>
        /// Selects and displays the first topic button when the window loads.
        /// </summary>
        private void LoadDefaultTopic()
        {
            var stackPanel = (this.Content as Grid)?.Children[0] as Border;

            if (stackPanel?.Child is ScrollViewer scrollViewer)
            {
                var stack = scrollViewer.Content as StackPanel;
                var firstButton = stack?.Children.OfType<Button>().FirstOrDefault();

                if (firstButton != null)
                {
                    Topic_Click(firstButton, new RoutedEventArgs());
                }
            }
        }

        /// <summary>
        /// Initializes all help topics and their associated text content.
        /// </summary>
        private void InitializeHelpContent()
        {
            helpContent["getting_started"] =
@"Card (tablet) weaving is a historical textile technique that uses cards (tablets) with holes to create bands with intricate patterns.

Card Weave helps you visualize and design card weaving patterns digitally.

**How to start**:
1. Set the number of holes per tablet using 'Number of holes'
2. Set the number of tablets (cards) using 'Number of tablets'
3. Set the number of rows using 'Number of rows'
4. Click on squares in the visualization to change thread colors
5. Use the Turning menu or click on hexagons in the visualization to change rotation
6. Follow the Weaving Guide to weave your band step by step
or
Load some pattern from Presets.

**Key concepts**:
• Tablet: A single card with holes for threading
• Thread: The yarn going through each hole
• Hole: A position in the tablet (labeled A, B, C, D...)
• Turning (rotation): Direction a tablet turns (Forward/Backward)
• Threading: Direction threads go through the hole (S or Z)"
;

            helpContent["band_settings"] = 
@"The 'Band Settings' panel controls the basic dimensions of your pattern.

**Number of holes**:
The number of thread holes in each tablet. Standard tablets have 4 holes.
• Minimum: 2
• Maximum: 8

**Number of tablets**:
How many cards are in your band. More tablets = wider band.
• Minimum: 1
• Maximum: 100

**Number of rows**:
How many rows your pattern will have. This defines the pattern length.
• Minimum: 1
• Maximum: 500

**Tip**: Start with 4 holes, 5-10 tablets, and 20 rows to learn the basics."
;

            helpContent["modifying_tablets"] = 
@"The 'Modify Tablets' panel allows you to add or remove tablets from your band.

**Add tablet**:
• Enter a position number (1-based indexing)
• The new tablet will be inserted at that position
• It will have the base setings (S threading, Forward turning and white color)
• Example: Position 3 inserts a tablet between tablets 2 and 3

**Remove tablet**:
• Enter the position of the tablet to remove
• The tablet at that position will be deleted
• Tablets after it will shift one position to the left
• Example: Position 2 removes the second tablet

**Constraints**:
• You cannot insert beyond the current tablet count + 1
• You cannot remove if the band has no tablets"
;

            helpContent["colors"] =
@"**Currently picked color**:
The color displayed in the 'Currently picked color' box is the color you'll apply next.

**Color palette**:
• Click any color square to select it
• Click the '+' button to add a custom color using the color picker
• Recently used colors are stored automatically

**To change a thread color**:
• Select the color you want
• Left-click on a square in the visualization to apply it
• Right-click a square to apply white color

**Band color palette**:
• Shows all colors currently used in your band.
• Hover over a color to see how many times it's used
• Left-click a palette color to select it as picked color
• Right-click a palette color to replace all instances with the picked color

**Color operations**:
• Colors > Set all to currently picked color: Fill entire band with one color
• Colors > Shuffle band colors: Randomly permute colors
• Colors > Set band background: Change the grid background color"
;

            helpContent["threading"] =
@"Threading direction (S or Z) determines how threads pass through tablet holes. S direction tilts the thred to the left, Z direction to the right.

**To change threading**:
• Click the S or Z above the tablet.

**Threading operations**:
• Threading > Reverse all threading: Flip S↔Z for all tablets
• Threading > Set all to S-threading: All tablets use S threading
• Threading > Set all to Z-threading: All tablets use Z threading
• Threading > Halved threading: First half S, second half Z
• Threading > Alternate S and Z threading: S, Z, S, Z..."
;

            helpContent["turning"] =
@"Turning (rotation) is the direction each tablet rotates per row. Forward rotation preserves the thread tilt, while backward rotation reverses it.

**To change turning**:
• Click on the hexagon (thread) in the visualization to reverse turning.

**Turning operations**:
• Turning > Reverse all turning: Flip all rotation directions
• Turning > Set all to forward: All tablets rotate forward
• Turning > Set all to backward: All tablets rotate backward
• Turning > Alternate forward/backward: 1F, 1B, 1F, 1B...
• Turning > 2 forward / 2 backward: 2F, 2B, 2F, 2B...
• Turning > 4 forward / 4 backward: 4F, 4B, 4F, 4B..."
;

            helpContent["visualization"] =
@"The View menu controls what information is displayed on your pattern.

**View > Show front/back side**:
• Switch between front and back of the band.

**View > Show/hide column numbers**:
• Show/hide tablet numbers (1, 2, 3...) above the pattern.
• Useful for reference when describing your pattern.

**View > Show/hide row numbers**:
• Show/hide row numbers (1, 2, 3...) on the left side.
• Helps track progress when weaving.

**View > Show/hide hole label column**:
• Show/hide the hole labels (A, B, C, D) on the left.
• Indicates threading positions for reference.

**View > Show/hide hole labels in grid**:
• Show/hide small letters in each cell showing which hole position it represents.
• Helpful when understanding thread paths."
;

            helpContent["file_operations"] =
@"**File > Save**:
• Saves your current pattern to the existing file.
• If no file was previously saved, it opens 'Save as' dialog.
• Keyboard shortcut: Ctrl+S

**File > Save As**:
• Saves your pattern to a new file.
• You'll be prompted to choose location and filename.
• Pattern is saved as JSON format.

**File > Open**:
• Opens a previously saved pattern file.
• The current pattern will be replaced.

**File > New**:
• Creates a new empty band (4 holes, 0 tablets, 0 rows).

**File > Export as image**:
• Saves the current visualization as PNG or JPEG image.
• Useful for documentation or sharing patterns.

**Presets**:
• The Presets menu contains a selection of patterns to inspire you.
• Click any preset to load it.

**Edit**:
• To revert changes: Edit > Undo (Ctrl+Z)
• To repeat actions: Edit > Redo (Ctrl+Y)"
;

            helpContent["weaving_guide"] =
@"The Weaving Guide helps you weave your pattern step-by-step.

**Starting the guide**:
• Click 'Weaving guide' button. Your band must have at least one tablet and one row.

**Features**:
• Shows instructions for the current row being woven
• Shows thread colors and hole labels on top for reference
• Displays recent rows completed
• Track progress with 'Rows completed' counter

**Pattern repeat**:
• Indicates after how many rows your pattern repeats.
• Pick one repeat from dispayed options.
• These options are based on the number of rows after which the first row repeats plus. One option represents a case where the pattern repeats only after all rows are completed.

**Using the guide**:
1. Open the Weaving Guide
2. Set Pattern repeat.
3. View the current row's instructions per tablet (F - Forward, B - Backward)
4. Turn each tablet according to the directions shown
5. Click next to advance to the next row
6. Use the guide as visual reference while physically weaving"
;

            helpContent["shortcuts"] =
@"**General**:
• Ctrl+Z: Undo last change
• Ctrl+Y: Redo last change
• Ctrl+S: Save pattern
• Enter: Confirm textbox input

**Weaving guide**:
• Space: Next row
• Right arrow: Next row
• Left arrow: Previous row

**Color palette**:
• Left-click: Select color
• Right-click: Replace all instances of this color with the picked color

**Square**:
• Left-click: Apply picked color to this thread
• Right-click: Apply white color to this thread

**Hexagon**:
• Left-click: Reverse rotation direction"
;
        }

        /// <summary>
        /// Formats text by converting marked parts (**text**) into bold.
        /// </summary>
        private void SetFormattedText(string text)
        {
            ContentTextBlock.Inlines.Clear();

            var parts = text.Split("**");

            for (int i = 0; i < parts.Length; i++)
            {
                var run = new Run(parts[i]);

                if (i % 2 == 1)
                {
                    run.FontWeight = FontWeights.Bold;
                }

                ContentTextBlock.Inlines.Add(run);
            }
        }

        /// <summary>
        /// Handles topic button clicks, loads corresponding text and updates button highlighting.
        /// </summary>
        private void Topic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string tag = button.Tag?.ToString() ?? "getting_started";

                if (helpContent.TryGetValue(tag, out string? content) && content != null)
                {
                    SetFormattedText(content);
                }

                if (currentSelectedButton != null)
                {
                    currentSelectedButton.FontWeight = FontWeights.Normal;
                }

                currentSelectedButton = button;
                button.FontWeight = FontWeights.Bold;
            }
        }
    }
}