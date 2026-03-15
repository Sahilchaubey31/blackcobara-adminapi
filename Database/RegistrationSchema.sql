-- Creates RegistrationDb, table and stored procedures for multi-step registration.
-- NOTE: Do NOT store CVV or ATM PIN. Hash passwords and OTPs in application code before sending to DB.

IF DB_ID(N'RegistrationDb') IS NULL
BEGIN
    CREATE DATABASE [RegistrationDb];
END
GO

USE [RegistrationDb];       
GO
    
IF OBJECT_ID(N'dbo.Registrations', N'U') IS NOT NULL
    DROP TABLE dbo.Registrations;
GO

CREATE TABLE dbo.Registrations
(
    Id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY, -- new identity column
    Username        NVARCHAR(100) NOT NULL,                 -- kept as unique key
    PasswordHash    NVARCHAR(256) NULL,
    MobileNumber    NVARCHAR(50) NULL,
    FatherName      NVARCHAR(200) NULL,
    MotherName      NVARCHAR(200) NULL,
    AadhaarNumber   NVARCHAR(50) NULL,
    AccountNumber   NVARCHAR(100) NULL,
    CifNumber       NVARCHAR(100) NULL,
    ProfilePasswordHash NVARCHAR(256) NULL,
    DateOfBirth     DATE NULL,
    CardLast4       NCHAR(4) NULL,
    CardExpiry      NVARCHAR(10) NULL,
    MaskedCard      NVARCHAR(50) NULL,      -- e.g. **** **** **** 1234
    OtpHash         NVARCHAR(256) NULL,
    RegistrationDate DATETIME2 NULL,
    UpdatedAt       DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- Step 1: Upsert user details
IF OBJECT_ID(N'dbo.usp_SaveUserDetails', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_SaveUserDetails;
GO
CREATE PROCEDURE dbo.usp_SaveUserDetails
    @Username NVARCHAR(100),
    @PasswordHash NVARCHAR(256),
    @MobileNumber NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Registrations WHERE Username = @Username)
    BEGIN
        UPDATE dbo.Registrations
        SET PasswordHash = @PasswordHash,
            MobileNumber = @MobileNumber,
            UpdatedAt = SYSUTCDATETIME()
        WHERE Username = @Username;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Registrations(Username, PasswordHash, MobileNumber, RegistrationDate, UpdatedAt)
        VALUES (@Username, @PasswordHash, @MobileNumber, SYSUTCDATETIME(), SYSUTCDATETIME());
    END
END
GO

-- Step 2: Personal & Bank Info
IF OBJECT_ID(N'dbo.usp_SavePersonalBankInfo', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_SavePersonalBankInfo;
GO
CREATE PROCEDURE dbo.usp_SavePersonalBankInfo
    @Username NVARCHAR(100),
    @FatherName NVARCHAR(200),
    @MotherName NVARCHAR(200),
    @AadhaarNumber NVARCHAR(50),
    @AccountNumber NVARCHAR(100),
    @CifNumber NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Registrations WHERE Username = @Username)
    BEGIN
        UPDATE dbo.Registrations
        SET FatherName = @FatherName,
            MotherName = @MotherName,
            AadhaarNumber = @AadhaarNumber,
            AccountNumber = @AccountNumber,
            CifNumber = @CifNumber,
            UpdatedAt = SYSUTCDATETIME()
        WHERE Username = @Username;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Registrations(Username, FatherName, MotherName, AadhaarNumber, AccountNumber, CifNumber, RegistrationDate, UpdatedAt)
        VALUES (@Username, @FatherName, @MotherName, @AadhaarNumber, @AccountNumber, @CifNumber, SYSUTCDATETIME(), SYSUTCDATETIME());
    END
END
GO

-- Step 3: Profile Setup (store hashed profile password and DOB)
IF OBJECT_ID(N'dbo.usp_SaveProfileSetup', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_SaveProfileSetup;
GO
CREATE PROCEDURE dbo.usp_SaveProfileSetup
    @Username NVARCHAR(100),
    @ProfilePasswordHash NVARCHAR(256),
    @DateOfBirth DATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Registrations WHERE Username = @Username)
    BEGIN
        UPDATE dbo.Registrations
        SET ProfilePasswordHash = @ProfilePasswordHash,
            DateOfBirth = @DateOfBirth,
            UpdatedAt = SYSUTCDATETIME()
        WHERE Username = @Username;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Registrations(Username, ProfilePasswordHash, DateOfBirth, RegistrationDate, UpdatedAt)
        VALUES (@Username, @ProfilePasswordHash, @DateOfBirth, SYSUTCDATETIME(), SYSUTCDATETIME());
    END
END
GO

-- Step 4: Card Verification (store last4 and masked card only)
IF OBJECT_ID(N'dbo.usp_SaveCardVerification', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_SaveCardVerification;
GO
CREATE PROCEDURE dbo.usp_SaveCardVerification
    @Username NVARCHAR(100),
    @CardNumber NVARCHAR(50),  -- full card number sent temporarily; recommended to tokenize in application
    @ExpiryDate NVARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @last4 NCHAR(4) = RIGHT(@CardNumber, 4);
    DECLARE @masked NVARCHAR(50) = '**** **** **** ' + @last4;

    IF EXISTS (SELECT 1 FROM dbo.Registrations WHERE Username = @Username)
    BEGIN
        UPDATE dbo.Registrations
        SET CardLast4 = @last4,
            CardExpiry = @ExpiryDate,
            MaskedCard = @masked,
            UpdatedAt = SYSUTCDATETIME()
        WHERE Username = @Username;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Registrations(Username, CardLast4, CardExpiry, MaskedCard, RegistrationDate, UpdatedAt)
        VALUES (@Username, @last4, @ExpiryDate, @masked, SYSUTCDATETIME(), SYSUTCDATETIME());
    END
END
GO

-- Step 5: OTP Verification
IF OBJECT_ID(N'dbo.usp_SaveOtpVerification', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_SaveOtpVerification;
GO
CREATE PROCEDURE dbo.usp_SaveOtpVerification
    @Username NVARCHAR(100),
    @OtpHash NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Registrations
    SET OtpHash = @OtpHash,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Username = @Username;
END
GO

-- Single-call complete registration
IF OBJECT_ID(N'dbo.usp_CompleteRegistration', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_CompleteRegistration;
GO
CREATE PROCEDURE dbo.usp_CompleteRegistration
    @Username NVARCHAR(100),
    @PasswordHash NVARCHAR(256),
    @MobileNumber NVARCHAR(50),
    @FatherName NVARCHAR(200),
    @MotherName NVARCHAR(200),
    @AadhaarNumber NVARCHAR(50),
    @AccountNumber NVARCHAR(100),
    @CifNumber NVARCHAR(100),
    @ProfilePasswordHash NVARCHAR(256),
    @DateOfBirth DATE,
    @CardNumber NVARCHAR(50),
    @CardExpiry NVARCHAR(10),
    @OtpHash NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @last4 NCHAR(4) = CASE WHEN @CardNumber IS NOT NULL THEN RIGHT(@CardNumber,4) ELSE NULL END;
    DECLARE @masked NVARCHAR(50) = CASE WHEN @last4 IS NOT NULL THEN '**** **** **** ' + @last4 ELSE NULL END;

    IF EXISTS (SELECT 1 FROM dbo.Registrations WHERE Username = @Username)
    BEGIN
        UPDATE dbo.Registrations
        SET PasswordHash = @PasswordHash,
            MobileNumber = @MobileNumber,
            FatherName = @FatherName,
            MotherName = @MotherName,
            AadhaarNumber = @AadhaarNumber,
            AccountNumber = @AccountNumber,
            CifNumber = @CifNumber,
            ProfilePasswordHash = @ProfilePasswordHash,
            DateOfBirth = @DateOfBirth,
            CardLast4 = @last4,
            CardExpiry = @CardExpiry,
            MaskedCard = @masked,
            OtpHash = @OtpHash,
            UpdatedAt = SYSUTCDATETIME()
        WHERE Username = @Username;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.Registrations (Username, PasswordHash, MobileNumber, FatherName, MotherName, AadhaarNumber, AccountNumber, CifNumber, ProfilePasswordHash, DateOfBirth, CardLast4, CardExpiry, MaskedCard, OtpHash, RegistrationDate, UpdatedAt)
        VALUES (@Username, @PasswordHash, @MobileNumber, @FatherName, @MotherName, @AadhaarNumber, @AccountNumber, @CifNumber, @ProfilePasswordHash, @DateOfBirth, @last4, @CardExpiry, @masked, @OtpHash, SYSUTCDATETIME(), SYSUTCDATETIME());
    END
END
GO

-- Get registration
IF OBJECT_ID(N'dbo.usp_GetRegistration', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetRegistration;
GO
CREATE PROCEDURE dbo.usp_GetRegistration
    @Username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Username, PasswordHash, MobileNumber, FatherName, MotherName, AadhaarNumber, AccountNumber, CifNumber, ProfilePasswordHash, DateOfBirth, CardLast4, CardExpiry, MaskedCard, RegistrationDate, UpdatedAt
    FROM dbo.Registrations
    WHERE Username = @Username;
END
GO  

-- Get registration by ID
IF OBJECT_ID(N'dbo.usp_GetRegistrationById', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetRegistrationById;
GO
CREATE PROCEDURE dbo.usp_GetRegistrationById
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Username, MobileNumber, FatherName, MotherName, AadhaarNumber, AccountNumber, CifNumber, DateOfBirth, CardLast4, CardExpiry, MaskedCard, RegistrationDate, UpdatedAt
    FROM dbo.Registrations
    WHERE Id = @Id;
END
GO

-- Get all registrations
IF OBJECT_ID(N'dbo.usp_GetAllRegistrations', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetAllRegistrations;
GO
CREATE PROCEDURE dbo.usp_GetAllRegistrations
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Username, MobileNumber, FatherName, MotherName, AadhaarNumber, AccountNumber, CifNumber, DateOfBirth, CardLast4, CardExpiry, MaskedCard, RegistrationDate, UpdatedAt
    FROM dbo.Registrations
    ORDER BY Id;
END
GO