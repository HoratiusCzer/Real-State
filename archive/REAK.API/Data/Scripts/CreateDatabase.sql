-- REAK Real Estate Platform - Database Creation Script
-- This script creates the database and applies initial setup
-- Run this script with SQL Server Management Studio or sqlcmd

-- Create Database
USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'real-state')
BEGIN
    CREATE DATABASE [real-state];
    PRINT 'Database [real-state] created successfully.';
END
ELSE
BEGIN
    PRINT 'Database [real-state] already exists.';
END
GO

USE [real-state];
GO

-- The tables will be created by EF Core migrations
-- Run: dotnet ef database update
PRINT 'Database is ready. Run "dotnet ef database update" to create tables.';
GO
