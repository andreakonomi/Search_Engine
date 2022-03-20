CREATE PROCEDURE dbo.DeleteTokensForDocument
	@DocumentId int
AS
BEGIN

	SET NOCOUNT ON;

    delete from dbo.Tokens
	where DocumentId = @DocumentId
END
GO
