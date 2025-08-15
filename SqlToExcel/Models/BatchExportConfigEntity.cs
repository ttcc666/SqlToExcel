using SqlSugar;

namespace SqlToExcel.Models
{
    [SugarTable("BatchExportConfigs")]
    public class BatchExportConfigEntity
    {
        [SugarColumn(IsPrimaryKey = true)]
        public string Key { get; set; } = null!;

        /// <summary>
        /// DataSource as JSON string
        /// </summary>
        public string DataSourceJson { get; set; } = null!;

        /// <summary>
        /// DataTarget as JSON string
        /// </summary>
        public string DataTargetJson { get; set; } = null!;

        public string Prefix { get; set; } = string.Empty;
    }
}