using SqlToExcel.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SqlToExcel.ViewModels
{
    public class TableComparisonResultViewModel : INotifyPropertyChanged
    {
        private string _tableName;
        private string _statusMessage;
        public ObservableCollection<ComparisonResultItem> ComparisonResults { get; set; }

        public string TableName
        {
            get => _tableName;
            set { _tableName = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public TableComparisonResultViewModel()
        {
            ComparisonResults = new ObservableCollection<ComparisonResultItem>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
