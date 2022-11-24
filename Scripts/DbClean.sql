use master;

go

---- check if trustus db exists
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'TrustUS')
BEGIN
    -- Close connection to the database
	ALTER DATABASE TrustUS 
	SET SINGLE_USER 
	WITH ROLLBACK IMMEDIATE;

	-- drop the db
	DROP DATABASE TrustUS;
END;

go
-- recreate database
create database TrustUS;
go

-- drop existing tables
DROP TABLE IF EXISTS [dbo].UserLocation;
DROP TABLE IF EXISTS [dbo].UserPasswords;
DROP TABLE IF EXISTS [dbo].Users;