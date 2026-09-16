# REAK Technical Decisions Log

**Last Updated**: 2026-09-16

---

## Technology Stack

### Backend
- **Framework**: ASP.NET Core Web API 8.0
- **Language**: C# 12
- **Database**: SQL Server (LocalDB for development)
- **ORM**: Entity Framework Core 8.0
- **Authentication**: JWT Bearer tokens
- **Documentation**: Swagger/OpenAPI

**Rationale**: 
- ASP.NET Core provides excellent performance, built-in DI, and strong typing
- EF Core offers code-first migrations and LINQ query capabilities
- SQL Server chosen per requirements with LocalDB for easy local development

### Frontend (Planned)
- **Framework**: React 18 or Next.js 14
- **Language**: TypeScript
- **State Management**: TBD (Redux Toolkit or Zustand)
- **UI Library**: TBD (Material-UI, Ant Design, or Tailwind CSS)
- **HTTP Client**: Axios

**Rationale**: TBD after backend completion

---

## Architecture Decisions

### 1. Repository Pattern
**Decision**: Implement Repository pattern with generic base repository

**Pros**:
- Abstraction over data access
- Easier unit testing
- Centralized query logic

**Cons**:
- Additional abstraction layer
- Can be overkill for simple CRUD

**Status**: ✅ Approved

---

### 2. JWT Token Expiration
**Decision**: 24-hour token expiration with refresh token mechanism

**Rationale**:
- Balance between security and user experience
- Refresh tokens allow seamless re-authentication
- Can be adjusted per role if needed

**Status**: ✅ Approved

---

### 3. Password Hashing
**Decision**: BCrypt with work factor of 12

**Rationale**:
- Industry standard for password hashing
- Adaptive algorithm that can increase work factor over time
- More secure than MD5/SHA256

**Status**: ✅ Approved

---

### 4. Role-Based Access Control
**Decision**: Five distinct roles with hierarchical permissions
- SuperAdmin > Admin > Manager > Agent > Client

**Implementation**:
- Claims-based authorization
- Role stored in JWT token
- [Authorize(Roles = "...")] attributes on controllers

**Status**: ✅ Approved

---

### 5. File Upload Strategy
**Decision**: Store images on local file system with database path references

**Rationale**:
- Simple for initial development
- Can migrate to cloud storage (Azure Blob, AWS S3) later
- Keeps database size manageable

**Future**: Consider cloud storage for production

**Status**: ✅ Approved

---

### 6. API Response Format
**Decision**: Consistent response wrapper for all endpoints

```csharp
{
  "success": true,
  "data": {...},
  "message": "Success",
  "errors": []
}
```

**Rationale**:
- Consistent client-side parsing
- Clear success/error indication
- Easier error handling

**Status**: ✅ Approved

---

### 7. Error Handling
**Decision**: Global exception middleware with custom error responses

**Implementation**:
- Catch all unhandled exceptions
- Return consistent error format
- Log errors for debugging
- Hide internal errors from clients

**Status**: ✅ Approved

---

### 8. Pagination Strategy
**Decision**: Offset-based pagination with configurable page size

```
GET /api/properties?page=1&pageSize=20
```

**Rationale**:
- Simple to implement
- Adequate for expected data volume
- Can switch to cursor-based if performance issues arise

**Status**: ✅ Approved

---

### 9. Database Migration Strategy
**Decision**: Code-first with EF Core migrations

**Process**:
1. Design entities in code
2. Generate migration
3. Review migration SQL
4. Apply to database

**Rationale**:
- Version control for database schema
- Easy to rollback changes
- Automated deployment

**Status**: ✅ Approved

---

### 10. Logging Strategy
**Decision**: Use built-in ILogger with Serilog for structured logging

**Levels**:
- **Information**: Major operations (login, property created)
- **Warning**: Validation failures, recoverable errors
- **Error**: Exceptions, system failures
- **Debug**: Development diagnostics

**Status**: 🔄 To Be Implemented

---

## Database Design Decisions

### Entity Relationships

1. **User → Branch**: One-to-Many (a user can manage one branch)
2. **Branch → Properties**: One-to-Many (branch has multiple properties)
3. **Property → PropertyImages**: One-to-Many (cascade delete)
4. **Client → Leads**: One-to-Many
5. **Property ← Leads**: One-to-Many
6. **Deal → Transaction**: One-to-Many
7. **Deal → Commission**: One-to-Many

### Soft Delete Strategy
**Decision**: Use `IsActive` or `IsDeleted` flags instead of hard deletes

**Rationale**:
- Data recovery possible
- Audit trail maintained
- Referential integrity preserved

**Status**: ✅ Approved

---

## Security Decisions

### 1. SQL Injection Prevention
**Method**: Use parameterized queries via EF Core (automatic)

### 2. CORS Configuration
**Decision**: Allow specific origins in production, open in development

```csharp
// Development: AllowAnyOrigin
// Production: Specific domain whitelist
```

### 3. HTTPS Enforcement
**Decision**: Enforce HTTPS in production only

**Status**: ✅ Development uses HTTP, production will require HTTPS

---

## Performance Considerations

### 1. Lazy Loading
**Decision**: Disabled by default, use explicit Include() statements

**Rationale**:
- Prevents N+1 query problems
- Explicit is better than implicit
- Better performance control

### 2. Caching Strategy
**Decision**: Defer caching until performance testing reveals needs

**Future**: Consider Redis for session caching

### 3. Connection Pooling
**Decision**: Enabled by default in connection string

**Status**: ✅ Approved

---

## Testing Strategy

### Unit Tests
- Test business logic in services
- Mock repositories
- Use xUnit framework

### Integration Tests
- Test API endpoints
- Use in-memory database
- Verify authentication flows

### E2E Tests (Future)
- Selenium or Playwright
- Test complete user workflows

**Status**: 🔄 To Be Implemented in Phase 7

---

## Deployment Strategy (Future)

### Development
- LocalDB SQL Server
- Kestrel web server
- dotnet run

### Production (TBD)
- Azure SQL Database / AWS RDS
- IIS or Azure App Service
- CI/CD pipeline (GitHub Actions / Azure DevOps)

---

## Open Questions

1. ❓ Which frontend framework? (React vs Next.js)
2. ❓ Cloud storage provider for images? (Azure vs AWS)
3. ❓ Real-time features needed? (SignalR for notifications?)
4. ❓ Email service provider? (SendGrid, AWS SES)
5. ❓ SMS service for notifications? (Twilio)
6. ❓ Payment gateway integration needed?
7. ❓ Map integration? (Google Maps API, Mapbox)

---

## Rejected Alternatives

### 1. GraphQL instead of REST
**Reason**: REST is simpler for team, adequate for requirements

### 2. NoSQL Database (MongoDB)
**Reason**: Requirements indicate relational data, SQL Server specified

### 3. Microservices Architecture
**Reason**: Overkill for initial version, monolith is sufficient

---

**Note**: This document will be updated as architectural decisions are made throughout development.
