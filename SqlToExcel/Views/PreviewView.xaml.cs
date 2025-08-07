using System.Windows;
using System.Windows.Controls;

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
            if (e.PropertyType == typeof(bool))
            {
                var textColumn = new DataGridTextColumn
                {
                    Header = e.Column.Header,
                    Binding = new System.Windows.Data.Binding(e.PropertyName)
                };
                e.Column = textColumn;
            }
        }
    }
}