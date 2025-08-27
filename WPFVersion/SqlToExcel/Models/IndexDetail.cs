namespace SqlToExcel.Models
{
    public class IndexDetail
    {
        public required string IndexName { get; set; }
        public required string ColumnName { get; set; }
        public required string IndexType { get; set; }
        public bool IsIncludedColumn { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }
        // 聚集
        public bool IsClustered { get; set; }
        // 非聚集
        public bool IsNonClustered { get; set; }
    }
}
