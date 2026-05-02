-- Create Database
CREATE DATABASE IF NOT EXISTS ExuberantPathfinders;
USE ExuberantPathfinders;

-- Identity Tables (AspNetUsers, AspNetRoles, etc.)
CREATE TABLE IF NOT EXISTS AspNetRoles (
    Id VARCHAR(255) PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    NormalizedName NVARCHAR(256),
    ConcurrencyStamp LONGTEXT
);

CREATE TABLE IF NOT EXISTS AspNetUsers (
    Id VARCHAR(255) PRIMARY KEY,
    UserName NVARCHAR(256),
    NormalizedUserName NVARCHAR(256),
    Email NVARCHAR(256),
    NormalizedEmail NVARCHAR(256),
    EmailConfirmed BIT DEFAULT 0,
    PasswordHash LONGTEXT,
    SecurityStamp LONGTEXT,
    ConcurrencyStamp LONGTEXT,
    PhoneNumber VARCHAR(255),
    PhoneNumberConfirmed BIT DEFAULT 0,
    TwoFactorEnabled BIT DEFAULT 0,
    LockoutEnd DATETIME(6),
    LockoutEnabled BIT DEFAULT 1,
    AccessFailedCount INT DEFAULT 0,
    FirstName NVARCHAR(256),
    LastName NVARCHAR(256),
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    LastModifiedAt DATETIME(6),
    IsActive BIT DEFAULT 1,
    UNIQUE KEY UQ_AspNetUsers_UserName (UserName),
    UNIQUE KEY UQ_AspNetUsers_NormalizedUserName (NormalizedUserName),
    UNIQUE KEY UQ_AspNetUsers_Email (Email),
    UNIQUE KEY UQ_AspNetUsers_NormalizedEmail (NormalizedEmail)
);

CREATE TABLE IF NOT EXISTS AspNetUserClaims (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId VARCHAR(255) NOT NULL,
    ClaimType LONGTEXT,
    ClaimValue LONGTEXT,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS AspNetUserLogins (
    LoginProvider VARCHAR(255) NOT NULL,
    ProviderKey VARCHAR(255) NOT NULL,
    ProviderDisplayName NVARCHAR(256),
    UserId VARCHAR(255) NOT NULL,
    PRIMARY KEY (LoginProvider, ProviderKey),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS AspNetUserTokens (
    UserId VARCHAR(255) NOT NULL,
    LoginProvider VARCHAR(255) NOT NULL,
    Name VARCHAR(255) NOT NULL,
    Value LONGTEXT,
    PRIMARY KEY (UserId, LoginProvider, Name),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS AspNetUserRoles (
    UserId VARCHAR(255) NOT NULL,
    RoleId VARCHAR(255) NOT NULL,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS AspNetRoleClaims (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    RoleId VARCHAR(255) NOT NULL,
    ClaimType LONGTEXT,
    ClaimValue LONGTEXT,
    FOREIGN KEY (RoleId) REFERENCES AspNetRoles(Id) ON DELETE CASCADE
);

-- Business Tables
CREATE TABLE IF NOT EXISTS ThematicAreas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    Description LONGTEXT,
    Code VARCHAR(50) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    UNIQUE KEY UQ_ThematicAreas_Code (Code),
    INDEX IX_ThematicAreas_IsActive (IsActive)
);

CREATE TABLE IF NOT EXISTS Programs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    Description LONGTEXT,
    ThematicAreaId INT NOT NULL,
    Budget DECIMAL(18, 2) NOT NULL,
    StartDate DATETIME(6) NOT NULL,
    EndDate DATETIME(6),
    ProgramOfficerId VARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    FOREIGN KEY (ThematicAreaId) REFERENCES ThematicAreas(Id) ON DELETE RESTRICT,
    FOREIGN KEY (ProgramOfficerId) REFERENCES AspNetUsers(Id) ON DELETE SET NULL,
    INDEX IX_Programs_ThematicAreaId (ThematicAreaId),
    INDEX IX_Programs_ProgramOfficerId (ProgramOfficerId),
    INDEX IX_Programs_IsActive (IsActive)
);

CREATE TABLE IF NOT EXISTS Applications (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ApplicantId VARCHAR(255) NOT NULL,
    ProgramId INT NOT NULL,
    Status INT DEFAULT 0, -- Draft = 0, Submitted = 1, UnderReview = 2, Approved = 3, Rejected = 4, OnHold = 5
    Title NVARCHAR(256) NOT NULL,
    Description LONGTEXT,
    RequestedAmount DECIMAL(18, 2) NOT NULL,
    SubmissionReference VARCHAR(100),
    SubmittedAt DATETIME(6),
    ReviewedAt DATETIME(6),
    ReviewNotes LONGTEXT,
    ReviewedById VARCHAR(255),
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    LastModifiedAt DATETIME(6),
    FOREIGN KEY (ApplicantId) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT,
    FOREIGN KEY (ProgramId) REFERENCES Programs(Id) ON DELETE RESTRICT,
    FOREIGN KEY (ReviewedById) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT,
    UNIQUE KEY UQ_Applications_SubmissionReference (SubmissionReference),
    INDEX IX_Applications_ApplicantId (ApplicantId),
    INDEX IX_Applications_ProgramId (ProgramId),
    INDEX IX_Applications_Status (Status),
    INDEX IX_Applications_CreatedAt (CreatedAt)
);

CREATE TABLE IF NOT EXISTS ApplicationStatusHistories (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ApplicationId INT NOT NULL,
    PreviousStatus INT NOT NULL,
    NewStatus INT NOT NULL,
    ChangedById VARCHAR(255),
    Reason LONGTEXT,
    ChangedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    FOREIGN KEY (ApplicationId) REFERENCES Applications(Id) ON DELETE CASCADE,
    FOREIGN KEY (ChangedById) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT,
    INDEX IX_ApplicationStatusHistories_ApplicationId (ApplicationId),
    INDEX IX_ApplicationStatusHistories_ChangedAt (ChangedAt)
);

CREATE TABLE IF NOT EXISTS Campaigns (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    Description LONGTEXT,
    ProgramId INT NOT NULL,
    TargetAmount DECIMAL(18, 2) NOT NULL,
    AmountRaised DECIMAL(18, 2) DEFAULT 0,
    StartDate DATETIME(6) NOT NULL,
    EndDate DATETIME(6) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    FOREIGN KEY (ProgramId) REFERENCES Programs(Id) ON DELETE CASCADE,
    INDEX IX_Campaigns_ProgramId (ProgramId),
    INDEX IX_Campaigns_IsActive (IsActive),
    INDEX IX_Campaigns_CreatedAt (CreatedAt)
);

CREATE TABLE IF NOT EXISTS Donations (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    DonorId VARCHAR(255) NOT NULL,
    CampaignId INT NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    Status INT DEFAULT 0, -- Pending = 0, Processing = 1, Completed = 2, Failed = 3, Refunded = 4
    Gateway INT DEFAULT 0, -- Paystack = 0, Manual = 1
    PaystackReference VARCHAR(255),
    PaystackAuthorizationUrl LONGTEXT,
    PaystackAccessCode VARCHAR(255),
    TransactionId VARCHAR(255),
    IsVerified BIT DEFAULT 0,
    VerifiedAt DATETIME(6),
    VerificationNotes LONGTEXT,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    ProcessedAt DATETIME(6),
    CompletedAt DATETIME(6),
    FOREIGN KEY (DonorId) REFERENCES AspNetUsers(Id) ON DELETE RESTRICT,
    FOREIGN KEY (CampaignId) REFERENCES Campaigns(Id) ON DELETE CASCADE,
    INDEX IX_Donations_DonorId (DonorId),
    INDEX IX_Donations_CampaignId (CampaignId),
    INDEX IX_Donations_Status (Status),
    INDEX IX_Donations_PaystackReference (PaystackReference),
    INDEX IX_Donations_TransactionId (TransactionId),
    INDEX IX_Donations_CreatedAt (CreatedAt)
);

CREATE TABLE IF NOT EXISTS AuditLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId VARCHAR(255),
    Action INT NOT NULL, -- Create = 0, Update = 1, Delete = 2, Approve = 3, Reject = 4
    EntityType VARCHAR(255) NOT NULL,
    EntityId INT NOT NULL,
    OldValues LONGTEXT,
    NewValues LONGTEXT,
    IPAddress VARCHAR(45),
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE SET NULL,
    INDEX IX_AuditLogs_CreatedAt (CreatedAt),
    INDEX IX_AuditLogs_UserId (UserId)
);

CREATE TABLE IF NOT EXISTS ContactMessages (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name NVARCHAR(256) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Subject NVARCHAR(512),
    Message LONGTEXT NOT NULL,
    IsResolved BIT DEFAULT 0,
    Response LONGTEXT,
    CreatedAt DATETIME(6) DEFAULT CURRENT_TIMESTAMP(6),
    RespondedAt DATETIME(6),
    INDEX IX_ContactMessages_CreatedAt (CreatedAt),
    INDEX IX_ContactMessages_IsResolved (IsResolved)
);

-- Insert Default Roles
INSERT INTO AspNetRoles (Id, Name, NormalizedName) VALUES
('admin-role', 'Admin', 'ADMIN'),
('officer-role', 'ProgramOfficer', 'PROGRAMOFFICER'),
('donor-role', 'Donor', 'DONOR'),
('applicant-role', 'Applicant', 'APPLICANT');

-- Insert Default Thematic Areas
INSERT INTO ThematicAreas (Name, Code, Description, IsActive, CreatedAt) VALUES
('Education', 'EDU', 'Educational programs and initiatives', 1, NOW()),
('Health', 'HEALTH', 'Healthcare and wellness programs', 1, NOW()),
('Environment', 'ENV', 'Environmental conservation programs', 1, NOW()),
('Community Development', 'COM', 'Community development initiatives', 1, NOW());
