using SqlToExcel.Models;
using SqlToExcel.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace SqlToExcel.Services
{
    public class ConfigFileService
    {
        private readonly IMessageService _messageService;
        private const string ConfigFileName = "batch_export_configs.json";

        public ConfigFileService(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task<List<BatchExportConfig>> LoadConfigsAsync()
        {
            try
            {
                if (!File.Exists(ConfigFileName))
                {
                    return new List<BatchExportConfig>();
                }

                var json = await File.ReadAllTextAsync(ConfigFileName);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                };
                return JsonSerializer.Deserialize<List<BatchExportConfig>>(json, options) ?? new List<BatchExportConfig>();
            }
            catch (Exception ex)
            {
                await _messageService.ShowErrorAsync($"加载配置文件时出错: {ex.Message}");
                return new List<BatchExportConfig>();
            }
        }

        public List<BatchExportConfig> LoadConfigs()
        {
            return LoadConfigsAsync().Result;
        }

        public async Task<bool> SaveConfigAsync(BatchExportConfig newConfig, bool overwrite = false)
        {
            try
            {
                var configs = await LoadConfigsAsync();

                // 检查键是否已存在
                var existingConfig = configs.FirstOrDefault(c =>
                    string.Equals(c.Key, newConfig.Key, StringComparison.OrdinalIgnoreCase));

                if (existingConfig != null)
                {
                    if (!overwrite)
                    {
                        var shouldOverwrite = await _messageService.ShowConfirmationAsync(
                            $"配置键 '{newConfig.Key}' 已存在。是否要覆盖现有配置？",
                            "配置已存在");

                        if (!shouldOverwrite)
                        {
                            return false;
                        }
                    }

                    // 移除旧配置
                    configs.Remove(existingConfig);
                }

                // 添加新配置
                configs.Add(newConfig);

                // 保存到文件
                await SaveToFileAsync(configs);
                return true;
            }
            catch (Exception ex)
            {
                await _messageService.ShowErrorAsync($"保存配置时出错: {ex.Message}");
                return false;
            }
        }

        public bool SaveConfig(BatchExportConfig newConfig, bool overwrite = false)
        {
            return SaveConfigAsync(newConfig, overwrite).Result;
        }

        public async Task<bool> DeleteConfigAsync(string key)
        {
            try
            {
                var configs = await LoadConfigsAsync();
                var configToRemove = configs.FirstOrDefault(c =>
                    string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase));

                if (configToRemove != null)
                {
                    configs.Remove(configToRemove);
                    await SaveToFileAsync(configs);
                    OnConfigsChanged(); // 触发事件
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                await _messageService.ShowErrorAsync($"删除配置时出错: {ex.Message}");
                return false;
            }
        }

        public bool DeleteConfig(string key)
        {
            return DeleteConfigAsync(key).Result;
        }

        public bool IsKeyExists(string key)
        {
            var configs = LoadConfigs();
            return configs.Any(c => string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase));
        }

        private async Task SaveToFileAsync(List<BatchExportConfig> configs)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(configs, options);
            await File.WriteAllTextAsync(ConfigFileName, json);
        }

        private void SaveToFile(List<BatchExportConfig> configs)
        {
            SaveToFileAsync(configs).Wait();
        }

        public event EventHandler? ConfigsChanged;

        protected virtual void OnConfigsChanged()
        {
            ConfigsChanged?.Invoke(this, EventArgs.Empty);
        }

        public void NotifyConfigsChanged()
        {
            OnConfigsChanged();
        }

        public async Task<IEnumerable<TableMapping>> ImportTableMappingsAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                var jsonMappings = JsonSerializer.Deserialize<List<JsonTableMapping>>(json, options);

                if (jsonMappings == null)
                {
                    return Enumerable.Empty<TableMapping>();
                }

                return jsonMappings.DistinctBy(jm => new { jm.source_table, jm.target_table })
                                 .Select(jm => new TableMapping
                {
                    SourceTable = jm.source_table,
                    TargetTable = jm.target_table
                });
            }
            catch (Exception ex)
            {
                await _messageService.ShowErrorAsync($"导入映射配置时出错: {ex.Message}");
                return Enumerable.Empty<TableMapping>();
            }
        }

        public IEnumerable<TableMapping> ImportTableMappings(string filePath)
        {
            return ImportTableMappingsAsync(filePath).Result;
        }
    }
}