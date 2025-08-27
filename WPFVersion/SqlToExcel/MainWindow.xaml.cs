using HandyControl.Controls;
using SqlToExcel.ViewModels;
using SqlToExcel.Views;
using SqlSugar;
using System.Windows;

namespace SqlToExcel
{
    public partial class MainWindow : BlurWindow
    {
        public MainWindow(BatchExportViewModel batchExportViewModel, TableMappingViewModel tableMappingViewModel, SchemaComparisonViewModel schemaComparisonViewModel)
        {
            InitializeComponent();
            BatchExportView.DataContext = batchExportViewModel;

            // 设置 TableMappingView 的 DataContext
            var tableMappingView = FindName("TableMappingView") as TableMappingView;
            if (tableMappingView != null)
            {
                tableMappingView.DataContext = tableMappingViewModel;
            }

            // 设置 SchemaComparisonView 的 DataContext
            var schemaComparisonView = FindName("SchemaComparisonView") as SchemaComparisonView;
            if (schemaComparisonView != null)
            {
                schemaComparisonView.DataContext = schemaComparisonViewModel;
            }
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