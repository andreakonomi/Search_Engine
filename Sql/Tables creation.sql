USE [Demo_Db]
GO

BEGIN

CREATE TABLE Documents
(
	Id INT NOT NULL,
	CONSTRAINT [PK_Documents_Id] PRIMARY KEY CLUSTERED (Id ASC)
)


CREATE TABLE Tokens
(
	Id INT PRIMARY KEY IDENTITY (1, 1),
	[Content] NVARCHAR(255),
	DocumentId INT NOT NULL,
	FOREIGN KEY (DocumentId) REFERENCES dbo.Documents (Id)
)

CREATE TYPE TokensList AS TABLE ( Content nvarchar(255) );

END;