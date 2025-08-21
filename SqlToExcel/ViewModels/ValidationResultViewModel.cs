using SqlToExcel.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using System;

namespace SqlToExcel.ViewModels
{
    public class ValidationResultViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<ValidationResultItem> _allResults;
        private const int PageSize = 50;
        private int _currentPage = 1;

        public ICollectionView ResultsView { get; }
        public string Summary { get; }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageInfo));
                ResultsView.Refresh();
            }
        }

        public int TotalPages { get; private set; }

        public string PageInfo => $"Page {CurrentPage} of {TotalPages}";

        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }

        public ValidationResultViewModel(ObservableCollection<ValidationResultItem> results, string summary)
        {
            _allResults = results;
            Summary = summary;

            TotalPages = (int)Math.Ceiling((double)_allResults.Count / PageSize);
            if (TotalPages == 0) TotalPages = 1;

            ResultsView = CollectionViewSource.GetDefaultView(_allResults);
            ResultsView.Filter = FilterResults;

            if (_allResults.Any(r => !string.IsNullOrEmpty(r.GroupName)))
            {
                ResultsView.GroupDescriptions.Add(new PropertyGroupDescription("GroupName"));
            }

            NextPageCommand = new RelayCommand(p => GoToNextPage(), p => CanGoToNextPage());
            PreviousPageCommand = new RelayCommand(p => GoToPreviousPage(), p => CanGoToPreviousPage());
        }

        private bool FilterResults(object item)
        {
            var resultItem = item as ValidationResultItem;
            if (resultItem == null) return false;

            int index = _allResults.IndexOf(resultItem);
            int lowerBound = (CurrentPage - 1) * PageSize;
            int upperBound = CurrentPage * PageSize;

            return index >= lowerBound && index < upperBound;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
