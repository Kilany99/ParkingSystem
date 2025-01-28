
# Parking Management System

## Overview

The **Parking Management System** is a full-stack application designed to streamline parking space management and improve user experience for both customers and administrators. This project integrates an **ASP.NET Core API**, **Angular** frontend, and a **Kotlin mobile app** to provide a seamless solution for parking space reservation, payment, and management. 

### Key Features:
- **Real-time Parking Availability**: Users can view available parking spaces in real-time.
- **User Registration and Authentication**: Users can register, log in, and reset passwords through secure authentication mechanisms.
- **Booking and Reservation**: Users can reserve parking spaces and view their reservation history.
- **Admin Panel**: Admins can manage parking spots, monitor reservations, and update parking space statuses.
- **Payment Integration**: Payment options are provided for users to pay for their parking bookings.
- **Mobile App Integration**: A mobile application built using Kotlin for a native Android experience, providing access to all features on the go.
- To be integrated with Automatic License Plate Recognition (ALPR) that reads the plate number from a camera and sends it in a certain format alongside with the QR code form the scanner to handle the reservation validation
## Tech Stack

- **Frontend**: Angular
- **Backend**: ASP.NET Core Web API
- **Mobile App**: Kotlin (Android)
- **Database**: SQL Server
- **Authentication**: JWT Token-based Authentication
- **Payment Integration**: (Include payment API if applicable, such as Stripe, PayPal, etc.)
  
## Project Structure

### 1. **ASP.NET Core API**:
The backend of the system is built using **ASP.NET Core** to provide a RESTful API that serves data to the Angular frontend and Kotlin mobile application. The API handles operations such as user authentication, parking space management, reservation processing, and payment handling.

### 2. **Angular Frontend**:
The frontend is developed using **Angular**, providing an intuitive user interface for both customers and admins. The application interacts with the backend API for tasks like parking space display, reservations, and payment.

### 3. **Kotlin Mobile App**:
A Kotlin-based **Android mobile app** offers all the features of the web application in a mobile-first design. The app allows users to interact with the parking system, including browsing available spaces, making reservations, and managing their accounts.

## How to Run

### 1. **Backend (ASP.NET Core API)**

1. Clone the repository:
   ```bash
   git clone https://github.com/kilany99/parkingSystem.git
   ```

2. Navigate to the **API project directory**:
   ```bash
   cd ParkingManagementAPI
   ```

3. Install required dependencies and run the API:
   ```bash
   dotnet restore
   dotnet run
   ```

4. The API will be available on `http://localhost:5000`.

### 2. **Frontend (Angular)**

1. Navigate to the **Frontend project directory**:
   ```bash
   cd parking-system
   ```

2. Install the dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   ng serve
   ```

4. The frontend will be available on `http://localhost:4200`.

### 3. **Mobile App (Kotlin)**

1. Open the project in **Android Studio**.
2. Set up an **Android Emulator** or connect a physical device.
3. Build and run the app.

## Screenshots

(Include screenshots of your application if available)

## Future Enhancements

- Integration with multiple payment gateways.
- Adding notifications for booking reminders and parking space availability.
- Implementing a rating system for users to rate parking spaces.
- Admin dashboard improvements for more advanced analytics.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

