# GEMINI Project Context: SqlToExcel

## Project Overview

This is a .NET 9 WPF application built using the MVVM (Model-View-ViewModel) architecture. Its primary purpose is to connect to two separate SQL databases (a source and a target), allow the user to build and execute `SELECT` queries against them, and then export the results from both queries into a single Excel file, with each result set on its own sheet.

The application features a modern UI using the HandyControl library, with support for light and dark themes.

### Key Technologies:
- **Framework:** .NET 9 (WPF)
- **Architecture:** MVVM
- **UI:** HandyControl
- **Database Access:** SqlSugarCore (an ORM)
- **Excel Export:** EPPlus (listed in `.csproj`, though `README.md` mentions MiniExcel)
- **Dependency Injection:** Microsoft.Extensions.DependencyInjection

## Building and Running

The project can be built and run using standard `dotnet` CLI commands.

1.  **Restore Dependencies:**
    ```bash
    dotnet restore SqlToExcelSolution.sln
    ```

2.  **Build the Project:**
    ```bash
    dotnet build SqlToExcelSolution.sln --configuration Release
    ```

3.  **Run the Application:**
    ```bash
    dotnet run --project SqlToExcel\SqlToExcel.csproj
    ```

## Development Conventions

### Architecture (MVVM)
The code is structured following the MVVM pattern:
- **Views:** Located in `SqlToExcel/Views/`. These are the XAML files that define the UI and bind to ViewModels. Examples: `MainWindow.xaml`, `DatabaseConfigView.xaml`.
- **ViewModels:** Located in `SqlToExcel/ViewModels/`. These classes contain the application's presentation logic and state. The `MainViewModel.cs` is the central ViewModel for the main window, orchestrating most of the application's functionality. `RelayCommand.cs` is used for implementing the `ICommand` interface.
- **Models:** Located in `SqlToExcel/Models/`. These are the data structures representing the application's domain objects. Examples: `TableMapping.cs`, `BatchExportConfig.cs`.
- **Services:** Located in `SqlToExcel/Services/`. These classes encapsulate specific functionalities like database interaction (`DatabaseService.cs`), configuration management (`ConfigService.cs`), and Excel exporting (`ExcelExportService.cs`).

### Dependency Injection
The application uses `Microsoft.Extensions.DependencyInjection` for managing dependencies. The container is configured in `App.xaml.cs`, where services and ViewModels are registered as singletons. The `ServiceProvider` is then used to resolve instances throughout the application.

### Database Configuration
- Database connection strings are not hardcoded. The user is prompted to configure them via the `DatabaseConfigView`.
- The configuration is likely stored locally, as indicated by the `config.db` file in the root directory and the `ConfigFileService.cs`.

### State Management
- The primary application state is managed within the `MainViewModel`.
- It holds observable collections for tables, columns, and sort orders, which the UI automatically updates.
- It exposes `ICommand` properties for all user actions (e.g., `ExportCommand`, `PreviewCommand`).

### Key Files for Understanding the Core Logic
- **`SqlToExcel/App.xaml.cs`**: Application startup, service registration, and main window creation.
- **`SqlToExcel/ViewModels/MainViewModel.cs`**: The most important file for understanding the application's core logic, state management, and user command handling.
- **`SqlToExcel/Services/DatabaseService.cs`**: Handles all interactions with the source and target SQL databases using SqlSugar.
- **`SqlToExcel/Services/ExcelExportService.cs`**: Contains the logic for exporting `DataTable` objects to an Excel file.
- **`SqlToExcel/SqlToExcel.csproj`**: Defines project dependencies and build settings.
