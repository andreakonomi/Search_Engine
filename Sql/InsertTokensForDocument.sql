USE [Demo_Db]
GO
/****** Object:  StoredProcedure [dbo].[InsertTokensForDocument]    Script Date: 19-03-2022 21:31:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- CREATE TYPE TokensList AS TABLE ( Content nvarchar(255) );

ALTER PROCEDURE [dbo].[InsertTokensForDocument]
	@DocumentId int,
	@TokensList TokensList READONLY

AS
BEGIN

	SET NOCOUNT ON;

	insert into dbo.Tokens
	(
		Content,
		DocumentId
	)
	select tbl.Content, @DocumentId
	from @TokensList tbl
END
