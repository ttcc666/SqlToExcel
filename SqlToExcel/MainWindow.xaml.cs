using HandyControl.Controls;
using SqlToExcel.ViewModels;
using SqlToExcel.Views;
using SqlSugar;
using System.Windows;

namespace SqlToExcel
{
    public partial class MainWindow : BlurWindow
    {
        public MainWindow(BatchExportViewModel batchExportViewModel)
        {
            InitializeComponent();
            BatchExportView.DataContext = batchExportViewModel;
        }

        private void OpenFieldTypeExtractor_Click(object sender, RoutedEventArgs e)
        {
            var fieldTypeExtractorWindow = new FieldTypeExtractorView();
            fieldTypeExtractorWindow.Owner = this;
            fieldTypeExtractorWindow.ShowDialog();
        }

        private void OpenFieldComparison_Click(object sender, RoutedEventArgs e)
        {
            var fieldComparisonView = new FieldComparisonView();
            fieldComparisonView.Owner = this;
            fieldComparisonView.ShowDialog();
        }
    }
}