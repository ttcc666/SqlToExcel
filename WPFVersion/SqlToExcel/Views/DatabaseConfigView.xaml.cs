using SqlToExcel.ViewModels;
using System.Windows;

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