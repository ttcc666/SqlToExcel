using SqlToExcel.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace SqlToExcel.Services
{
    public class ConfigService
    {
        private readonly DatabaseService _dbService;
        private static readonly Lazy<ConfigService> _instance = new Lazy<ConfigService>(() => new ConfigService());
        public static ConfigService Instance => _instance.Value;

        private ConfigService()
        {
            _dbService = DatabaseService.Instance;
        }

        public async Task<List<BatchExportConfig>> LoadConfigsAsync()
        {
            try
            {
                var entities = await _dbService.LocalDb.Queryable<BatchExportConfigEntity>().ToListAsync();
                return entities.Select(MapToModel).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"从数据库加载配置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<BatchExportConfig>();
            }
        }

        public async Task<bool> SaveConfigAsync(BatchExportConfig newConfig, bool overwrite = false)
        {
            try
            {
                var existing = await _dbService.LocalDb.Queryable<BatchExportConfigEntity>().InSingleAsync(newConfig.Key);
                if (existing != null)
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
                }

                var entity = MapToEntity(newConfig);
                await _dbService.LocalDb.Storageable(entity).ExecuteCommandAsync();
                OnConfigsChanged();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置到数据库时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteConfigAsync(string key)
        {
            try
            {
                var result = await _dbService.LocalDb.Deleteable<BatchExportConfigEntity>().In(key).ExecuteCommandAsync();
                if (result > 0)
                {
                    OnConfigsChanged();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"从数据库删除配置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> IsKeyExistsAsync(string key)
        {
            return await _dbService.LocalDb.Queryable<BatchExportConfigEntity>().AnyAsync(it => it.Key == key);
        }

        public async Task ExportConfigsToJsonAsync(string filePath)
        {
            try
            {
                var configs = await LoadConfigsAsync();
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(configs, options);
                await File.WriteAllTextAsync(filePath, json);
                MessageBox.Show($"配置已成功导出到: {filePath}", "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出配置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task SaveAllConfigsAsync(IEnumerable<BatchExportConfig> configs)
        {
            try
            {
                var entities = configs.Select(MapToEntity).ToList();

                await _dbService.LocalDb.Ado.UseTranAsync(async () =>
                {
                    await _dbService.LocalDb.Deleteable<BatchExportConfigEntity>().ExecuteCommandAsync();
                    await _dbService.LocalDb.Insertable(entities).ExecuteCommandAsync();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"自动保存配置到数据库时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private BatchExportConfigEntity MapToEntity(BatchExportConfig model)
        {
            return new BatchExportConfigEntity
            {
                Key = model.Key,
                DataSourceJson = JsonSerializer.Serialize(model.DataSource),
                DataTargetJson = JsonSerializer.Serialize(model.DataTarget),
                Destination = model.Destination,
                Prefix = model.Prefix
            };
        }

        private BatchExportConfig MapToModel(BatchExportConfigEntity entity)
        {
            return new BatchExportConfig
            {
                Key = entity.Key,
                DataSource = JsonSerializer.Deserialize<QueryConfig>(entity.DataSourceJson) ?? new QueryConfig(),
                DataTarget = JsonSerializer.Deserialize<QueryConfig>(entity.DataTargetJson) ?? new QueryConfig(),
                Destination = entity.Destination,
                Prefix = entity.Prefix
            };
        }

        public event EventHandler? ConfigsChanged;
        protected virtual void OnConfigsChanged()
        {
            ConfigsChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task<List<TableMapping>> GetTableMappingsAsync()
        {
            return await _dbService.LocalDb.Queryable<TableMapping>().ToListAsync();
        }

        public async Task<bool> SaveTableMappingAsync(TableMapping mapping)
        {
            var result = await _dbService.LocalDb.Insertable(mapping).ExecuteCommandAsync();
            if (result > 0) EventService.Publish(new MappingsChangedEvent());
            return result > 0;
        }

        public async Task<bool> DeleteTableMappingAsync(int id)
        {
            var result = await _dbService.LocalDb.Deleteable<TableMapping>().In(id).ExecuteCommandAsync();
            if (result > 0) EventService.Publish(new MappingsChangedEvent());
            return result > 0;
        }

        public async Task SaveMissingTablesAsync(IEnumerable<string> tableNames)
        {
            var entities = tableNames.Select(name => new MissingTable
            {
                TableName = name,
                ComparisonDate = DateTime.Now
            }).ToList();

            if (entities.Any())
            {
                await _dbService.LocalDb.Insertable(entities).ExecuteCommandAsync();
            }
        }

        public async Task<List<MissingTable>> GetMissingTablesAsync()
        {
            return await _dbService.LocalDb.Queryable<MissingTable>().OrderBy(it => it.ComparisonDate, SqlSugar.OrderByType.Desc).ToListAsync();
        }

        public async Task DeleteMissingTablesAsync(IEnumerable<int> ids)
        {
            await _dbService.LocalDb.Deleteable<MissingTable>().In(ids).ExecuteCommandAsync();
        }
    }
}