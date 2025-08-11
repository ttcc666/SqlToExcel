using SqlSugar;

namespace SqlToExcel.Models
{
    public class TableMapping
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        public string SourceTable { get; set; }
        public string TargetTable { get; set; }
    }
}