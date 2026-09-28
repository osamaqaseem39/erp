-- Create the erpnew database
USE [master]
GO

-- Drop database if it exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'erpnew')
BEGIN
    ALTER DATABASE [erpnew] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [erpnew];
END
GO

-- Create new database
CREATE DATABASE [erpnew]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'erpnew', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\erpnew.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'erpnew_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\erpnew_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO

ALTER DATABASE [erpnew] SET COMPATIBILITY_LEVEL = 160
GO

ALTER DATABASE [erpnew] SET READ_WRITE 
GO

PRINT 'Database erpnew created successfully!'
GO

