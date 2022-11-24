USE master
GO

-- Check if trust us database exists
-- if it doesnt exists create it
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'StoredPasswords')
BEGIN
  CREATE DATABASE StoredPasswords;
END;
GO

-- use the newly created database
use StoredPasswords;
go

/*
*
* Create tables
*
*/
IF OBJECT_ID(N'[dbo].LeakedPasswords', N'U') IS NULL
create table LeakedPasswords(
  HashedPassword varchar(60) PRIMARY KEY,
  PwnCount integer
)


/*
* Create stored procedure to get hashedpassword from hashed password
*/
DROP PROCEDURE IF EXISTS dbo.SP_HashedPasswordExists

go

CREATE PROCEDURE SP_HashedPasswordExists 
@HashedPassword varchar(80)
AS

Select COUNT(*) from dbo.LeakedPasswords
WITH (NOLOCK)
where HashedPassword = @HashedPassword
GO
