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
    public class ConfigService
    {
        private readonly DatabaseService _dbService;
        private readonly IMessageService _messageService;

        public ConfigService(DatabaseService dbService, IMessageService messageService)
        {
            _dbService = dbService;
            _messageService = messageService;
        }

        public async Task<List<BatchExportConfig>> LoadConfigsAsync()
        {
            try
            {
                if (_dbService.LocalDb == null)
                {
                    await _messageService.ShowErrorAsync("数据库未初始化，无法加载配置");
                    return new List<BatchExportConfig>();
                }

                var entities = await _dbService.LocalDb.Queryable<BatchExportConfigEntity>().ToListAsync();
                return entities.Select(MapToModel).ToList();
            }
            catch (Exception ex)
            {
                await _messageService.ShowErrorAsync($"从数据库加载配置时出错: {ex.Message}");
                return new List<BatchExportConfig>();
            }
        }

        public async Task<bool> SaveConfigAsync(BatchExportConfig newConfig, bool overwrite = false)
        {
            try
            {
                if (_dbService.LocalDb == null)
                {
                    await _messageService.ShowErrorAsync("数据库未初始化，无法保存配置");
                    return false;
                }

                var existing = await _dbService.LocalDb.Queryable<BatchExportConfigEntity>().InSingleAsync(newConfig.Key);
                if (existing != null)
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
                }

                var entity = MapToEntity(newConfig);
                await _dbService.LocalDb.Storageable(entity).ExecuteCommandAsync();
                OnConfigsChanged();
                return true;
            }
            catch (Exception ex)
            {
                await _messageService.ShowErrorAsync($"保存配置到数据库时出错: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteConfigAsync(string key)
        {
            try
            {
                if (_dbService.LocalDb == null)
                {
                    await _messageService.ShowErrorAsync("数据库未初始化，无法删除配置");
                    return false;
                }

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
                await _messageService.ShowErrorAsync($"从数据库删除配置时出错: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsKeyExistsAsync(string key)
        {
            if (_dbService.LocalDb == null)
            {
                return false;
            }
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
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                var json = JsonSerializer.Serialize(configs, options);
                await File.WriteAllTextAsync(filePath, json);
                await _messageService.ShowInformationAsync($"配置已成功导出到: {filePath}", "导出成功");
            }
            catch (Exception ex)
            {
                await _messageService.ShowErrorAsync($"导出配置时出错: {ex.Message}");
            }
        }

        public async Task SaveAllConfigsAsync(IEnumerable<BatchExportConfig> configs)
        {
            try
            {
                var entities = configs.Select(MapToEntity).ToList();

                if (_dbService.LocalDb != null)
                {
                    await _dbService.LocalDb.Ado.UseTranAsync(async () =>
                    {
                        await _dbService.LocalDb.Deleteable<BatchExportConfigEntity>().ExecuteCommandAsync();
                        await _dbService.LocalDb.Insertable(entities).ExecuteCommandAsync();
                    });
                }
            }
            catch (Exception ex)
            {
                await _messageService.ShowErrorAsync($"自动保存配置到数据库时出错: {ex.Message}");
            }
        }

        private BatchExportConfigEntity MapToEntity(BatchExportConfig model)
        {
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return new BatchExportConfigEntity
            {
                Key = model.Key,
                DataSourceJson = JsonSerializer.Serialize(model.DataSource, options),
                DataTargetJson = JsonSerializer.Serialize(model.DataTarget, options),
                Destination = model.Destination,
                Prefix = model.Prefix
            };
        }

        private BatchExportConfig MapToModel(BatchExportConfigEntity entity)
        {
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            return new BatchExportConfig
            {
                Key = entity.Key,
                DataSource = JsonSerializer.Deserialize<QueryConfig>(entity.DataSourceJson, options) ?? new QueryConfig(),
                DataTarget = JsonSerializer.Deserialize<QueryConfig>(entity.DataTargetJson, options) ?? new QueryConfig(),
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
            if (_dbService.LocalDb == null) return new List<TableMapping>();
            return await _dbService.LocalDb.Queryable<TableMapping>().ToListAsync();
        }

        public async Task<bool> SaveTableMappingAsync(TableMapping mapping)
        {
            if (_dbService.LocalDb == null) return false;
            var result = await _dbService.LocalDb.Insertable(mapping).ExecuteCommandAsync();
            if (result > 0) EventService.Publish(new MappingsChangedEvent());
            return result > 0;
        }

        public async Task<bool> DeleteTableMappingAsync(int id)
        {
            if (_dbService.LocalDb == null) return false;
            var result = await _dbService.LocalDb.Deleteable<TableMapping>().In(id).ExecuteCommandAsync();
            if (result > 0) EventService.Publish(new MappingsChangedEvent());
            return result > 0;
        }

        public async Task SaveAllTableMappingsAsync(IEnumerable<TableMapping> mappings)
        {
            try
            {
                if (_dbService.LocalDb != null)
                {
                    await _dbService.LocalDb.Ado.UseTranAsync(async () =>
                    {
                        await _dbService.LocalDb.Deleteable<TableMapping>().ExecuteCommandAsync(); // Delete all
                        await _dbService.LocalDb.Insertable(mappings.ToList()).ExecuteCommandAsync();
                    });
                    EventService.Publish(new MappingsChangedEvent());
                }
            }
            catch (Exception ex)
            {
                await _messageService.ShowErrorAsync($"保存所有表映射时出错: {ex.Message}");
            }
        }

        public async Task SaveMissingTablesAsync(IEnumerable<string> tableNames)
        {
            var entities = tableNames.Select(name => new MissingTable
            {
                TableName = name,
                ComparisonDate = DateTime.Now
            }).ToList();

            if (entities.Any() && _dbService.LocalDb != null)
            {
                await _dbService.LocalDb.Insertable(entities).ExecuteCommandAsync();
            }
        }

        public async Task<List<MissingTable>> GetMissingTablesAsync()
        {
            if (_dbService.LocalDb == null) return new List<MissingTable>();
            return await _dbService.LocalDb.Queryable<MissingTable>().OrderBy(it => it.ComparisonDate, SqlSugar.OrderByType.Desc).ToListAsync();
        }

        public async Task DeleteMissingTablesAsync(IEnumerable<int> ids)
        {
            if (_dbService.LocalDb != null)
            {
                await _dbService.LocalDb.Deleteable<MissingTable>().In(ids).ExecuteCommandAsync();
            }
        }
    }
}