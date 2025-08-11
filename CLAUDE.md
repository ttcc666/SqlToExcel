# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

To build and run the application, use the following commands from the root directory:

- `dotnet build`: Build the solution.
- `dotnet run --project SqlToExcel`: Run the application.

## Architecture

This is a .NET WPF application designed to connect to two separate SQL databases, execute queries, and export the results into a single Excel file. The application supports both manual query editing and dynamic SQL generation based on table/column selection.

The project follows the MVVM (Model-View-ViewModel) design pattern and utilizes dependency injection, configured in `App.xaml.cs`, to manage services and view models.

- **Views**: Contains the WPF windows and user controls (`.xaml` files) that define the UI.
  - `MainWindow.xaml`: The main application window.
  - `DatabaseConfigView.xaml`: For configuring database connections.
  - `PreviewView.xaml` & `DualPreviewView.xaml`: For previewing query results.
  - `ColumnSelectorView.xaml`: A dialog for selecting columns to generate `SELECT` statements.
  - `ColumnSortView.xaml`: A dialog for defining the `ORDER BY` clause.
  - `BatchExportView.xaml`: A view for managing and executing batch export configurations.

- **ViewModels**: Contains the presentation logic and state for the views.
  - `MainViewModel.cs`: The primary view model, coordinating UI logic, database interactions, previews, and single exports.
  - `BatchExportViewModel.cs`: Manages the logic for the batch export feature, loading and saving configurations from `batch_export_configs.json`.
  - Other view models correspond to their respective views (`DatabaseConfigViewModel`, `PreviewViewModel`, etc.).

- **Services**: Contains the core business logic, decoupled from the UI.
  - `DatabaseService.cs`: Manages database connections for source and target databases using `SqlSugarCore`. It handles fetching schema information (tables, columns) and executing queries.
  - `ExcelExportService.cs`: Handles the logic for exporting `DataTable` objects into formatted Excel files using `EPPlus`.
  - `ThemeService.cs`: Manages dynamic theme switching between light ("Default") and dark ("Dark") modes.

- **Configuration Files**:
  - `table_mappings.json`: Configures default table-to-table mappings between the source and target databases to streamline selection.
  - `batch_export_configs.json`: Stores saved configurations for the batch export feature.

### Key Libraries

- **HandyControl**: A UI library providing modern controls for WPF.
- **SqlSugarCore**: An ORM used for database interaction.
- **EPPlus**: A library for creating and manipulating Excel files.
- **Microsoft.Extensions.DependencyInjection**: Used for setting up dependency injection.

## Theme Switching

The application supports dynamic theme switching, managed by the `ThemeService`. The `MainViewModel` invokes this service to change the theme between "Dark" and "Default" skins. The service updates the application's merged resource dictionaries to apply the new theme globally.
