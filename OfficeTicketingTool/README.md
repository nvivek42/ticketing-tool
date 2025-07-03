# Ticketing Tool Application

A modern WPF-based ticketing system built with .NET and Entity Framework Core.

## Features

- User authentication and role-based access control
- Ticket management (create, read, update, delete)
- Comment system for tickets
- User management (for 
istrators)
- Category management for tickets
- Modern MVVM architecture
- Responsive UI with Material Design

## Prerequisites

- .NET 7.0 or later
- SQL Server (LocalDB is used by default)
- Visual Studio 2022 (recommended) or VS Code

## Getting Started

1. Clone the repository
2. Open the solution in Visual Studio
3. Update the connection string in `App.xaml.cs` if needed
4. Build and run the application

## Database Setup

The application uses Entity Framework Core with code-first migrations. The database will be created automatically when you first run the application with sample data.

## Default Users

- **Admin User**
  - Username: admin
  - Password: admin123
  - Role: Admin

- **Support Agent**
  - Username: agent1
  - Password: agent123
  - Role: SupportAgent

- **Regular User**
  - Username: user1
  - Password: user123
  - Role: User

## Project Structure

```

/
├── Data/               # Data access layer
├── Models/             # Domain models
├── Services/           # Business logic services
├── ViewModels/         # ViewModels for MVVM pattern
├── Views/              # XAML views
└── App.xaml            # Application entry point
```

## Dependencies

- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.Extensions.DependencyInjection

## License

This project is licensed under the MIT License - see the LICENSE file for details.
