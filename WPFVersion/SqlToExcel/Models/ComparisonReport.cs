using SqlSugar;
using System;

namespace SqlToExcel.Models
{
    [SugarTable("comparison_reports")]
    public class ComparisonReport
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string TableName { get; set; }

        [SugarColumn(IsJson = true, ColumnDataType = "text")]
        public string[] JsonFields { get; set; }

        [SugarColumn(IsJson = true, ColumnDataType = "text")]
        public string[] DbFields { get; set; }

        public DateTime ComparisonDate { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int JsonOnlyCount { get; set; }

        [SugarColumn(IsIgnore = true)]
        public int DbOnlyCount { get; set; }
    }
}
