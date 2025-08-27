using SqlToExcel.Models;
using SqlToExcel.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System;

namespace SqlToExcel.ViewModels
{
    public class ValidationRowResultViewModel
    {
        public string? GroupName { get; }
        public string MismatchedColumnsSummary { get; }
        public ObservableCollection<ValidationResultItem> Mismatches { get; }

        public ValidationRowResultViewModel(string? groupName, IEnumerable<ValidationResultItem> mismatches)
        {
            GroupName = groupName;
            Mismatches = new ObservableCollection<ValidationResultItem>(mismatches);
            var mismatchedColumns = Mismatches.Select(m => m.DisplayColumnName).ToList();
            MismatchedColumnsSummary = $"验证失败，不一致的列: [{string.Join(", ", mismatchedColumns)}]";
        }
    }

    public class ValidationResultViewModel : INotifyPropertyChanged
    {
        private readonly List<ValidationRowResultViewModel> _allRowResults;
        public ObservableCollection<ValidationRowResultViewModel> PaginatedRowResults { get; } = new ObservableCollection<ValidationRowResultViewModel>();
        
        private const int PageSize = 10; // 每页显示的原始行数
        private int _currentPage = 1;

        public string Summary { get; }

        private readonly ExcelExportService _excelExportService;

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage == value) return;
                _currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageInfo));
                UpdatePaginatedResults();
            }
        }

        public int TotalPages { get; private set; }

        public string PageInfo => $"Page {CurrentPage} of {TotalPages}";

        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand ExportToExcelCommand { get; }

        public ValidationResultViewModel(ObservableCollection<ValidationResultItem> results, string summary, ExcelExportService excelExportService)
        {
            Summary = summary;
            _excelExportService = excelExportService;

            _allRowResults = results
                .Where(r => !string.IsNullOrEmpty(r.GroupName))
                .GroupBy(r => r.GroupName)
                .Select(g => new ValidationRowResultViewModel(g.Key, g.ToList()))
                .ToList();

            TotalPages = (int)Math.Ceiling((double)_allRowResults.Count / PageSize);
            if (TotalPages == 0) TotalPages = 1;

            UpdatePaginatedResults();

            NextPageCommand = new RelayCommand(_ => GoToNextPage(), _ => CanGoToNextPage());
            PreviousPageCommand = new RelayCommand(_ => GoToPreviousPage(), _ => CanGoToPreviousPage());
            ExportToExcelCommand = new RelayCommand(async _ => await ExportToExcelAsync(), _ => CanExportToExcel());
        }

        private void UpdatePaginatedResults()
        {
            PaginatedRowResults.Clear();
            var currentPageResults = _allRowResults
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);
            foreach (var item in currentPageResults)
            {
                PaginatedRowResults.Add(item);
            }
        }

        private void GoToNextPage()
        {
            CurrentPage++;
        }

        private bool CanGoToNextPage()
        {
            return CurrentPage < TotalPages;
        }

        private void GoToPreviousPage()
        {
            CurrentPage--;
        }

        private bool CanGoToPreviousPage()
        {
            return CurrentPage > 1;
        }

        private bool CanExportToExcel()
        {
            return _allRowResults.Any();
        }

        private async Task ExportToExcelAsync()
        {
            try
            {
                bool success = await _excelExportService.ExportValidationResultsToExcelAsync(_allRowResults);
                if (success)
                {
                    System.Windows.MessageBox.Show("验证结果已成功导出到Excel。", "导出成功", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"导出验证结果时发生错误: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
