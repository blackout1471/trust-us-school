USE master
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TrustUS')
BEGIN
  CREATE DATABASE TrustUS;
END;
GO

use TrustUS;
go

DROP TABLE IF EXISTS [dbo].UserLocation;
DROP TABLE IF EXISTS [dbo].UserPasswords;
DROP TABLE IF EXISTS [dbo].Users;

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

create table SecretKeyCounter(
      ID int IDENTITY(1,1) PRIMARY KEY,
      UserID int,
      SecretKey varchar(200),
      Counter BIGINT,
      LastRequestDate datetime
)

create table UserLocation(
      ID int IDENTITY(1,1) PRIMARY KEY,
      UserID int,
      IP binary,
      IPType int,
      UserAgent varchar(256)
)


ALTER TABLE UserPasswords
ADD FOREIGN KEY (UserID) REFERENCES Users(ID);

ALTER TABLE UserLocation
ADD FOREIGN KEY (UserID) REFERENCES Users(ID);

ALTER TABLE SecretKeyCounter
ADD FOREIGN KEY (UserID) REFERENCES Users(ID);

go


DROP PROCEDURE IF EXISTS dbo.SP_Createuser;

go

CREATE PROCEDURE SP_Createuser 
@Email varchar(100),
@HashedPassword varchar(200),
@Salt varchar(200),
@SecretKey varchar(200),
@Counter BIGINT
AS
insert into Users (Email) values (@Email)

declare @CreatedUserID int

set @CreatedUserID = (select top(1)ID from Users where Email = @Email)

insert into UserPasswords (UserID, HashedPassword, Salt)
values (@CreatedUserID, @HashedPassword, @Salt)

insert into SecretKeyCounter (UserID, SecretKey, Counter)
values (@CreatedUserID, @SecretKey, @Counter)

select * from Users
join UserPasswords
on UserPasswords.UserID = Users.ID
join SecretKeyCounter
on SecretKeyCounter.UserID = Users.ID
where Users.Email = @Email

GO

DROP PROCEDURE IF EXISTS dbo.SP_UserExists;
go

CREATE PROCEDURE SP_UserExists 
@Email varchar(100)
as 
select * from Users
join UserPasswords
on UserPasswords.UserID = Users.ID
where Users.Email = @Email
go

DROP PROCEDURE IF EXISTS dbo.SP_UpdateCounter;
go

CREATE PROCEDURE SP_UpdateCounter 
@Email varchar(100),
@Counter BIGINT
as 
Update SecretKeyCounter
Set Counter = @Counter

select * from Users
join SecretKeyCounter
on SecretKeyCounter.UserID = Users.ID
where Users.Email = @Email
go

DROP PROCEDURE IF EXISTS dbo.SP_UpdateLastRequest;
go

CREATE PROCEDURE SP_UpdateLastRequest 
@Email varchar(100)
as 
Update SecretKeyCounter
Set LastRequestDate = GETDate()

select * from Users
join SecretKeyCounter
on SecretKeyCounter.UserID = Users.ID
where Users.Email = @Email
go