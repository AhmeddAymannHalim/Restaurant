# Restaurant

A complete restaurant management system, built from scratch by **Halim** with ASP.NET Core 8 MVC.

It runs the whole restaurant in one place: taking orders, sending them to the kitchen, table reservations, delivery, the menu, the stock, staff and salaries, customers and suppliers, and the money. It also keeps the accounting records that go with all of that.

The app works in **Arabic and English**.

---

## Who this project helps

| Person | How it helps them |
|---|---|
| **Restaurant owner / manager** | Sees sales, expenses, stock and profit on the dashboard and in reports, from one system instead of paper and Excel. |
| **Cashier / waiter** | Takes dine-in, takeaway and delivery orders quickly, picks a table, and prints the bill. |
| **Kitchen staff** | Gets the orders on the kitchen screen as soon as they are placed, so nothing is forgotten. |
| **Storekeeper** | Tracks products, units (gram / kilo...), stores, stock movements and transfers between stores. |
| **Accountant** | Gets a chart of accounts, journal entries, customer and supplier payments, deposits and expenses, all created automatically from daily work. |
| **HR** | Manages employees, departments, jobs, contracts, allowances, deductions, leaves, insurance and salaries. |
| **Customers** | Get faster service, table reservations, delivery zones and customer tiers (loyalty levels). |

---

## Main features

- **Orders & POS**: dine-in, takeaway and delivery orders, bills, tables and table reservations
- **Kitchen screen**: live kitchen orders
- **Menu**: menu sections, products, product categories, units and unit prices
- **Inventory**: stores, stock movements, store transfers and assets
- **People**: customers, suppliers and employees. Each one automatically gets its own account in the chart of accounts.
- **Accounting**: chart of accounts, cost centers, journal entries, payments, deposits, expenses and currencies
- **HR & payroll**: departments, jobs, contracts, allowances, deductions, leaves, insurance and salaries
- **Company setup**: companies, branches, users, roles and permissions, application and account settings
- **Reports & dashboard**: export to **Excel** and **PDF**
- **Two languages**: full Arabic (RTL) and English interface

---

## How it was made

### Technologies

| Part | Used |
|---|---|
| Framework | ASP.NET Core 8 MVC (.NET 8) |
| Database | SQL Server with Entity Framework Core 8 (code-first, migrations) |
| Login & permissions | ASP.NET Core Identity |
| Mapping | AutoMapper |
| Validation | FluentValidation |
| Front end | Razor views, Bootstrap, jQuery, AJAX |
| Excel / PDF | ClosedXML / Rotativa |
| Tests | xUnit, Moq, SQLite in-memory, Playwright (browser tests) |

### Project structure

The solution is split into layers, and each layer has one job:

```
ResturantApp.sln
├── Shared/            → ViewModels, enums and shared helpers
├── Restaurant.DAL/    → Database: entities, DbContext, migrations, repositories (all the logic and queries)
├── Restaurant.BLL/    → Services: map between entities and ViewModels (AutoMapper)
├── ResturantPL/       → Web app: controllers, Razor views, CSS/JS, Arabic/English resources
├── Restaurant.Tests/  → Unit and integration tests (repositories, services, controllers, mapping)
└── Restaurant.E2E/    → Browser tests with Playwright
```

Each request goes through the layers in order:

```
Browser → Controller → Service → Repository → Database
```

- **Controllers** only receive the request and return the page or the JSON.
- **Services** only convert data with AutoMapper.
- **Repositories** hold the business logic and the database queries.
- **Views** only display ViewModels. Database entities never reach the page.

### ERP-ready

The data model follows the same tables and names as a full ERP system (accounts, bills, journal entries, items, units and stores). This means the restaurant's data can later move into a full ERP without any changes.

---

## How to run it

1. Install the **.NET 8 SDK** and **SQL Server** (Express is fine).
2. Clone the repo:
   ```bash
   git clone https://github.com/AhmeddAymannHalim/Restaurant.git
   ```
3. In `ResturantPL/appsettings.json`, set the `ConnectionStrings:Default` value to point to your SQL Server.
4. Create the database:
   ```bash
   dotnet ef database update --project Restaurant.DAL --startup-project ResturantPL
   ```
5. Run the app:
   ```bash
   dotnet run --project ResturantPL
   ```
6. Run the tests:
   ```bash
   dotnet test Restaurant.Tests
   ```

---

## Author

**Halim**: [github.com/AhmeddAymannHalim](https://github.com/AhmeddAymannHalim)
