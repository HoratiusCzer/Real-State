-- REAK Real Estate Platform - Seed Data Script
-- This script inserts initial data for testing and development
-- Note: Passwords are hashed using BCrypt (example hashes shown)

USE [real-state];
GO

-- Insert Super Admin User
-- Password: Admin@123 (you should hash this properly in production)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@reak.com')
BEGIN
    INSERT INTO Users (Email, PasswordHash, Role, IsActive, FullName, Phone, CreatedAt)
    VALUES ('admin@reak.com', '$2a$11$placeholder-hash-here', 1, 1, 'System Administrator', '+1234567890', GETUTCDATE());
    PRINT 'Super Admin user created.';
END
GO

-- Insert Sample Branch
IF NOT EXISTS (SELECT 1 FROM Branches WHERE Name = 'Main Branch')
BEGIN
    DECLARE @AdminId INT = (SELECT TOP 1 Id FROM Users WHERE Role = 1);

    INSERT INTO Branches (Name, Location, Address, Phone, ManagerId, IsActive, CreatedAt)
    VALUES ('Main Branch', 'Downtown', '123 Main Street, City Center', '+1234567891', @AdminId, 1, GETUTCDATE());
    PRINT 'Main Branch created.';
END
GO

-- Insert Sample Property Types
DECLARE @BranchId INT = (SELECT TOP 1 Id FROM Branches);

IF NOT EXISTS (SELECT 1 FROM Properties WHERE Title = 'Luxury Villa Downtown')
BEGIN
    INSERT INTO Properties (Title, Description, Type, Status, Price, Area, Location, Address, BranchId, Bedrooms, Bathrooms, IsFeatured, CreatedAt)
    VALUES
    ('Luxury Villa Downtown', 'Beautiful 4-bedroom villa with modern amenities', 1, 1, 850000.00, 3500.00, 'Downtown', '456 Oak Avenue', @BranchId, '4', '3', 1, GETUTCDATE()),
    ('Commercial Office Space', 'Prime commercial space in business district', 2, 1, 1200000.00, 5000.00, 'Business District', '789 Commerce Blvd', @BranchId, NULL, NULL, 1, GETUTCDATE()),
    ('Agricultural Land', '50 acres of fertile agricultural land', 3, 1, 500000.00, 217800.00, 'Rural Area', 'County Road 15', @BranchId, NULL, NULL, 0, GETUTCDATE());
    PRINT 'Sample properties created.';
END
GO

PRINT 'Seed data script completed successfully.';
GO
