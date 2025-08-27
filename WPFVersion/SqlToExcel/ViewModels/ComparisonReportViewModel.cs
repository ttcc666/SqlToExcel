using Microsoft.Extensions.DependencyInjection;
using SqlToExcel.Models;
using SqlToExcel.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SqlToExcel.ViewModels
{
    public class ComparisonReportViewModel : INotifyPropertyChanged
    {
        private ComparisonReport? _selectedReport;
        private bool _isLoading = false;

        public ObservableCollection<ComparisonReport> Reports { get; set; }
        public ObservableCollection<ComparisonResultItem> DetailedResults { get; set; }

        public ComparisonReport? SelectedReport
        {
            get => _selectedReport;
            set
            {
                _selectedReport = value;
                OnPropertyChanged();
                LoadDetailedResults();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand DeleteSelectedCommand { get; }

        public ComparisonReportViewModel()
        {
            Reports = new ObservableCollection<ComparisonReport>();
            DetailedResults = new ObservableCollection<ComparisonResultItem>();
            RefreshCommand = new RelayCommand(async p => await LoadReportsAsync());
            ExportCommand = new RelayCommand(p => ExportAllReports(), p => Reports.Any());
            DeleteSelectedCommand = new RelayCommand(p => DeleteSelected(p), p => p is System.Collections.IList list && list.Count > 0);

            EventService.Subscribe<ComparisonReportUpdatedEvent>(e => _ = LoadReportsAsync());
            _ = LoadReportsAsync();
        }

        private async Task LoadReportsAsync()
        {
            IsLoading = true;
            Reports.Clear();
            DetailedResults.Clear();

            var reports = await DatabaseService.Instance.GetComparisonReportsAsync();
            foreach (var report in reports)
            {
                // 使用与详情视图完全相同的逻辑来计算差异数量，确保一致性
                var jsonFields = new HashSet<string>(report.JsonFields, StringComparer.OrdinalIgnoreCase);
                var dbFields = new HashSet<string>(report.DbFields, StringComparer.OrdinalIgnoreCase);

                report.DbOnlyCount = dbFields.Count(dbField => !jsonFields.Contains(dbField));
                report.JsonOnlyCount = jsonFields.Count(jsonField => !dbFields.Contains(jsonField));
                
                Reports.Add(report);
            }
            IsLoading = false;
        }

        private void LoadDetailedResults()
        {
            DetailedResults.Clear();
            if (SelectedReport == null) return;

            var jsonFields = new HashSet<string>(SelectedReport.JsonFields, StringComparer.OrdinalIgnoreCase);
            foreach (var dbField in SelectedReport.DbFields.OrderBy(f => f))
            {
                DetailedResults.Add(new ComparisonResultItem
                {
                    FieldName = dbField,
                    IsInJson = jsonFields.Contains(dbField)
                });
            }
        }

        private void ExportAllReports()
        {
            if (!Reports.Any()) return;

            var allTabs = new List<TableComparisonResultViewModel>();

            foreach (var report in Reports)
            {
                var detailItems = new ObservableCollection<ComparisonResultItem>();
                var jsonFields = new HashSet<string>(report.JsonFields, StringComparer.OrdinalIgnoreCase);
                foreach (var dbField in report.DbFields.OrderBy(f => f))
                {
                    detailItems.Add(new ComparisonResultItem
                    {
                        FieldName = dbField,
                        IsInJson = jsonFields.Contains(dbField)
                    });
                }

                allTabs.Add(new TableComparisonResultViewModel
                {
                    TableName = report.TableName,
                    ComparisonResults = detailItems
                });
            }

            var excelService = App.ServiceProvider.GetRequiredService<ExcelExportService>();
            excelService.ExportComparisonResults(allTabs);
        }

        private async void DeleteSelected(object? selectedItems)
        {
            if (selectedItems is not System.Collections.IList items || items.Count == 0) return;

            var reportsToDelete = items.Cast<ComparisonReport>().ToList();

            var result = System.Windows.MessageBox.Show($"您确定要删除选中的 {reportsToDelete.Count} 条报告吗？此操作不可恢复。", 
                                                       "确认删除", 
                                                       System.Windows.MessageBoxButton.YesNo, 
                                                       System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    var tableNamesToDelete = reportsToDelete.Select(r => r.TableName).ToList();
                    await DatabaseService.Instance.DeleteComparisonReportsAsync(tableNamesToDelete);
                    await LoadReportsAsync(); // 重新加载以刷新UI
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"删除报告时出错: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
