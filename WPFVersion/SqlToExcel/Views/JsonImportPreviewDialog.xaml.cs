using System.Windows;

namespace SqlToExcel.Views
{
    public partial class JsonImportPreviewDialog : HandyControl.Controls.BlurWindow
    {
        public JsonImportPreviewDialog()
        {
            InitializeComponent();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
