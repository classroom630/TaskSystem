# Task Management System

A comprehensive Task Management System built with ASP.NET Core featuring role-based authentication, multi-layered architecture, and modern web technologies.

## 🚀 Features

### Authentication & Authorization
- **JWT Authentication** with refresh tokens for secure API access
- **Role-based authorization** with three levels: Admin, Manager, User
- **Cookie-based authentication** for MVC frontend
- **Identity Framework** integration for user management

### Architecture & Design Patterns
- **Repository Pattern** with generic and specific implementations
- **Multi-layered architecture**: Repository → Service → Controller
- **AutoMapper** for seamless object mapping between models and DTOs
- **Dependency Injection** throughout the application
- **Global exception handling** middleware with custom error responses

### Database & Data Access
- **Entity Framework Core** with Code-First migrations
- **SQLite database** for cross-platform compatibility
- **Identity Framework** integration for user management
- **Comprehensive data models** with proper relationships and constraints

## 🏗️ Project Structure

```
UserTaskManagement.sln
├── UserTaskManagement.Domain/           # Core domain entities and enums
│   ├── Entities/
│   │   ├── User.cs                      # User entity with Identity integration
│   │   ├── Task.cs                      # Task entity with relationships
│   │   ├── RefreshToken.cs              # JWT refresh token management
│   │   └── BaseEntity.cs                # Base entity with common properties
│   └── Enums/
│       └── Role.cs                      # User roles (Admin, Manager, User)
├── UserTaskManagement.Application/      # Business logic and DTOs
│   ├── DTOs/                           # Data Transfer Objects
│   ├── ViewModels/                     # MVC ViewModels
│   ├── Interfaces/                     # Service and repository interfaces
│   └── Services/                       # Business logic implementation
├── UserTaskManagement.Infrastructure/   # Data access and external services
│   ├── Data/                           # EF Core DbContext and configurations
│   ├── Repositories/                   # Repository implementations
│   ├── Services/                       # Infrastructure services (Email, etc.)
│   └── Mappings/                       # AutoMapper profiles
├── TaskSystem.API/                     # Web API controllers and configuration
│   ├── Controllers/                    # API controllers
│   ├── Middleware/                     # Custom middleware
│   └── Program.cs                      # API configuration and setup
├── TaskSystem.WebApp/                  # MVC web application (In Progress)
└── UserTaskManagement.Tests/          # Unit tests (Planned)
```

## 🔐 Default Users

The system comes with pre-seeded data:

### Admin User
- **Email**: admin@tasksystem.com
- **Password**: Admin123!
- **Role**: Admin
- **Permissions**: Full system access, user management, all tasks

## 🌐 API Endpoints

### Authentication Endpoints
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login with email/password
- `POST /api/auth/refresh` - Refresh JWT token
- `POST /api/auth/revoke` - Revoke refresh token (logout)

### User Management Endpoints
- `GET /api/users` - Get all users (Admin only)
- `GET /api/users/{id}` - Get user by ID
- `GET /api/users/me` - Get current user profile
- `POST /api/users` - Create new user (Admin only)
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user (Admin only)

### Task Management Endpoints
- `GET /api/tasks` - Get tasks (filtered by role)
- `GET /api/tasks/{id}` - Get task by ID
- `GET /api/tasks/my-tasks` - Get current user's tasks
- `GET /api/tasks/stats` - Get task statistics
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task
- `POST /api/tasks/{id}/toggle` - Toggle task completion

## 🛠️ Setup Instructions

### Prerequisites
- .NET 8.0 SDK
- SQLite (included with .NET)

### Running the API

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd TaskSystem
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Run the API**
   ```bash
   cd TaskSystem.API
   dotnet run
   ```

4. **Access Swagger UI**
   - Open browser to `http://localhost:5000`
   - Use the interactive API documentation

### Database Setup

The database is automatically created and seeded when the API starts for the first time.

- **Database file**: `tasksystem-dev.db` (in API project directory)
- **Migrations**: Automatically applied on startup
- **Seeding**: Default admin user and roles are created

## 🧪 Testing the API

### Login as Admin
```bash
curl -X POST "http://localhost:5000/api/auth/login" \
     -H "Content-Type: application/json" \
     -d '{"email": "admin@tasksystem.com", "password": "Admin123!"}'
```

### Register New User
```bash
curl -X POST "http://localhost:5000/api/auth/register" \
     -H "Content-Type: application/json" \
     -d '{
       "firstName": "John",
       "lastName": "Doe",
       "email": "john@example.com", 
       "password": "Test123!",
       "confirmPassword": "Test123!",
       "role": 1
     }'
```

### Get Task Statistics (with JWT token)
```bash
curl -X GET "http://localhost:5000/api/tasks/stats" \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## 🔒 Role-Based Access Control

### Admin
- Full system access
- Manage all users (create, update, delete, assign roles)
- View and manage all tasks
- Access to all API endpoints

### Manager  
- View and manage team tasks
- Create tasks and assign to users
- Cannot manage user accounts
- Limited administrative functions

### User
- View and manage only their own tasks
- Create tasks for themselves
- Update their own profile
- No administrative access

## 🏭 Production Considerations

- **Security**: Update JWT secret key in production
- **Database**: Switch to SQL Server/PostgreSQL for production
- **Email**: Configure SMTP settings for email notifications
- **Logging**: Configure structured logging with Serilog
- **HTTPS**: Enable HTTPS in production environment

## 🚧 Development Status

- ✅ **Domain Layer**: Complete
- ✅ **Application Layer**: Complete  
- ✅ **Infrastructure Layer**: Complete
- ✅ **API Layer**: Complete and tested
- 🚧 **Web MVC Layer**: In Progress
- ⏳ **Unit Tests**: Planned
- ⏳ **Integration Tests**: Planned

## 🤝 Contributing

This is a demonstration project showcasing modern .NET development practices including:
- Clean Architecture
- Domain-Driven Design principles
- SOLID principles
- Repository and Service patterns
- JWT authentication
- Role-based authorization
- Entity Framework Core
- AutoMapper
- Dependency Injection
- Global exception handling
- Structured logging

## 📝 License

This project is for educational and demonstration purposes.