using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SqlToExcel.Models
{
    public class ComparisonResultItem : INotifyPropertyChanged
    {
        private string _fieldName = string.Empty;
        private bool _isInJson;

        public string FieldName
        {
            get => _fieldName;
            set { _fieldName = value; OnPropertyChanged(); }
        }

        public bool IsInJson
        {
            get => _isInJson;
            set { _isInJson = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
