using System.Windows;
using SqlToExcel.ViewModels;

namespace SqlToExcel.Views
{
    public partial class DatabaseConfigView : Window
    {
        public DatabaseConfigView()
        {
            InitializeComponent();
            DataContext = new DatabaseConfigViewModel();
        }
    }
}
