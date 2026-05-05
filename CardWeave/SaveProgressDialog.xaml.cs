using System.Windows;

namespace CardWeave
{
    /// <summary>
    /// Dialog prompting the user to save progress before continuing.
    /// Returns a MessageBoxResult based on the selected option.
    /// </summary>
    public partial class SaveProgressDialog : Window
    {
        /// <summary> Result of the dialog (Yes, No, or Cancel). Default is to Cancel. </summary>
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.Cancel;

        /// <summary>
        /// Initializes the dialog window.
        /// </summary>
        public SaveProgressDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Confirms saving progress and closes the dialog.
        /// </summary>
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        /// <summary>
        /// Declines saving progress and closes the dialog.
        /// </summary>
        private void No_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        /// <summary>
        /// Cancels the action and closes the dialog without changing the result.
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

