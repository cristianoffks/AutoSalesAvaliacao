-- Estrutura atual do AutoSales. Execute o arquivo completo no SSMS.
-- Destinado a um banco novo ou criado por esta migração consolidada.
USE [master];
GO
IF DB_ID(N'AutoSales') IS NULL
    CREATE DATABASE [AutoSales];
GO
USE [AutoSales];
GO
SET XACT_ABORT ON;
GO
BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Brands', N'U') IS NOT NULL
        OR OBJECT_ID(N'dbo.VehicleModels', N'U') IS NOT NULL
        OR COL_LENGTH(N'dbo.Vehicles', N'ModelId') IS NOT NULL
        THROW 50001, N'Este script cria a estrutura atual em um banco novo. O banco existente possui estrutura antiga.', 1;

    IF OBJECT_ID(N'dbo.People', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[People] (
            [Id] int IDENTITY(1,1) NOT NULL,
            [FullName] nvarchar(150) NOT NULL,
            [Cpf] nvarchar(11) NOT NULL,
            [BirthDate] date NOT NULL,
            [Email] nvarchar(254) NOT NULL,
            [Phone] nvarchar(30) NOT NULL,
            CONSTRAINT [PK_People] PRIMARY KEY ([Id])
        );
    END;

    IF OBJECT_ID(N'dbo.Vehicles', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[Vehicles] (
            [Id] int IDENTITY(1,1) NOT NULL,
            [Plate] nvarchar(7) NOT NULL,
            [ManufactureYear] int NOT NULL,
            [Brand] nvarchar(80) NOT NULL,
            [Model] nvarchar(100) NOT NULL,
            [PersonId] int NOT NULL,
            CONSTRAINT [PK_Vehicles] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_Vehicles_People_PersonId] FOREIGN KEY ([PersonId])
                REFERENCES [dbo].[People] ([Id]) ON DELETE NO ACTION
        );
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.People') AND name = N'IX_People_Cpf')
        CREATE UNIQUE INDEX [IX_People_Cpf] ON [dbo].[People] ([Cpf]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Vehicles') AND name = N'IX_Vehicles_Plate')
        CREATE UNIQUE INDEX [IX_Vehicles_Plate] ON [dbo].[Vehicles] ([Plate]);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Vehicles') AND name = N'IX_Vehicles_PersonId')
        CREATE INDEX [IX_Vehicles_PersonId] ON [dbo].[Vehicles] ([PersonId]);

    IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[__EFMigrationsHistory] (
            [MigrationId] nvarchar(150) NOT NULL,
            [ProductVersion] nvarchar(32) NOT NULL,
            CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
        );
    END;

    IF EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] <> N'20261106130000_InitialCreate')
        THROW 50002, N'O banco possui outro histórico de migrações. Use um banco novo para esta versão consolidada.', 1;

    IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = N'20261106130000_InitialCreate')
        INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (N'20261106130000_InitialCreate', N'10.0.10');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

