# Parking System Backend API

The Parking System Backend API is an ASP.NET Core-based RESTful service that powers a comprehensive parking management solution. This API supports user authentication and authorization, vehicle management, parking zone and spot management, reservations with QR code generation, payment processing, and more. It leverages modern technologies such as JWT for security, RabbitMQ for asynchronous notifications, Docker for containerization, rate limiting middleware for enhanced security, and SQL Server Reporting Services (SSRS) for database reporting.

---

## Table of Contents

- [Features](#features)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Usage](#usage)
- [API Documentation](#api-documentation)
- [Email Notifications & RabbitMQ Integration](#email-notifications--rabbitmq-integration)
- [Reporting](#reporting)
- [Rate Limiting](#rate-limiting)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

---

## Features

- **User Management:**
  - Supports Admin and User roles.
  - Secure JWT authentication and authorization.
  - Password reset functionality.

- **Vehicle Management:**
  - Users can add and manage their cars.

- **Parking Management (Admin):**
  - Admins can add and manage parking zones and spots.
  - Real-time management of parking availability.

- **Reservation System:**
  - Create reservations with secure QR code generation.
  - QR codes are valid for 24 hours; unused reservations are automatically cancelled via background services.
  
- **Payment Processing:**
  - Integrated payment handling for parking reservations.

- **Notifications:**
  - RabbitMQ integration to publish reservation events.
  - A background consumer sends email notifications (e.g., reservation confirmations, QR codes).

- **Containerization:**
  - The API is containerized using Docker for easy deployment and scalability.

- **Security:**
  - Rate limiter middleware to prevent abuse.
  - JWT-based authentication.
 
- **Database:**
- Microsoft SQL server database.

- **Reporting:**
  - Integration with SQL Server Reporting Services (SSRS) to generate database reports.

---

## Technology Stack

- **Framework:** ASP.NET Core (version 6/7/8)
- **Database:** SQL Server
- **Authentication:** JWT
- **Messaging:** RabbitMQ
- **Containerization:** Docker
- **Reporting:** SQL Server Reporting Services (SSRS)
- **Other Libraries:** AutoMapper, MailKit, QRCoder, EF Core, Kendo UI (for examples), AspNetCoreRateLimit

---

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- [RabbitMQ](https://www.rabbitmq.com/download.html) (or run via Docker)
- [Docker](https://www.docker.com/get-started)
- [SQL Server Reporting Services (SSRS)](https://docs.microsoft.com/en-us/sql/reporting-services/)
- An IDE such as Visual Studio or Visual Studio Code

---

## Installation

1. **Clone the Repository:**

   ```bash
   git clone https://github.com/kilany99/ParkingSystem.git
   cd ParkingSystem
   ```

2. **Restore NuGet Packages:**

   ```bash
   dotnet restore
   ```

3. **Update Configuration:**
   - Edit `appsettings.json` with your SQL Server connection string, JWT settings, email settings (including Gmail/App Password for notifications), RabbitMQ settings, and rate limit configurations.

4. **Apply Database Migrations:**

   ```bash
   dotnet ef database update
   ```

---

## Usage

### Running Locally

1. **Start the API:**

   ```bash
   dotnet run
   ```
   The API will be available at `http://localhost:5000` (or your configured port).

2. **Access Swagger UI:**

   Visit `http://localhost:5000/swagger` to explore and test the API endpoints interactively.

### Running with Docker

1. **Build the Docker Image:**

   ```bash
   docker build -t parking-system-backend .
   ```

2. **Run the Docker Container:**

   ```bash
   docker run -d -p 5000:80 --name parking-api parking-system-backend
   ```

3. **Access the API:**

   The API will be available at `http://localhost:5000`.

---

## API Documentation

The API is documented using Swagger/OpenAPI. After running the application, you can access the documentation at `/swagger` to view endpoint details, try out API calls, and review models and responses.

---

## Email Notifications & RabbitMQ Integration

- **Event Publishing:**  
  When a reservation is created, an event is published to RabbitMQ containing details such as the reservation ID, user ID, email, creation timestamp, and QR code.

- **Event Consumption:**  
  A background service listens to these events via RabbitMQ. Upon receiving an event, it sends a notification email with a message like:

  > "Thank you for using Easy Park! Your reservation will be kept on hold for 24 hours until [expiration time]. Please find attached the QR code of your reservation."

- **Email Sending:**  
  Emails are sent using MailKit, with Gmail as the SMTP server (using an App Password for 2-Step Verification). The QR code is generated using QRCoder and attached to the email.

---

## Reporting

The system integrates with SQL Server Reporting Services (SSRS) to generate comprehensive database reports (e.g., reservations, payments, usage statistics). Access the SSRS web portal (e.g., `http://localhost/Reports`) for viewing and managing reports.

---

## Rate Limiting

The API uses AspNetCoreRateLimit middleware to enforce rate limits on API endpoints, protecting against abuse and ensuring better security and performance. Configuration for rate limiting is available in `appsettings.json` under the `IpRateLimit` section.

---

## Contributing

Contributions are welcome! Please fork the repository and create a pull request with your changes. For major changes, please open an issue first to discuss what you would like to modify.

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

## Contact

For any questions or support, please contact [abdallah_elkilany@hotmail.com](mailto:abdallah_elkilany@hotmail.com).

