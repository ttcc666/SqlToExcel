namespace SqlToExcel.Models
{
    public class BatchExportConfig
    {
        public string Key { get; set; } = null!;
        public QueryConfig DataSource { get; set; } = null!;
        public QueryConfig DataTarget { get; set; } = null!;
    }

    public class QueryConfig
    {
        public string SheetName { get; set; } = null!;
        public string Sql { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
