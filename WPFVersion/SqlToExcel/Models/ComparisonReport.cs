using SqlSugar;
using System;

namespace SqlToExcel.Models
{
    [SugarTable("comparison_reports")]
    public class ComparisonReport
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string TableName { get; set; } = string.Empty;

        [SugarColumn(IsJson = true, ColumnDataType = "text")]
        public string[] JsonFields { get; set; } = Array.Empty<string>();

        [SugarColumn(IsJson = true, ColumnDataType = "text")]
        public string[] DbFields { get; set; } = Array.Empty<string>();

        public DateTime ComparisonDate { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int JsonOnlyCount { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int DbOnlyCount { get; set; }
    }
}
