
CREATE PROCEDURE dbo.GetDocument 
	@DocumentId int
AS
BEGIN

	SET NOCOUNT ON;

    select top(1)
		Id = d.Id,
		TokenId = t.Id,
		Content = t.Content
	from dbo.Documents d
		left join dbo.Tokens t on d.Id = t.DocumentId

END
GO
