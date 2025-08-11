using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SqlToExcel.Views
{
    public partial class PreviewView : Window
    {
        public PreviewView()
        {
            InitializeComponent();
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // 默认情况下，DataGrid 会为 DateTime 创建一个 DataGridTextColumn。
            // 我们可以直接修改它的 StringFormat。
            if (e.PropertyType == typeof(DateTime) || e.PropertyType == typeof(DateTime?))
            {
                if (e.Column is DataGridTextColumn textColumn)
                {
                    // 设置日期的显示格式为24小时制
                    textColumn.Binding.StringFormat = "yyyy-MM-dd HH:mm:ss";
                }
            }
            // 对于布尔类型，我们自定义列以显示 0/1 而不是 True/False
            else if (e.PropertyType == typeof(bool))
            {
                var textColumn = new DataGridTextColumn
                {
                    Header = e.Column.Header,
                    Binding = new Binding(e.PropertyName) { Converter = (IValueConverter)FindResource("BoolToZeroOneConverter") }
                };
                // 替换掉自动生成的列
                e.Column = textColumn;
            }
        }
    }
}