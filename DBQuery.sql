CREATE DATABASE [TestLib];
GO

USE [TestLib];
GO

CREATE TABLE [Books](
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[ISBN] [nvarchar](100) NOT NULL UNIQUE,
	[Title] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](100) NULL,
	[Author] [nvarchar](100) NULL,
	[Count] [int] NOT NULL,
 );
GO

CREATE TABLE [Users](
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[FullName] [nvarchar](50) NOT NULL,
	[UserName] [nvarchar](50) NOT NULL UNIQUE,
	[Password] [nvarchar](100) NOT NULL,
	[Role] [nvarchar](50) NOT NULL,
);
GO 

CREATE TABLE [Borrows](
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[UserId] [int] NOT NULL FOREIGN KEY REFERENCES [Users](Id),
	[BookId] [int] NOT NULL  FOREIGN KEY REFERENCES [Books](Id),
	[BorrowDate] [datetime] NOT NULL,
	[IsReturned] [bit] NOT NULL,
);
GO

INSERT INTO [Users] VALUES ('Administrator', 'admin', 'ISMvKXpXpadDiUoOSoAfww==', 'ADMINISTRATOR');
GO