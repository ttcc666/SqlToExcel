# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

To build and run the application, use the following commands from the root directory:

- `dotnet build`: Build the solution.
- `dotnet run --project SqlToExcel`: Run the application.

## Architecture

This is a .NET WPF application designed to connect to two separate SQL databases, execute queries against them, and export the results into a single Excel file. The application allows for both automatic SQL generation and manual query editing.

The project follows the MVVM (Model-View-ViewModel) design pattern:

- **Views**: Contains the WPF windows and user controls (`.xaml` files).
  - `MainWindow.xaml`: The main application window, hosting the dual query views.
  - `DatabaseConfigView.xaml`: A view for configuring the source and target database connections.
  - `PreviewView.xaml`: A view for previewing a single query result.
  - `DualPreviewView.xaml`: A view for previewing the results of both queries side-by-side.
  - `ColumnSelectorView.xaml`: A dialog for selecting tables and columns to dynamically generate `SELECT` statements.
  - `ColumnSortView.xaml`: A dialog for defining the `ORDER BY` clause for a query.
- **ViewModels**: Contains the presentation logic for the views.
  - `MainViewModel.cs`: The main view model, managing the two database contexts and coordinating the UI.
  - `DatabaseConfigViewModel.cs`: The view model for the database configuration view.
  - `PreviewViewModel.cs`: The view model for the single query preview.
  - `DualPreviewViewModel.cs`: The view model for the dual query preview.
  - `ColumnSelectorViewModel.cs`: The view model for the column selection dialog.
  - `ColumnSortViewModel.cs`: The view model for the column sorting dialog.
  - `RelayCommand.cs`: A generic `ICommand` implementation for MVVM.
- **Services**: Contains the core business logic.
  - `DatabaseService.cs`: Handles database connections and executing queries using `SqlSugarCore`.
  - `ExcelExportService.cs`: Handles exporting data from both queries into a single Excel file using `MiniExcel`.

### Key Libraries

- **HandyControl**: A UI library providing modern controls for WPF.
- **SqlSugarCore**: Used as an ORM to interact with the SQL databases.
- **MiniExcel**: Used to generate Excel files from the query results.
- **Microsoft.Data.SqlClient**: The ADO.NET provider for SQL Server.

## Theme Switching

The application uses a custom theme switching mechanism located in `App.xaml.cs`. The `UpdateTheme` method takes a skin name ("Dark" or "Default") and switches the application's theme by replacing the skin's `ResourceDictionary` in the application's merged dictionaries. This allows for dynamic changing between light and dark modes.
