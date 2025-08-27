using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlToExcel.Services.Interfaces
{
    /// <summary>
    /// 数据库连接配置模型
    /// </summary>
    public class DatabaseConnectionConfig
    {
        public string Name { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseType { get; set; } = string.Empty;
        public bool IsEncrypted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 连接字符串服务接口，用于管理数据库连接配置
    /// </summary>
    public interface IConnectionStringService
    {
        /// <summary>
        /// 获取源数据库连接字符串
        /// </summary>
        /// <returns>解密后的连接字符串</returns>
        Task<string?> GetSourceConnectionStringAsync();

        /// <summary>
        /// 获取目标数据库连接字符串
        /// </summary>
        /// <returns>解密后的连接字符串</returns>
        Task<string?> GetTargetConnectionStringAsync();

        /// <summary>
        /// 设置源数据库连接字符串
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="encrypt">是否加密存储</param>
        /// <returns>操作是否成功</returns>
        Task<bool> SetSourceConnectionStringAsync(string connectionString, bool encrypt = true);

        /// <summary>
        /// 设置目标数据库连接字符串
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="encrypt">是否加密存储</param>
        /// <returns>操作是否成功</returns>
        Task<bool> SetTargetConnectionStringAsync(string connectionString, bool encrypt = true);

        /// <summary>
        /// 验证连接字符串是否有效
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="databaseType">数据库类型</param>
        /// <returns>验证结果和错误信息</returns>
        Task<(bool IsValid, string? ErrorMessage)> ValidateConnectionStringAsync(string connectionString, string databaseType = "SqlServer");

        /// <summary>
        /// 获取所有保存的连接配置
        /// </summary>
        /// <returns>连接配置列表</returns>
        Task<List<DatabaseConnectionConfig>> GetAllConnectionConfigsAsync();

        /// <summary>
        /// 保存连接配置
        /// </summary>
        /// <param name="config">连接配置</param>
        /// <returns>操作是否成功</returns>
        Task<bool> SaveConnectionConfigAsync(DatabaseConnectionConfig config);

        /// <summary>
        /// 删除连接配置
        /// </summary>
        /// <param name="name">配置名称</param>
        /// <returns>操作是否成功</returns>
        Task<bool> DeleteConnectionConfigAsync(string name);

        /// <summary>
        /// 导出连接配置到文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="includePasswords">是否包含密码（加密存储）</param>
        /// <returns>操作是否成功</returns>
        Task<bool> ExportConnectionConfigsAsync(string filePath, bool includePasswords = false);

        /// <summary>
        /// 从文件导入连接配置
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>导入的配置数量</returns>
        Task<int> ImportConnectionConfigsAsync(string filePath);
    }
}
