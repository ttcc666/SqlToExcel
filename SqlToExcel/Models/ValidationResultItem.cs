namespace SqlToExcel.Models
{
    public class ValidationResultItem
    {
        public string ColumnName { get; set; }
        public string SourceValue { get; set; }
        public string TargetValue { get; set; }
        public bool IsMatch { get; set; }

        public ValidationResultItem(string columnName, string sourceValue, string targetValue)
        {
            ColumnName = columnName;
            SourceValue = sourceValue;
            TargetValue = targetValue;
            IsMatch = sourceValue == targetValue;
        }
    }
}
