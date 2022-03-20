-- CREATE TYPE TokensList AS TABLE ( Token nvarchar(255) );

CREATE PROCEDURE dbo.InsertDocument
	@tokensTbl TokensList READONLY

AS
BEGIN

	SET NOCOUNT ON;

	-- insert new record
    INSERT dbo.Documents DEFAULT VALUES;

	-- get the newly added id
	declare @lastIdentity as int = @@IDENTITY

	-- add the tokens records
	insert into dbo.Tokens
	(
		Content,
		DocumentId
	)
	select tbl.Token, @lastIdentity
	from @tokensTbl tbl
END
GO
