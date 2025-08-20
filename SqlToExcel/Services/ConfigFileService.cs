using SqlToExcel.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows;

namespace SqlToExcel.Services
{
    public class ConfigFileService
    {
        private const string ConfigFileName = "batch_export_configs.json";
        private static ConfigFileService? _instance;
        private static readonly object _lock = new object();

        public static ConfigFileService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigFileService();
                        }
                    }
                }
                return _instance;
            }
        }

        private ConfigFileService() { }

        public List<BatchExportConfig> LoadConfigs()
        {
            try
            {
                if (!File.Exists(ConfigFileName))
                {
                    return new List<BatchExportConfig>();
                }

                var json = File.ReadAllText(ConfigFileName);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                };
                return JsonSerializer.Deserialize<List<BatchExportConfig>>(json, options) ?? new List<BatchExportConfig>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置文件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<BatchExportConfig>();
            }
        }

        public bool SaveConfig(BatchExportConfig newConfig, bool overwrite = false)
        {
            try
            {
                var configs = LoadConfigs();
                
                // 检查键是否已存在
                var existingConfig = configs.FirstOrDefault(c => 
                    string.Equals(c.Key, newConfig.Key, StringComparison.OrdinalIgnoreCase));
                
                if (existingConfig != null)
                {
                    if (!overwrite)
                    {
                        var result = MessageBox.Show(
                            $"配置键 '{newConfig.Key}' 已存在。是否要覆盖现有配置？",
                            "配置已存在",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                        
                        if (result != MessageBoxResult.Yes)
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
                SaveToFile(configs);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool DeleteConfig(string key)
        {
            try
            {
                var configs = LoadConfigs();
                var configToRemove = configs.FirstOrDefault(c => 
                    string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase));
                
                if (configToRemove != null)
                {
                    configs.Remove(configToRemove);
                    SaveToFile(configs);
                    OnConfigsChanged(); // 触发事件
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除配置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool IsKeyExists(string key)
        {
            var configs = LoadConfigs();
            return configs.Any(c => string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase));
        }

        private void SaveToFile(List<BatchExportConfig> configs)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var json = JsonSerializer.Serialize(configs, options);
            File.WriteAllText(ConfigFileName, json);
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
    }
}