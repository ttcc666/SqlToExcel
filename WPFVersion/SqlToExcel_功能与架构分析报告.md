# SqlToExcel 应用功能与架构分析报告

本应用是一个功能强大的数据处理与迁移工具，主要围绕数据库的查询、导出、对比和管理等核心需求构建。以下是其主要功能模块和技术架构的详细分析。

---

## 第一部分：UI界面功能分析

### 1. 主窗口 (`MainWindow.xaml`)

主窗口是应用的入口和核心操作区，采用经典的菜单栏+标签页布局，集成了应用的所有核心功能。

#### 功能概述
主窗口提供了一个统一的工作区，用户可以通过顶部的菜单栏进行全局配置（如数据库连接、主题切换），并通过标签页在不同的功能模块间切换，包括手动数据导出、批量任务管理、表结构映射和数据/结构对比等。

#### 主要控件与事件
*   **菜单栏 (Menu)**:
    *   **文件 (_File)**:
        *   `数据库配置`: 打开数据库连接配置窗口 (`DatabaseConfigView`)。
        *   `切换主题`: 切换应用界面的亮/暗主题。
        *   `退出`: 关闭应用程序。
    *   **工具 (_Tools)**:
        *   `字段类型提取`: 打开一个工具，用于从C#实体类代码中提取字段及其类型 (`FieldTypeExtractorView`)。
        *   `字段对比`: 打开字段对比工具 (`FieldComparisonView`)。
        *   `Target表信息比对`: 打开表结构对比工具 (`TableComparisonView`)。
*   **标签页 (TabControl)**:
    *   **手动导出 (Manual Export)**: 这是默认打开的核心功能区。
        *   **查询1 (源)** 和 **查询2 (目标)**: 左右两个区域，分别对应源数据库和目标数据库的查询配置。
        *   `选择表`: 下拉框，列出数据库中的所有表，支持搜索，选择后可快速生成查询语句。
        *   `选择列`: 按钮，打开列选择器 (`ColumnSelectorView`)，用于定制查询的字段。
        *   `排序`: 按钮，打开排序设置对话框 (`ColumnSortView`)。
        *   `从JSON导入`: 按钮，打开JSON导入功能 (`ImportJsonDialog`)，可将JSON数据作为查询源。
        *   `SQL查询框`: 手动编写或修改SQL查询语句。
        *   `Sheet名称`: 指定导出到Excel时对应工作表的名称。
        *   `预览`: 按钮，在新窗口 (`PreviewView`) 中预览当前查询的返回结果。
        *   `同时预览`: 按钮，同时预览两个查询的结果 (`DualPreviewView`)。
        *   `导出到Excel`: 按钮，将两个查询的结果分别导出到同一个Excel文件的不同Sheet中。
        *   `保存为批量配置`: 按钮，将当前的手动导出设置保存为一个批量任务 (`SaveConfigDialog`)。
    *   **批量导出 (Batch Export)**: 嵌入 `BatchExportView`，用于管理和执行预设的批量导出任务。
    *   **表映射配置 (Table Mapping Config)**: 嵌入 `TableMappingView`，用于配置源表到目标表的映射关系。
    *   **主键/索引对比 (Primary Key/Index Comparison)**: 嵌入 `SchemaComparisonView`，用于对比两个数据库的表结构差异。
    *   **对比报表 (Comparison Report)**: 嵌入 `ComparisonReportView`，用于展示和管理数据对比后生成的报告。
*   **状态栏 (StatusBar)**: 位于窗口底部，用于显示操作过程中的状态信息，如“连接成功”、“导出完成”等。

#### 核心逻辑
应用启动时，加载默认的数据库配置和主题。用户在“手动导出”页面配置好查询后，可以进行预览、导出或保存为批量任务。其他标签页则提供了数据管理、迁移和校验的辅助功能，模块之间相互独立但又可通过数据（如数据库配置）进行关联。

---

### 2. 视图模块分析 (`/Views/`)

以下是对 `Views` 文件夹下所有界面的逐一分析。

#### 主要功能视图
*   **`BatchExportView.xaml`**: **批量导出管理界面**。它允许用户查看、执行、编辑和删除所有已保存的导出配置。核心是通过 `DataGrid` 展示任务列表，并提供按钮来启动整个批量流程。
*   **`DatabaseConfigView.xaml`**: **数据库连接配置界面**。在此配置源数据库和目标数据库的连接信息。配置将被加密保存在本地，供整个应用使用。
*   **`SchemaComparisonView.xaml`**: **数据库主键/索引对比界面**。用于比较源数据库和目标数据库之间表结构（主要是主键和索引）的差异，并将结果结构化地展示出来。
*   **`TableMappingView.xaml`**: **表映射配置界面**。用于定义数据迁移或对比时，源表与目标表之间的对应关系，支持自动映射和手动配置。
*   **`ComparisonReportView.xaml`**: **数据对比报告查看界面**。作为报告管理中心，列出历史生成的数据对比报告，方便用户随时回顾和分析。
*   **`TableComparisonView.xaml`**: **目标表信息对比工具**。提供更细粒度的、针对单个表的字段级比对功能。
*   **`FieldComparisonView.xaml`**: **独立的字段对比工具**。允许用户直接粘贴两段文本（如字段列表）进行快速比较，不依赖数据库连接。
*   **`FieldTypeExtractorView.xaml`**: **字段类型提取工具**。能从C#的实体类代码中解析出所有公共属性（字段）及其数据类型，方便开发者快速获取模型结构。

#### 对话框与辅助视图
这些视图通常作为主流程中的弹出窗口，用于完成特定子任务。

*   **`ColumnSelectorView.xaml`**: **列选择器**。在手动导出时，点击“选择列”弹出，以复选框列表的形式让用户选择需要查询的字段。
*   **`ColumnSortView.xaml`**: **排序设置对话框**。点击“排序”时弹出，允许用户添加、删除和调整用于 `ORDER BY` 子句的排序列及其排序方式（升序/降序）。
*   **`PreviewView.xaml` / `DualPreviewView.xaml`**: **预览窗口**。分别用于显示单个或两个查询的前N条结果，使用 `DataGrid` 展示，帮助用户在导出前验证SQL的正确性。
*   **`SaveConfigDialog.xaml`**: **保存配置对话框**。当用户点击“保存为批量配置”时弹出，要求用户为该配置输入一个唯一的名称。
*   **`BatchExportProgressDialog.xaml`**: **批量导出进度对话框**。执行批量导出时显示，包含一个进度条和日志文本框，实时反馈每个任务的执行状态。
*   **`EditBatchSqlDialog.xaml`**: **编辑批量SQL对话框**。在批量导出界面点击“编辑”时弹出，允许修改已保存任务的SQL语句和Sheet名。
*   **`ImportJsonDialog.xaml`**: **JSON导入对话框**。允许用户选择一个JSON文件，并配置如何将其解析为表格数据。
*   **`JsonImportPreviewDialog.xaml`**: **JSON导入预览对话框**。在选择JSON文件后，预览解析成的表格数据，确认无误后才能导入。
*   **`ValidationResultView.xaml`**: **验证结果视图**。用于显示某些操作（如配置校验）的结果列表，每一项包含成功或失败的状态和消息。

---

## 第二部分：应用架构与核心逻辑

### 4. 应用架构与设计模式

通过对项目文件结构的分析，可以清晰地看出该应用采用了经典的 **MVVM (Model-View-ViewModel)** 设计模式。这是一个在WPF应用开发中非常成熟和推荐的模式，其核心思想是将用户界面（View）、业务逻辑与状态（ViewModel）以及数据（Model）分离开来，以实现更好的代码组织、可测试性和可维护性。

*   **Model (模型)**: 位于 `SqlToExcel/Models/` 目录。
    *   **职责**: 定义应用的核心数据结构。这些是纯粹的C#类（POCOs），仅包含属性，用于承载数据。
    *   **示例**: `BatchExportConfig.cs` 定义了批量导出任务的数据结构；`TableMapping.cs` 定义了表映射关系的数据结构。

*   **View (视图)**: 位于 `SqlToExcel/Views/` 目录和 `MainWindow.xaml`。
    *   **职责**: 负责用户界面的呈现。它包含所有的UI元素，并通过数据绑定与ViewModel进行交互，自身不包含业务逻辑。
    *   **示例**: `DatabaseConfigView.xaml` 负责展示数据库配置的输入框和按钮。

*   **ViewModel (视图模型)**: 位于 `SqlToExcel/ViewModels/` 目录。
    *   **职责**: 作为View和Model之间的桥梁，负责处理应用的业务逻辑和UI状态。它向View暴露属性和命令（Commands）。
    *   **示例**: `MainViewModel.cs` 包含了主窗口几乎所有的逻辑，如执行导出、预览等命令。

### 5. 核心服务层 (`/Services/`)

为了进一步解耦，项目将通用逻辑抽象到了服务层。ViewModel不直接进行文件或数据库操作，而是调用服务来完成。

*   **`ConfigService.cs` / `ConfigFileService.cs`**: 负责所有配置文件的读取和写入，实现配置管理的逻辑集中化。
*   **`DatabaseService.cs`**: 封装了所有与数据库的交互。它使用 `SqlSugar` 等库来简化跨数据库类型的操作。
*   **`ExcelExportService.cs`**: 封装了将数据写入Excel文件的所有逻辑，使用 `EPPlus` 库来创建和格式化Excel。
*   **`EventService.cs`**: 一个事件聚合器（消息总线），允许不同ViewModel之间进行松耦合的通信。
*   **`ThemeService.cs`**: 负责管理和切换应用的主题（亮色/暗色模式）。

### 6. 整体数据流示例（手动导出）

1.  **用户操作**: 用户在 `MainWindow.xaml` 点击“导出到 Excel”按钮。
2.  **View -> ViewModel**: 点击操作触发 `MainViewModel` 中的 `ExportCommand`。
3.  **ViewModel 处理逻辑**: `ExportCommand` 从自身属性中收集SQL查询等参数。
4.  **ViewModel -> Service**: `MainViewModel` 调用 `DatabaseService` 来执行查询，然后调用 `ExcelExportService` 并将查询结果传递给它。
5.  **Service 执行任务**: `DatabaseService` 返回数据，`ExcelExportService` 创建Excel文件并填充数据。
6.  **反馈给用户**: `MainViewModel` 更新 `StatusMessage` 属性，通过数据绑定在UI状态栏上显示“导出成功”等信息。

---

### 总结

该项目是一个结构清晰、功能完备的WPF应用程序。它成功地运用了MVVM设计模式，将UI、逻辑和数据清晰地分离开来。通过引入服务层和事件总线，进一步降低了模块间的耦合度，使得整个系统易于扩展和维护。
