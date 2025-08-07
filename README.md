# SqlToExcel

A .NET WPF application designed to connect to two separate SQL databases, execute queries against them, and export the results into a single Excel file.

## Key Features

- **Dual Database Connectivity**: Connect to a source and a target database simultaneously.
- **Dynamic SQL Generation**: Automatically generate SQL `SELECT` statements by choosing tables and columns from a searchable list.
- **Manual Query Editing**: Manually write or modify SQL queries in the provided text editors.
- **Data Preview**: Preview query results for each database side-by-side in a dual-pane window.
- **Custom Sorting**: Define the `ORDER BY` clause for your queries by selecting columns and sort direction.
- **Excel Export**: Export the results from both queries into a single `.xlsx` file, with each result set in its own sheet.
- **Modern UI**: A clean, modern user interface built with HandyControl, featuring switchable Light and Dark themes.

## Technology Stack

- **Framework**: .NET 9 (WPF)
- **Architecture**: Model-View-ViewModel (MVVM)
- **UI Library**: [HandyControl](https://github.com/handyorg/handycontrol)
- **ORM**: [SqlSugarCore](https://github.com/sqlSugar/SqlSugar)
- **Excel Export**: [MiniExcel](https://github.com/mini-software/MiniExcel)

## Getting Started

1.  **Configure Database Connections**:
    -   Launch the application.
    -   From the "文件" (File) menu, select "数据库配置" (Database Configuration).
    -   Enter the connection strings for both the source and target databases and save.

2.  **Build the Project**:
    ```bash
    dotnet build
    ```

3.  **Run the Application**:
    ```bash
    dotnet run --project SqlToExcel
    ```
