namespace SqlToExcel.Models
{
    public class ValidationResultItem
    {
        public string ColumnName { get; set; }
        public string SourceValue { get; set; }
        public string TargetValue { get; set; }
        public bool IsMatch { get; set; }
        public string GroupName { get; set; } // To group by row, for example

        // Constructor updated to include an optional groupName parameter
        public ValidationResultItem(string columnName, string sourceValue, string targetValue, string groupName = null)
        {
            ColumnName = columnName;
            SourceValue = sourceValue;
            TargetValue = targetValue;
            IsMatch = sourceValue == targetValue;
            GroupName = groupName;
        }
    }
}
