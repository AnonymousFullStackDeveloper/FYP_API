# Friend Locator with History — Backend

This repository contains the backend API for the **Friend Locator with History** mobile application. It provides APIs for managing users, friends, groups, locations, location history, geofences, and notifications.

## Technologies Used

* C#
* ASP.NET Web API
* SQL Server
* Entity Framework
* REST APIs
* Postman
* Git

## Features

* User registration and login
* Friend requests and friend management
* Create and manage groups
* Join groups using invite codes
* Share and update user locations
* Store location history
* Retrieve location history
* Create and manage geofences
* Track geofence entry and exit events
* Manage notifications

## API Modules

* **User API** – User registration, login, and profile management
* **Friend API** – Add, accept, and manage friends
* **Group API** – Create groups and manage members
* **Location API** – Update and retrieve locations
* **History API** – Manage location history
* **Geofence API** – Create and manage geofences
* **Notification API** – Manage application notifications

## Database

The backend uses **Microsoft SQL Server** to store:

* User information
* Friend relationships
* Groups and group members
* Location data
* Location history
* Geofence information
* Notifications

## API Testing

All APIs were tested using **Postman** to verify requests, responses, and backend functionality.

## Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
cd backend
```

### 2. Configure Database

Update the SQL Server connection string in:

```text
appsettings.json
```

### 3. Run Database Migration

```bash
dotnet ef database update
```

### 4. Run the Project

```bash
dotnet run
```

## Project Purpose

The backend provides the required services for the Friend Locator with History application and handles communication between the mobile application and the database.

## Developer

**Kamran Laghari**

Computer Science Graduate
Arid Agriculture University, Rawalpindi

### Skills

`C#` `ASP.NET Web API` `SQL Server` `Entity Framework` `REST API` `Postman` `Git`
