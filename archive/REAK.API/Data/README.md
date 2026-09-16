# REAK Real Estate Platform - Database Documentation

## Entity Framework Core Models

This directory contains all EF Core models, migrations, and database configuration for the REAK real estate platform.

## Database Schema Overview

### Core Entities

1. **User** - System users with role-based access (SuperAdmin, Admin, Manager, Agent, Client)
2. **Branch** - Physical office branches managed by branch managers
3. **Property** - Real estate properties (Residential, Commercial, Agricultural)
4. **PropertyImage** - Multiple images per property with display ordering
5. **Client** - Client information with lead scoring
6. **Lead** - Sales leads with status tracking and agent assignment
7. **Deal** - Closed deals with commission tracking
8. **Transaction** - Financial transactions (payments, commissions, refunds)
9. **Commission** - Agent commissions from deals
10. **Activity** - Audit log of all system activities
11. **Notification** - User notifications
12. **Task** - Task management system

## Key Design Decisions

### Relationships & Cascade Behaviors

- **User to Client**: One-to-One with CASCADE delete (client profile belongs to user)
- **Branch to Properties**: One-to-Many with RESTRICT delete (prevent deletion of branches with properties)
- **Property to Images**: One-to-Many with CASCADE delete (images deleted with property)
- **Lead relationships**: RESTRICT on Client/Property, SET NULL on Agent (preserve data integrity)
- **Deal relationships**: RESTRICT on all FKs (prevent accidental deletion of critical business data)
- **Task relationships**: RESTRICT (prevent deletion of users with assigned/created tasks)

### Enumerations

All enums are stored as integers for performance:
- UserRole: SuperAdmin(1), Admin(2), Manager(3), Agent(4), Client(5)
- PropertyType: Residential(1), Commercial(2), Agricultural(3)
- PropertyStatus: Available(1), UnderOffer(2), Sold(3), Rented(4), OffMarket(5)
- LeadStatus: New(1), Contacted(2), Qualified(3), Negotiation(4), Won(5), Lost(6)
- DealStatus: Draft(1), Pending(2), Approved(3), Completed(4), Cancelled(5)
- CommissionStatus: Pending(1), Approved(2), Paid(3), Cancelled(4)
- TaskStatus: Pending(1), InProgress(2), Completed(3), Cancelled(4)
- TaskPriority: Low(1), Medium(2), High(3), Urgent(4)

### Indexes

- **User.Email**: Unique index for authentication
- **Client.Email, Client.Phone**: Indexes for fast lookups
- **Deal.ContractNumber**: Unique index
- **Activity**: Composite index on (EntityType, EntityId) and Timestamp
- **Notification**: Composite index on (UserId, IsRead) for efficient queries

### Data Types

- **Decimal fields**: `decimal(18,2)` for currency (Price, Amount, Commission)
- **Decimal rates**: `decimal(5,2)` for percentages (CommissionRate, PercentageRate)
- **Dates**: UTC datetime for all timestamps
- **Strings**: Appropriate max lengths based on use case

## Database Setup

### Prerequisites

- .NET 10.0 SDK
- SQL Server 2019+ or SQL Server Express/LocalDB
- Entity Framework Core Tools

### Initial Setup

1. **Update connection string** in `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=real-state;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
   }
   ```

2. **Create database** (optional - EF will create it):
   ```bash
   sqlcmd -S YOUR_SERVER -i Data/Scripts/CreateDatabase.sql
   ```

3. **Apply migrations**:
   ```bash
   cd REAK.API
   dotnet ef database update
   ```

4. **Seed initial data** (optional):
   ```bash
   sqlcmd -S YOUR_SERVER -d real-state -i Data/Scripts/SeedData.sql
   ```

### Development Workflow

#### Add a new migration:
```bash
dotnet ef migrations add MigrationName --output-dir Data/Migrations --context ReakDbContext
```

#### Update database:
```bash
dotnet ef database update
```

#### Rollback migration:
```bash
dotnet ef database update PreviousMigrationName
```

#### Remove last migration (if not applied):
```bash
dotnet ef migrations remove
```

#### Generate SQL script:
```bash
dotnet ef migrations script --output Data/Scripts/Migration.sql
```

### Connection String Examples

**Local Development (LocalDB)**:
```
Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=real-state;Integrated Security=True;Encrypt=True;TrustServerCertificate=True
```

**SQL Server with Windows Authentication**:
```
Server=localhost;Database=real-state;Integrated Security=True;TrustServerCertificate=True
```

**SQL Server with SQL Authentication**:
```
Server=localhost;Database=real-state;User Id=sa;Password=YourPassword;TrustServerCertificate=True
```

**Azure SQL Database**:
```
Server=tcp:yourserver.database.windows.net,1433;Database=real-state;User ID=yourusername;Password=yourpassword;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Entity Relationship Diagram

```
User (1) ----< (*) Branch [Manager]
User (1) ---- (1) Client
User (1) ----< (*) Lead [AssignedAgent]
User (1) ----< (*) Deal [Agent]
User (1) ----< (*) Commission [Agent]
User (1) ----< (*) Activity
User (1) ----< (*) Notification
User (1) ----< (*) Task [AssignedTo]
User (1) ----< (*) Task [CreatedBy]

Branch (1) ----< (*) Property

Property (1) ----< (*) PropertyImage
Property (1) ----< (*) Lead
Property (1) ----< (*) Deal

Client (1) ----< (*) Lead
Client (1) ----< (*) Deal

Deal (1) ----< (*) Transaction
Deal (1) ----< (*) Commission

Lead (*) ---- (0..1) Property
```

## Security Considerations

1. **Password Hashing**: Use BCrypt or similar for password hashing (not implemented in seed script)
2. **Soft Deletes**: Consider implementing soft deletes for critical entities
3. **Audit Trail**: Activity table logs all major operations
4. **Data Encryption**: Consider encrypting sensitive fields (SSN, payment info)
5. **Row-Level Security**: Implement authorization filters based on user roles

## Performance Optimization

1. **Indexes**: Created on frequently queried columns
2. **Eager Loading**: Use `.Include()` for related data when needed
3. **Pagination**: Implement for large datasets
4. **Caching**: Consider caching for frequently accessed reference data
5. **Async Operations**: All EF operations should use async methods

## Maintenance

- Regular backups scheduled
- Monitor query performance
- Review and optimize indexes periodically
- Archive old activity logs
- Clean up expired notifications

## Support

For database-related issues or questions, contact the development team.
