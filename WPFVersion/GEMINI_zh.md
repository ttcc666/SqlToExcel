# GEMINI 项目分析: SqlToExcel

## 项目概述

这是一个 .NET 9 WPF 应用程序，旨在帮助数据库管理员和开发人员轻松地将来自两个不同 SQL Server 数据库的数据导出到单个 Excel 文件中。该应用程序提供了一个用户友好的界面来构建和执行 SQL 查询、预览结果，并将它们导出到 `.xlsx` 文件中的不同工作表。

该项目遵循 **Model-View-ViewModel (MVVM)** 架构，确保了关注点的清晰分离。使用的关键技术包括：

*   **UI 框架:** WPF 结合 [HandyControl](https://github.com/handyorg/handycontrol) 库，提供现代化的外观和感觉。
*   **数据库 ORM:** [SqlSugarCore](https://github.com/sqlSugar/SqlSugar) 用于所有数据库交互，包括连接到源/目标 SQL Server 数据库以及管理用于配置存储的本地 SQLite 数据库。
*   **Excel 导出:** [EPPlus](https://github.com/EPPlusSoftware/EPPlus) 库处理 Excel 文件的创建和格式化。
*   **依赖注入:** `Microsoft.Extensions.DependencyInjection` 用于管理服务和视图模型的生命周期。

该应用程序支持以下功能：
- 双数据库连接（源和目标）。
- 通过 UI 动态生成 SQL 查询。
- 手动编辑 SQL 查询。
- 并排数据显示预览。
- 查询的自定义排序。
- 保存和管理批量导出配置。
- 用于存储用户配置的本地 SQLite 数据库 (`config.db`)。

## 构建和运行

### 先决条件
- .NET 9 SDK

### 构建
要构建项目，请从根目录运行以下命令：
```bash
dotnet build
```

### 运行
要运行应用程序，请使用以下命令：
```bash
dotnet run --project SqlToExcel
```

## 开发约定

*   **MVVM 模式:** 代码严格按照 MVVM 模式组织。
    *   **Views:** 位于 `SqlToExcel/Views` 目录中。它们负责 UI 布局和用户交互。
    *   **ViewModels:** 位于 `SqlToExcel/ViewModels` 目录中。它们包含应用程序逻辑并向视图公开数据。
    *   **Models:** 位于 `SqlToExcel/Models` 目录中。它们代表应用程序的数据结构。
*   **服务:** 业务逻辑和外部交互（如数据库访问和 Excel 导出）被封装在服务中，位于 `SqlToExcel/Services` 目录中。
*   **依赖注入:** 服务和 ViewModel 在 `App.xaml.cs` 中注册，并在需要时注入。这促进了松散耦合和可测试性。
*   **数据库配置:** 数据库连接字符串存储在用户设置中，并通过 `DatabaseConfigView` 进行管理。
*   **本地配置存储:** 本地 SQLite 数据库 (`config.db`) 用于存储批量导出配置和表映射。`DatabaseService` 管理对该数据库的访问。
*   **编码风格:** 代码使用 C# 编写，并包含中文注释。命名约定符合 .NET 标准。
