using SqlToExcel.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SqlToExcel.Models
{
    public class SchemaComparisonResult : INotifyPropertyChanged
    {
        private string _sourceTableName = string.Empty;
        public string SourceTableName
        {
            get => _sourceTableName;
            set { _sourceTableName = value; OnPropertyChanged(); }
        }

        private string _sourcePrimaryKeys = string.Empty;
        public string SourcePrimaryKeys
        {
            get => _sourcePrimaryKeys;
            set { _sourcePrimaryKeys = value; OnPropertyChanged(); }
        }

        private List<IndexDetailViewModel> _sourceIndexes = new();
        public List<IndexDetailViewModel> SourceIndexes
        {
            get => _sourceIndexes;
            set { _sourceIndexes = value; OnPropertyChanged(); }
        }

        private string _targetTableName = string.Empty;
        public string TargetTableName
        {
            get => _targetTableName;
            set { _targetTableName = value; OnPropertyChanged(); }
        }

        private string _targetPrimaryKeys = string.Empty;
        public string TargetPrimaryKeys
        {
            get => _targetPrimaryKeys;
            set { _targetPrimaryKeys = value; OnPropertyChanged(); }
        }

        private List<IndexDetailViewModel> _targetIndexes = new();
        public List<IndexDetailViewModel> TargetIndexes
        {
            get => _targetIndexes;
            set { _targetIndexes = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
