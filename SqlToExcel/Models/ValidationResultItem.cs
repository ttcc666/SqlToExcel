namespace SqlToExcel.Models
{
    public class ValidationResultItem
    {
        public string DisplayColumnName { get; set; }
        public string SourceColumnName { get; set; }
        public string TargetColumnName { get; set; }
        public string SourceValue { get; set; }
        public string TargetValue { get; set; }
        public bool IsMatch { get; set; }
        public string GroupName { get; set; } // To group by row, for example

        public ValidationResultItem(string sourceColumnName, string targetColumnName, string sourceValue, string targetValue, string groupName = null)
        {
            SourceColumnName = sourceColumnName;
            TargetColumnName = targetColumnName;
            DisplayColumnName = $"{(string.IsNullOrEmpty(sourceColumnName) ? "(空)" : sourceColumnName)} / {(string.IsNullOrEmpty(targetColumnName) ? "(空)" : targetColumnName)}";
            SourceValue = sourceValue;
            TargetValue = targetValue;
            IsMatch = sourceValue == targetValue;
            GroupName = groupName;
        }
    }
}

