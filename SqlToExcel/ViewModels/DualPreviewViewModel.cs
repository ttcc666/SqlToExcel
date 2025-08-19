using SqlToExcel.Models;
using SqlToExcel.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class DualPreviewViewModel : INotifyPropertyChanged
    {
        public DataTable Data1 { get; }
        public DataTable Data2 { get; }
        public int RecordCount1 => Data1.Rows.Count;
        public int RecordCount2 => Data2.Rows.Count;

        public ICommand ValidateFirstRowCommand { get; }
        public ICommand ValidateMiddleRowCommand { get; }
        public ICommand ValidateLastRowCommand { get; }

        public DualPreviewViewModel(DataTable data1, DataTable data2)
        {
            Data1 = data1;
            Data2 = data2;

            ValidateFirstRowCommand = new RelayCommand(p => ValidateRows(0), p => RecordCount1 > 0 && RecordCount2 > 0);
            ValidateMiddleRowCommand = new RelayCommand(p => ValidateRows(RecordCount1 / 2), p => RecordCount1 > 0 && RecordCount2 > 0);
            ValidateLastRowCommand = new RelayCommand(p => ValidateRows(RecordCount1 - 1), p => RecordCount1 > 0 && RecordCount2 > 0);
        }

        private void ValidateRows(int rowIndex)
        {
            var results = new ObservableCollection<ValidationResultItem>();
            string summary;

            if (rowIndex < 0 || rowIndex >= RecordCount1 || rowIndex >= RecordCount2)
            {
                summary = "无法验证：索引超出范围或行数不足。";
            }
            else
            {
                var row1 = Data1.Rows[rowIndex];
                var row2 = Data2.Rows[rowIndex];
                int mismatchCount = 0;

                for (int i = 0; i < Data1.Columns.Count; i++)
                {
                    var colName = Data1.Columns[i].ColumnName;
                    var value1 = row1[i]?.ToString() ?? "(null)";
                    string value2;
                    bool isMatch;

                    if (i < Data2.Columns.Count)
                    {
                        value2 = row2[i]?.ToString() ?? "(null)";
                        isMatch = value1 == value2;
                    }
                    else
                    {
                        value2 = "(列不存在)";
                        isMatch = false;
                    }

                    var resultItem = new ValidationResultItem(colName, value1, value2);
                    results.Add(resultItem);

                    if (!resultItem.IsMatch)
                    {
                        mismatchCount++;
                    }
                }

                if (mismatchCount == 0)
                {
                    summary = $"验证通过：基于源列顺序的第 {rowIndex + 1} 行数据完全一致。";
                }
                else
                {
                    summary = $"验证完成：基于源列顺序的第 {rowIndex + 1} 行发现 {mismatchCount} 处不一致。";
                }
            }

            var viewModel = new ValidationResultViewModel(results, summary);
            var view = new ValidationResultView
            {
                DataContext = viewModel,
                Owner = System.Windows.Application.Current.MainWindow
            };
            view.Show();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}