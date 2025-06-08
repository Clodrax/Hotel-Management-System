# Hotel Management System

## Overview

This is a Hotel Management System built with ASP.NET Core Razor Pages targeting .NET 8. The application manages hotel operations such as guest management, room management, employee management, and user authentication.

## Project Structure

- **Controllers/**
  - `HomeController.cs`: Handles requests for the home page.
  - `DashBoardController.cs`: Manages dashboard-related actions.
  - `GuestsController.cs`: Handles guest-related operations.
  - `RoomsController.cs`: Manages room-related actions.
- **DAL/**
  - `AppDbContext.cs`: Entity Framework Core database context for managing data access.
- **Views/**
  - `Account/`: Contains views for user authentication (e.g., `Login.cshtml`).
  - `Guests/`: Views for guest management (`Index.cshtml`, etc.).
  - `Rooms/`: Views for room management (`Index.cshtml`, `Create.cshtml`, `Update.cshtml`).
  - `Employees/`: Views for employee management (`Index.cshtml`).
- **appsettings.json**: Configuration file for connection strings and other settings.
- **Program.cs**: Application startup and configuration.

## Database Configuration

The application uses SQL Server for data storage. The connection string is defined in `appsettings.json`:


## Key Features

- **User Authentication**: Login functionality for secure access.
- **Guest Management**: Add, update, and view guests.
- **Room Management**: Create, update, and list rooms.
- **Employee Management**: View employee information.
- **Dashboard**: Overview of hotel operations.

## How to Run

1. Ensure you have .NET 8 SDK installed.
2. Update the connection string in `appsettings.json` if needed.
3. Open the solution in Visual Studio 2022.
4. Build and run the project (`F5` or __Debug > Start Debugging__).

## Notes

- The project uses Entity Framework Core for data access.
- Razor Pages is used for the UI layer.
- Make sure your SQL Server instance is running and accessible.
