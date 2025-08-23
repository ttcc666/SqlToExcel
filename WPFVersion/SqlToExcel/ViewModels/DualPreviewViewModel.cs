using SqlToExcel.Models;
using SqlToExcel.Services;
using SqlToExcel.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class DualPreviewViewModel : INotifyPropertyChanged
    {
        public DataTable Data1 { get; }
        public DataTable Data2 { get; }
        private readonly ExcelExportService _excelExportService;
        public int RecordCount1 => Data1.Rows.Count;
        public int RecordCount2 => Data2.Rows.Count;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        private HashSet<string> ExcludedColumns { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public ICommand ValidateFirstRowCommand { get; }
        public ICommand ValidateMiddleRowCommand { get; }
        public ICommand ValidateLastRowCommand { get; }
        public ICommand ValidateAllCommand { get; }
        public ICommand ConfigureValidationCommand { get; }

        public DualPreviewViewModel(DataTable data1, DataTable data2, ExcelExportService excelExportService)
        {
            Data1 = data1;
            Data2 = data2;
            _excelExportService = excelExportService;

            ValidateFirstRowCommand = new RelayCommand(p => ValidateRows(0), p => CanValidate());
            ValidateMiddleRowCommand = new RelayCommand(p => ValidateRows(RecordCount1 / 2), p => CanValidate());
            ValidateLastRowCommand = new RelayCommand(p => ValidateRows(RecordCount1 - 1), p => CanValidate());
            ValidateAllCommand = new RelayCommand(async p => await ValidateAllRowsAsync(), p => CanValidate() && !IsBusy);
            ConfigureValidationCommand = new RelayCommand(p => OpenColumnSelector(), p => CanValidate());
        }

        private void OpenColumnSelector()
        {
            var allColumns = Data2.Columns.Cast<DataColumn>()
                                    .Select(c => c.ColumnName)
                                    .ToList();

            var selectableColumns = allColumns.Select(colName =>
            {
                var dbColInfo = new SqlSugar.DbColumnInfo { DbColumnName = colName };
                var selectableCol = new SelectableDbColumn(dbColInfo)
                {
                    IsSelected = !ExcludedColumns.Contains(colName)
                };
                return selectableCol;
            }).ToList();

            var includedColumns = selectableColumns.Where(c => c.IsSelected).Select(c => c.Column.DbColumnName);

            var viewModel = new ColumnSelectorViewModel(selectableColumns, includedColumns);
            var view = new ColumnSelectorView
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };

            if (view.ShowDialog() == true)
            {
                var newIncludedSet = new HashSet<string>(viewModel.SelectedColumnNamesInOrder, StringComparer.OrdinalIgnoreCase);
                ExcludedColumns = new HashSet<string>(allColumns.Where(c => !newIncludedSet.Contains(c)), StringComparer.OrdinalIgnoreCase);
            }
        }

        private bool CanValidate()
        {
            return RecordCount1 > 0 && RecordCount2 > 0;
        }

        private async Task ValidateAllRowsAsync()
        {
            IsBusy = true;
            var results = new ObservableCollection<ValidationResultItem>();
            string summary;
            int totalMismatchCount = 0;
            var rowsWithMismatches = new HashSet<int>();
            int comparedColumnCount = 0;

            try
            {
                await Task.Run(() =>
                {
                    int rowCount = Math.Min(RecordCount1, RecordCount2);
                    int colCount = Math.Min(Data1.Columns.Count, Data2.Columns.Count);
                    var includedColumns = Data1.Columns.Cast<DataColumn>()
                                               .Select(c => c.ColumnName)
                                               .Where(c => !ExcludedColumns.Contains(c))
                                               .ToList();
                    comparedColumnCount = includedColumns.Count;

                    for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                    {
                        var row1 = Data1.Rows[rowIndex];
                        var row2 = Data2.Rows[rowIndex];
                        bool rowHasMismatch = false;

                        for (int i = 0; i < colCount; i++)
                        {
                            var sourceColName = Data1.Columns[i].ColumnName;
                            var targetColName = Data2.Columns[i].ColumnName;
                            if (ExcludedColumns.Contains(targetColName)) continue;

                            var value1 = row1[i]?.ToString() ?? "(null)";
                            var value2 = row2[i]?.ToString() ?? "(null)";

                            if (value1 != value2)
                            {
                                totalMismatchCount++;
                                rowHasMismatch = true;
                                var resultItem = new ValidationResultItem(sourceColName, targetColName, value1, value2, $"第 {rowIndex + 1} 行");
                                results.Add(resultItem);
                            }
                        }
                        if (rowHasMismatch)
                        {
                            rowsWithMismatches.Add(rowIndex);
                        }
                    }
                });

                if (totalMismatchCount == 0)
                {
                    summary = $"验证通过！共比较 {RecordCount1} 行，{comparedColumnCount} 列。所有数据（已排除的列除外）完全一致。";
                }
                else
                {
                    summary = $"验证完成。共比较 {RecordCount1} 行，{comparedColumnCount} 列。在 {rowsWithMismatches.Count} 行中发现 {totalMismatchCount} 处不一致。";
                }
            }
            catch (Exception ex)
            {
                summary = $"验证过程中发生错误: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                var viewModel = new ValidationResultViewModel(results, summary, _excelExportService);
                var view = new ValidationResultView
                {
                    DataContext = viewModel,
                    Owner = Application.Current.MainWindow
                };
                view.Show();
            });
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
                int colCount = Math.Min(Data1.Columns.Count, Data2.Columns.Count);
                var includedColumns = Data1.Columns.Cast<DataColumn>()
                                           .Select(c => c.ColumnName)
                                           .Where(c => !ExcludedColumns.Contains(c))
                                           .ToList();
                int comparedColumnCount = includedColumns.Count;

                for (int i = 0; i < colCount; i++)
                {
                    var sourceColName = Data1.Columns[i].ColumnName;
                    var targetColName = Data2.Columns[i].ColumnName;
                    if (ExcludedColumns.Contains(targetColName)) continue;

                    var value1 = row1[i]?.ToString() ?? "(null)";
                    var value2 = row2[i]?.ToString() ?? "(null)";

                    var resultItem = new ValidationResultItem(sourceColName, targetColName, value1, value2, $"第 {rowIndex + 1} 行");
                    results.Add(resultItem);

                    if (!resultItem.IsMatch)
                    {
                        mismatchCount++;
                    }
                }

                if (mismatchCount == 0)
                {
                    summary = $"验证通过！第 {rowIndex + 1} 行的 {comparedColumnCount} 列数据（已排除的列除外）完全一致。";
                }
                else
                {
                    summary = $"验证完成。在第 {rowIndex + 1} 行的 {comparedColumnCount} 列中发现 {mismatchCount} 处不一致。";
                }
            }

            var viewModel = new ValidationResultViewModel(results, summary, _excelExportService);
            var view = new ValidationResultView
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
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