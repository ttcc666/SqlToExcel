# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

To build and run the application, use the following commands from the root directory:

- `dotnet build`: Build the solution.
- `dotnet run --project SqlToExcel`: Run the application.

## Architecture

This is a .NET WPF application that allows users to connect to a SQL database, run a query, and export the results to an Excel file.

The project follows the MVVM (Model-View-ViewModel) design pattern:

- **Views**: Contains the WPF windows and user controls (`.xaml` files).
  - `MainWindow.xaml`: The main application window.
  - `DatabaseConfigView.xaml`: A view for configuring the database connection.
  - `PreviewView.xaml`: A view for previewing a single query result.
  - `DualPreviewView.xaml`: A view for previewing the results of both queries simultaneously.
  - `ColumnSelectorView.xaml`: A dialog for selecting columns from a table.
  - `ColumnSortView.xaml`: A dialog for defining the sort order of columns.
- **ViewModels**: Contains the logic for the views.
  - `MainViewModel.cs`: The view model for the main window.
  - `DatabaseConfigViewModel.cs`: The view model for the database configuration view.
  - `PreviewViewModel.cs`: The view model for the single query preview.
  - `DualPreviewViewModel.cs`: The view model for the dual query preview.
  - `ColumnSelectorViewModel.cs`: The view model for the column selection dialog.
  - `ColumnSortViewModel.cs`: The view model for the column sorting dialog.
  - `RelayCommand.cs`: A generic `ICommand` implementation.
- **Services**: Contains the business logic.
  - `DatabaseService.cs`: Handles database connections and queries using `SqlSugarCore`.
  - `ExcelExportService.cs`: Handles exporting data to Excel using `MiniExcel`.

Key Libraries:
- **HandyControl**: A UI library for WPF.
- **SqlSugarCore**: Used as an ORM to interact with the database.
- **MiniExcel**: Used to generate Excel files from the query results.
- **Microsoft.Data.SqlClient**: The ADO.NET provider for SQL Server.

## Theme Switching
The application uses a custom theme switching mechanism in `App.xaml.cs`. The `UpdateTheme` method takes a skin name ("Dark" or "Default") and switches the application's theme by replacing the skin's `ResourceDictionary` in the application's merged dictionaries.
