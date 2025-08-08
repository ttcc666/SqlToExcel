using SqlSugar;

namespace SqlToExcel.Services
{
    public class DatabaseService
    {
        private static readonly Lazy<DatabaseService> _instance = new Lazy<DatabaseService>(() => new DatabaseService());
        public static DatabaseService Instance => _instance.Value;

        public SqlSugarScope? Db { get; private set; }

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
            if (!IsConfigured())
            {
                Db = null;
                return true;
            }

            var sourceConn = new ConnectionConfig()
            {
                ConfigId = "source",
                ConnectionString = Properties.Settings.Default.SourceConnectionString,
                DbType = DbType.SqlServer,
                IsAutoCloseConnection = true
            };

            var targetConn = new ConnectionConfig()
            {
                ConfigId = "target",
                ConnectionString = Properties.Settings.Default.TargetConnectionString,
                DbType = DbType.SqlServer,
                IsAutoCloseConnection = true
            };

                        try
            {
                Db = new SqlSugarScope(new List<ConnectionConfig> { sourceConn, targetConn });
                Db.GetConnection("source").Ado.IsValidConnection();
                Db.GetConnection("target").Ado.IsValidConnection();
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
    }
}