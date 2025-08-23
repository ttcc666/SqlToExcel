using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SqlToExcel.ViewModels
{
    public class FieldMappingPair
    {
        public string OldField { get; set; }
        public string NewField { get; set; }
    }

    public class JsonImportPreviewViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<FieldMappingPair> FieldMappings { get; } = new();

        public bool IsFieldCountMismatched { get; }

        public string MismatchMessage { get; }

        public JsonImportPreviewViewModel(List<string> oldFields, List<string> newFields)
        {
            IsFieldCountMismatched = oldFields.Count != newFields.Count;

            if (IsFieldCountMismatched)
            {
                MismatchMessage = $"警告：源字段数量 ({oldFields.Count}) 与目标字段数量 ({newFields.Count}) 不匹配。";
            }
            else
            {
                MismatchMessage = string.Empty;
            }

            int maxCount = System.Math.Max(oldFields.Count, newFields.Count);
            for (int i = 0; i < maxCount; i++)
            {
                FieldMappings.Add(new FieldMappingPair
                {
                    OldField = i < oldFields.Count ? oldFields[i] : "(无)",
                    NewField = i < newFields.Count ? newFields[i] : "(无)"
                });
            }

            OnPropertyChanged(nameof(FieldMappings));
            OnPropertyChanged(nameof(IsFieldCountMismatched));
            OnPropertyChanged(nameof(MismatchMessage));
        }
    }
}
