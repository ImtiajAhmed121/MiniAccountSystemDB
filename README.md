# Mini Account Management System
This is a web-based **Mini Account Management System** built using **ASP.NET Core Razor Pages**, **ADO.NET with stored procedures**, and **ASP.NET Identity** for authentication and role-based access. The system allows secure, role-specific management of the Chart of Accounts and Vouchers, with built-in Excel export functionality.
## Features
### Authentication & Authorization
- Login/Register using ASP.NET Identity
- Role-based access control:
- **Admin** – Full access with user role management  
- **Accountant** – Can create, edit, and delete accounts and vouchers  
- **Viewer** – Can view only

### Chart of Accounts Management
- Create, Edit, Delete accounts
- Hierarchical structure with parent accounts
- Account types: Asset, Liability, Equity, Income, Expense
- Prevent deletion of accounts in use
- Export full Chart of Accounts to Excel
### Voucher Management
- Create, Edit, Delete vouchers
- Attach entries to accounts
- Export all vouchers to Excel
- Prevent duplicate account entries in a single voucher
## Technology Stack
- ASP.NET Core Razor Pages (.NET 6/7)
- ADO.NET with SQL Server Stored Procedures (No LINQ)
- ASP.NET Identity
- SQL Server Database
- Bootstrap 5 + Toastr for UI & alerts
- EPPlus for Excel export
## Setup Instructions
1. **Clone the repository**
bash
git clone https://github.com/ImtiajAhmed121/MiniAccountSystemDB
## Project Structure
Pages/ – Razor pages (UI and logic per route)  
Models/ – ViewModels or DTOs used for data binding  
Data/ – Contains your SQL DB-related scripts or migrations  
wwwroot/ – Static files like CSS, JS, and images


## Screenshots

### Home Page  
![Home](screenshots/home.png)

### Register / Login  
![Register / Login](screenshots/register-login.png)

### Chart of Accounts  
![Chart of Accounts](screenshots/chart-of-accounts.png)

### Voucher Entry  
![Voucher](screenshots/vouchar.png)

### User Management  
![User Management](screenshots/user-management.png)

### User Account  
![User Account](screenshots/user-account.png)


 Finalized README with full feature list, stack, structure, and setup.
