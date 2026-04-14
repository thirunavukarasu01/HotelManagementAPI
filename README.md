# 🏨 Hotel Management API

A RESTful API built with ASP.NET Core, Entity Framework & JWT Authentication.

## 🛠️ Technologies Used
- ASP.NET Core 8.0
- Entity Framework Core
- SQL Server
- JWT Authentication
- BCrypt Password Hashing
- Swagger UI

## ✨ Features
- User Signup & Login with JWT
- Role Based Access (Admin / Guest)
- Room Management (CRUD)
- Booking Management
- Password Hashing with BCrypt

## 📋 API Endpoints

### Auth
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/auth/signup | Register new user |
| POST | /api/auth/login | Login & get token |
| POST | /api/auth/create-admin | Create admin user |

### Rooms
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/room | Get all rooms |
| GET | /api/room/available | Get available rooms |
| POST | /api/room | Add room (Admin) |
| PUT | /api/room/{id} | Update room (Admin) |
| DELETE | /api/room/{id} | Delete room (Admin) |

### Bookings
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/booking | Book a room |
| GET | /api/booking/my-bookings | My bookings |
| PUT | /api/booking/cancel/{id} | Cancel booking |
| GET | /api/booking/all | All bookings (Admin) |

## 🚀 How to Run
1. Clone the repo
2. Update connection string in appsettings.json
3. Run `Update-Database` in Package Manager Console
4. Press F5 to run
5. Open Swagger at https://localhost:xxxx/swagger
