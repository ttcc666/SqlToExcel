using SqlToExcel.ViewModels;
using System.Windows;

namespace SqlToExcel.Views
{
    public partial class FieldComparisonView : Window
    {
        public FieldComparisonView()
        {
            InitializeComponent();
            DataContext = new FieldComparisonViewModel();
            Owner = Application.Current.MainWindow;
        }
    }
}
