using SqlToExcel.ViewModels;
using System.Windows;

namespace SqlToExcel.Views
{
    public partial class DatabaseConfigView : Window
    {
        public DatabaseConfigView(DatabaseConfigViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}