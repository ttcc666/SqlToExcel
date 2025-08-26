using SqlToExcel.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SqlToExcel.Models
{
    public class SchemaComparisonResult : INotifyPropertyChanged
    {
        private string _sourceTableName;
        public string SourceTableName
        {
            get => _sourceTableName;
            set { _sourceTableName = value; OnPropertyChanged(); }
        }

        private string _sourcePrimaryKeys;
        public string SourcePrimaryKeys
        {
            get => _sourcePrimaryKeys;
            set { _sourcePrimaryKeys = value; OnPropertyChanged(); }
        }

        private List<IndexDetailViewModel> _sourceIndexes;
        public List<IndexDetailViewModel> SourceIndexes
        {
            get => _sourceIndexes;
            set { _sourceIndexes = value; OnPropertyChanged(); }
        }

        private string _targetTableName;
        public string TargetTableName
        {
            get => _targetTableName;
            set { _targetTableName = value; OnPropertyChanged(); }
        }

        private string _targetPrimaryKeys;
        public string TargetPrimaryKeys
        {
            get => _targetPrimaryKeys;
            set { _targetPrimaryKeys = value; OnPropertyChanged(); }
        }

        private List<IndexDetailViewModel> _targetIndexes;
        public List<IndexDetailViewModel> TargetIndexes
        {
            get => _targetIndexes;
            set { _targetIndexes = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
