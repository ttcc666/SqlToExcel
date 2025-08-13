using SqlToExcel.ViewModels;
using System.Windows.Controls;

namespace SqlToExcel.Views
{
    public partial class ComparisonReportView : UserControl
    {
        public ComparisonReportView()
        {
            InitializeComponent();
            DataContext = new ComparisonReportViewModel();
        }
    }
}
