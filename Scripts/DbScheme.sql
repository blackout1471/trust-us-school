USE master
GO

-- Check if trust us database exists
-- if it doesnt exists create it
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TrustUS')
BEGIN
  CREATE DATABASE TrustUS;
END;
GO

-- use the newly created database
use TrustUS;
go

-- Drop pre exisitng tables if they exist
DROP TABLE IF EXISTS [dbo].UserLocation;
DROP TABLE IF EXISTS [dbo].UserPasswords;
DROP TABLE IF EXISTS [dbo].Users;

/*
*
* Create tables
*
*/
create table Users(
  ID int IDENTITY(1,1) PRIMARY KEY,
  Email varchar(100),
  HashedVeritification varchar(200),
  IsVerified bit default(0), 
  FirstName varchar(50),
  LastName varchar(50),
  PhoneNumber varchar(20),
  FailedTries int default(0),
  IsLocked bit default(0),
  LockedDate datetime
)

create table UserPasswords( /*Consider doing it in new namespace*/
      ID int IDENTITY(1,1) PRIMARY KEY,
      UserID int,
      HashedPassword varchar(200),
      Salt varchar(200)
)

create table UserLocation(
      ID int IDENTITY(1,1) PRIMARY KEY,
      UserID int,
      IP binary,
      IPType int,
      UserAgent varchar(256)
)

/*
*
* Add constrains and foreign keys
*
*/

ALTER TABLE UserPasswords
ADD FOREIGN KEY (UserID) REFERENCES Users(ID);

ALTER TABLE UserLocation
ADD FOREIGN KEY (UserID) REFERENCES Users(ID);

go

/*
*
* Create stored procedures
*
*/

DROP PROCEDURE IF EXISTS dbo.SP_Createuser;

go

CREATE PROCEDURE SP_Createuser 
@Email varchar(100),
@HashedPassword varchar(200),
@Salt varchar(200),
@FirstName varchar(50) = null,
@LastName varchar(50) = null,
@PhoneNUmber varchar(50) = null
AS

-- Insert user information into database
insert into Users (Email, FirstName, LastName, PhoneNumber) 
values (@Email, @FirstName, @LastName, @PhoneNUmber)

-- Get newly inserted user id
declare @CreatedUserID int
set @CreatedUserID = (select top(1)ID from Users where Email = @Email)

-- Insert the password and salt into the other table
insert into UserPasswords (UserID, HashedPassword, Salt)
values (@CreatedUserID, @HashedPassword, @Salt)

-- Select all user information
-- together with hashedpassword and salt
select * from Users
join UserPasswords
on UserPasswords.UserID = Users.ID
where Users.Email = @Email

GO


DROP PROCEDURE IF EXISTS dbo.SP_UserExists;
go

CREATE PROCEDURE SP_UserExists 
@Email varchar(100)
as 
-- get id of user with that email
declare @ID varchar(200) = (select top(1)users.ID from Users where Users.Email = @Email)
exec SP_UnlockAccount @UserID = @ID

-- Select all user information
-- together with hashedpassword and salt
select * from Users
join UserPasswords
on UserPasswords.UserID = Users.ID
where Users.Email = @Email

go


go

DROP PROCEDURE IF EXISTS dbo.SP_UpdateUserFailedTries;
go

CREATE PROCEDURE SP_UpdateUserFailedTries 
@UserID int
as 

declare @UserTries as int

-- Get tries of the user
set @UserTries = (select top(1)users.FailedTries from Users
where Users.ID = @UserID)

-- Add extra try
set @UserTries = @UserTries + 1

-- If user already failed 3 tries
if @UserTries >= 3
 begin
	-- Set the user account to locked
	update Users
	set Users.FailedTries = 3,
	IsLocked = 1,
	LockedDate = getdate()
	where Users.ID = @UserID
 end 
else -- If tries are less then 3
 begin
	-- Update the failed tries for user
	update Users
	set Users.FailedTries = @UserTries
	where Users.ID = @UserID
 end

-- Select all user information
-- together with hashedpassword and salt
select * from Users
join UserPasswords
on UserPasswords.UserID = Users.ID
where Users.ID = @UserID

go


DROP PROCEDURE IF EXISTS dbo.SP_UnlockAccount;
go

CREATE PROCEDURE dbo.SP_UnlockAccount 
@UserID int
as

declare @LockDate datetime
set @LockDate = (select top(1)users.LockedDate from Users where users.ID = @UserID)

-- check if there is a lock date
if @LockDate is not null
begin
	-- Check if lock date expired 
	if DATEADD(MINUTE, 30, @LockDate) < getdate()
	begin
		-- Reset the failed tries and lockdate
		update Users
		set LockedDate = null,
		FailedTries = 0,
		IsLocked = 0
		where users.id = @UserID
	end
end
go

DROP PROCEDURE IF EXISTS dbo.SP_UserLoggedIn;
go

CREATE PROCEDURE dbo.SP_UserLoggedIn 
@UserID int
as

update Users
set FailedTries = 0,
IsLocked = 0,
LockedDate = null
where Users.id = @UserID

-- Select all user information
-- together with hashedpassword and salt
select * from Users
join UserPasswords
on UserPasswords.UserID = Users.ID
where Users.ID = @UserID

go