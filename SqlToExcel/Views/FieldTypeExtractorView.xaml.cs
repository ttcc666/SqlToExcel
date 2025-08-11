using SqlToExcel.ViewModels;
using System.Windows;

namespace SqlToExcel.Views
{
    /// <summary>
    /// FieldTypeExtractorView.xaml 的交互逻辑
    /// </summary>
    public partial class FieldTypeExtractorView : Window
    {
        public FieldTypeExtractorView()
        {
            InitializeComponent();
            DataContext = new FieldTypeExtractorViewModel();
        }
    }
}