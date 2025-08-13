# Gemini 上下文：SqlToExcel 项目

本文档为 AI 助手提供了关于 `SqlToExcel` 项目的上下文信息，以便进行高效的协作。

## 1. 项目概述

`SqlToExcel` 是一个基于 .NET 9 和 C# 的 Windows Presentation Foundation (WPF) 桌面应用程序。其核心功能是允许用户连接到多种类型的 SQL 数据库，执行 SQL 查询，并将结果方便地导出到 Microsoft Excel 文件中。

项目采用了 MVVM (Model-View-ViewModel) 设计模式，确保了业务逻辑和用户界面的清晰分离。此外，项目非常注重 UI/UX 设计，拥有详细的设计规范和实施计划文档，并使用 `HandyControl` UI 库来构建现代化、美观且用户友好的界面。

## 2. 核心技术栈

- **框架**: .NET 9 / C#
- **UI**: WPF (Windows Presentation Foundation)
- **UI 库**: HandyControl (一个用于 WPF 的开源控件库)
- **数据库/ORM**: SqlSugarCore (一个支持多种数据库的 ORM 框架)
- **Excel 操作**: EPPlus (用于创建和读取 Excel 文件的库)
- **依赖注入**: `Microsoft.Extensions.DependencyInjection`

## 3. 项目结构与架构

项目遵循 MVVM 模式，主要目录结构如下：

- **`/`**: 包含主窗口 (`MainWindow.xaml`)、应用程序入口 (`App.xaml`) 和项目配置文件 (`SqlToExcel.csproj`)。
- **`/Views`**: 存放所有用户界面（XAML 文件），如数据库配置、批量导出、预览等视图。
- **`/ViewModels`**: 包含与 `Views` 对应的视图模型，处理所有 UI 逻辑和数据绑定。
- **`/Services`**: 存放核心业务逻辑服务，例如：
    - `DatabaseService.cs`: 封装了所有与数据库相关的操作（连接、查询、获取元数据等）。
    - `ExcelExportService.cs`: 负责将 `DataTable` 导出为 Excel 文件。
    - `ThemeService.cs`: 管理应用程序的主题（如浅色/深色模式）。
- **`/Models`**: 定义应用程序使用的数据模型和实体。
- **`/Resources`**: 存放应用的资源文件，如样式、图标等。
- **`UI_*.md`**: 包含详细的 UI 设计规范、实施计划和优化摘要，是理解项目 UI/UX 目标的关键。

## 4. 构建与运行

标准的 .NET 命令可用于构建和运行此项目。

- **构建项目**:
  ```shell
  dotnet build
  ```

- **运行项目**:
  ```shell
  dotnet run
  ```

- **发布项目**:
  ```shell
  dotnet publish -c Release
  ```

## 5. 开发与设计约定

- **MVVM 模式**: 严格遵守 MVVM 模式，所有 UI 逻辑都应在 ViewModel 中实现。
- **依赖注入**: 服务通过构造函数注入到 ViewModel 中，由 `App.xaml.cs` 统一配置。
- **UI 设计**:
    - 所有新的 UI 组件都应遵循 `UI_DESIGN_SPECIFICATION.md` 中的规范。
    - 优先使用 `HandyControl` 提供的控件。
    - 颜色、字体、间距等样式应使用在 `Resources/UnifiedStyles.xaml` 中定义的静态资源。
- **数据库兼容性**: `DatabaseService` 使用 `SqlSugarClient`，理论上支持多种数据库。代码应保持数据库无关性。
- **异步编程**: 对于耗时操作（如数据库查询、文件导出），应使用 `async/await` 以避免 UI 线程阻塞。
- **配置文件**: 批量导出等配置存储在根目录的 `batch_export_configs.json` 文件中。

## 6. TODO / 潜在改进

- **添加单元测试/集成测试**: 项目目前缺少测试项目，建议为 `Services` 和 `ViewModels` 添加单元测试。
- **完善错误处理**: 增强数据库连接、SQL 执行和文件操作中的错误处理和用户反馈。
- **国际化与本地化**: 为 UI 文本提供多语言支持。
- **增加数据导入功能**: 在现有导出功能的基础上，增加从 Excel 导入数据到数据库的功能。
