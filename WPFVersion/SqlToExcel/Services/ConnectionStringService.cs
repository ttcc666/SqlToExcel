using SqlToExcel.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SqlSugar;

namespace SqlToExcel.Services
{
    /// <summary>
    /// 连接字符串服务实现
    /// </summary>
    public class ConnectionStringService : IConnectionStringService
    {
        private readonly string _configFilePath;
        private readonly byte[] _encryptionKey;
        private readonly IMessageService _messageService;

        public ConnectionStringService(IMessageService messageService)
        {
            _messageService = messageService;
            _configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SqlToExcel", "connections.json");
            _encryptionKey = Encoding.UTF8.GetBytes("SqlToExcel2024Key!"); // 在实际应用中应该使用更安全的密钥管理
            
            // 确保配置目录存在
            var configDir = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
        }

        public async Task<string?> GetSourceConnectionStringAsync()
        {
            var configs = await LoadConnectionConfigsAsync();
            var sourceConfig = configs.FirstOrDefault(c => c.Name == "Source");
            return sourceConfig?.IsEncrypted == true ? DecryptConnectionString(sourceConfig.ConnectionString) : sourceConfig?.ConnectionString;
        }

        public async Task<string?> GetTargetConnectionStringAsync()
        {
            var configs = await LoadConnectionConfigsAsync();
            var targetConfig = configs.FirstOrDefault(c => c.Name == "Target");
            return targetConfig?.IsEncrypted == true ? DecryptConnectionString(targetConfig.ConnectionString) : targetConfig?.ConnectionString;
        }

        public async Task<bool> SetSourceConnectionStringAsync(string connectionString, bool encrypt = true)
        {
            var config = new DatabaseConnectionConfig
            {
                Name = "Source",
                ConnectionString = encrypt ? EncryptConnectionString(connectionString) : connectionString,
                DatabaseType = "SqlServer",
                IsEncrypted = encrypt,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return await SaveConnectionConfigAsync(config);
        }

        public async Task<bool> SetTargetConnectionStringAsync(string connectionString, bool encrypt = true)
        {
            var config = new DatabaseConnectionConfig
            {
                Name = "Target",
                ConnectionString = encrypt ? EncryptConnectionString(connectionString) : connectionString,
                DatabaseType = "SqlServer",
                IsEncrypted = encrypt,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return await SaveConnectionConfigAsync(config);
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateConnectionStringAsync(string connectionString, string databaseType = "SqlServer")
        {
            try
            {
                var dbType = databaseType.ToLower() switch
                {
                    "sqlserver" => DbType.SqlServer,
                    "mysql" => DbType.MySql,
                    "postgresql" => DbType.PostgreSQL,
                    "sqlite" => DbType.Sqlite,
                    "oracle" => DbType.Oracle,
                    _ => DbType.SqlServer
                };

                var db = new SqlSugarClient(new ConnectionConfig
                {
                    ConnectionString = connectionString,
                    DbType = dbType,
                    IsAutoCloseConnection = true
                });

                // 尝试连接数据库
                await Task.Run(() => db.Ado.IsValidConnection());
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<DatabaseConnectionConfig>> GetAllConnectionConfigsAsync()
        {
            return await LoadConnectionConfigsAsync();
        }

        public async Task<bool> SaveConnectionConfigAsync(DatabaseConnectionConfig config)
        {
            try
            {
                var configs = await LoadConnectionConfigsAsync();
                var existingConfig = configs.FirstOrDefault(c => c.Name == config.Name);
                
                if (existingConfig != null)
                {
                    configs.Remove(existingConfig);
                }
                
                configs.Add(config);
                await SaveConnectionConfigsAsync(configs);
                return true;
            }
            catch (Exception ex)
            {
                await _messageService.ShowErrorAsync($"保存连接配置时出错: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteConnectionConfigAsync(string name)
        {
            try
            {
                var configs = await LoadConnectionConfigsAsync();
                var configToRemove = configs.FirstOrDefault(c => c.Name == name);
                
                if (configToRemove != null)
                {
                    configs.Remove(configToRemove);
                    await SaveConnectionConfigsAsync(configs);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                await _messageService.ShowErrorAsync($"删除连接配置时出错: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ExportConnectionConfigsAsync(string filePath, bool includePasswords = false)
        {
            try
            {
                var configs = await LoadConnectionConfigsAsync();
                
                if (!includePasswords)
                {
                    // 移除密码信息
                    configs = configs.Select(c => new DatabaseConnectionConfig
                    {
                        Name = c.Name,
                        ConnectionString = RemovePasswordFromConnectionString(c.IsEncrypted ? DecryptConnectionString(c.ConnectionString) : c.ConnectionString),
                        DatabaseType = c.DatabaseType,
                        IsEncrypted = false,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt
                    }).ToList();
                }

                var json = JsonSerializer.Serialize(configs, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                await _messageService.ShowErrorAsync($"导出连接配置时出错: {ex.Message}");
                return false;
            }
        }

        public async Task<int> ImportConnectionConfigsAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var importedConfigs = JsonSerializer.Deserialize<List<DatabaseConnectionConfig>>(json);
                
                if (importedConfigs == null) return 0;

                var existingConfigs = await LoadConnectionConfigsAsync();
                int importedCount = 0;

                foreach (var config in importedConfigs)
                {
                    var existingConfig = existingConfigs.FirstOrDefault(c => c.Name == config.Name);
                    if (existingConfig != null)
                    {
                        var shouldOverwrite = await _messageService.ShowConfirmationAsync(
                            $"连接配置 '{config.Name}' 已存在。是否要覆盖现有配置？", "配置已存在");
                        
                        if (!shouldOverwrite) continue;
                        
                        existingConfigs.Remove(existingConfig);
                    }
                    
                    config.UpdatedAt = DateTime.Now;
                    existingConfigs.Add(config);
                    importedCount++;
                }

                await SaveConnectionConfigsAsync(existingConfigs);
                return importedCount;
            }
            catch (Exception ex)
            {
                await _messageService.ShowErrorAsync($"导入连接配置时出错: {ex.Message}");
                return 0;
            }
        }

        private async Task<List<DatabaseConnectionConfig>> LoadConnectionConfigsAsync()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    return new List<DatabaseConnectionConfig>();
                }

                var json = await File.ReadAllTextAsync(_configFilePath);
                return JsonSerializer.Deserialize<List<DatabaseConnectionConfig>>(json) ?? new List<DatabaseConnectionConfig>();
            }
            catch
            {
                return new List<DatabaseConnectionConfig>();
            }
        }

        private async Task SaveConnectionConfigsAsync(List<DatabaseConnectionConfig> configs)
        {
            var json = JsonSerializer.Serialize(configs, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_configFilePath, json);
        }

        private string EncryptConnectionString(string connectionString)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = _encryptionKey.Take(32).ToArray(); // AES-256 需要32字节密钥
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor();
                var plainTextBytes = Encoding.UTF8.GetBytes(connectionString);
                var encryptedBytes = encryptor.TransformFinalBlock(plainTextBytes, 0, plainTextBytes.Length);
                
                // 将IV和加密数据组合
                var result = new byte[aes.IV.Length + encryptedBytes.Length];
                Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
                Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);
                
                return Convert.ToBase64String(result);
            }
            catch
            {
                return connectionString; // 加密失败时返回原始字符串
            }
        }

        private string DecryptConnectionString(string encryptedConnectionString)
        {
            try
            {
                var encryptedData = Convert.FromBase64String(encryptedConnectionString);
                
                using var aes = Aes.Create();
                aes.Key = _encryptionKey.Take(32).ToArray();
                
                // 提取IV
                var iv = new byte[aes.IV.Length];
                Array.Copy(encryptedData, 0, iv, 0, iv.Length);
                aes.IV = iv;
                
                // 提取加密数据
                var encryptedBytes = new byte[encryptedData.Length - iv.Length];
                Array.Copy(encryptedData, iv.Length, encryptedBytes, 0, encryptedBytes.Length);
                
                using var decryptor = aes.CreateDecryptor();
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                return encryptedConnectionString; // 解密失败时返回原始字符串
            }
        }

        private string RemovePasswordFromConnectionString(string connectionString)
        {
            // 简单的密码移除逻辑，实际应用中可能需要更复杂的解析
            var parts = connectionString.Split(';');
            var filteredParts = parts.Where(part => 
                !part.Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase) &&
                !part.Trim().StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase)).ToArray();
            
            return string.Join(";", filteredParts);
        }
    }
}
