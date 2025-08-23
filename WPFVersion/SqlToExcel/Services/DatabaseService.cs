using SqlSugar;
using SqlToExcel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlToExcel.Services
{
    public class DatabaseService
    {
        private static readonly Lazy<DatabaseService> _instance = new Lazy<DatabaseService>(() => new DatabaseService());
        public static DatabaseService Instance => _instance.Value;
        private const string LocalDbFileName = "config.db";

        public SqlSugarScope? Db { get; private set; }
        public ISqlSugarClient? LocalDb => Db?.GetConnection("local");

        private DatabaseService()
        { }

        public bool IsConfigured()
        {
            var source = Properties.Settings.Default.SourceConnectionString;
            var target = Properties.Settings.Default.TargetConnectionString;
            return !string.IsNullOrWhiteSpace(source) && !string.IsNullOrWhiteSpace(target);
        }

        public bool Initialize()
        {
            var connectionConfigs = new List<ConnectionConfig>();

            // Always add local SQLite DB for configs
            connectionConfigs.Add(new ConnectionConfig()
            {
                ConfigId = "local",
                ConnectionString = $"DataSource={LocalDbFileName}",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true
            });

            // Add source and target if configured
            if (IsConfigured())
            {
                connectionConfigs.Add(new ConnectionConfig()
                {
                    ConfigId = "source",
                    ConnectionString = Properties.Settings.Default.SourceConnectionString,
                    DbType = DbType.SqlServer,
                    IsAutoCloseConnection = true
                });
                connectionConfigs.Add(new ConnectionConfig()
                {
                    ConfigId = "target",
                    ConnectionString = Properties.Settings.Default.TargetConnectionString,
                    DbType = DbType.SqlServer,
                    IsAutoCloseConnection = true
                });

                var frameworkConnection = Properties.Settings.Default.FrameworkConnectionString;
                if (!string.IsNullOrWhiteSpace(frameworkConnection))
                {
                    connectionConfigs.Add(new ConnectionConfig()
                    {
                        ConfigId = "framework",
                        ConnectionString = frameworkConnection,
                        DbType = DbType.SqlServer,
                        IsAutoCloseConnection = true
                    });
                }
            }

            try
            {
                Db = new SqlSugarScope(connectionConfigs);

                // Initialize config table
                LocalDb?.CodeFirst.InitTables<BatchExportConfigEntity>();
                LocalDb?.CodeFirst.InitTables<TableMapping>();
                LocalDb?.CodeFirst.InitTables<ComparisonReport>();
                LocalDb?.CodeFirst.InitTables<MissingTable>();

                // Validate source/target connections if they exist
                if (IsConfigured())
                {
                    Db.GetConnection("source").Ado.IsValidConnection();
                    Db.GetConnection("target").Ado.IsValidConnection();
                    if (connectionConfigs.Any(c => c.ConfigId == "framework"))
                    {
                        Db.GetConnection("framework").Ado.IsValidConnection();
                    }
                }
                return true;
            }
            catch (Exception)
            {
                Db = null;
                return false;
            }
        }

        public List<DbTableInfo> GetTables(string dbKey)
        {
            var db = GetDbConnection(dbKey);
            return db?.DbMaintenance.GetTableInfoList(false).OrderBy(t => t.Name).ToList() ?? new List<DbTableInfo>();
        }

        public List<DbColumnInfo> GetColumns(string dbKey, string tableName)
        {
            var db = GetDbConnection(dbKey);
            return db?.DbMaintenance.GetColumnInfosByTableName(tableName, false) ?? new List<DbColumnInfo>();
        }

        public async System.Threading.Tasks.Task<long> GetTableCountAsync(string dbKey, string tableName)
        {
            var db = GetDbConnection(dbKey);
            if (db == null) return 0;
            return await db.Queryable<object>().AS(tableName).CountAsync();
        }

        private ISqlSugarClient GetDbConnection(string dbKey)
        {
            if (Db == null)
            {
                throw new InvalidOperationException("数据库连接未初始化。");
            }
            return Db.GetConnection(dbKey.ToLower());
        }

        public Dictionary<string, string> GetFieldTypesInfo(string dbKey, string tableName, List<string> fieldNames)
        {
            var db = GetDbConnection(dbKey);
            var columns = db?.DbMaintenance.GetColumnInfosByTableName(tableName, false);
            
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            if (columns == null)
            {
                foreach (var fieldName in fieldNames)
                {
                    result[fieldName] = "表不存在";
                }
                return result;
            }

            foreach (var fieldName in fieldNames)
            {
                var column = columns.FirstOrDefault(c =>
                    c.DbColumnName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                
                if (column != null)
                {
                    string fullType = BuildFullTypeString(column);
                    result[fieldName] = fullType;
                }
                else
                {
                    result[fieldName] = "字段不存在";
                }
            }
            return result;
        }

        private string BuildFullTypeString(DbColumnInfo column)
        {
            string baseType = column.DataType.ToLower();
            
            // 处理字符串类型
            if (baseType.Contains("varchar"))
            {
                if (column.Length == -1)
                    return $"{column.DataType}(MAX)";
                else if (column.Length > 0)
                    return $"{column.DataType}({column.Length})";
                else
                    return column.DataType;
            }
            else if (baseType.Contains("nvarchar"))
            {
                if (column.Length == -1)
                    return $"{column.DataType}(MAX)";
                else if (column.Length > 0)
                    return $"{column.DataType}({column.Length / 2})"; // nvarchar长度需要除以2
                else
                    return column.DataType;
            }
            else if (baseType.Contains("char") || baseType.Contains("nchar"))
            {
                return column.Length > 0 ? $"{column.DataType}({column.Length})" : column.DataType;
            }
            // 处理数值类型
            else if (baseType.Contains("decimal") || baseType.Contains("numeric"))
            {
                if (column.DecimalDigits > 0 || column.Scale > 0)
                    return $"{column.DataType}({column.DecimalDigits},{column.Scale})";
                else
                    return column.DataType;
            }
            else if (baseType.Contains("float"))
            {
                return column.Length > 0 ? $"{column.DataType}({column.Length})" : column.DataType;
            }
            // 处理二进制类型
            else if (baseType.Contains("binary") || baseType.Contains("varbinary"))
            {
                if (column.Length == -1)
                    return $"{column.DataType}(MAX)";
                else if (column.Length > 0)
                    return $"{column.DataType}({column.Length})";
                else
                    return column.DataType;
            }
            // 其他类型直接返回
            else
            {
                return column.DataType;
            }
        }

        public bool IsTableExists(string dbKey, string tableName)
        {
            try
            {
                var db = GetDbConnection(dbKey);
                var tables = db?.DbMaintenance.GetTableInfoList(false);
                return tables?.Any(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase)) ?? false;
            }
            catch
            {
                return false;
            }
        }

        public bool IsTableExistsInTarget(string tableName)
        {
            try
            {
                var db = GetDbConnection("target");
                var tables = db?.DbMaintenance.GetTableInfoList(false);
                return tables?.Any(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase)) ?? false;
            }
            catch
            {
                return false;
            }
        }

        public async System.Threading.Tasks.Task SaveComparisonReportAsync(ComparisonReport report)
        {
            if (LocalDb == null) throw new InvalidOperationException("本地数据库未初始化。");
            await LocalDb.Storageable(report).ExecuteCommandAsync();
        }

        public async System.Threading.Tasks.Task<System.Collections.Generic.List<ComparisonReport>> GetComparisonReportsAsync()
        {
            if (LocalDb == null) throw new InvalidOperationException("本地数据库未初始化。");
            return await LocalDb.Queryable<ComparisonReport>().OrderBy(r => r.ComparisonDate, OrderByType.Desc).ToListAsync();
        }

        public async System.Threading.Tasks.Task DeleteComparisonReportsAsync(System.Collections.Generic.IEnumerable<string> tableNames)
        {
            if (LocalDb == null) throw new InvalidOperationException("本地数据库未初始化。");
            await LocalDb.Deleteable<ComparisonReport>().In(tableNames).ExecuteCommandAsync();
        }

        public async System.Threading.Tasks.Task<List<string>> GetTableNamesAsync()
        {
            var db = GetDbConnection("source");
            if (db == null) return new List<string>();
            var tables = await System.Threading.Tasks.Task.Run(() => db.DbMaintenance.GetTableInfoList(false));
            return tables?.Where(t=> !t.Name.StartsWith("_") && !Regex.IsMatch(t.Name,"[0-9]+")).Select(t => t.Name).ToList() ?? new List<string>();
        }

        public async Task<List<string>> GetPrimaryKeysAsync(string dbKey, string tableName)
        {
            var db = GetDbConnection(dbKey);
            if (db == null || !await Task.Run(() => IsTableExists(dbKey, tableName)))
            {
                return new List<string>();
            }
            // GetPrimaries is sync, but we run it in a task to not block the UI thread
            return await Task.Run(() => db.DbMaintenance.GetPrimaries(tableName));
        }

        public async Task<List<IndexDetail>> GetIndexDetailsAsync(string dbKey, string tableName)
        {
            var db = GetDbConnection(dbKey);
            if (db == null || !await Task.Run(() => IsTableExists(dbKey, tableName)))
            {
                return new List<IndexDetail>();
            }

            string sql = @"
            SELECT 
                i.name AS IndexName,
                c.name AS ColumnName,
                i.type_desc AS IndexType,
                ic.is_included_column AS IsIncludedColumn
            FROM 
                sys.indexes AS i
            INNER JOIN 
                sys.index_columns AS ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
            INNER JOIN 
                sys.columns AS c ON ic.object_id = c.object_id AND c.column_id = ic.column_id
            INNER JOIN 
                sys.tables AS t ON i.object_id = t.object_id
            WHERE 
                t.name = @tableName
            ORDER BY 
                i.name, ic.key_ordinal;
            ";

            return await db.Ado.SqlQueryAsync<IndexDetail>(sql, new { tableName });
        }
    }
}