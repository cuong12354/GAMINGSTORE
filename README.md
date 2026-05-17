# GAMINGSTORE

## Project Overview

GAMINGSTORE is a comprehensive e-commerce web application built with ASP.NET Core MVC, designed specifically for gaming products. The application provides a full-featured online store experience with user authentication, product management, shopping cart functionality, and administrative controls.

## Features

### User Features
- **User Registration & Authentication**: Secure user accounts with ASP.NET Core Identity
- **Product Browsing**: Browse gaming products by categories
- **Shopping Cart**: Add, remove, and manage cart items with session storage
- **Order Management**: Place orders with multiple payment methods
- **User Dashboard**: View order history and account information

### Administrative Features
- **Product Management**: CRUD operations for products and categories
- **Order Management**: View and manage customer orders
- **User Management**: Manage user accounts and roles
- **Dashboard Analytics**: Revenue charts and key performance indicators
- **Coupon Management**: Create and manage discount coupons

## Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Razor Views, Bootstrap CSS, JavaScript
- **Charts**: Chart.js for dashboard analytics
- **Session Management**: ASP.NET Core Session for shopping cart

## Project Structure

```
GAMINGSTORE/
├── Controllers/           # MVC Controllers
│   ├── AccountController.cs
│   ├── Categories.cs
│   ├── HomeController.cs
│   ├── ProductController.cs
│   └── ShoppingCartController.cs
├── Models/               # Data Models
│   ├── ApplicationUser.cs
│   ├── CartItem.cs
│   ├── Category.cs
│   ├── Order.cs
│   ├── Product.cs
│   └── ShoppingCart.cs
├── Views/                # Razor Views
│   ├── Home/
│   ├── Product/
│   ├── ShoppingCart/
│   └── Shared/
├── Areas/                # Admin Area
│   └── Admin/
│       ├── Controllers/
│       └── Views/
├── Data/                 # Database Context
│   └── ApplicationDbContext.cs
├── Repositories/         # Repository Pattern
│   ├── IProductRepository.cs
│   ├── EFProductRepository.cs
│   └── ...
├── Migrations/           # EF Core Migrations
└── wwwroot/              # Static Files
    ├── css/
    ├── js/
    └── images/
```

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd GAMINGSTORE
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update database**
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - Open browser to `https://localhost:5001` (HTTPS) or `http://localhost:5000` (HTTP)

### Default Accounts

After running migrations, the following accounts are available:

- **Admin Account**:
  - Email: admin@gamingstore.com
  - Password: Admin@123

- **Regular User**:
  - Email: user@gamingstore.com
  - Password: User@123

## Configuration

### Database Connection
Update `appsettings.json` for database connection:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GamingStoreDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### Email Configuration
Configure SMTP settings in `appsettings.json` for email functionality:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

## API Endpoints

### Public Endpoints
- `GET /` - Home page
- `GET /Product` - Product listing
- `GET /Product/Details/{id}` - Product details
- `POST /ShoppingCart/AddToCart` - Add item to cart
- `GET /ShoppingCart` - View shopping cart

### Admin Endpoints (Requires Admin Role)
- `GET /Admin/Dashboard` - Admin dashboard
- `GET /Admin/Product` - Product management
- `GET /Admin/Coupon/Create` - Create coupon
- `POST /Admin/Coupon/Create` - Save coupon

## Database Schema

### Key Tables
- **AspNetUsers**: User accounts (Identity)
- **AspNetRoles**: User roles
- **Products**: Gaming products
- **Categories**: Product categories
- **Orders**: Customer orders
- **OrderDetails**: Order line items
- **Coupons**: Discount coupons
- **ShoppingCartItems**: Cart items (session-based)

## Development Guidelines

### Code Style
- Follow C# coding conventions
- Use async/await for I/O operations
- Implement repository pattern for data access
- Use dependency injection

### Security
- Implement authorization policies
- Validate user input
- Use HTTPS in production
- Store sensitive data securely

### Testing
- Unit tests for business logic
- Integration tests for controllers
- UI tests for critical user flows

## Deployment

### IIS Deployment
1. Publish the application:
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Configure IIS site pointing to `./publish` folder

3. Set up SSL certificate

### Azure Deployment
1. Create Azure App Service
2. Configure connection strings
3. Deploy using Azure DevOps or GitHub Actions

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make changes and test
4. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support, email support@gamingstore.com or create an issue in the repository.