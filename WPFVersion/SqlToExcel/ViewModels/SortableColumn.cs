using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SqlToExcel.ViewModels
{
    public enum SortDirection
    {
        Ascending,
        Descending
    }

    public class SortableColumn : INotifyPropertyChanged
    {
        private SortDirection _direction;

        public string ColumnName { get; }

        public SortDirection Direction
        {
            get => _direction;
            set { _direction = value; OnPropertyChanged(); }
        }

        public SortableColumn(string columnName)
        {
            ColumnName = columnName;
            Direction = SortDirection.Ascending;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}