# Task Management System

A full-stack task management application built with ASP.NET Core, featuring a REST API and web interface for efficient task organization and productivity tracking.

## Features

- **Task Management**: Create, read, update, and delete tasks
- **Task Status Tracking**: Pending, In Progress, Completed, Cancelled
- **Priority Levels**: Low, Medium, High, Critical
- **Due Dates**: Set and track task deadlines
- **Filtering**: Filter tasks by status
- **Responsive Design**: Works on desktop and mobile devices
- **REST API**: Full CRUD operations via REST endpoints
- **Database**: SQLite database with Entity Framework Core

## Projects

### TaskSystem.API
- REST API built with ASP.NET Core Web API
- Entity Framework Core with SQLite database
- Swagger documentation available at `/swagger`
- CORS enabled for web client communication

**API Endpoints:**
- `GET /api/tasks` - Get all tasks
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update existing task
- `DELETE /api/tasks/{id}` - Delete task
- `GET /api/tasks/status/{status}` - Get tasks by status

### TaskSystem.WebApp
- Web application built with ASP.NET Core MVC
- Bootstrap 5 for responsive UI
- Font Awesome icons
- Consumes TaskSystem.API via HTTP client

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code (optional)

### Running the Application

1. **Start the API** (required first):
   ```bash
   cd TaskSystem.API
   dotnet run
   ```
   The API will be available at `https://localhost:7240` and `http://localhost:5249`

2. **Start the Web Application**:
   ```bash
   cd TaskSystem.WebApp  
   dotnet run
   ```
   The web app will be available at `https://localhost:7183` and `http://localhost:5210`

3. **Access the Application**:
   - Web Interface: Navigate to the web app URL
   - API Documentation: Visit `https://localhost:7240/swagger`

### Database
The application uses SQLite database (`tasks.db`) which is automatically created with sample data when the API starts for the first time.

## Architecture

- **Clean Architecture**: Separation of concerns with models, DTOs, and services
- **Repository Pattern**: Entity Framework Core as data access layer
- **Service Layer**: HTTP client service for API communication
- **MVC Pattern**: Controllers, Views, and ViewModels for web interface

## Technologies Used

- **Backend**: ASP.NET Core 8.0, Entity Framework Core, SQLite
- **Frontend**: ASP.NET Core MVC, Bootstrap 5, Font Awesome
- **API Documentation**: Swagger/OpenAPI
- **Development**: Visual Studio, .NET CLI

## Sample Data

The application includes sample tasks to demonstrate functionality:
1. "Complete project setup" (Completed, High Priority)
2. "Implement API endpoints" (In Progress, High Priority) 
3. "Design user interface" (Pending, Medium Priority)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request