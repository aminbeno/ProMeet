# ProMeet - Technical Overview

This document provides a technical summary of the ProMeet application for review and audit purposes.

## 1. Project Architecture

ProMeet is built using the **ASP.NET Core MVC (Model-View-Controller)** framework, targeting .NET 8.0. It follows a standard layered architecture:

*   **Controllers:** Handle HTTP requests, manage application flow, and interact with data access layers.
*   **Views:** Render the user interface using Razor syntax (`.cshtml`).
*   **Models:** Define the data structures and business logic entities.
*   **Data Layer:** Manages database connections and operations using `MongoDbContext`.

### Key Technologies
*   **Framework:** ASP.NET Core 8.0
*   **Language:** C#
*   **Database:** MongoDB (NoSQL)
*   **ORM/Driver:** MongoDB.Driver
*   **Authentication:** ASP.NET Core Identity (customized for MongoDB with `AspNetCore.Identity.MongoDbCore`)
*   **Frontend:** HTML5, CSS3, Bootstrap 5, jQuery
*   **Real-time Communication:** SignalR (Pending Implementation)

## 2. Database Structure (MongoDB)

The application uses a document-oriented database. The main collections are:

*   **Users (`AspNetUsers`):** Stores user credentials, roles, and profile information.
    *   Extends `MongoIdentityUser`.
    *   Distinguishes between 'Client' and 'Professional' via `UserType` property.
*   **Professionals:** Stores specific details for professional users.
    *   Contains embedded `User` object for denormalized access to basic info (Name, City, etc.).
    *   Fields: `Specialty`, `Experience`, `Price` (Base Consultation), `Rating`.
*   **Services:** Services offered by professionals.
    *   Linked to `ProfessionalID`.
    *   Fields: `Name`, `Description`, `Price` (Decimal128).
*   **Appointments:** Booking records.
    *   Links `ClientID` and `ProfessionalID`.
    *   Status flow: `Pending` -> `Confirmed` / `Declined`.
*   **Reviews:** Client feedback.
    *   Linked to `ProfessionalID` and `ClientID`.
*   **Notifications:** System alerts for users.

## 3. Key Functional Flows

### Authentication & Authorization
*   **Identity:** Uses standard ASP.NET Core Identity.
*   **Roles:** `Professional` and `Client`.
*   **Authorization:** Controllers use `[Authorize]` and `[Authorize(Roles = "Professional")]` attributes to restrict access.

### Booking System
1.  Client searches for a Professional.
2.  Client selects a Service or Base Consultation.
3.  Client chooses a date/time (Validation ensures no conflicts).
4.  `Appointment` record created with status `Pending`.
5.  Professional sees request in Dashboard.

### Search & Filtering
*   **Search Engine:** Custom implementation in `ProfessionalController/Search`.
*   **Filters:** Category, City, Price Range, Text Search.
*   **Sorting:** Price (Low/High), Rating, Experience.

## 4. Technical Points of Interest (For Control)

### Data Consistency
*   **Denormalization:** The `Professional` collection embeds a copy of `User` data (Name, City, etc.) for performance.
*   **Syncing:** Updates to `ApplicationUser` (e.g., in Profile) trigger updates to the corresponding `Professional` document to maintain consistency.

### Currency Handling
*   All monetary values are stored as `Decimal128` in MongoDB to preserve precision.
*   Application uses `decimal` type in C#.
*   Standardized to Moroccan Dirham (DH).

### Security Measures
*   **CSRF Protection:** `[ValidateAntiForgeryToken]` on POST actions.
*   **Input Validation:** DataAnnotations (`[Required]`, `[Range]`, `[EmailAddress]`) on ViewModels.
*   **Password Hashing:** Handled by ASP.NET Core Identity (default PBKDF2).
*   **File Uploads:** Validates file type (Image) and size limit (5MB) before saving to `wwwroot`.

## 5. Directory Structure
*   `/Controllers`: API endpoints and Page logic.
*   `/Models`: Database entities.
*   `/ViewModels`: DTOs for View-Controller communication.
*   `/Views`: UI Templates.
*   `/Data`: Database Context (`MongoDbContext`).
*   `/Services`: Background services (e.g., `AppointmentReminderService`).
*   `/Hubs`: SignalR Hubs (e.g., `NotificationHub`).
*   `/wwwroot`: Static assets (CSS, JS, Images).

## 6. Pending Implementation & TODOs
*   **Real-time Notifications:** SignalR integration in `AppointmentController` (Book action) is currently marked as TODO. The `NotificationHub` structure exists but needs to be fully wired up for live updates.
*   **Appointment Logic:** Advanced conflict checking logic is implemented but can be further optimized for high concurrency.
