# Exuberant Pathfinders Web Application

A robust web application built with ASP.NET Core 8, Entity Framework Core, and MySQL. This project serves as a foundation for managing users, roles, applications, and donations, and includes a powerful, automated database schema documentation system.

## ✨ Features

- **Complete Identity Management**: Full user lifecycle including registration with email confirmation, login, password change/reset, and profile management.
- **Role-Based Access Control (RBAC)**: A flexible system for managing user roles and fine-grained permissions.
- **Admin Dashboard**: A secure area for administrators to manage users, roles, permissions, and view application data.
- **Database Schema Exporter**: A standalone console tool that generates detailed Markdown documentation from the live database schema.
- **CI/CD Automation**: A GitHub Action that automatically runs the schema exporter on every push to `main`, ensuring documentation is always up-to-date.
- **Service-Oriented Architecture**: Key functionalities like email, auditing, and role management are abstracted into services.

## 🛠️ Tech Stack

- **Backend**: .NET 8, ASP.NET Core MVC
- **Database**: MySQL 8.0
- **ORM**: Entity Framework Core 8
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, Razor Views
- **CI/CD**: GitHub Actions

## 🚀 Getting Started

Follow these instructions to get a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

- .NET 8 SDK
- MySQL Server or another compatible MySQL provider.
- A Git client.

### Installation

1. **Clone the repository:**
   ```sh
   git clone <your-repository-url>
   cd ExuberantPathfinders.Web
   ```

2. **Configure the database connection:**
   - Open `appsettings.Development.json`.
   - Update the `DefaultConnection` string with your MySQL server details.
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=exuberant_db;Uid=root;Pwd=your_password;"
   }
   ```

3. **Install EF Core Tools (if not already installed):**
   ```sh
   dotnet tool install --global dotnet-ef
   ```

4. **Apply database migrations:**
   This command will create the database (if it doesn't exist) and apply all the tables and configurations.
   ```sh
   dotnet ef database update --project ExuberantPathfinders.Web.csproj
   ```

5. **Run the application:**
   ```sh
   dotnet run --project ExuberantPathfinders.Web.csproj
   ```
   The application will be available at `http://localhost:5118/`.

## 📄 Automated Schema Documentation

This project includes a GitHub Action defined in `.github/workflows/schema-export.yml`.

- **On every push to the `main` branch**, the action spins up a temporary MySQL database.
- It applies all Entity Framework migrations to build the schema.
- It runs the `SchemaExporter` tool against the temporary database.
- If the schema has changed, the action automatically commits the updated `DATABASE_SCHEMA.md` and `DATABASE_SCHEMA_ERD.mmd` files back to the repository.

This ensures that your database documentation is always in sync with your codebase.

---

*This README was generated with the assistance of Gemini Code Assist.*