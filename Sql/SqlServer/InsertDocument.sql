
CREATE PROCEDURE dbo.InsertDocument
	@DocumentId int
AS
BEGIN

	SET NOCOUNT ON;

    insert into dbo.Documents(Id) 
	values (@DocumentId)
END
GO
