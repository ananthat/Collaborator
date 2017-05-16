Create Database CKANDB
go
use CKANDB
go
CREATE TABLE TblUsers (
    ID int  IDENTITY(1,1) PRIMARY KEY,
    EMail varchar(200),
    IsActive bit,
    License_Allowed int,
    License_Total int,
	User_Role int
);
go
CREATE TABLE TblCollaborator (
    ID int IDENTITY(1,1) PRIMARY KEY,
	UserID int FOREIGN KEY REFERENCES TblUsers(ID),
	VaultID int,
	CollabeEmail varchar(200),
    CollabID int,
	Token varchar(200)
);
go
INSERT INTO [dbo].[TblUsers]
           ([EMail]
           ,[IsActive]
           ,[License_Allowed]
           ,[License_Total]
           ,[User_Role])
     VALUES
           ('test@gmail.com',1,5,10,1001)
go

-- =============================================
-- Author:	<Ananth Nagarajan>
-- Create date: <06-May-2017>
-- Description:	<To create a new Collaborator EMail>
-- =============================================
CREATE PROCEDURE SpCreateCollaborator 
	-- Add the parameters for the stored procedure here
	@VaultID int,
	@CollabeMail nvarchar(200),
	@UserID int,
	@CollabID int,
	@Token nvarchar(200)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @NewCollaboratorID INT 
	DECLARE @CNT INT
	DECLARE @EXISTCNT INT
	SELECT  @CNT= COUNT(*) FROM TblUsers WHERE ID=@UserID AND [License_Allowed] > 0
    -- Insert statements for procedure here
	IF(@CNT>0)
		BEGIN
			SELECT @EXISTCNT = COUNT(*) FROM [TblCollaborator] WHERE 
								USERID=@UserID AND VAULTID=@VaultID AND COLLABEEMAIL=@CollabeMail
			IF(@EXISTCNT=0)
				BEGIN
					INSERT INTO [dbo].[TblCollaborator]
						   ([UserID],
						   [VaultID],
						   [CollabeEmail],
						   [CollabID],
						   [Token])
					 VALUES
						   (@UserID,
						   @VaultID,
						   @CollabeMail,
						   @CollabID,
						   @Token)
		
					SET @NewCollaboratorID=SCOPE_IDENTITY()
		
					UPDATE [dbo].[TblUsers]
						SET [License_Allowed] = ([License_Allowed] - 1)
					WHERE ID=@UserID

					SELECT '200'
				END
			ELSE
				BEGIN
					SELECT '400'
				END
		END
	ELSE
		BEGIN
			SELECT '400'
		END
END
GO

-- =============================================
-- Author:	<Ananth Nagarajan>
-- Create date: <06-May-2017>
-- Description:	<To delete a Collaborator EMail>
-- =============================================
CREATE PROCEDURE SpDeleteCollaborator 
	-- Add the parameters for the stored procedure here
	@ID int,
	@UserID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @EXISTCNT INT

		SELECT @EXISTCNT = COUNT(*) FROM [TblCollaborator] WHERE 
								USERID=@UserID AND ID=@ID
		IF(@EXISTCNT>0)
			BEGIN	
				DELETE FROM TblCollaborator WHERE ID=@ID AND USERID=@UserID
				UPDATE [TblUsers] SET [License_Allowed] = ([License_Allowed] + 1) WHERE ID=@UserID
				SELECT '200'
			END
		ELSE
			BEGIN
				SELECT '400'
			END
END
GO

CREATE TABLE TblActivityNames (
    ID INT IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(100),
	Notes VARCHAR(300))
GO

INSERT INTO TblActivityNames(Name,Notes)
VALUES('Vault Added','A user added a vault')
GO
INSERT INTO TblActivityNames(Name,Notes)
VALUES('Vault Deleted','A user deleted a vault')
GO
INSERT INTO TblActivityNames(Name,Notes)
VALUES('Box Added','A user deleted a box')
GO
INSERT INTO TblActivityNames(Name,Notes)
VALUES('Box Deleted','A user deleted a box')
GO
INSERT INTO TblActivityNames(Name,Notes)
VALUES('Item Added','A file has been uploaded')
GO
INSERT INTO TblActivityNames(Name,Notes)
VALUES('Item downloaded','An file or folder was downloaded')
GO
INSERT INTO TblActivityNames(Name,Notes)
VALUES('Collaborator Added','A collaborator was added')
GO
INSERT INTO TblActivityNames(Name,Notes)
VALUES('Collaborator Removed','A collaborator was removed')
GO
INSERT INTO TblActivityNames(Name,Notes)
VALUES('Status Added','A status was added')
GO
INSERT INTO TblActivityNames(Name,Notes)
VALUES('Status Removed','A status was removed')
GO
INSERT INTO TblActivityNames(Name,Notes)
VALUES('Stage Completed','A stage was completed')
GO

CREATE TABLE TblActivity (
    ID INT IDENTITY(1,1) PRIMARY KEY NOT NULL,
	ActivityNames_ID INT FOREIGN KEY REFERENCES TblActivityNames(ID) NOT NULL,
	IPAddress VARCHAR(20) NOT NULL,
	CreatedBy INT FOREIGN KEY REFERENCES TblUsers(ID) NOT NULL,
	CreatedDate DATETIME NOT NULL,
	Collaborator_ID INT, --FOREIGN KEY REFERENCES TblCollaborator(ID) NULL,
	Box_ID INT,
	Status_ID INT,
	Status_Name VARCHAR(200),
	FileName VARCHAR(200),
	FileSize INT,
	Ver INT)
GO

-- =============================================
-- Author:	<Ananth Nagarajan>
-- Create date: <14-May-2017>
-- Description:	<To verify the Collaborator>
-- =============================================
CREATE PROCEDURE SpVerifyCollaborator
	-- Add the parameters for the stored procedure here
	@VaultID int,
	@CollabeMail nvarchar(200),
	@Token nvarchar(200)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @COLLABORATOR_ID INT

	SELECT @COLLABORATOR_ID = ID FROM TblCollaborator WHERE 
		VaultID = @VaultID AND 
		CollabeEmail = @CollabeMail AND 
		Token = @Token

	IF (@COLLABORATOR_ID IS NOT NULL AND @COLLABORATOR_ID > 0)
		BEGIN
			UPDATE TblCollaborator SET Token=1 WHERE
			ID = @COLLABORATOR_ID

			SELECT '200'
		END
	ELSE
		BEGIN
			SELECT '400'
		END

END
GO

-- =============================================
-- Author:	Ananth Nagarajan
-- Create date: 14-May-2017
-- Description:	To log the Activities (Audit)
-- =============================================
CREATE PROCEDURE SpLogActivities 
	-- Add the parameters for the stored procedure here
	@ActivityNames_ID INT,
	@IPAddress VARCHAR(20),
	@CreatedBy INT,
	@Collaborator_ID INT,
	@Box_ID INT,
	@Status_ID INT,
	@Status_Name VARCHAR(200),
	@FileName VARCHAR(200),
	@FileSize INT,
	@Ver INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @CreatedDate DATETIME;
	SELECT @CreatedDate = GETDATE()
    -- Insert statements for procedure here
	INSERT INTO [dbo].[TblActivity]
           ([ActivityNames_ID]
           ,[IPAddress]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[Collaborator_ID]
           ,[Box_ID]
           ,[Status_ID]
           ,[Status_Name]
           ,[FileName]
           ,[FileSize]
           ,[Ver])
     VALUES
           (@ActivityNames_ID,
		   @IPAddress,
           @CreatedBy,
		   @CreatedDate,
		   @Collaborator_ID,
           @Box_ID,
           @Status_ID,
		   @Status_Name,
           @FileName,
           @FileSize,
           @Ver)
END
GO

-- =============================================
-- Author:		<Ananth Nagarajan>
-- Create date: <06-May-2017>
-- Description:	<To create a new Collaborator EMail>
-- =============================================
ALTER PROCEDURE [dbo].[SpCreateCollaborator] 
	-- Add the parameters for the stored procedure here
	@VaultID int,
	@CollabeMail nvarchar(200),
	@UserID int,
	@CollabID int,
	@Token nvarchar(200)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @NewCollaboratorID INT 
	DECLARE @CNT INT
	DECLARE @EXISTCNT INT
	SELECT  @CNT= COUNT(*) FROM TblUsers WHERE ID=@UserID AND [License_Allowed] > 0
    -- Insert statements for procedure here
	IF(@CNT>0)
		BEGIN
			SELECT @EXISTCNT = COUNT(*) FROM [TblCollaborator] WHERE 
								USERID=@UserID AND VAULTID=@VaultID AND COLLABEEMAIL=@CollabeMail
			IF(@EXISTCNT=0)
				BEGIN
					INSERT INTO [dbo].[TblCollaborator]
						   ([UserID],
						   [VaultID],
						   [CollabeEmail],
						   [CollabID],
						   [Token])
					 VALUES
						   (@UserID,
						   @VaultID,
						   @CollabeMail,
						   @CollabID,
						   @Token)
		
					SET @NewCollaboratorID=SCOPE_IDENTITY()
		
					UPDATE [dbo].[TblUsers]
						SET [License_Allowed] = ([License_Allowed] - 1)
					WHERE ID=@UserID

					EXEC [dbo].[SpLogActivities]
					   7,'192.168.100.23',@UserID,@NewCollaboratorID,NULL,NULL,NULL,NULL,NULL,1

					SELECT '200'
				END
			ELSE
				BEGIN
					SELECT '400'
				END
		END
	ELSE
		BEGIN
			SELECT '400'
		END
END
GO

-- =============================================
-- Author:		<Ananth Nagarajan>
-- Create date: <06-May-2017>
-- Description:	<To delete a Collaborator EMail>
-- =============================================
ALTER PROCEDURE [dbo].[SpDeleteCollaborator] 
	-- Add the parameters for the stored procedure here
	@ID int,
	@UserID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @EXISTCNT INT

		SELECT @EXISTCNT = COUNT(*) FROM [TblCollaborator] WHERE 
								USERID=@UserID AND ID=@ID
		IF(@EXISTCNT>0)
			BEGIN	
				DELETE FROM TblCollaborator WHERE ID=@ID AND USERID=@UserID
				UPDATE [TblUsers] SET [License_Allowed] = ([License_Allowed] + 1) WHERE ID=@UserID
				EXEC [dbo].[SpLogActivities]
					   8,'192.168.100.23',@UserID,@ID,NULL,NULL,NULL,NULL,NULL,1
				SELECT '200'
			END
		ELSE
			BEGIN
				SELECT '400'
			END
END