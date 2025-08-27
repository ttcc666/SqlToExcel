namespace SqlToExcel.Models
{
    public enum DestinationType { Target, Framework }

    public class BatchExportConfig
    {
        public string Key { get; set; } = null!;
        public DestinationType Destination { get; set; }
        public QueryConfig DataSource { get; set; } = null!;
        public QueryConfig DataTarget { get; set; } = null!;
        public string Prefix { get; set; } = string.Empty;
    }

    public class QueryConfig
    {
        public string SheetName { get; set; } = null!;
        public string? TableName { get; set; }
        public string Sql { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
