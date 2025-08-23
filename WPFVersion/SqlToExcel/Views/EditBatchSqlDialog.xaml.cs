using SqlToExcel.ViewModels;
using System.Windows;

namespace SqlToExcel.Views
{
    public partial class EditBatchSqlDialog : Window
    {
        public EditBatchSqlDialog()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is EditBatchSqlViewModel vm)
            {
                vm.SaveChanges();
            }
            DialogResult = true;
            Close();
        }
    }
}
