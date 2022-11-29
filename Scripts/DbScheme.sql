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


/*
*
* Create tables
*
*/

-- Check if table exists, U is for user defined table
IF OBJECT_ID(N'[dbo].Users', N'U') IS NULL
create table Users(
  ID int IDENTITY(1,1) PRIMARY KEY,
  Email varchar(100),
  IsVerified bit default(0), 
  FirstName varchar(50),
  LastName varchar(50),
  PhoneNumber varchar(20),
  FailedTries int default(0),
  IsLocked bit default(0),
  LockedDate datetime
)
GO

-- Check if table exists, U is for user defined table
IF OBJECT_ID(N'[dbo].UserPasswords', N'U') IS NULL
create table UserPasswords( /*Consider doing it in new namespace*/
      ID int IDENTITY(1,1) PRIMARY KEY,
      UserID int,
      HashedPassword varchar(200),
      Salt varchar(200)
)
-- Check if table exists, U is for user defined table
IF OBJECT_ID(N'[dbo].SecretKeyCounter', N'U') IS NULL
create table SecretKeyCounter(
      ID int IDENTITY(1,1) PRIMARY KEY,
      UserID int,
      SecretKey varchar(200),
      Counter BIGINT,
      LastRequestDate datetime
)
-- Check if table exists, U is for user defined table
IF OBJECT_ID(N'[dbo].UserLocation', N'U') IS NULL
create table UserLocation(
      ID int IDENTITY(1,1) PRIMARY KEY,
      UserID int,
      IP varchar(50),
      UserAgent varchar(256),
	  Successful bit default(0),
	  CreateDate datetime default(getdate())
)

/*
*
* Add constrains and foreign keys
*
*/

ALTER TABLE UserPasswords
ADD FOREIGN KEY (UserID) REFERENCES Users(ID);
ALTER TABLE SecretKeyCounter
ADD FOREIGN KEY (UserID) REFERENCES Users(ID);
go

/*
*
* Create stored procedures
*
*/

DROP PROCEDURE IF EXISTS dbo.SP_FetchFullUser;

go

create procedure SP_FetchFullUser
@UserID int = -1,
@Email varchar(200) = null
as
-- Select all user information
-- together with hashedpassword and salt
select * from Users
join UserPasswords
on UserPasswords.UserID = Users.ID
join SecretKeyCounter
on SecretKeyCounter.UserID = Users.ID
where Users.ID = @UserID or users.Email = @Email

go

DROP PROCEDURE IF EXISTS dbo.SP_Createuser;

go

CREATE PROCEDURE SP_Createuser 
@Email varchar(100),
@HashedPassword varchar(200),
@Salt varchar(200),
@FirstName varchar(50) = null,
@LastName varchar(50) = null,
@PhoneNUmber varchar(50) = null,
@SecretKey varchar(200),
@Counter BIGINT
AS

-- Insert user information into database
insert into Users (Email, FirstName, LastName, PhoneNumber) 
values (@Email, @FirstName, @LastName, @PhoneNUmber)

-- Get newly inserted user id
declare @CreatedUserID int = (select top(1)ID from Users where Email = @Email)

-- Insert the password and salt into the other table
insert into UserPasswords (UserID, HashedPassword, Salt)
values (@CreatedUserID, @HashedPassword, @Salt)

-- Insert the secret and counter into the other table
insert into SecretKeyCounter (UserID, SecretKey, Counter)
values (@CreatedUserID, @SecretKey, @Counter)

-- Select all user information
exec SP_FetchFullUser @UserID = @CreatedUserID

GO

DROP PROCEDURE IF EXISTS dbo.SP_UnlockAccount;
go

CREATE PROCEDURE dbo.SP_UnlockAccount 
@UserID int
as

declare @LockDate datetime = (select top(1)users.LockedDate from Users where users.ID = @UserID)

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


DROP PROCEDURE IF EXISTS dbo.SP_UserExists;
go

CREATE PROCEDURE SP_UserExists 
@Email varchar(100)
as 
-- get id of user with that email
declare @ID varchar(200) = (select top(1)users.ID from Users where Users.Email = @Email)
exec SP_UnlockAccount @UserID = @ID

-- Select all user information
exec SP_FetchFullUser @UserID = @ID

go


DROP PROCEDURE IF EXISTS dbo.SP_UpdateUserFailedTries;
go

CREATE PROCEDURE SP_UpdateUserFailedTries 
@UserID int
as 
-- Get tries of the user
declare @UserTries int = (select top(1)users.FailedTries from Users
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
exec SP_FetchFullUser @UserID = @UserID

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
exec SP_FetchFullUser @UserID = @UserID

go

CREATE PROCEDURE dbo.SP_UserLoggedInWithVerification 
@UserID int
as

update Users
set FailedTries = 0,
IsLocked = 0,
LockedDate = null
where Users.id = @UserID

Update SecretKeyCounter
Set LastRequestDate = null
where @UserID = SecretKeyCounter.UserID

-- Select all user information
exec SP_FetchFullUser @UserID = @UserID

go

DROP PROCEDURE IF EXISTS dbo.SP_UserLoggedInLocations;
go

create procedure dbo.SP_UserLoggedInLocations
@UserID int,
@IP varchar(50),
@UserAgent varchar(256) = null
as

select 
count(*) as LoggedInCount
from UserLocation
where 
UserLocation.IP = @IP
and
UserLocation.UserAgent = @UserAgent
and
UserLocation.UserID = @UserID
and
UserLocation.Successful = 1
go

DROP PROCEDURE IF EXISTS dbo.SP_AddLocation;
go


create procedure dbo.SP_AddLocation
@UserID int = 0,
@IP varchar(50),
@UserAgent varchar(256),
@SuccessFul bit
as

insert into UserLocation (UserID, IP, UserAgent, Successful)
values (@UserID, @IP, @UserAgent, @SuccessFul)

-- get the latest generated id for the query
declare @LocationID int = (SELECT SCOPE_IDENTITY())

-- reset previous attemps with that ip adress and user agent
if @SuccessFul = 1
begin
 update UserLocation
 set UserLocation.Successful = 1
 where
 UserLocation.IP = @IP
 and 
 UserLocation.UserAgent = @UserAgent
 and 
 UserLocation.UserID = @UserID
end

select * from UserLocation
where UserLocation.ID = @LocationID

go

DROP PROCEDURE IF EXISTS dbo.SP_IsIPLocked;
go

create procedure dbo.SP_IsIPLocked
@IP varchar(50)
as

select 
case 
when count(*) < 20 then '0' -- check if there was less then 20 attempts with that ip address, return 0
else '1' -- if there was more then 20 return 1
end as IsIPLocked
from UserLocation 
where
UserLocation.IP = @IP 
and
UserLocation.Successful = 0
and
DATEADD(minute, 10, UserLocation.CreateDate) > getdate() -- in the last 10 minutes
go

-- Updates last request
DROP PROCEDURE IF EXISTS dbo.SP_UpdateLastRequest;
go

CREATE PROCEDURE SP_UpdateLastRequest 
@UserID int
as 
Update SecretKeyCounter
Set LastRequestDate = GETDate(), Counter = Counter +1
where @UserID = SecretKeyCounter.UserID

exec SP_FetchFullUser @UserID = @UserID
go