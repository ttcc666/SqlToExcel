# GEMINI Project Analysis: SqlToExcel

## Project Overview

This is a .NET 9 WPF application designed for database administrators and developers to easily export data from two different SQL Server databases into a single Excel file. The application provides a user-friendly interface to build and execute SQL queries, preview the results, and export them to separate sheets in an `.xlsx` file.

The project follows the **Model-View-ViewModel (MVVM)** architecture, ensuring a clean separation of concerns. Key technologies used are:

*   **UI Framework:** WPF with the [HandyControl](https://github.com/handyorg/handycontrol) library for a modern look and feel.
*   **Database ORM:** [SqlSugarCore](https://github.com/sqlSugar/SqlSugar) is used for all database interactions, including connecting to source/target SQL Server databases and managing a local SQLite database for configuration storage.
*   **Excel Export:** The [EPPlus](https://github.com/EPPlusSoftware/EPPlus) library handles the creation and formatting of Excel files.
*   **Dependency Injection:** `Microsoft.Extensions.DependencyInjection` is used to manage the lifecycle of services and view models.

The application supports features like:
- Dual database connectivity (source and target).
- Dynamic SQL query generation via a UI.
- Manual SQL query editing.
- Side-by-side data previews.
- Custom sorting for queries.
- Saving and managing batch export configurations.
- A local SQLite database (`config.db`) for storing user configurations.

## Building and Running

### Prerequisites
- .NET 9 SDK

### Build
To build the project, run the following command from the root directory:
```bash
dotnet build
```

### Run
To run the application, use the following command:
```bash
dotnet run --project SqlToExcel
```

## Development Conventions

*   **MVVM Pattern:** The code is strictly organized following the MVVM pattern.
    *   **Views:** Located in the `SqlToExcel/Views` directory. They are responsible for the UI layout and user interactions.
    *   **ViewModels:** Located in the `SqlToExcel/ViewModels` directory. They contain the application logic and expose data to the Views.
    *   **Models:** Located in the `SqlToExcel/Models` directory. They represent the data structures of the application.
*   **Services:** Business logic and external interactions (like database access and Excel export) are encapsulated in services, found in the `SqlToExcel/Services` directory.
*   **Dependency Injection:** Services and ViewModels are registered in `App.xaml.cs` and injected where needed. This promotes loose coupling and testability.
*   **Database Configuration:** Database connection strings are stored in the user's settings and managed through the `DatabaseConfigView`.
*   **Local Configuration Storage:** A local SQLite database (`config.db`) is used to store batch export configurations and table mappings. The `DatabaseService` manages access to this database.
*   **Coding Style:** The code is written in C# and includes Chinese comments. The naming conventions are consistent with .NET standards.