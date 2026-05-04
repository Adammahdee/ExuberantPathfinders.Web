# Exuberant Pathfinders Web Application

A robust ASP.NET Core 8 web application designed for secure administration of users, roles, permissions, applications, and donations. The system is structured for organizations such as NGOs, nonprofit platforms, faith-based institutions, and administrative service providers that require controlled access, accountability, and centralized management.

This project provides a strong foundation for identity management, role-based access control, donation tracking, and administrative operations, supported by automated database documentation and CI/CD workflow integration.

## Features

- Complete Identity Management
  - User registration with email confirmation
  - Secure login and logout
  - Password reset and password change
  - Profile management

- Role-Based Access Control (RBAC)
  - User role assignment
  - Fine-grained permission management
  - Secure authorization policies

- Admin Dashboard
  - User administration
  - Role and permission management
  - Application and donation monitoring
  - Centralized system control for administrators

- Database Schema Exporter
  - Standalone console tool for generating Markdown-based schema documentation directly from the live database

- CI/CD Automation
  - GitHub Actions workflow that automatically updates schema documentation after every push to the main branch

- Service-Oriented Architecture
  - Modular services for email handling, auditing, role management, and core business operations

## Tech Stack

- Backend: .NET 8, ASP.NET Core MVC
- Database: MySQL 8.0
- ORM: Entity Framework Core 8
- Authentication: ASP.NET Core Identity
- Frontend: Razor Views, Bootstrap 5
- CI/CD: GitHub Actions

## Getting Started

## Prerequisites

Ensure the following are installed:

- .NET 8 SDK
- MySQL Server (or compatible MySQL provider)
- Git

## Installation

### 1. Clone the repository

```sh
git clone https://github.com/Adammahdee/ExuberantPathfinders.Web.git
cd ExuberantPathfinders.Web
```

### 2. Configure the database connection

Open:

```text
appsettings.Development.json
```

Update the connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=exuberant_db;Uid=root;Pwd=your_password;"
}
```

### 3. Install EF Core tools

```sh
dotnet tool install --global dotnet-ef
```

### 4. Apply database migrations

This creates the database if it does not exist and applies all Entity Framework migrations.

```sh
dotnet ef database update --project ExuberantPathfinders.Web.csproj
```

### 5. Run the application

```sh
dotnet run --project ExuberantPathfinders.Web.csproj
```

The application will be available at:

```text
http://localhost:5118/
```

## Automated Schema Documentation

This project includes a GitHub Actions workflow located at:

```text
.github/workflows/schema-export.yml
```

On every push to the `main` branch:

1. A temporary MySQL database is created
2. Entity Framework migrations are applied automatically
3. The SchemaExporter tool reads the generated schema
4. Updated documentation is generated for:
   - `DATABASE_SCHEMA.md`
   - `DATABASE_SCHEMA_ERD.mmd`
5. If schema changes exist, the workflow commits the updated files back to the repository

This ensures that database documentation always stays synchronized with the application code.

## Recommended Documentation for Academic Defense

For stronger project presentation and supervisor review, include:

- Use Case Diagram
- Entity Relationship Diagram (ERD)
- System Architecture Diagram
- Flowchart of Core Operations
- Deployment Guide
- Security and Authorization Design

These improve both technical defense and professional portfolio value.

## Deployment Notes

For production deployment, document:

- Hosting environment (IIS, Linux server, cloud platform, etc.)
- Production database configuration
- Environment variables
- SMTP email configuration
- Backup and recovery strategy
- Security policies and audit logging

A dedicated `DEPLOYMENT.md` file is recommended for this purpose.

---

This project is designed to be both academically defensible and practically deployable, with emphasis on maintainability, security, and professional software engineering standards.
