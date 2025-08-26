---
name: "SqlToExcel"
description: "一个基于 WPF 的桌面应用程序，旨在帮助用户从各种数据库中提取数据，执行复杂的 SQL 查询，并将结果导出到 Excel。它支持批量导出配置管理、数据库表与字段选择、数据预览、以及数据库架构和字段类型比较等功能，简化了数据处理和报告生成流程。"
category: "桌面应用"
author: "未知"
authorUrl: ""
tags: ["C#", ".NET", "WPF", "SqlSugarCore", "EPPlus", "HandyControl", "Microsoft.Extensions.DependencyInjection", "System.Text.Json", "SQL"]
lastUpdated: "2025-08-25"
---

# SqlToExcel

## 项目概述

SqlToExcel 是一个功能强大的桌面应用程序，专为数据专业人员和开发者设计，用于简化从关系型数据库到 Excel 的数据提取和转换过程。该工具提供直观的用户界面，允许用户连接到多个数据库实例，执行自定义 SQL 查询，预览查询结果，并将数据高效地导出为 Excel 格式。

核心功能包括批量导出配置的保存和管理，方便重复性任务的执行；灵活的表和字段选择器，支持用户精确控制导出内容；以及数据库架构和字段类型比较工具，辅助数据迁移和同步。通过集成 `SqlSugarCore` 和 `EPPlus` 等库，SqlToExcel 提供了高性能的数据操作和卓越的 Excel 导出能力，极大地提升了数据工作流的效率。

## 技术栈

列出项目使用的主要技术和工具，并简要说明其作用：

- **编程语言**：
    - C#: 项目的主要开发语言，用于构建应用程序的逻辑和界面。
- **框架**：
    - .NET 9.0-windows: 微软的跨平台开发框架，提供构建 Windows 桌面应用程序的基础。
    - WPF (Windows Presentation Foundation): 用于构建富客户端桌面应用程序的 UI 框架，提供了强大的数据绑定、样式和模板功能。
- **库**：
    - SqlSugarCore: 一个高性能的 .NET ORM (对象关系映射) 库，用于简化数据库操作，支持多种数据库类型（如 SQLite 用于本地配置存储，以及其他外部数据库）。
    - EPPlus: 一个用于读写 Excel 文件的 .NET 库，支持生成复杂的 Excel 报告，包括样式、图表等。
    - HandyControl: 一个功能丰富的 WPF UI 控件库，提供了美观且实用的自定义控件，增强了用户界面的体验。
    - Microsoft.Extensions.DependencyInjection: 微软官方的依赖注入库，用于管理应用程序组件的生命周期和依赖关系，提高了代码的可维护性和可测试性。
    - System.Text.Json: .NET 内置的高性能 JSON 序列化和反序列化库，用于配置的存储和读取。
- **数据库**：
    - SQLite: 作为应用程序的本地配置存储数据库，用于保存批量导出配置和表映射等信息。
- **其他工具**：
    - Visual Studio: 用于 C# 和 .NET 项目开发的集成开发环境 (IDE)。

## 项目结构

描述项目的推荐目录结构，并说明各部分的作用：

```
SqlToExcel/
├── WPFVersion/
│   ├── SqlToExcel/
│   │   ├── App.config: 应用程序配置
│   │   ├── App.xaml: 应用程序资源和启动定义
│   │   ├── App.xaml.cs: 应用程序启动逻辑和依赖注入容器配置
│   │   ├── MainWindow.xaml: 主窗口的 UI 定义
│   │   ├── MainWindow.xaml.cs: 主窗口的后端代码，处理 UI 交互
│   │   ├── SqlToExcel.csproj: 项目文件，定义项目结构、依赖和构建设置
│   │   ├── Converters/: WPF 值转换器，用于数据绑定时的格式转换
│   │   ├── Models/: 定义应用程序的数据模型和实体类
│   │   ├── Properties/: 项目属性和资源文件
│   │   ├── Services/: 包含核心业务逻辑服务，如数据库操作、Excel 导出、配置管理等
│   │   ├── ViewModels/: 遵循 MVVM 模式的视图模型，封装视图逻辑和数据
│   │   └── Views/: 包含 WPF 用户界面文件 (.xaml) 及其后端代码 (.xaml.cs)
│   └── SqlToExcelSolution.sln: 解决方案文件，管理项目
├── .gitignore: Git 版本控制忽略文件
└── README.md: 项目说明文档
```

## 开发指南

### 代码风格

- **C# 语言特性**: 项目广泛使用 C# 10 及更高版本的新特性，如 `ImplicitUsings`、`Nullable` 等。
- **MVVM 模式**: 采用 MVVM (Model-View-ViewModel) 设计模式，分离 UI、业务逻辑和数据。
- **依赖注入**: 核心服务和视图模型通过 `Microsoft.Extensions.DependencyInjection` 进行依赖注入，提高模块化和可测试性。
- **命名约定**: 遵循 .NET 命名约定 (PascalCase 用于类名、方法名、属性名；camelCase 用于局部变量和私有字段)。
- **异步编程**: 广泛使用 `async/await` 处理 I/O 密集型操作（如数据库查询、文件导出），避免 UI 阻塞。
- **异常处理**: 通过 `try-catch` 块捕获和处理异常，通常会通过 `MessageBox` 向用户显示错误信息。

### 命名约定

- **文件命名**: 通常与其中包含的类名一致 (如 `MainViewModel.cs` 包含 `MainViewModel` 类)。
- **类命名**: 使用 PascalCase，并带有明确的后缀 (如 `Service`、`ViewModel`、`View`、`Config`、`Entity`)。
- **变量命名**: 私有字段使用 `_` 前缀和 camelCase (如 `_sqlQuery1`)；公共属性使用 PascalCase。
- **函数命名**: 使用 PascalCase，动词开头 (如 `LoadConfigsAsync`, `ExportAsync`)。
- **常量命名**: 如果有，通常使用 PascalCase。

### Git 工作流

- **分支策略**: 未明确指定，但通常推荐使用功能分支 (Feature Branch) 或 Gitflow 工作流。
- **提交信息**: 推荐使用清晰、简洁的提交信息，描述本次提交的目的和内容。

## 环境设置

### 开发要求

- **操作系统**: Windows 10/11
- **.NET SDK**: .NET SDK 9.0 或更高版本。
- **IDE**: Visual Studio 2022 或更高版本 (推荐)。
- **包管理器**: .NET SDK 内置的 NuGet 包管理器。

### 安装步骤

```bash
# 1. 克隆项目
git clone https://github.com/your-repo/SqlToExcel.git
cd SqlToExcel/WPFVersion

# 2. 打开解决方案
# 在 Visual Studio 中打开 SqlToExcelSolution.sln 文件

# 3. 恢复 NuGet 包
# Visual Studio 会自动恢复 NuGet 包。如果未自动恢复，可以在解决方案资源管理器中右键点击解决方案，选择“恢复 NuGet 包”。

# 4. 构建项目
# 在 Visual Studio 中，点击“生成”->“生成解决方案”或按 F6。

# 5. 启动开发服务器 (直接运行应用程序)
# 在 Visual Studio 中，点击“调试”->“开始调试”或按 F5。
```

## 核心功能实现

### 数据查询与导出

核心的数据查询和导出功能由 `ExcelExportService` 和 `DatabaseService` 协同完成。

- `DatabaseService`: 负责管理数据库连接 (`SqlSugarClient`)，执行 SQL 查询，并获取表和列信息。它支持配置多个数据库连接（如 `source`, `target`, `framework`）。
- `ExcelExportService`: 负责执行 SQL 查询获取 `DataTable`，然后使用 `EPPlus` 库将 `DataTable` 写入 Excel 文件。

```csharp
// WPFVersion/SqlToExcel/Services/ExcelExportService.cs 示例
public async Task<bool> ExportToExcelAsync(string sql1, string sheetName1, string sql2, string sheetName2, string dbKey2, string prefix, string suffix)
{
    // Simplified example, actual implementation involves data retrieval and ExcelPackage operations
    try
    {
        DataTable dt1 = await _dbService.GetDataTableFromSqlAsync(sql1, "source");
        DataTable dt2 = await _dbService.GetDataTableFromSqlAsync(sql2, dbKey2);

        using (ExcelPackage package = new ExcelPackage())
        {
            var worksheet1 = package.Workbook.Worksheets.Add(sheetName1);
            worksheet1.Cells["A1"].LoadFromDataTable(dt1, true);

            var worksheet2 = package.Workbook.Worksheets.Add(sheetName2);
            worksheet2.Cells["A1"].LoadFromDataTable(dt2, true);

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"{prefix}{sheetName1.Replace(" (Source)", "")}-{sheetName2.Replace(" (Target)", "")}{suffix}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                FileInfo excelFile = new FileInfo(saveDialog.FileName);
                await package.SaveAsAsync(excelFile);
                MessageBox.Show($"文件已成功导出到: {saveDialog.FileName}", "导出成功", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            return false;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"导出时发生错误: {ex.Message}", "导出失败", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
    }
}

// WPFVersion/SqlToExcel/Services/DatabaseService.cs 示例 (部分)
public async Task<DataTable> GetDataTableFromSqlAsync(string sql, string dbKey)
{
    var client = GetClient(dbKey);
    return await client.Ado.GetDataTableAsync(sql);
}
```

### 配置管理与持久化

应用程序的配置（包括批量导出配置和表映射）通过 `ConfigService` 进行管理，并持久化到本地 SQLite 数据库。

- `ConfigService`: 提供了保存、加载、删除配置以及将配置导出为 JSON 的功能。它使用 `SqlSugarCore` 来操作本地数据库，并使用 `System.Text.Json` 进行复杂对象的序列化和反序列化。
- `BatchExportConfig` 和 `TableMapping`: 定义了配置的数据结构。

```csharp
// WPFVersion/SqlToExcel/Services/ConfigService.cs 示例 (部分)
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

public async Task<List<TableMapping>> GetTableMappingsAsync()
{
    return await _dbService.LocalDb.Queryable<TableMapping>().ToListAsync();
}
```

### UI 交互与数据绑定

UI 逻辑主要通过 MVVM 模式实现，`MainViewModel` 负责大部分主界面的数据和命令。

- `MainViewModel`: 包含用户界面的状态、数据集合 (如 `Tables1`, `Columns1`)、命令 (如 `ExportCommand`, `SaveConfigCommand`) 和业务逻辑。它通过 `INotifyPropertyChanged` 实现数据绑定，确保 UI 随着数据的变化而更新。
- `RelayCommand`: 一个简单的 ICommand 实现，用于将 UI 控件 (如按钮) 绑定到 ViewModel 中的方法。

```csharp
// WPFVersion/SqlToExcel/ViewModels/MainViewModel.cs 示例 (部分)
public class MainViewModel : INotifyPropertyChanged
{
    private string _sqlQuery1 = "";
    public string SqlQuery1 { get => _sqlQuery1; set { _sqlQuery1 = value; OnPropertyChanged(); CommandManager.RequerySuggested += (s, e) => { }; } }

    private string _sheetName1 = "SourceData";
    public string SheetName1 { get => _sheetName1; set { _sheetName1 = value; OnPropertyChanged(); } }

    public ICommand ExportCommand { get; }
    public ICommand SaveConfigCommand { get; }
    public ICommand SwitchThemeCommand { get; }

    private readonly ExcelExportService _exportService;
    private readonly ThemeService _themeService;

    public MainViewModel(ExcelExportService exportService, ThemeService themeService)
    {
        _exportService = exportService;
        _themeService = themeService;
        ExportCommand = new RelayCommand(async p => await ExportAsync(), p => IsCoreFunctionalityEnabled && !string.IsNullOrWhiteSpace(SqlQuery1) && !string.IsNullOrWhiteSpace(SqlQuery2));
        SaveConfigCommand = new RelayCommand(async p => await SaveConfigAsync(), p => CanSaveConfig());
        SwitchThemeCommand = new RelayCommand(p => SwitchTheme());
        // ... 其他命令和初始化 ...
    }

    private void SwitchTheme()
    {
        _isDarkTheme = !_isDarkTheme;
        _themeService.ChangeTheme(_isDarkTheme ? "Dark" : "Default");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

## 测试策略

通过文件列表，没有发现专门的测试项目或测试文件 (例如 `Tests/` 目录或 `*.Tests.csproj`)。这表明项目可能没有采用自动化单元测试或集成测试框架。

- **手动测试**: 核心功能可能主要通过手动方式进行测试，即通过运行应用程序并进行 UI 操作来验证功能。
- **集成测试**: 数据库连接、SQL 查询执行、Excel 导出等功能可能在应用程序运行时进行集成测试。

如果后续需要添加自动化测试，可以考虑以下框架：

- **单元测试**: MSTest, NUnit, xUnit.net
- **UI 自动化测试**: Coded UI Tests, Appium (对于桌面应用)

## 部署指南

### 构建过程

项目是一个标准的 .NET WPF 应用程序，使用 `dotnet build` 命令或 Visual Studio 进行构建。

```bash
# 在 WPFVersion/SqlToExcel/ 目录下执行
dotnet build -c Release
```

构建成功后，可执行文件 (SqlToExcel.exe) 将位于 `WPFVersion/SqlToExcel/bin/Release/net9.0-windows/` 目录下。

### 部署步骤

1.  **准备生产环境**:
    - 确保目标机器安装了 .NET 9.0 Desktop Runtime。
2.  **打包应用程序**:
    - 可以将 `bin/Release/net9.0-windows/` 目录下的所有文件打包成 ZIP 文件，或者使用安装程序 (如 WiX Toolset, Inno Setup) 创建 MSI 安装包。
3.  **分发**:
    - 将打包好的应用程序分发给用户。

### 环境变量

目前项目代码中没有发现明确的环境变量配置。数据库连接字符串等敏感信息是通过应用程序内部配置或用户界面输入进行管理。

## 性能优化

### 前端优化 (WPF UI)

- **数据虚拟化**: 对于大量数据的列表显示，WPF 提供了数据虚拟化技术 (如 `VirtualizingStackPanel`)，可以只渲染可见区域的 UI 元素，提高滚动性能。
- **UI 线程不阻塞**: 通过 `async/await` 模式将耗时操作 (如数据库查询、文件导出) 放在后台线程执行，避免阻塞 UI 线程，保持界面的响应性。
- **资源优化**: 优化 XAML 资源字典的使用，避免不必要的资源加载和查找开销。

### 后端优化 (数据操作)

- **数据库查询优化**:
    - `SqlSugarCore` 提供了强大的 ORM 功能，应合理利用其查询优化能力，如使用 `Where`, `OrderBy`, `Select` 等方法构建高效的查询。
    - 确保 SQL 查询语句是优化的，避免全表扫描，合理使用索引。
    - `MainViewModel` 中对大数据量查询使用了 `TOP {MaxRowCount}` 限制，避免一次性加载过多数据到内存。
- **Excel 导出优化**:
    - `EPPlus` 库本身性能较高，但对于超大数据量的导出，可能需要考虑分批写入或流式写入。
- **缓存策略**:
    - `DatabaseService` 和 `ConfigService` 使用了单例模式 (`Lazy<T>`)，确保服务实例的唯一性，减少资源消耗。
    - 数据库表和列信息的加载可能存在缓存机制，避免频繁查询元数据。

## 安全考虑

### 数据安全

- **输入验证**: 应用程序可能需要对用户输入的 SQL 查询进行验证，以防止 SQL 注入攻击。尽管当前代码中没有直接看到显式的 SQL 注入防护措施，但 `SqlSugarCore` 作为 ORM 框架，通常会提供参数化查询来自动防止这类攻击。
- **敏感数据处理**: 数据库连接字符串等敏感信息应妥善存储，避免硬编码或明文存储在代码中。目前看来，连接字符串是通过用户配置界面输入并可能存储在本地 `config.db` 中。对于生产环境，应考虑更安全的存储方式。

### 认证与授权

该应用程序似乎是一个单机桌面工具，没有涉及用户认证和授权机制。如果需要连接到具有认证要求的数据库，认证信息将由数据库本身处理，并在连接字符串中提供。

## 监控和日志

### 应用监控

- **错误追踪**: 应用程序通过 `MessageBox.Show` 来显示错误信息，这对于用户交互是直接的，但对于开发者进行问题诊断和监控则不够。可以考虑集成日志框架 (如 NLog, Serilog) 来记录应用程序的运行时错误和异常，以便后续分析。
- **性能指标监控**: 目前没有看到显式的性能监控机制。

### 日志管理

- **日志级别定义**: 建议引入日志级别 (如 DEBUG, INFO, WARN, ERROR) 来区分不同重要性的日志信息。
- **日志格式规范**: 统一日志输出格式，包含时间戳、日志级别、消息内容、来源等。
- **日志存储策略**: 可以将日志写入文件、数据库或发送到远程日志服务。

## 常见问题

### 问题 1: 应用程序启动时提示“数据库未配置”或“数据库初始化失败”。

**解决方案**:
1.  **检查权限**: 确保应用程序有权在 `config.db` 所在的目录下创建和修改文件。
2.  **重新配置数据库**: 在应用程序中，通过“文件”菜单打开数据库配置界面，重新配置源数据库和目标数据库的连接信息。
3.  **重启应用程序**: 有时重启应用程序可以解决临时的数据库连接问题。

### 问题 2: 导出 Excel 文件时报错或导出内容不正确。

**解决方案**:
1.  **检查 SQL 查询**: 确认输入的 SQL 查询语句语法正确，并且能够从数据库中返回预期的数据。可以在数据库客户端工具中测试 SQL。
2.  **检查表和列选择**: 确保选择了正确的源表和目标表，并且选定的列在查询结果中存在。
3.  **文件权限**: 确保应用程序有权在目标路径创建和写入 Excel 文件。
4.  **数据类型兼容性**: 检查查询结果中的数据类型与 Excel 导出的兼容性，特别是日期和数字格式。

## 参考资源

- [.NET 文档](https://docs.microsoft.com/en-us/dotnet/)
- [WPF 文档](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [SqlSugarCore GitHub 仓库](https://github.com/sunkaixuan/SqlSugar)
- [EPPlus GitHub 仓库](https://github.com/EPPlusSoftware/EPPlus)
- [HandyControl GitHub 仓库](https://github.com/HandyOrg/HandyControl)

## 更新日志

### v1.0.0 (2025-08-25)

- 初始版本发布
- 实现基本功能：数据库连接、SQL 查询执行、数据导出到 Excel
- 支持批量导出配置的保存、加载和管理
- 提供表和字段选择器，辅助 SQL 生成
- 集成主题切换功能