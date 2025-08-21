using SqlToExcel.Models;
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

        public DualPreviewViewModel(DataTable data1, DataTable data2)
        {
            Data1 = data1;
            Data2 = data2;

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

            try
            {
                await Task.Run(() =>
                {
                    int rowCount = Math.Min(RecordCount1, RecordCount2);
                    for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                    {
                        var row1 = Data1.Rows[rowIndex];
                        var row2 = Data2.Rows[rowIndex];

                        foreach (DataColumn col in Data1.Columns)
                        {
                            var colName = col.ColumnName;
                            if (ExcludedColumns.Contains(colName)) continue;

                            var value1 = row1[colName]?.ToString() ?? "(null)";
                            string value2;

                            if (Data2.Columns.Contains(colName))
                            {
                                value2 = row2[colName]?.ToString() ?? "(null)";
                            }
                            else
                            {
                                value2 = "(列不存在)";
                            }

                            if (value1 != value2)
                            {
                                totalMismatchCount++;
                                var resultItem = new ValidationResultItem(colName, value1, value2, $"第 {rowIndex + 1} 行");
                                results.Add(resultItem);
                            }
                        }
                    }
                });

                if (totalMismatchCount == 0)
                {
                    summary = $"验证通过：所有 {RecordCount1} 行数据（已排除的列除外）完全一致。";
                }
                else
                {
                    summary = $"验证完成：共发现 {totalMismatchCount} 处不一致。";
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
                var viewModel = new ValidationResultViewModel(results, summary);
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

                for (int i = 0; i < Data1.Columns.Count; i++)
                {
                    var colName = Data1.Columns[i].ColumnName;
                    if (ExcludedColumns.Contains(colName)) continue;

                    var value1 = row1[i]?.ToString() ?? "(null)";
                    string value2;
                    
                    if (Data2.Columns.Contains(colName))
                    {
                        value2 = row2[colName]?.ToString() ?? "(null)";
                    }
                    else
                    {
                        value2 = "(列不存在)";
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
                    summary = $"验证通过：基于源列顺序的第 {rowIndex + 1} 行数据（已排除的列除外）完全一致。";
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