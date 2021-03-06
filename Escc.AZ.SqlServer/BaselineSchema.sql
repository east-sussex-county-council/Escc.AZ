USE [ESCCWebsiteAZ]
GO
/****** Object:  StoredProcedure [dbo].[usp_CategoryUpdateIpsvVersion]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Rick Mason, Web Team
-- Create date: 11 September 2008
-- Description:	Update the version of IPSV used by the ESCC Website A-Z. Expects the new IPSV data to be in a 
-- table called IPSV, which is imported from a CSV version of IPSV available from http://www.esd.org.uk/standards/ipsv/.
-- Minimum requirement is that IPSV table contains varchar columns for ConceptId, Id, Name and Preferred.
-- There's no interface for this stored procedure, just run it from the database at the appropriate time.
-- =============================================
CREATE PROCEDURE [dbo].[usp_CategoryUpdateIpsvVersion]
AS
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON
	
	-- Getting half-way through an IPSV upgrade would be a mess, so wrap everything in a transaction
    BEGIN TRAN
	
	-- First, delete any concepts which have been removed from the new version of IPSV.

    -- Usually this happens when two concepts are merged because they're describing the 
    -- same thing. (Officially this isn't deleting a concept but, when they merge two, 
    -- one concept id has to go...)

    -- In this case the preferred term which has been done away with usually ends up as a
    -- non-preferred term of the new, merged concept. This means we can follow the term to
    -- find the new, merged concept and update any headings which used the old concept.

    -- There's a chance that a single heading already references both concepts which have 
    -- been merged into one. If we simply translated the new concept to the merged concept,
    -- we'd end up with two references to the same concept. This is bad practice and, if 
    -- Heading_Category had a proper composite primary key, it would cause an error.
    
    -- Solution is to get the headings and categories we want to insert, delete any duplicates
    -- of them that are already in Heading_Category, then insert the new relationships safely.

	-- Step 1: Get the new preferred terms for merged concepts, and the headings we need to
    -- relate them to.

	SELECT DISTINCT Heading_Category.HeadingId, NewCategory.CategoryId
	INTO #IpsvImport_MergedConcepts
	FROM Category 
		INNER JOIN Heading_Category ON Category.CategoryId = Heading_Category.CategoryId -- Only need to update concepts which are in use in the A-Z
		LEFT JOIN IPSV ON Category.Identifier = Ipsv.Id -- Join to latest IPSV to find which concepts are missing
		LEFT JOIN Category AS NewCategory ON IPSV.ConceptId = NewCategory.ConceptId -- IPSV says which concept it was merged with, join back to category to find out our CategoryId for it
	WHERE 
		IPSV.Preferred = 'False' -- We're looking for non-preferred terms which used to be preferred
		AND Category.Scheme = 'IPSV' 
		AND NewCategory.Scheme = 'IPSV' 
		AND Category.ConceptId NOT IN 
				(SELECT ConceptId FROM IPSV AS NewIpsv WHERE Preferred = 'True') -- Only concepts which aren't in the new IPSV

	IF @@ERROR != 0
	BEGIN
		ROLLBACK TRAN
		RETURN
	END
	
	-- Step 2: We know what to insert, so get rid of any potential duplicates so that we can insert it safely.
	DELETE FROM Heading_Category 
	WHERE Heading_CategoryId IN 
		(SELECT Heading_CategoryId 
		 FROM #IpsvImport_MergedConcepts INNER JOIN Heading_Category 
			  ON #IpsvImport_MergedConcepts.HeadingId = Heading_Category.HeadingId 
			  AND #IpsvImport_MergedConcepts.CategoryId = Heading_Category.CategoryId)

	IF @@ERROR != 0
	BEGIN
		ROLLBACK TRAN
		RETURN
	END
	
	-- Step 3: Insert the new relationships to the merged categories
	INSERT INTO Heading_Category (HeadingId, CategoryId)
	SELECT HeadingId, CategoryId FROM #IpsvImport_MergedConcepts

	IF @@ERROR != 0
	BEGIN
		ROLLBACK TRAN
		RETURN
	END
	
	-- Finished with the temporary table, so clean up
	DROP TABLE #IpsvImport_MergedConcepts

	IF @@ERROR != 0
	BEGIN
		ROLLBACK TRAN
		RETURN
	END
	
	-- Now delete any references to the old concepts which have been ditched or merged into another
    DELETE FROM Heading_Category WHERE CategoryId IN 
		(SELECT CategoryId FROM Category WHERE Scheme = 'IPSV' AND ConceptId NOT IN 
			(SELECT ConceptId FROM IPSV WHERE Preferred = 'True')
		)

	IF @@ERROR != 0
	BEGIN
		ROLLBACK TRAN
		RETURN
	END
	
    -- And, finally, delete the discarded concepts themselves
	DELETE FROM Category WHERE Scheme = 'IPSV' AND ConceptId NOT IN 
		(SELECT ConceptId FROM IPSV WHERE Preferred = 'True')
	
	IF @@ERROR != 0
	BEGIN
		ROLLBACK TRAN
		RETURN
	END
	
	-- Next, update any concepts which are present in both versions so that 
    -- we have the currently-preferred identifier and term (for most records
    -- this will mean nothing changes)

	UPDATE Category SET 
		Category.Identifier = IPSV.Id,
		Category.CommonName = IPSV.Name
	FROM Category 
		INNER JOIN IPSV ON Category.ConceptId = IPSV.ConceptId
	WHERE 
		Category.Scheme = 'IPSV' 
		AND IPSV.Preferred = 'True'

    -- Insert any concepts that have been added in the new version

	INSERT INTO Category 
		(ConceptId, Identifier, CommonName, Scheme)
	SELECT DISTINCT ConceptId, Id, Name, 'IPSV' 
	FROM IPSV 
	WHERE Preferred = 'True' 
	AND ConceptId NOT IN 
		(SELECT ConceptId FROM Category WHERE Scheme = 'IPSV')	
	
	IF @@ERROR != 0
	BEGIN
		ROLLBACK TRAN
		RETURN
	END
	
	COMMIT TRAN
	

	-- Audit this process
	INSERT INTO dbo.Audit
	(AuditDate, StoredProcedureName, AuditDetails)
	VALUES
	(GETDATE(), 'usp_CategoryUpdateIpsvVersion', '')


	RETURN 


GO
/****** Object:  StoredProcedure [dbo].[usp_DeleteCategoriesForHeading]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[usp_DeleteCategoriesForHeading] 
(
	@headingId int
)
AS

SET NOCOUNT ON
DELETE FROM Heading_Category WHERE HeadingId = @headingId

-- Audit this deletion
INSERT INTO dbo.Audit
(AuditDate, StoredProcedureName, AuditDetails)
VALUES
(GETDATE(), 'usp_DeleteCategoriesForHeading', 'HeadingId=' + CONVERT(nvarchar, @headingId))


SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_DeleteContact]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[usp_DeleteContact]
(
	@contactId int
)
AS
SET NOCOUNT ON

-- delete contact
DELETE FROM Contact WHERE ContactId = @contactId

-- Audit this deletion
INSERT INTO dbo.Audit
(AuditDate, StoredProcedureName, AuditDetails)
VALUES
(GETDATE(), 'usp_DeleteContact', 'ContactId=' + CONVERT(nvarchar, @contactId))



SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_DeleteHeading]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[usp_DeleteHeading] 
(
	@headingId int
)
AS
SET NOCOUNT ON
BEGIN TRAN

-- delete any link records to services (though there shouldn't be any)
DELETE FROM Heading_AZService WHERE Heading_AZService.HeadingId = @headingId
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- delete any link records to related headings
DELETE FROM Heading_Heading WHERE Heading_Heading.HeadingId = @headingId OR Heading_Heading.RelatedHeadingId = @headingId
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- delete any link records to categories
EXEC usp_DeleteCategoriesForHeading @headingId
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- delete the heading itself
DELETE FROM Heading WHERE Heading.HeadingId = @headingId

-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
COMMIT TRAN

-- Audit this deletion
INSERT INTO dbo.Audit
(AuditDate, StoredProcedureName, AuditDetails)
VALUES
(GETDATE(), 'usp_DeleteHeading', 'HeadingId=' + CONVERT(nvarchar, @headingId))


SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_DeleteHeadingFromService]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_DeleteHeadingFromService]
(
	@serviceId int,
	@headingId int
)
AS
SET NOCOUNT ON
DELETE FROM Heading_AZService WHERE Heading_AZService.ServiceId = @serviceId AND Heading_AZService.HeadingId = @headingId
-- recalculate the ServiceCount fields
EXEC usp_UpdateDerivedServiceCount @headingId

-- Audit this deletion
INSERT INTO dbo.Audit
(AuditDate, StoredProcedureName, AuditDetails)
VALUES
(GETDATE(), 'usp_DeleteHeadingFromService', 'ServiceId=' + CONVERT(nvarchar, @serviceId) + 'HeadingId=' + CONVERT(nvarchar, @headingId))


SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[usp_DeleteRelatedHeading]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[usp_DeleteRelatedHeading]
(
	@headingId int,
	@relatedHeadingId int
)
AS
SET NOCOUNT ON
BEGIN TRAN
-- remove the relationship in both directions - part 1
DELETE FROM Heading_Heading WHERE Heading_Heading.HeadingId = @headingId AND Heading_Heading.RelatedHeadingId = @relatedHeadingId
-- check for errors and rollback if required
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- remove the relationship in both directions - part 2
DELETE FROM Heading_Heading WHERE Heading_Heading.HeadingId = @relatedHeadingId AND Heading_Heading.RelatedHeadingId = @headingId
-- check for errors and rollback if required
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
COMMIT TRAN

-- Audit this deletion
INSERT INTO dbo.Audit
(AuditDate, StoredProcedureName, AuditDetails)
VALUES
(GETDATE(), 'usp_DeleteRelatedHeading', 'HeadingId=' + CONVERT(nvarchar, @headingId) + 'RelatedHeadingId=' + CONVERT(nvarchar, @relatedHeadingId))

SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[usp_DeleteService]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_DeleteService]
(
	@serviceId int
)
AS
SET NOCOUNT ON
BEGIN TRAN
-- delete any contacts
DELETE FROM Contact WHERE Contact.ServiceId = @serviceId
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- delete any urls
DELETE FROM ServiceUrl WHERE ServiceUrl.ServiceId = @serviceId
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- Make a note of related headings before we remove the relationship
DECLARE @relatedHeadings TABLE (HeadingId int)
INSERT @relatedHeadings
	SELECT 
		Heading_AZService.HeadingId 
	FROM 
		Heading_AZService 
	WHERE 
		Heading_AZService.ServiceId = @serviceId
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- delete any links to headings
DELETE FROM Heading_AZService WHERE Heading_AZService.ServiceId = @serviceId
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- Loop through associated headings and recalculate their ServiceCount fields
DECLARE @headingId int
DECLARE headingCursor CURSOR LOCAL FORWARD_ONLY STATIC READ_ONLY FOR
	SELECT 
		HeadingId
	FROM 
		@relatedHeadings
	
OPEN headingCursor
	
	FETCH NEXT FROM headingCursor
	INTO @headingId
	
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC usp_UpdateDerivedServiceCount @headingId
	  
		IF @@ERROR <> 0
		BEGIN
			CLOSE headingCursor
			DEALLOCATE headingCursor
			RETURN 1
		END
		FETCH NEXT FROM headingCursor
	INTO @headingId
	END
CLOSE headingCursor
DEALLOCATE headingCursor
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- delete the service itself
DELETE FROM Service WHERE Service.ServiceId = @serviceId
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
COMMIT TRAN

-- Audit this deletion
INSERT INTO dbo.Audit
(AuditDate, StoredProcedureName, AuditDetails)
VALUES
(GETDATE(), 'usp_DeleteService', 'ServiceId=' + CONVERT(nvarchar, @serviceId))

SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[usp_DeleteUrl]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[usp_DeleteUrl]
(
	@urlId int
)
AS
SET NOCOUNT ON

-- delete url
DELETE FROM ServiceUrl WHERE ServiceUrlId = @urlId

-- Audit this deletion
INSERT INTO dbo.Audit
(AuditDate, StoredProcedureName, AuditDetails)
VALUES
(GETDATE(), 'usp_DeleteUrl', 'UrlId=' + CONVERT(nvarchar, @urlId))


SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_HeadingInsert]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[usp_HeadingInsert]
(
	@headingId int output,
	@heading varchar(255),
	@redirectUrl varchar(255),
	@redirectTitle varchar(255),
	@relatedHeadingId int = NULL,
	@serviceId int = NULL,
	@ipsvNonPreferredTerms varchar(1000),
	@gclCategories varchar(255),
	@ipsv bit
)
AS
SET NOCOUNT ON
-- multi command operation, so use a transaction
BEGIN TRAN
-- first command updates heading table
INSERT INTO Heading
	(Heading, RedirectUrl, RedirectTitle, IpsvNonPreferredTerms, GclCategories, Ipsv)
VALUES
	(LTRIM(RTRIM(@heading)), LTRIM(RTRIM(@redirectUrl)), LTRIM(RTRIM(@redirectTitle)), LTRIM(RTRIM(@ipsvNonPreferredTerms)), LTRIM(RTRIM(@gclCategories)), @ipsv)
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- get new id
SELECT @headingId = @@IDENTITY
-- write linked records
-- write both ways so that the link works both ways
IF @relatedHeadingId IS NOT NULL
BEGIN
	INSERT INTO Heading_Heading
		(Heading_Heading.HeadingId, Heading_Heading.RelatedHeadingId)
	VALUES
		(@headingId, @relatedHeadingId)
	INSERT INTO Heading_Heading
		(Heading_Heading.HeadingId, Heading_Heading.RelatedHeadingId)
	VALUES
		(@relatedHeadingId, @headingId)
END
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- write link to service
IF @serviceId IS NOT NULL
BEGIN
	INSERT INTO Heading_AZService
		(HeadingId, ServiceId)
	VALUES
		(@headingId, @serviceId)
END
-- calculate the ServiceCount fields
EXEC usp_UpdateDerivedServiceCount @headingId
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- all ok, so write the changes
COMMIT TRAN
SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[usp_HeadingUpdate]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[usp_HeadingUpdate]
(
	@headingId int,
	@heading varchar(255),
	@redirectUrl varchar(255),
	@redirectTitle varchar(255),
	@relatedHeadingId int = NULL,
	@serviceId int = NULL,
	@ipsvNonPreferredTerms varchar(1000),
	@gclCategories varchar(255),
	@ipsv bit
)
AS
SET NOCOUNT ON
-- multi command operation, so use a transaction
BEGIN TRAN
-- first command updates heading table
UPDATE 
	Heading
SET
	Heading.Heading = LTRIM(RTRIM(@heading)),
	Heading.RedirectUrl = LTRIM(RTRIM(@redirectUrl)),
	Heading.RedirectTitle = LTRIM(RTRIM(@redirectTitle)),
	Heading.IpsvNonPreferredTerms = LTRIM(RTRIM(@ipsvNonPreferredTerms)),
	Heading.GclCategories = LTRIM(RTRIM(@gclCategories)),
	Heading.Ipsv = @ipsv,
	Heading.LastAmended = GETDATE()
WHERE
	Heading.HeadingId = @headingId
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- write linked records
-- write both ways so that the link works both ways
IF @relatedHeadingId IS NOT NULL
BEGIN
	INSERT INTO Heading_Heading
		(Heading_Heading.HeadingId, Heading_Heading.RelatedHeadingId)
	VALUES
		(@headingId, @relatedHeadingId)
	INSERT INTO Heading_Heading
		(Heading_Heading.HeadingId, Heading_Heading.RelatedHeadingId)
	VALUES
		(@relatedHeadingId, @headingId)
END
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- if heading redirected, remove services
IF @redirectUrl IS NOT NULL AND @redirectUrl <> ''
	BEGIN
		DELETE FROM Heading_AZService WHERE Heading_AZService.HeadingId = @headingId
	END
ELSE
	BEGIN
	-- otherwise if service newly specified, write link to service
	IF @serviceId IS NOT NULL
		BEGIN
			-- check it's not there already, to avoid a violation of primary key error
			SELECT ServiceId FROM Heading_AZService WHERE HeadingId = @headingId AND ServiceId = @serviceId
			IF @@ROWCOUNT = 0
			BEGIN
				INSERT INTO Heading_AZService
					(HeadingId, ServiceId)
				VALUES
					(@headingId, @serviceId)
			END
		END
	END
-- recalculate the ServiceCount fields
EXEC usp_UpdateDerivedServiceCount @headingId
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- all ok, so write the changes
COMMIT TRAN

-- Audit this action
INSERT INTO dbo.Audit
(AuditDate, StoredProcedureName, AuditDetails)
VALUES
(GETDATE(), 'usp_HeadingUpdate', 'HeadingId=' + CONVERT(nvarchar, @headingId))


SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[usp_HeadingUpdateIpsv]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Rick Mason
-- Create date: 15 June 2006
-- Description:	Update the IPSV field for a specified heading
-- =============================================
CREATE PROCEDURE [dbo].[usp_HeadingUpdateIpsv] 
	-- Add the parameters for the stored procedure here
	@headingId int, 
	@ipsv bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	UPDATE Heading SET Ipsv = @ipsv WHERE Heading.HeadingId = @headingId
END


GO
/****** Object:  StoredProcedure [dbo].[usp_InsertCategoryForHeading]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_InsertCategoryForHeading] 
(
	@headingId int,
	@conceptId varchar(200),
	@scheme varchar(200)
)
AS

SET NOCOUNT ON

-- Get the category id for the specified category
DECLARE @categoryId int

SELECT 
	@categoryId = CategoryId 
FROM
	Category
WHERE
	Category.ConceptId = @conceptId
	AND Category.Scheme  =@scheme

-- Add new link from heading to category
IF @categoryId != NULL
BEGIN
	INSERT INTO Heading_Category (HeadingId, CategoryId) VALUES (@headingId, @categoryId)
END

SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_InsertContact]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_InsertContact]
(
	@contactId int out,
	@serviceId int,
	@firstName varchar(35),
	@lastName varchar(35),
	@description varchar(255),
	@phoneArea char(5),
	@phone varchar(9),
	@phoneExtension varchar(8),
	@faxArea char(5),
	@fax varchar(8),
	@faxExtension varchar(8),
	@email varchar(255),
	@emailText varchar(75),
	@paon varchar(100),
	@saon varchar(100),
	@streetDescription varchar(100),
	@locality varchar(35),
	@town varchar(30),
	@county varchar(30),
	@postcode char(8),
	@addressUrl varchar(255),
	@addressUrlText varchar(75)
)
AS
SET NOCOUNT ON
-- ignore a blank submission
IF 
(
	(@firstName IS NULL OR @firstName = '')
	AND (@lastName IS NULL OR @lastName = '')
	AND (@description IS NULL OR @description = '')
	AND (@phoneArea IS NULL OR @phoneArea = '')
	AND (@phone IS NULL OR @phone = '')
	AND (@phoneExtension IS NULL OR @phoneExtension = '')
	AND (@faxArea IS NULL OR @faxArea = '')
	AND (@fax IS NULL OR @fax = '')
	AND (@faxExtension IS NULL OR @faxExtension = '')
	AND (@email IS NULL OR @email = '')
	AND (@emailText IS NULL OR @emailText = '')
	AND (@paon IS NULL OR @paon = '')
	AND (@saon IS NULL OR @saon = '')
	AND (@streetDescription IS NULL OR @streetDescription = '')
	AND (@locality IS NULL OR @locality = '')
	AND (@town IS NULL OR @town = '')
	AND (@county IS NULL OR @county = '')
	AND (@postcode IS NULL OR @postcode = '')
	AND (@addressUrl IS NULL OR @addressUrl = '')
	AND (@addressUrlText IS NULL OR @addressUrlText = '')
)
BEGIN
	RETURN
END
-- create the contact
INSERT INTO Contact
	(ServiceId, FirstName, LastName, ContactDescription, PhoneArea, Phone, PhoneExtension, FaxArea, Fax, FaxExtension, Email, EmailText, 
	PAON, SAON, StreetDescription, Locality, Town, County, Postcode, AddressUrl, AddressUrlText)
VALUES
	(@serviceId, @firstName, @lastName, @description, @phoneArea, @phone, @phoneExtension, @faxArea, @fax, @faxExtension, 
	@email, @emailText, @paon, @saon, @streetDescription, @locality, @town, @county, UPPER(@postcode), @addressUrl, @addressUrlText)

SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_SelectAllHeadings]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[usp_SelectAllHeadings] 
AS
SET NOCOUNT ON
SELECT 
	Heading.HeadingId, Heading.Heading
FROM 
	Heading
ORDER BY
	Heading ASC
SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_SelectAllServices]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[usp_SelectAllServices]
AS
SET NOCOUNT ON
SELECT 
	Service.ServiceId, Service.Service, Service.Authority
FROM 
	Service
ORDER BY
	Service.Service ASC
SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[usp_SelectContactForEdit]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[usp_SelectContactForEdit]
(
	@contactId int
)
AS
SET NOCOUNT ON
SELECT 
	Contact.ServiceId, Contact.ContactId, Contact.FirstName, Contact.ContactDescription, Contact.LastName, 
	Contact.PhoneArea, Contact.Phone, Contact.PhoneExtension, Contact.FaxArea, Contact.Fax, Contact.FaxExtension, Contact.Email, Contact.EmailText,  
	Contact.PAON, Contact.SAON, Contact.StreetDescription, Contact.Locality, Contact.Town, Contact.County, Contact.Postcode,
	Contact.AddressUrl, Contact.AddressUrlText
FROM
	Contact
WHERE
	Contact.ContactId = @contactId
SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_SelectHeadingForEdit]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_SelectHeadingForEdit] 
(
	@headingId int
)
AS
SET NOCOUNT ON
SELECT 
	Heading.Heading, Heading.RedirectUrl, Heading.RedirectTitle, 
	RelatedHeading.HeadingId AS RelatedHeadingId, RelatedHeading.Heading AS RelatedHeading,
	Heading_AZService.SortPriority,
	Service.ServiceId, Service.Service, Service.Authority, 
	Category.CategoryId, Category.CommonName
FROM
	Heading
	LEFT OUTER JOIN Heading_Heading On Heading.HeadingId = Heading_Heading.HeadingId
	LEFT OUTER JOIN Heading AS RelatedHeading ON Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId
	LEFT OUTER JOIN Heading_AZService On Heading.HeadingId = Heading_AZService.HeadingId
	LEFT OUTER JOIN Service ON Heading_AZService.ServiceId = Service.ServiceId
	LEFT OUTER JOIN Heading_Category ON Heading.HeadingId = Heading_Category.HeadingId
	LEFT OUTER JOIN Category ON Heading_Category.CategoryId = Category.CategoryId
WHERE
	Heading.HeadingId = @headingId
ORDER BY 
	Category.CommonName, Heading_AZService.SortPriority DESC
SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_SelectHeadingsByIndex]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_SelectHeadingsByIndex]
(
	@indexChar char(1),
	@eastSussex bit,
	@eastbourne bit,
	@hastings bit,	
	@lewes bit,
	@rother bit,
	@wealden bit
)
AS
SET NOCOUNT ON
-- Check for a query on 0 authorities, and switch to "all authorities" as a default behaviour
IF @eastSussex = 0 AND @eastbourne = 0 AND @hastings = 0 AND @lewes = 0 AND @rother = 0 AND @wealden = 0
BEGIN
	SET @eastSussex = 1
	SET @eastbourne = 1
	SET @hastings = 1
	SET @lewes = 1
	SET @rother = 1
	SET @wealden = 1
END
-- Create table to collect relevant heading ids
DECLARE @relevantHeadings TABLE (HeadingId int)
-- If we're interested in ESCC, add its headings to the collection.
-- Queries for other authorities have the same structure, so only this one is commented
IF @eastSussex = 1
BEGIN
	INSERT @relevantHeadings
		SELECT 
			Heading.HeadingId
		FROM
			-- Get headings and related headings. Admin pages should insert link records in both directions, so join would work either way
			Heading
			LEFT OUTER JOIN Heading_Heading ON Heading.HeadingId = Heading_Heading.HeadingId
			LEFT OUTER JOIN Heading AS RelatedHeading ON Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId 
		WHERE
			-- Get headings beginning with the specified character
			(LOWER(SUBSTRING(Heading.Heading,1,1)) = LOWER(@indexChar))
			-- Look for ESCC services either under this heading or a related heading
			AND 
			(
				(Heading.RedirectUrl IS NOT NULL AND Heading.RedirectUrl <> '')
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
				OR Heading.ServiceCountEscc > 0
				OR RelatedHeading.ServiceCountEscc > 0
			)
END		
-- If we're interested in Eastbourne, add its headings to the collection
IF @eastbourne = 1
BEGIN
	INSERT @relevantHeadings
		SELECT 
			Heading.HeadingId
		FROM
			Heading
			LEFT OUTER JOIN Heading_Heading ON Heading.HeadingId = Heading_Heading.HeadingId
			LEFT OUTER JOIN Heading AS RelatedHeading ON Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId 
		WHERE
			(LOWER(SUBSTRING(Heading.Heading,1,1)) = LOWER(@indexChar))
			AND 
			(
				(Heading.RedirectUrl IS NOT NULL AND Heading.RedirectUrl <> '')
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
				OR Heading.ServiceCountEastbourne > 0 
				OR RelatedHeading.ServiceCountEastbourne > 0
			)
END		
-- If we're interested in Hastings, add its headings to the collection
IF @hastings = 1
BEGIN
	INSERT @relevantHeadings
		SELECT 
			Heading.HeadingId
		FROM
			Heading
			LEFT OUTER JOIN Heading_Heading ON Heading.HeadingId = Heading_Heading.HeadingId
			LEFT OUTER JOIN Heading AS RelatedHeading ON Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId 
		WHERE
			(LOWER(SUBSTRING(Heading.Heading,1,1)) = LOWER(@indexChar))
			AND 
			(
				(Heading.RedirectUrl IS NOT NULL AND Heading.RedirectUrl <> '')
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
				OR Heading.ServiceCountHastings > 0 
				OR RelatedHeading.ServiceCountHastings > 0
			)
END		
-- If we're interested in Lewes, add its headings to the collection
IF @lewes = 1
BEGIN
	INSERT @relevantHeadings
		SELECT 
			Heading.HeadingId
		FROM
			Heading
			LEFT OUTER JOIN Heading_Heading ON Heading.HeadingId = Heading_Heading.HeadingId
			LEFT OUTER JOIN Heading AS RelatedHeading ON Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId 
		WHERE
			(LOWER(SUBSTRING(Heading.Heading,1,1)) = LOWER(@indexChar))
			AND 
			(
				(Heading.RedirectUrl IS NOT NULL AND Heading.RedirectUrl <> '')
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
				OR Heading.ServiceCountLewes > 0 
				OR RelatedHeading.ServiceCountLewes > 0
			)
END		
-- If we're interested in Rother, add its headings to the collection
IF @rother = 1
BEGIN
	INSERT @relevantHeadings
		SELECT 
			Heading.HeadingId
		FROM
			Heading
			LEFT OUTER JOIN Heading_Heading ON Heading.HeadingId = Heading_Heading.HeadingId
			LEFT OUTER JOIN Heading AS RelatedHeading ON Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId 
		WHERE
			(LOWER(SUBSTRING(Heading.Heading,1,1)) = LOWER(@indexChar))
			AND 
			(
				(Heading.RedirectUrl IS NOT NULL AND Heading.RedirectUrl <> '')
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
				OR Heading.ServiceCountRother > 0 
				OR RelatedHeading.ServiceCountRother > 0
			)
END		
-- If we're interested in Wealden, add its headings to the collection
IF @wealden = 1
BEGIN
	INSERT @relevantHeadings
		SELECT 
			Heading.HeadingId
		FROM
			Heading
			LEFT OUTER JOIN Heading_Heading ON Heading.HeadingId = Heading_Heading.HeadingId
			LEFT OUTER JOIN Heading AS RelatedHeading ON Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId 
		WHERE
			(LOWER(SUBSTRING(Heading.Heading,1,1)) = LOWER(@indexChar))
			AND 
			(
				(Heading.RedirectUrl IS NOT NULL AND Heading.RedirectUrl <> '')
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
				OR Heading.ServiceCountWealden > 0 
				OR RelatedHeading.ServiceCountWealden > 0
			)
END		

-- Select complete data for the headings collected in @relevantHeadings
SELECT 
	Heading.HeadingId, Heading.Heading, Heading.RedirectUrl, Heading.RedirectTitle,
	Heading.ServiceCountEscc, Heading.ServiceCountEastbourne, Heading.ServiceCountHastings, Heading.ServiceCountLewes, Heading.ServiceCountRother, Heading.ServiceCountWealden,
	RelatedHeading.HeadingId AS RelatedHeadingId, RelatedHeading.Heading AS RelatedHeading, RelatedHeading.RedirectUrl AS RelatedRedirectUrl, RelatedHeading.RedirectTitle AS RelatedRedirectTitle,
	RelatedHeading.ServiceCountEscc AS RelatedServiceCountEscc, RelatedHeading.ServiceCountEastbourne AS RelatedServiceCountEastbourne, RelatedHeading.ServiceCountHastings AS RelatedServiceCountHastings, 
	RelatedHeading.ServiceCountLewes AS RelatedServiceCountLewes, RelatedHeading.ServiceCountRother AS RelatedServiceCountRother, RelatedHeading.ServiceCountWealden AS RelatedServiceCountWealden
FROM 
	-- Second JOIN checks ServiceCount to ensure we don't get any related headings which don't have services
	Heading
	LEFT OUTER JOIN Heading_Heading ON Heading.HeadingId = Heading_Heading.HeadingId
	LEFT OUTER JOIN Heading AS RelatedHeading ON 
		(
			Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId
			AND
			(
				RelatedHeading.ServiceCountEscc > 0 
				OR RelatedHeading.ServiceCountEastbourne > 0 
				OR RelatedHeading.ServiceCountHastings > 0 
				OR RelatedHeading.ServiceCountLewes > 0 
				OR RelatedHeading.ServiceCountRother > 0 
				OR RelatedHeading.ServiceCountWealden > 0
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
			)
		)
WHERE
	Heading.HeadingId IN 
		(SELECT HeadingId FROM @relevantHeadings)
ORDER BY
	-- Sort alphabetically (for display), but make sure multiple rows are grouped together (for loop control)
	Heading.Heading ASC, Heading.HeadingId ASC, RelatedHeading.Heading ASC
	
SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[usp_SelectHeadingsByIndexForEdit]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_SelectHeadingsByIndexForEdit]
(
	@indexChar char(1)
)
AS
SET NOCOUNT ON
SELECT 
	Heading.HeadingId, Heading.Heading, 
	(Heading.ServiceCountEscc + Heading.ServiceCountEastbourne + Heading.ServiceCountHastings + Heading.ServiceCountLewes + Heading.ServiceCountRother + Heading.ServiceCountWealden) AS ServiceCount
FROM 
	Heading
WHERE
	-- get headings beginning with supplied character
	LOWER(SUBSTRING(Heading.Heading,1,1)) = LOWER(@indexChar)
ORDER BY
	Heading.Heading ASC
SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[usp_SelectHeadingsBySearch]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_SelectHeadingsBySearch]
(
	@search varchar(50),
	@eastSussex bit,
	@eastbourne bit,
	@hastings bit,	
	@lewes bit,
	@rother bit,
	@wealden bit
)
AS
SET NOCOUNT ON
-- Check for a query on 0 authorities, and switch to "all authorities" as a default behaviour
IF @eastSussex = 0 AND @eastbourne = 0 AND @hastings = 0 AND @lewes = 0 AND @rother = 0 AND @wealden = 0
BEGIN
	SET @eastSussex = 1
	SET @eastbourne = 1
	SET @hastings = 1
	SET @lewes = 1
	SET @rother = 1
	SET @wealden = 1
END
-- Create table to collect relevant heading ids
DECLARE @relevantHeadings TABLE (HeadingId int)
-- Exclude the "District and Borough Councils" heading from all searching by its HeadingId
DECLARE @ignoreHeadingId int;
SET @ignoreHeadingId = 577;
-- If we're interested in ESCC, add its headings to the collection.
-- Queries for other authorities have the same structure, so only this one is commented
IF @eastSussex = 1
BEGIN
	INSERT @relevantHeadings
		SELECT 
			Heading.HeadingId
		FROM
			-- Get headings and related headings. Admin pages should insert link records in both directions, so join would work either way
			Heading
			LEFT OUTER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
			LEFT OUTER JOIN Service ON (Heading_AZService.ServiceId = Service.ServiceId AND Service.Authority = 0)
			LEFT OUTER JOIN Heading_Heading ON (Heading.HeadingId = Heading_Heading.HeadingId AND Heading_Heading.RelatedHeadingId <> @ignoreHeadingId)
			LEFT OUTER JOIN Heading AS RelatedHeading ON Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId 
			
			-- This join allows searching of services under related headings
			LEFT OUTER JOIN Heading_AZService AS RelatedHeading_AZService ON RelatedHeading.HeadingId = RelatedHeading_AZService.HeadingId
			LEFT OUTER JOIN Service AS RelatedService ON (RelatedHeading_AZService.ServiceId = RelatedService.ServiceId AND RelatedService.Authority = 0)
		WHERE
			-- Get headings matching search term
			(
				Heading.Heading LIKE '%' + @search + '%'
				OR Service.Service LIKE '%' + @search + '%'
				OR Service.Description LIKE '%' + @search + '%'
				OR Service.Keywords LIKE '%' + @search + '%'
				OR RelatedHeading.Heading LIKE '%' + @search + '%'
				OR RelatedService.Service LIKE '%' + @search + '%'
				OR RelatedService.Description LIKE '%' + @search + '%'
			)
			
			AND Heading.HeadingId <> @ignoreHeadingId
			
			-- Look for ESCC services either under this heading or a related heading
			AND 
			(
				(Heading.RedirectUrl IS NOT NULL AND Heading.RedirectUrl <> '')
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
				OR Heading.ServiceCountEscc > 0 OR 
				RelatedHeading.ServiceCountEscc > 0
			)
END		
-- If we're interested in Eastbourne, add its headings to the collection
IF @eastbourne = 1
BEGIN
	INSERT @relevantHeadings
		SELECT 
			Heading.HeadingId
		FROM
			Heading
			LEFT OUTER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
			LEFT OUTER JOIN Service ON (Heading_AZService.ServiceId = Service.ServiceId AND Service.Authority = 1)
			LEFT OUTER JOIN Heading_Heading ON (Heading.HeadingId = Heading_Heading.HeadingId AND Heading_Heading.RelatedHeadingId <> @ignoreHeadingId)
			LEFT OUTER JOIN Heading AS RelatedHeading ON Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId 
			LEFT OUTER JOIN Heading_AZService AS RelatedHeading_AZService ON RelatedHeading.HeadingId = RelatedHeading_AZService.HeadingId
			LEFT OUTER JOIN Service AS RelatedService ON (RelatedHeading_AZService.ServiceId = RelatedService.ServiceId AND RelatedService.Authority = 1)
		WHERE
			(
				Heading.Heading LIKE '%' + @search + '%'
				OR Service.Service LIKE '%' + @search + '%'
				OR Service.Description LIKE '%' + @search + '%'
				OR Service.Keywords LIKE '%' + @search + '%'
				OR RelatedHeading.Heading LIKE '%' + @search + '%'
				OR RelatedService.Service LIKE '%' + @search + '%'
				OR RelatedService.Description LIKE '%' + @search + '%'
			)
			AND Heading.HeadingId <> @ignoreHeadingId
			AND 
			(
				(Heading.RedirectUrl IS NOT NULL AND Heading.RedirectUrl <> '')
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
				OR Heading.ServiceCountEastbourne > 0 
				OR RelatedHeading.ServiceCountEastbourne > 0
			)
END		
-- If we're interested in Hastings, add its headings to the collection
IF @hastings = 1
BEGIN
	INSERT @relevantHeadings
		SELECT 
			Heading.HeadingId
		FROM
			Heading
			LEFT OUTER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
			LEFT OUTER JOIN Service ON (Heading_AZService.ServiceId = Service.ServiceId AND Service.Authority = 2)
			LEFT OUTER JOIN Heading_Heading ON (Heading.HeadingId = Heading_Heading.HeadingId AND Heading_Heading.RelatedHeadingId <> @ignoreHeadingId)
			LEFT OUTER JOIN Heading AS RelatedHeading ON Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId 
			LEFT OUTER JOIN Heading_AZService AS RelatedHeading_AZService ON RelatedHeading.HeadingId = RelatedHeading_AZService.HeadingId
			LEFT OUTER JOIN Service AS RelatedService ON (RelatedHeading_AZService.ServiceId = RelatedService.ServiceId AND RelatedService.Authority = 2)
		WHERE
			(
				Heading.Heading LIKE '%' + @search + '%'
				OR Service.Service LIKE '%' + @search + '%'
				OR Service.Description LIKE '%' + @search + '%'
				OR Service.Keywords LIKE '%' + @search + '%'
				OR RelatedHeading.Heading LIKE '%' + @search + '%'
				OR RelatedService.Service LIKE '%' + @search + '%'
				OR RelatedService.Description LIKE '%' + @search + '%'
			)
			AND Heading.HeadingId <> @ignoreHeadingId
			AND 
			(
				(Heading.RedirectUrl IS NOT NULL AND Heading.RedirectUrl <> '')
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
				OR Heading.ServiceCountHastings > 0 
				OR RelatedHeading.ServiceCountHastings > 0
			)
END		
-- If we're interested in Lewes, add its headings to the collection
IF @lewes = 1
BEGIN
	INSERT @relevantHeadings
		SELECT 
			Heading.HeadingId
		FROM
			Heading
			LEFT OUTER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
			LEFT OUTER JOIN Service ON (Heading_AZService.ServiceId = Service.ServiceId AND Service.Authority = 3)
			LEFT OUTER JOIN Heading_Heading ON (Heading.HeadingId = Heading_Heading.HeadingId AND Heading_Heading.RelatedHeadingId <> @ignoreHeadingId)
			LEFT OUTER JOIN Heading AS RelatedHeading ON Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId 
			LEFT OUTER JOIN Heading_AZService AS RelatedHeading_AZService ON RelatedHeading.HeadingId = RelatedHeading_AZService.HeadingId
			LEFT OUTER JOIN Service AS RelatedService ON (RelatedHeading_AZService.ServiceId = RelatedService.ServiceId AND RelatedService.Authority = 3)
		WHERE
			(
				Heading.Heading LIKE '%' + @search + '%'
				OR Service.Service LIKE '%' + @search + '%'
				OR Service.Description LIKE '%' + @search + '%'
				OR Service.Keywords LIKE '%' + @search + '%'
				OR RelatedHeading.Heading LIKE '%' + @search + '%'
				OR RelatedService.Service LIKE '%' + @search + '%'
				OR RelatedService.Description LIKE '%' + @search + '%'
			)
			AND Heading.HeadingId <> @ignoreHeadingId
			AND 
			(
				(Heading.RedirectUrl IS NOT NULL AND Heading.RedirectUrl <> '')
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
				OR Heading.ServiceCountLewes > 0 
				OR RelatedHeading.ServiceCountLewes > 0
			)
END		
-- If we're interested in Rother, add its headings to the collection
IF @rother = 1
BEGIN
	INSERT @relevantHeadings
		SELECT 
			Heading.HeadingId
		FROM
			Heading
			LEFT OUTER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
			LEFT OUTER JOIN Service ON (Heading_AZService.ServiceId = Service.ServiceId AND Service.Authority = 4)
			LEFT OUTER JOIN Heading_Heading ON (Heading.HeadingId = Heading_Heading.HeadingId AND Heading_Heading.RelatedHeadingId <> @ignoreHeadingId)
			LEFT OUTER JOIN Heading AS RelatedHeading ON Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId 
			LEFT OUTER JOIN Heading_AZService AS RelatedHeading_AZService ON RelatedHeading.HeadingId = RelatedHeading_AZService.HeadingId
			LEFT OUTER JOIN Service AS RelatedService ON (RelatedHeading_AZService.ServiceId = RelatedService.ServiceId AND RelatedService.Authority = 4)
		WHERE
			(
				Heading.Heading LIKE '%' + @search + '%'
				OR Service.Service LIKE '%' + @search + '%'
				OR Service.Description LIKE '%' + @search + '%'
				OR Service.Keywords LIKE '%' + @search + '%'
				OR RelatedHeading.Heading LIKE '%' + @search + '%'
				OR RelatedService.Service LIKE '%' + @search + '%'
				OR RelatedService.Description LIKE '%' + @search + '%'
			)
			AND Heading.HeadingId <> @ignoreHeadingId
			AND 
			(
				(Heading.RedirectUrl IS NOT NULL AND Heading.RedirectUrl <> '')
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
				OR Heading.ServiceCountRother > 0 
				OR RelatedHeading.ServiceCountRother > 0
			)
END		
-- If we're interested in Wealden, add its headings to the collection
IF @wealden = 1
BEGIN
	INSERT @relevantHeadings
		SELECT 
			Heading.HeadingId
		FROM
			Heading
			LEFT OUTER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
			LEFT OUTER JOIN Service ON (Heading_AZService.ServiceId = Service.ServiceId AND Service.Authority = 5)
			LEFT OUTER JOIN Heading_Heading ON (Heading.HeadingId = Heading_Heading.HeadingId AND Heading_Heading.RelatedHeadingId <> @ignoreHeadingId)
			LEFT OUTER JOIN Heading AS RelatedHeading ON Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId 
			LEFT OUTER JOIN Heading_AZService AS RelatedHeading_AZService ON RelatedHeading.HeadingId = RelatedHeading_AZService.HeadingId
			LEFT OUTER JOIN Service AS RelatedService ON (RelatedHeading_AZService.ServiceId = RelatedService.ServiceId AND RelatedService.Authority = 5)
		WHERE
			(
				Heading.Heading LIKE '%' + @search + '%'
				OR Service.Service LIKE '%' + @search + '%'
				OR Service.Description LIKE '%' + @search + '%'
				OR Service.Keywords LIKE '%' + @search + '%'
				OR RelatedHeading.Heading LIKE '%' + @search + '%'
				OR RelatedService.Service LIKE '%' + @search + '%'
				OR RelatedService.Description LIKE '%' + @search + '%'
			)
			AND Heading.HeadingId <> @ignoreHeadingId
			AND 
			(
				(Heading.RedirectUrl IS NOT NULL AND Heading.RedirectUrl <> '')
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
				OR Heading.ServiceCountWealden > 0
				OR RelatedHeading.ServiceCountWealden > 0
			)
END		

-- Select complete data for the headings collected in @relevantHeadings
SELECT 
	Heading.HeadingId, Heading.Heading, Heading.RedirectUrl, Heading.RedirectTitle,
	Heading.ServiceCountEscc, Heading.ServiceCountEastbourne, Heading.ServiceCountHastings, Heading.ServiceCountLewes, Heading.ServiceCountRother, Heading.ServiceCountWealden,
	RelatedHeading.HeadingId AS RelatedHeadingId, RelatedHeading.Heading AS RelatedHeading, RelatedHeading.RedirectUrl AS RelatedRedirectUrl, RelatedHeading.RedirectTitle AS RelatedRedirectTitle,
	RelatedHeading.ServiceCountEscc AS RelatedServiceCountEscc, RelatedHeading.ServiceCountEastbourne AS RelatedServiceCountEastbourne, RelatedHeading.ServiceCountHastings AS RelatedServiceCountHastings, 
	RelatedHeading.ServiceCountLewes AS RelatedServiceCountLewes, RelatedHeading.ServiceCountRother AS RelatedServiceCountRother, RelatedHeading.ServiceCountWealden AS RelatedServiceCountWealden
FROM 
	-- Second JOIN checks ServiceCount to ensure we don't get any related headings which don't have services
	Heading
	LEFT OUTER JOIN Heading_Heading ON Heading.HeadingId = Heading_Heading.HeadingId
	LEFT OUTER JOIN Heading AS RelatedHeading ON 
		(
			Heading_Heading.RelatedHeadingId = RelatedHeading.HeadingId
			AND
			(
				RelatedHeading.ServiceCountEscc > 0 
				OR RelatedHeading.ServiceCountEastbourne > 0 
				OR RelatedHeading.ServiceCountHastings > 0 
				OR RelatedHeading.ServiceCountLewes > 0 
				OR RelatedHeading.ServiceCountRother > 0 
				OR RelatedHeading.ServiceCountWealden > 0
				OR (RelatedHeading.RedirectUrl IS NOT NULL AND RelatedHeading.RedirectUrl <> '')
			)
		)
WHERE
	Heading.HeadingId IN 
		(SELECT HeadingId FROM @relevantHeadings)
ORDER BY
	-- Sort alphabetically (for display), but make sure multiple rows are grouped together (for loop control)
	Heading.Heading ASC, Heading.HeadingId ASC, RelatedHeading.Heading ASC
	
SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[usp_SelectOrphanedServicesForEdit]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_SelectOrphanedServicesForEdit]
AS
SET NOCOUNT ON
SELECT 
	Service.ServiceId, Service.Service, Service.IpsvImported, Service.Authority
FROM 
	Service 
	LEFT OUTER JOIN Heading_AZService ON Service.ServiceId = Heading_AZService.ServiceId
	LEFT OUTER JOIN Heading ON Heading_AZService.HeadingId = Heading.HeadingId
WHERE 
	-- only interested in rows where there's no heading
	Heading.HeadingId IS NULL
ORDER BY
	Service.Service ASC
SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_SelectServiceForEdit]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_SelectServiceForEdit]
(
	@serviceId int
)
AS
SET NOCOUNT ON
SELECT 
	Service.ServiceId, Service.Service, Service.Description, Service.Keywords, Service.Authority, Service.IpsvImported,
	Heading.HeadingId, Heading.Heading, 
	ServiceUrl.ServiceUrlId, ServiceUrl.Url, ServiceUrl.UrlTitle, ServiceUrl.UrlDescription,
	Contact.ContactId, Contact.FirstName, Contact.LastName, Contact.ContactDescription, Contact.PhoneArea, Contact.Phone, Contact.PhoneExtension, 
	Contact.FaxArea, Contact.Fax, Contact.FaxExtension, Contact.Email, Contact.EmailText, 
	Contact.PAON, Contact.SAON, Contact.StreetDescription, Contact.Locality, Contact.Town, Contact.County, Contact.Postcode,
	Contact.AddressUrl, Contact.AddressUrlText
FROM
	Service
	LEFT OUTER JOIN Heading_AZService ON Service.ServiceId = Heading_AZService.ServiceId
	LEFT OUTER JOIN Heading ON Heading_AZService.HeadingId = Heading.HeadingId
	LEFT OUTER JOIN ServiceUrl ON Service.ServiceId = ServiceUrl.ServiceId
	LEFT OUTER JOIN Contact ON Service.ServiceId = Contact.ServiceId
WHERE
	Service.ServiceId = @serviceId
ORDER BY
	ServiceUrl.SortPriority DESC, ServiceUrl.UrlTitle ASC
SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[usp_SelectServicesByIndexForEdit]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_SelectServicesByIndexForEdit]
(
	@indexChar char(1)
)
AS
SET NOCOUNT ON
SELECT 
	Service.ServiceId, Service.Service, Service.Authority
FROM 
	Service
WHERE
	-- get services beginning with supplied character
	LOWER(SUBSTRING(Service.Service,1,1)) = LOWER(@indexChar)
ORDER BY
	Service.Service ASC
SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_SelectServicesForExport]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[usp_SelectServicesForExport]
AS

SET NOCOUNT ON

SELECT 
	Heading.HeadingId, Heading.Heading, Heading.IpsvNonPreferredTerms, Heading.GclCategories,
	Service.ServiceId, Service.Service, Service.Description, Service.Keywords, Service.Authority, Service.SortPriority,
	ServiceUrl.ServiceUrlId, ServiceUrl.Url, ServiceUrl.UrlTitle, ServiceUrl.UrlDescription,
	Contact.ContactId, Contact.FirstName, Contact.LastName, Contact.ContactDescription, 
	Contact.PhoneArea, Contact.Phone, Contact.PhoneExtension, Contact.FaxArea, Contact.Fax, Contact.FaxExtension, Contact.Email, Contact.EmailText,
	Contact.PAON, Contact.SAON, Contact.StreetDescription, Contact.Locality, Contact.Town, Contact.County, Contact.Postcode,
	Contact.AddressUrl, Contact.AddressUrlText,
	Category.Identifier, Category.CommonName
FROM
	Heading 
	INNER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
	INNER JOIN Heading_Category ON Heading.HeadingId = Heading_Category.HeadingId
	INNER JOIN Category ON Heading_Category.CategoryId = Category.CategoryId
	INNER JOIN Service ON Heading_AZService.ServiceId = Service.ServiceId
	LEFT OUTER JOIN ServiceUrl ON Service.ServiceId = ServiceUrl.ServiceId
	LEFT OUTER JOIN Contact ON Service.ServiceId = Contact.ServiceId
WHERE 
	Service.Authority = 0
ORDER BY
	-- Sort by heading and service to keep multiple rows of data for each service together,
    -- and sort by ServiceUrl fields so that sort order specified by editors is retained: 
    -- important because, at the time this process was set up, only the first one gets imported
    -- into the shared A-Z and editors need to be able to control which one that will be.
    Heading.HeadingId, Service.ServiceId, ServiceUrl.SortPriority DESC, ServiceUrl.UrlTitle ASC

SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[usp_SelectServicesForHeading]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_SelectServicesForHeading]
(
	@headingId int,
	@eastSussex bit,
	@eastbourne bit,
	@hastings bit,	
	@lewes bit,
	@rother bit,
	@wealden bit,
	@totalServices int OUTPUT
)
AS
SET NOCOUNT ON

-- SQL Server won't allow the main query to be sorted directly, so instead the main
-- query is selected into a temporary table, and then the contents of the temporary
-- table are selected with a sort order. This also enables us to count all the 
-- available services in the first query (it should always be less than 20 or so)
-- and then filter by authority in the second.
-- This is a temporary table
DECLARE @sortableData TABLE 
	(
	HeadingId int, Heading varchar(255), IpsvNonPreferredTerms varchar(1000), ServiceSortPriority int,
	ServiceId int, Service varchar(250), Description varchar(2500), Keywords varchar(250), Authority tinyint, SortPriority int,
	ServiceUrlId int, Url varchar(255), UrlTitle varchar(75), UrlDescription varchar(300), UrlSort tinyint,
	ContactId int, FirstName varchar(35), LastName varchar(35), ContactDescription varchar(255), 
	PhoneArea char(5), Phone varchar(9), PhoneExtension varchar(8), FaxArea char(5), Fax varchar(8), FaxExtension varchar(8), Email varchar(255), EmailText varchar(75),
	PAON varchar(100), SAON varchar(100), StreetDescription varchar(100), Locality varchar(35), Town varchar(30), County varchar(30), Postcode char(8), 
	AddressUrl varchar(255), AddressUrlText varchar(75),
	Identifier varchar(200), CommonName varchar(100)
	)
-- This is the main query
INSERT @sortableData
	SELECT 
		Heading.HeadingId, Heading.Heading, Heading.IpsvNonPreferredTerms, Heading_AZService.SortPriority,
		Service.ServiceId, Service.Service, Service.Description, Service.Keywords, Service.Authority, Service.SortPriority,
		ServiceUrl.ServiceUrlId, ServiceUrl.Url, ServiceUrl.UrlTitle, ServiceUrl.UrlDescription, ServiceUrl.SortPriority,
		Contact.ContactId, Contact.FirstName, Contact.LastName, Contact.ContactDescription, 
		Contact.PhoneArea, Contact.Phone, Contact.PhoneExtension, Contact.FaxArea, Contact.Fax, Contact.FaxExtension, Contact.Email, Contact.EmailText,
		Contact.PAON, Contact.SAON, Contact.StreetDescription, Contact.Locality, Contact.Town, Contact.County, Contact.Postcode, 
		Contact.AddressUrl, Contact.AddressUrlText,
		Category.Identifier, Category.CommonName
	FROM
		Heading 
		INNER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
		INNER JOIN Service ON Heading_AZService.ServiceId = Service.ServiceId
		INNER JOIN Heading_Category ON Heading.HeadingId = Heading_Category.HeadingId
		INNER JOIN Category ON Heading_Category.CategoryId = Category.CategoryId
		LEFT OUTER JOIN ServiceUrl ON Service.ServiceId = ServiceUrl.ServiceId
		LEFT OUTER JOIN Contact ON Service.ServiceId = Contact.ServiceId
	WHERE
		Heading.HeadingId = @headingId
-- Find out how many services in total, excluding duplicate rows due to relationships
SELECT
	@totalServices = COUNT(ServiceId) 
FROM
(
	SELECT 
		ServiceId
	FROM 
		@sortableData
	GROUP BY 
		Serviceid
) 
AS ServiceData
-- Check for a query on 0 authorities, and switch to "all authorities" as a default behaviour
IF @eastSussex = 0 AND @eastbourne = 0 AND @hastings = 0 AND @lewes = 0 AND @rother = 0 AND @wealden = 0
BEGIN
	SET @eastSussex = 1
	SET @eastbourne = 1
	SET @hastings = 1
	SET @lewes = 1
	SET @rother = 1
	SET @wealden = 1
END
-- Set id for each authority to 255, which will not be found in a search
DECLARE @eastSussexTerm tinyint
DECLARE @eastbourneTerm tinyint
DECLARE @hastingsTerm tinyint
DECLARE @lewesTerm tinyint
DECLARE @rotherTerm tinyint
DECLARE @wealdenTerm tinyint
SET @eastSussexTerm = 255
SET @eastbourneTerm = 255
SET @hastingsTerm = 255
SET @lewesTerm = 255
SET @rotherTerm = 255
SET @wealdenTerm = 255
-- For selected authorities, replace the dummy id with the real id to match
IF @eastSussex = 1 SET @eastSussexTerm = 0
IF @eastbourne = 1 SET @eastbourneTerm = 1
IF @hastings = 1 SET @hastingsTerm =2
IF @lewes = 1 SET @lewesTerm = 3
IF @rother = 1 SET @rotherTerm = 4
IF @wealden = 1 SET @wealdenTerm = 5
-- This sorts, filters out unwanted councils, and returns the data
SELECT 
		HeadingId, Heading, IpsvNonPreferredTerms, 
		ServiceId, Service, Description, Keywords, Authority, SortPriority,
		ServiceUrlId, Url, UrlTitle, UrlDescription,
		ContactId, FirstName, LastName, ContactDescription, 
		PhoneArea, Phone, PhoneExtension, FaxArea, Fax, FaxExtension, Email, EmailText,
		PAON, SAON, StreetDescription, Locality, Town, County, Postcode, 
		AddressUrl, AddressUrlText,
		Identifier, CommonName
FROM 
	@sortableData
WHERE
		Authority IN (@eastSussexTerm, @eastbourneTerm, @hastingsTerm, @lewesTerm, @rotherTerm, @wealdenTerm)
ORDER BY 
	SortPriority DESC, ServiceSortPriority DESC, Service ASC, ServiceId ASC, UrlSort DESC, UrlTitle ASC
SET NOCOUNT OFF
GO
/****** Object:  StoredProcedure [dbo].[usp_ServiceAddIpsvImported]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_ServiceAddIpsvImported]
(
	@serviceId int,
	@ipsvTerm varchar(100)
)
AS

/*
This SP is used when importing district and borough services. Editors need to see which IPSV terms the districts
and boroughs have assigned to their services, so that they can influence which headings they'll be assigned to.

That's the sole purpose of the IpsvImported field. IPSV terms for the live A-Z come from the 
Heading <--> Heading_Category <--> Category relationship.
*/


/* We're going to use the counter, so make sure it's enabled */
SET NOCOUNT OFF

/* If there are no terms already, insert the supplied term */
UPDATE 
	Service 
SET
	IpsvImported = @ipsvTerm
WHERE
	ServiceId = @serviceId
	AND (IpsvImported IS NULL OR LEN(IpsvImported) = 0)

/* If there are terms already, append the supplied term */
IF @@ROWCOUNT = 0 
	UPDATE
		Service
	SET
		IpsvImported = IpsvImported + '; ' + @ipsvTerm
	WHERE
		ServiceId = @serviceId

GO
/****** Object:  StoredProcedure [dbo].[usp_ServiceInsert]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[usp_ServiceInsert]
(
	@serviceId int out,
	@service varchar(250),
	@description varchar(2500),
	@keywords varchar(250),
	@authorityId tinyint,
	@sortPriority tinyint,
	@headingId int = NULL,
	@url varchar(255) = NULL,
	@urlTitle varchar(75) = '',
	@urlDescription varchar(300) = NULL,
	@ipsv bit = 0
)
AS
SET NOCOUNT ON
-- multi command operation, so use a transaction
BEGIN TRAN
-- write flatfile service data
INSERT INTO Service 
	(Service, Description, Keywords, Authority, SortPriority, Ipsv)
VALUES (LTRIM(RTRIM(@service)), LTRIM(RTRIM(@description)), LTRIM(RTRIM(@keywords)), @authorityId, @sortPriority, @ipsv)
SELECT @serviceId = @@IDENTITY
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- add link to heading, if specified
IF @headingId IS NOT NULL
BEGIN
	EXEC usp_ServiceRelateToHeading @serviceId, @headingId
END
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- add url, if specified
IF @url IS NOT NULL
BEGIN
	EXEC usp_UrlInsert @serviceId, @url, @urlTitle, @urlDescription
END
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- all ok, so write the changes
COMMIT TRAN
SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[usp_ServiceRelateToCategory]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[usp_ServiceRelateToCategory] 
(
	@serviceId int,
	@categoryIdentifier varchar(200),
	@scheme varchar(200)
)
AS

SET NOCOUNT ON

-- Get the category id for the specified category
DECLARE @categoryId int

SELECT 
	@categoryId = CategoryId 
FROM
	Category
WHERE
	Category.Identifier = @categoryIdentifier
	AND Category.Scheme  =@scheme

IF @categoryId != NULL
BEGIN
	
	-- Get headings linked to the category
	DECLARE @headings TABLE (RowId int Identity(1,1) PRIMARY KEY, HeadingId int)

	INSERT INTO @headings
		SELECT HeadingId FROM Heading_Category WHERE CategoryId = @categoryId

	-- Foreach heading, create a relationship between the heading and the service
	DECLARE @headingCount int, @rowId int, @headingId int
	SELECT @headingCount = ROWCOUNT_BIG()
	SET @rowId = 1

	WHILE @rowId <= @headingCount
	BEGIN
		
		SELECT @headingId = HeadingId FROM @headings WHERE RowId = @rowId
		EXEC usp_ServiceRelateToHeading @serviceId, @headingId

		-- recalculate the ServiceCount fields
		EXEC usp_UpdateDerivedServiceCount @headingId
		
		SET @rowId = @rowId + 1

	END
END

SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_ServiceRelateToHeading]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_ServiceRelateToHeading]
(
	@serviceId int,
	@headingId int
)
AS

-- add link to heading, if specified
IF @serviceId IS NOT NULL AND @headingId IS NOT NULL
BEGIN
	SELECT 
		HeadingId 
	FROM 
		Heading_AZService
	WHERE
		HeadingId = @headingId AND ServiceId = @serviceId

	IF @@ERROR = 0 AND @@ROWCOUNT = 0

	BEGIN
		BEGIN TRAN

		INSERT INTO Heading_AZService 
			(HeadingId, ServiceId)
		VALUES
			(@headingId, @serviceId)
	
		-- Roll back transaction if there were errors
		IF @@error != 0
		BEGIN
			ROLLBACK TRAN
			RETURN
		END

		-- recalculate the ServiceCount fields
		EXEC usp_UpdateDerivedServiceCount @headingId

		-- Roll back transaction if there were errors
		IF @@error != 0
		BEGIN
			ROLLBACK TRAN
			RETURN
		END

		-- All ok, so save
		COMMIT TRAN
	END
END

GO
/****** Object:  StoredProcedure [dbo].[usp_ServicesDeleteForAuthority]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[usp_ServicesDeleteForAuthority]
(
	@authorityId tinyint
)
AS

-- Deletes all services for an entire authority. Use with care!

-- Designed for use when importing data from other authorities. 
-- All data for that authority is deleted using this SP, then it's all
-- recreated from their XML export.

SET NOCOUNT ON

-- Multi-command operation, so use a transaction
BEGIN TRAN

-- Delete related urls
DELETE FROM 
	ServiceUrl 
WHERE 
	ServiceId IN 
	(
		SELECT ServiceId 
		FROM Service
		WHERE Authority = @authorityId
	)

-- Roll back transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- Delete related contacts
DELETE FROM 
	Contact
WHERE 
	ServiceId IN 
	(
		SELECT ServiceId 
		FROM Service
		WHERE Authority = @authorityId
	)

-- Roll back transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- Delete links to headings
DELETE FROM 
	Heading_AZService
WHERE 
	ServiceId IN 
	(
		SELECT ServiceId 
		FROM Service
		WHERE Authority = @authorityId
	)

-- Roll back transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- Delete the services themselves
DELETE FROM 
	Service
WHERE
	Authority = @authorityId

-- Roll back transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- Audit this big deletion
INSERT INTO dbo.Audit
(AuditDate, StoredProcedureName, AuditDetails)
VALUES
(GETDATE(), 'usp_ServicesDeleteForAuthority', 'AuthorityId=' + CONVERT(nvarchar, @authorityId))

-- Roll back transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- All ok, so write the changes
COMMIT TRAN
SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_ServiceSortWithinHeading]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Rick Mason, Web Team
-- Create date: 17 Dec 2008
-- Description:	When several services are related to a heading, the order they appear in 
--              under that heading is controlled by the Headind_AZService.SortPriority field.
--              This SP allows update of that field, and therefore of the sort order.
-- =============================================
CREATE PROCEDURE [dbo].[usp_ServiceSortWithinHeading]
	@serviceId int,
	@headingId int,
	@sortPriority int
AS
BEGIN
	UPDATE Heading_AZService SET SortPriority = @sortPriority, LastAmended = GETDATE() WHERE HeadingId = @headingId AND ServiceId = @serviceId
END


GO
/****** Object:  StoredProcedure [dbo].[usp_ServiceUpdate]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[usp_ServiceUpdate]
(
	@serviceId int,
	@service varchar(250),
	@description varchar(2500),
	@keywords varchar(250),
	@authorityId tinyint,
	@sortPriority tinyint,
	@headingId int = NULL,
	@url varchar(255) = NULL,
	@urlTitle varchar(75) = '',
	@urlDescription varchar(300) = NULL,
	@ipsv bit = 0
)
AS
SET NOCOUNT ON
-- multi command operation, so use a transaction
BEGIN TRAN
-- write flatfile service data
UPDATE
	Service 
SET
	Service.Service = LTRIM(RTRIM(@service)),
	Service.Description = LTRIM(RTRIM(@description)),
	Service.Keywords = LTRIM(RTRIM(@keywords)),
	Service.Authority = @authorityId,
	Service.SortPriority = @sortPriority,
	Service.Ipsv = @ipsv,
	Service.LastAmended = GETDATE()
WHERE
	Service.ServiceId = @serviceId
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- add link to heading, if specified
IF @headingId IS NOT NULL
BEGIN
	EXEC usp_ServiceRelateToHeading @serviceId, @headingId
END
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END
-- add url, if specified
IF @url IS NOT NULL
BEGIN
	EXEC usp_UrlInsert @serviceId, @url, @urlTitle, @urlDescription
END
-- rollback transaction if there were errors
IF @@error != 0
BEGIN
	ROLLBACK TRAN
	RETURN
END

-- all ok, so write the changes
COMMIT TRAN
SET NOCOUNT OFF


GO
/****** Object:  StoredProcedure [dbo].[usp_ServiceUpdateIpsv]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Rick Mason
-- Create date: 16 June 2006
-- Description:	Update the IPSV field for a specified service
-- =============================================
CREATE PROCEDURE [dbo].[usp_ServiceUpdateIpsv] 
	-- Add the parameters for the stored procedure here
	@serviceId int, 
	@ipsv bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	UPDATE Service SET Ipsv = @ipsv WHERE Service.ServiceId = @serviceId
END


GO
/****** Object:  StoredProcedure [dbo].[usp_UpdateContact]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_UpdateContact]
(
	@contactId int,
	@serviceId int,
	@firstName varchar(35),
	@lastName varchar(35),
	@description varchar(255),
	@phoneArea char(5),
	@phone varchar(9),
	@phoneExtension varchar(8),
	@faxArea char(5),
	@fax varchar(8),
	@faxExtension varchar(8),
	@email varchar(255),
	@emailText varchar(75),
	@paon varchar(100),
	@saon varchar(100),
	@streetDescription varchar(100),
	@locality varchar(35),
	@town varchar(30),
	@county varchar(30),
	@postcode char(8),
	@addressUrl varchar(255),
	@addressUrlText varchar(75)
)
AS
SET NOCOUNT ON
-- treat a blank submission as request to delete contact
IF 
(
	(@firstName IS NULL OR @firstName = '')
	AND (@lastName IS NULL OR @lastName = '')
	AND (@description IS NULL OR @description = '')
	AND (@phoneArea IS NULL OR @phoneArea = '')
	AND (@phone IS NULL OR @phone = '')
	AND (@phoneExtension IS NULL OR @phoneExtension = '')
	AND (@faxArea IS NULL OR @faxArea = '')
	AND (@fax IS NULL OR @fax = '')
	AND (@faxExtension IS NULL OR @faxExtension = '')
	AND (@email IS NULL OR @email = '')
	AND (@emailText IS NULL OR @emailText = '')
	AND (@paon IS NULL OR @paon = '')
	AND (@saon IS NULL OR @saon = '')
	AND (@streetDescription IS NULL OR @streetDescription = '')
	AND (@locality IS NULL OR @locality = '')
	AND (@town IS NULL OR @town = '')
	AND (@county IS NULL OR @county = '')
	AND (@postcode IS NULL OR @postcode = '')
	AND (@addressUrl IS NULL OR @addressUrl = '')
	AND (@addressUrlText IS NULL OR @addressUrlText = '')
)
BEGIN
	EXEC usp_DeleteContact @contactId
END
-- otherwise update the contact
UPDATE Contact
SET
	Contact.FirstName = @firstName,
	Contact.LastName = @lastName,
	Contact.ContactDescription = @description, 
	Contact.PhoneArea = @phoneArea,
	Contact.Phone = @phone,
	Contact.PhoneExtension = @phoneExtension,
	Contact.FaxArea = @faxArea,
	Contact.Fax = @fax,
	Contact.FaxExtension = @faxExtension,
	Contact.Email = @email,
	Contact.EmailText = @emailText,
	Contact.PAON = @paon,
	Contact.SAON = @saon,
	Contact.StreetDescription = @streetDescription,
	Contact.Locality = @locality,
	Contact.Town = @town,
	Contact.County = @county,
	Contact.Postcode = UPPER(@postcode),
	Contact.AddressUrl = @addressUrl,
	Contact.AddressUrlText = @addressUrlText,
	Contact.LastAmended = GETDATE()
WHERE
	Contact.ContactId = @contactId
SET NOCOUNT OFF

GO
/****** Object:  StoredProcedure [dbo].[usp_UpdateDerivedServiceCount]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_UpdateDerivedServiceCount] 
(
	@headingId int
)
AS
-- Heading.ServiceCount* fields are derived data (for performance), so calling this regularly helps to keep them up-to-date.
-- This counts the services actually assigned and uses that value, rather than basing any updated ServiceCount on an 
-- existing value of ServiceCount.
DECLARE @serviceCount int
-- 1: Update ESCC ServiceCount
SELECT 
	@serviceCount = COUNT(Service.ServiceId) 
FROM 
	Heading INNER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
	INNER JOIN Service on Heading_AZService.ServiceId = Service.ServiceId
WHERE 
	Service.Authority = 0
	AND Heading.HeadingId = @headingId
GROUP BY
	Heading.HeadingId
IF @serviceCount IS NULL SET @serviceCount = 0
UPDATE Heading
SET Heading.ServiceCountEscc = @serviceCount
WHERE Heading.HeadingId = @headingId

-- 2: Update Eastbourne ServiceCount
SELECT @serviceCount = 0
SELECT 
	@serviceCount = COUNT(Service.ServiceId) 
FROM 
	Heading INNER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
	INNER JOIN Service on Heading_AZService.ServiceId = Service.ServiceId
WHERE 
	Service.Authority = 1
	AND Heading.HeadingId = @headingId
GROUP BY
	Heading.HeadingId
IF @serviceCount IS NULL SET @serviceCount = 0
UPDATE Heading
SET Heading.ServiceCountEastbourne = @serviceCount
WHERE Heading.HeadingId = @headingId

-- 3: Update Hastings ServiceCount
SELECT @serviceCount = 0
SELECT 
	@serviceCount = COUNT(Service.ServiceId) 
FROM 
	Heading INNER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
	INNER JOIN Service on Heading_AZService.ServiceId = Service.ServiceId
WHERE 
	Service.Authority = 2
	AND Heading.HeadingId = @headingId
GROUP BY
	Heading.HeadingId
IF @serviceCount IS NULL SET @serviceCount = 0
UPDATE Heading
SET Heading.ServiceCountHastings = @serviceCount
WHERE Heading.HeadingId = @headingId

-- 4: Update Lewes ServiceCount
SELECT @serviceCount = 0
SELECT 
	@serviceCount = COUNT(Service.ServiceId) 
FROM 
	Heading INNER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
	INNER JOIN Service on Heading_AZService.ServiceId = Service.ServiceId
WHERE 
	Service.Authority = 3
	AND Heading.HeadingId = @headingId
GROUP BY
	Heading.HeadingId
IF @serviceCount IS NULL SET @serviceCount = 0
UPDATE Heading
SET Heading.ServiceCountLewes = @serviceCount
WHERE Heading.HeadingId = @headingId

-- 5: Update Rother ServiceCount
SELECT @serviceCount = 0
SELECT 
	@serviceCount = COUNT(Service.ServiceId) 
FROM 
	Heading INNER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
	INNER JOIN Service on Heading_AZService.ServiceId = Service.ServiceId
WHERE 
	Service.Authority = 4
	AND Heading.HeadingId = @headingId
GROUP BY
	Heading.HeadingId
IF @serviceCount IS NULL SET @serviceCount = 0
UPDATE Heading
SET Heading.ServiceCountRother = @serviceCount
WHERE Heading.HeadingId = @headingId

-- 6: Update Wealden ServiceCount
SELECT @serviceCount = 0
SELECT 
	@serviceCount = COUNT(Service.ServiceId) 
FROM 
	Heading INNER JOIN Heading_AZService ON Heading.HeadingId = Heading_AZService.HeadingId
	INNER JOIN Service on Heading_AZService.ServiceId = Service.ServiceId
WHERE 
	Service.Authority = 5
	AND Heading.HeadingId = @headingId
GROUP BY
	Heading.HeadingId
IF @serviceCount IS NULL SET @serviceCount = 0
UPDATE Heading
SET Heading.ServiceCountWealden = @serviceCount
WHERE Heading.HeadingId = @headingId

GO
/****** Object:  StoredProcedure [dbo].[usp_UpdateDerivedServiceCountAllHeadings]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_UpdateDerivedServiceCountAllHeadings]
AS
-- Run this manually if any ServiceCounts are out-of-date, eg if the A-Z is linking to
-- headings which have no services.
--
-- usp_UpdateDerivedServiceCount recalculates the ServiceCount fields for a single heading.
-- This loops through every Heading and runs usp_UpdateDerivedServiceCount
SET NOCOUNT ON
DECLARE @headingId int
DECLARE headingCursor CURSOR LOCAL FORWARD_ONLY STATIC READ_ONLY FOR
	SELECT 
		HeadingId
	FROM 
		Heading
	
OPEN headingCursor
	
	FETCH NEXT FROM headingCursor
	INTO @headingId
	
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC usp_UpdateDerivedServiceCount @headingId
	  
		IF @@ERROR <> 0
		BEGIN
			CLOSE headingCursor
			DEALLOCATE headingCursor
			RETURN 1
		END
		FETCH NEXT FROM headingCursor
	INTO @headingId
	END
CLOSE headingCursor
DEALLOCATE headingCursor
SET NOCOUNT OFF
RETURN 0


GO
/****** Object:  StoredProcedure [dbo].[usp_UrlInsert]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_UrlInsert] 
(
	@serviceId int,
	@url varchar(255) ,
	@urlTitle varchar(75) ,
	@urlDescription varchar(300)
)
AS

/* Work out whether the url belongs to ESCC, and whether it's a form */
DECLARE @isEsccForm bit, @authorityId tinyint

SELECT 
	@authorityId = Authority 
FROM 
	Service 
WHERE 
	ServiceId = @serviceId

IF @authorityId = 0 AND LEFT(LOWER(@urlTitle), 5) = 'form '
	SELECT @isEsccForm = 1
ELSE
	SELECT @isEsccForm = 0

/* Work out the highest sort priority, and make this one higher (ie: last) */
DECLARE @sort tinyint
SELECT 
	@sort = MAX(SortPriority)
FROM 
	ServiceUrl
WHERE
	ServiceId = @serviceId

IF @sort IS NULL 
	SET @sort = 0
ELSE
	SET @sort = @sort+1

/* Insert the new url */
INSERT INTO ServiceUrl
	(ServiceId, Url, UrlTitle, UrlDescription, IsEsccForm, SortPriority)
VALUES
	(@serviceId, @url, @urlTitle, @urlDescription, @isEsccForm, @sort)

GO
/****** Object:  StoredProcedure [dbo].[usp_UrlSelectForms]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[usp_UrlSelectForms]
(
	@indexChars varchar(5)
)
AS

/*
Gets links to ESCC forms which start with the specified letter(s) 
All the LEFT/RIGHT stuff is because the titles start with 'Form - ', 
which makes sense in contexts other than this one where everything is a form.
*/
SELECT
	Url, UPPER(LEFT(RIGHT(UrlTitle, LEN(UrlTitle)-7) ,1)) + RIGHT(UrlTitle, LEN(UrlTitle)-8) AS UrlTitle, UrlDescription
FROM
	ServiceUrl
WHERE
	IsEsccForm = 1
	AND LEN (UrlTitle) > 6
	AND LOWER(LEFT(RIGHT(UrlTitle, LEN(UrlTitle)-7) ,1)) LIKE '[' + @indexChars + ']%'
ORDER BY 
	UrlTitle ASC
GO
/****** Object:  StoredProcedure [dbo].[usp_UrlSelectFormsForEdit]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[usp_UrlSelectFormsForEdit]
AS

/*
Gets all ESCC forms ready for editing their Popularity field.
All the LEFT/RIGHT stuff is because the titles start with 'Form - ', 
which makes sense in contexts other than this one where everything is a form.
*/
SELECT
	ServiceUrlId, UPPER(LEFT(RIGHT(UrlTitle, LEN(UrlTitle)-7) ,1)) + RIGHT(UrlTitle, LEN(UrlTitle)-8) AS UrlTitle, ServiceUrl.SortPriority,
	Service.ServiceId, Service.Service
FROM
	ServiceUrl INNER JOIN Service ON ServiceUrl.ServiceId = Service.ServiceId
WHERE
	IsEsccForm = 1
ORDER BY 
	Popularity DESC

GO
/****** Object:  StoredProcedure [dbo].[usp_UrlSelectForScanner]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Rick Mason, Web Team
-- Create date: 7 April 2009
-- Description:	Selects all links in the A-Z to include in the CMS link scanner used by web co-ordinators.
--              See project in SourceSafe EsccWebTeam.Cms.CmsScanner.
-- =============================================
CREATE PROCEDURE [dbo].[usp_UrlSelectForScanner]
	AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT Heading.HeadingId, Heading, Service, Service.ServiceId, Url, UrlTitle 
	FROM ServiceUrl
	INNER JOIN Service on ServiceUrl.ServiceId = Service.ServiceId
	INNER JOIN Heading_AZService ON Service.ServiceId = Heading_AZService.ServiceId
	INNER JOIN Heading ON Heading_AZService.HeadingId = Heading.HeadingId
	ORDER BY Heading.Heading
END


GO
/****** Object:  StoredProcedure [dbo].[usp_UrlSelectPopularForms]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
CREATE PROCEDURE [dbo].[usp_UrlSelectPopularForms]
AS

/*
Gets links to ESCC forms which have a Popularity > 0. These are forms which have been manually selected as 'popular'.
All the LEFT/RIGHT stuff is because the titles start with 'Form - ', 
which makes sense in contexts other than this one where everything is a form.
*/
SELECT
	Url, UPPER(LEFT(RIGHT(UrlTitle, LEN(UrlTitle)-7) ,1)) + RIGHT(UrlTitle, LEN(UrlTitle)-8) AS UrlTitle
FROM
	ServiceUrl
WHERE
	IsEsccForm = 1
	AND Popularity > 0
ORDER BY 
	Popularity DESC
GO
/****** Object:  StoredProcedure [dbo].[usp_UrlSort]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[usp_UrlSort] 
(
	@urlId int,
	@sortUp bit
)
AS

-- Get current details of URL
DECLARE @serviceId int, @currentSort int, @urlTitle varchar(75)

SELECT 
	@serviceId = ServiceId, @currentSort = SortPriority, @urlTitle = UrlTitle
FROM 
	ServiceUrl 
WHERE 
	ServiceUrlId = @urlId

/* Check we got a record */
IF @serviceId IS NOT NULL
BEGIN


	/* SortPriority distribution is likely to be uneven, which is difficult to work with, 
	    so get the links for the service in the correct order and assign them new, 
	    evenly distributed, priorities using an IDENTITY column. */
	
	CREATE TABLE #TempTable (NewPriority tinyint IDENTITY(4,2) PRIMARY KEY, ServiceUrlId int)
	INSERT #TempTable (ServiceUrlId) SELECT ServiceUrlId FROM ServiceUrl WHERE ServiceId = @serviceId ORDER BY SortPriority ASC, UrlTitle DESC

	IF @@error != 0
		RETURN

	DECLARE @count tinyint, @lastRow tinyint
	SET @count = 4
	SET @lastRow = (((SELECT COUNT(*) FROM #TempTable)*2)+2)

	IF @@error != 0
		RETURN

	/* These should succeed or fail together to make sure the current sort order is preserved */
	BEGIN TRAN

	WHILE @count <= @lastRow
		BEGIN		
			UPDATE ServiceUrl SET SortPriority = @count WHERE ServiceUrlId = (SELECT ServiceUrlId FROM #TempTable WHERE NewPriority = @count)

			IF @@error != 0
			BEGIN
				ROLLBACK TRAN
				RETURN
			END

			SET @count = @count+2
		END

	/* Now update the SortPriority of the relevant URL. Even distribution means there's a space in the sequence 3 integers away. */
	IF @sortUp = 1
		UPDATE ServiceUrl SET SortPriority = SortPriority+3 WHERE ServiceUrlId = @urlId
	ELSE
		UPDATE ServiceUrl SET SortPriority = SortPriority-3 WHERE ServiceUrlId = @urlId

	IF @@error != 0
	BEGIN
		ROLLBACK TRAN
		RETURN
	END
	
	COMMIT TRAN	

END

GO
/****** Object:  StoredProcedure [dbo].[usp_UrlUpdate]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[usp_UrlUpdate]
(
	@urlId int,
	@url varchar(255) ,
	@urlTitle varchar(75) ,
	@urlDescription varchar(300)
)
AS

/* Work out whether the url belongs to ESCC, and whether it's a form */
DECLARE @isEsccForm bit, @authorityId tinyint

SELECT 
	@authorityId = Authority 
FROM 
	Service 
WHERE 
	ServiceId = (SELECT ServiceId FROM ServiceUrl WHERE ServiceUrlId = @urlId)

IF @authorityId = 0 AND LEFT(LOWER(@urlTitle), 5) = 'form '
	SELECT @isEsccForm = 1
ELSE
	SELECT @isEsccForm = 0

/* Update the url */
UPDATE 
	ServiceUrl
SET
	Url = @url,
	UrlTitle = @urlTitle,
	UrlDescription = @urlDescription,
	IsEsccForm = @isEsccForm
WHERE
	ServiceUrlId = @urlId

GO
/****** Object:  StoredProcedure [dbo].[usp_UrlUpdatePopularity]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[usp_UrlUpdatePopularity]
(
	@serviceUrlId int,
	@popularity tinyint
)
AS

UPDATE 
	ServiceUrl 
SET 
	Popularity = @popularity
WHERE 
	ServiceUrlId = @serviceUrlId
	AND IsEsccForm = 1 		-- should only be for ESCC forms, so doesn't hurt to double-check

GO
/****** Object:  Table [dbo].[Category]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Category](
	[CategoryId] [int] IDENTITY(1,1) NOT NULL,
	[Scheme] [varchar](200) NOT NULL,
	[Identifier] [varchar](200) NULL,
	[CommonName] [varchar](100) NOT NULL,
	[ConceptId] [varchar](200) NULL,
 CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED 
(
	[CategoryId] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Contact]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Contact](
	[ContactId] [int] IDENTITY(1,1) NOT NULL,
	[ServiceId] [int] NOT NULL,
	[FirstName] [varchar](35) NULL,
	[LastName] [varchar](35) NULL,
	[ContactDescription] [varchar](255) NULL,
	[Email] [varchar](255) NULL,
	[EmailText] [varchar](75) NULL,
	[PhoneArea] [char](5) NULL,
	[Phone] [varchar](9) NULL,
	[PhoneExtension] [varchar](8) NULL,
	[FaxArea] [char](5) NULL,
	[Fax] [varchar](8) NULL,
	[FaxExtension] [varchar](8) NULL,
	[PAON] [varchar](100) NULL,
	[SAON] [varchar](100) NULL,
	[StreetDescription] [varchar](100) NULL,
	[Locality] [varchar](35) NULL,
	[Town] [varchar](30) NULL,
	[County] [varchar](30) NULL,
	[Postcode] [char](8) NULL,
	[AddressUrl] [varchar](255) NULL,
	[AddressUrlText] [varchar](75) NULL,
	[LastAmended] [datetime] NOT NULL,
 CONSTRAINT [PK_Contact] PRIMARY KEY CLUSTERED 
(
	[ContactId] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Heading]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Heading](
	[HeadingId] [int] IDENTITY(1,1) NOT NULL,
	[Heading] [varchar](255) NOT NULL,
	[RedirectUrl] [varchar](255) NULL,
	[RedirectTitle] [varchar](255) NULL,
	[LastAmended] [datetime] NOT NULL,
	[ServiceCountEscc] [int] NOT NULL,
	[ServiceCountEastbourne] [int] NOT NULL,
	[ServiceCountHastings] [int] NOT NULL,
	[ServiceCountRother] [int] NOT NULL,
	[ServiceCountWealden] [int] NOT NULL,
	[ServiceCountLewes] [int] NOT NULL,
	[IpsvNonPreferredTerms] [varchar](1000) NULL,
	[GclCategories] [varchar](255) NULL,
	[Ipsv] [bit] NOT NULL,
 CONSTRAINT [PK_Heading] PRIMARY KEY CLUSTERED 
(
	[HeadingId] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Heading_AZService]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Heading_AZService](
	[HeadingId] [int] NOT NULL,
	[ServiceId] [int] NOT NULL,
	[SortPriority] [int] NOT NULL,
	[LastAmended] [datetime] NOT NULL,
 CONSTRAINT [PK_Heading_Service] PRIMARY KEY CLUSTERED 
(
	[HeadingId] ASC,
	[ServiceId] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Heading_Category]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Heading_Category](
	[Heading_CategoryId] [int] IDENTITY(1,1) NOT NULL,
	[HeadingId] [int] NOT NULL,
	[CategoryId] [int] NOT NULL
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Heading_Heading]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Heading_Heading](
	[HeadingId] [int] NOT NULL,
	[RelatedHeadingId] [int] NOT NULL,
	[LastAmended] [datetime] NOT NULL,
 CONSTRAINT [PK_Heading_Heading] PRIMARY KEY CLUSTERED 
(
	[HeadingId] ASC,
	[RelatedHeadingId] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Service]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Service](
	[ServiceId] [int] IDENTITY(1,1) NOT NULL,
	[Service] [varchar](250) NOT NULL,
	[Description] [varchar](2500) NULL,
	[Keywords] [varchar](250) NULL,
	[IpsvImported] [varchar](1000) NULL,
	[Authority] [tinyint] NOT NULL,
	[SortPriority] [tinyint] NOT NULL,
	[LastAmended] [datetime] NOT NULL,
	[Ipsv] [bit] NOT NULL,
 CONSTRAINT [PK_Service] PRIMARY KEY CLUSTERED 
(
	[ServiceId] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ServiceUrl]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ServiceUrl](
	[ServiceUrlId] [int] IDENTITY(1,1) NOT NULL,
	[ServiceId] [int] NOT NULL,
	[Url] [varchar](255) NOT NULL,
	[UrlTitle] [varchar](75) NULL,
	[UrlDescription] [varchar](300) NULL,
	[IsEsccForm] [bit] NOT NULL,
	[SortPriority] [tinyint] NOT NULL,
	[Popularity] [tinyint] NOT NULL,
	[LastAmended] [datetime] NOT NULL,
 CONSTRAINT [PK_Table1] PRIMARY KEY CLUSTERED 
(
	[ServiceUrlId] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit]    Script Date: 29/01/2014 12:41:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit](
	[AuditId] [int] IDENTITY(1,1) NOT NULL,
	[AuditDate] [datetime] NOT NULL,
	[StoredProcedureName] [nvarchar](50) NOT NULL,
	[AuditDetails] [text] NOT NULL,
 CONSTRAINT [PK_Audit] PRIMARY KEY CLUSTERED 
(
	[AuditId] ASC
)WITH FILLFACTOR = 90 ON [PRIMARY]
) ON [PRIMARY]
SET ANSI_PADDING OFF
GO
/****** Object:  View [dbo].[Services not assigned to a heading during import]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Services not assigned to a heading during import]
AS
SELECT     TOP 100 PERCENT Service, IpsvImported AS [IPSV Term]
FROM         dbo.Service
WHERE     (IpsvImported IS NOT NULL) AND (LEN(IpsvImported) > 0) AND (ServiceId NOT IN
                          (SELECT     serviceid
                            FROM          heading_azservice))
ORDER BY IpsvImported


GO
/****** Object:  View [dbo].[vw_Service_SelectAll]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_Service_SelectAll]
AS
SELECT     TOP 100 PERCENT ServiceId, Service, Authority
FROM         dbo.Service
ORDER BY Service


GO
/****** Object:  View [dbo].[vw_ServiceContact_SelectOnlyESCC]    Script Date: 29/01/2014 09:49:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_ServiceContact_SelectOnlyESCC]
AS
SELECT     TOP 100 PERCENT dbo.Service.ServiceId, dbo.Service.Service, dbo.Contact.PhoneArea + ' ' + dbo.Contact.Phone AS Phone
FROM         dbo.Service INNER JOIN
                      dbo.Contact ON dbo.Contact.ServiceId = dbo.Service.ServiceId
WHERE     (dbo.Service.Authority = 0)
ORDER BY dbo.Service.Service


GO
ALTER TABLE [dbo].[Contact] ADD  CONSTRAINT [DF_Contact_LastAmended]  DEFAULT (getdate()) FOR [LastAmended]
GO
ALTER TABLE [dbo].[Heading] ADD  CONSTRAINT [DF_Heading_LastAmended]  DEFAULT (getdate()) FOR [LastAmended]
GO
ALTER TABLE [dbo].[Heading] ADD  CONSTRAINT [DF_Heading_ServiceCount]  DEFAULT (0) FOR [ServiceCountEscc]
GO
ALTER TABLE [dbo].[Heading] ADD  CONSTRAINT [DF_Heading_ServiceCountEastbourne]  DEFAULT (0) FOR [ServiceCountEastbourne]
GO
ALTER TABLE [dbo].[Heading] ADD  CONSTRAINT [DF_Heading_ServiceCountHastings]  DEFAULT (0) FOR [ServiceCountHastings]
GO
ALTER TABLE [dbo].[Heading] ADD  CONSTRAINT [DF_Heading_ServiceCountRother]  DEFAULT (0) FOR [ServiceCountRother]
GO
ALTER TABLE [dbo].[Heading] ADD  CONSTRAINT [DF_Heading_ServiceCountWealden]  DEFAULT (0) FOR [ServiceCountWealden]
GO
ALTER TABLE [dbo].[Heading] ADD  CONSTRAINT [DF_Heading_ServiceCountLewes]  DEFAULT (0) FOR [ServiceCountLewes]
GO
ALTER TABLE [dbo].[Heading] ADD  CONSTRAINT [DF_Heading_Ipsv]  DEFAULT (0) FOR [Ipsv]
GO
ALTER TABLE [dbo].[Heading_AZService] ADD  CONSTRAINT [DF_Heading_AZService_SortPriority]  DEFAULT (0) FOR [SortPriority]
GO
ALTER TABLE [dbo].[Heading_AZService] ADD  CONSTRAINT [DF_Heading_Service_LastAmended]  DEFAULT (getdate()) FOR [LastAmended]
GO
ALTER TABLE [dbo].[Heading_Heading] ADD  CONSTRAINT [DF_Heading_Heading_LastAmended]  DEFAULT (getdate()) FOR [LastAmended]
GO
ALTER TABLE [dbo].[Service] ADD  CONSTRAINT [DF_Service_Authority]  DEFAULT (0) FOR [Authority]
GO
ALTER TABLE [dbo].[Service] ADD  CONSTRAINT [DF_Service_SortPriority]  DEFAULT (1) FOR [SortPriority]
GO
ALTER TABLE [dbo].[Service] ADD  CONSTRAINT [DF_Service_LastAmended]  DEFAULT (getdate()) FOR [LastAmended]
GO
ALTER TABLE [dbo].[Service] ADD  CONSTRAINT [DF_Service_Ipsv]  DEFAULT (0) FOR [Ipsv]
GO
ALTER TABLE [dbo].[ServiceUrl] ADD  CONSTRAINT [DF_ServiceUrl_IsEsccForm]  DEFAULT (0) FOR [IsEsccForm]
GO
ALTER TABLE [dbo].[ServiceUrl] ADD  CONSTRAINT [DF_ServiceUrl_SortPriority]  DEFAULT (0) FOR [SortPriority]
GO
ALTER TABLE [dbo].[ServiceUrl] ADD  CONSTRAINT [DF_ServiceUrl_Popularity]  DEFAULT (0) FOR [Popularity]
GO
ALTER TABLE [dbo].[ServiceUrl] ADD  CONSTRAINT [DF_Table1_LastAmended]  DEFAULT (getdate()) FOR [LastAmended]
GO
ALTER TABLE [dbo].[Contact]  WITH CHECK ADD  CONSTRAINT [FK_Contact_Service] FOREIGN KEY([ServiceId])
REFERENCES [dbo].[Service] ([ServiceId])
GO
ALTER TABLE [dbo].[Contact] CHECK CONSTRAINT [FK_Contact_Service]
GO
ALTER TABLE [dbo].[Heading_AZService]  WITH CHECK ADD  CONSTRAINT [FK_Heading_AZService_Service] FOREIGN KEY([ServiceId])
REFERENCES [dbo].[Service] ([ServiceId])
GO
ALTER TABLE [dbo].[Heading_AZService] CHECK CONSTRAINT [FK_Heading_AZService_Service]
GO
ALTER TABLE [dbo].[Heading_AZService]  WITH CHECK ADD  CONSTRAINT [FK_Heading_Service_Heading] FOREIGN KEY([HeadingId])
REFERENCES [dbo].[Heading] ([HeadingId])
GO
ALTER TABLE [dbo].[Heading_AZService] CHECK CONSTRAINT [FK_Heading_Service_Heading]
GO
ALTER TABLE [dbo].[Heading_Category]  WITH CHECK ADD  CONSTRAINT [FK_Heading_Category_Category] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([CategoryId])
GO
ALTER TABLE [dbo].[Heading_Category] CHECK CONSTRAINT [FK_Heading_Category_Category]
GO
ALTER TABLE [dbo].[Heading_Category]  WITH CHECK ADD  CONSTRAINT [FK_Heading_Category_Heading] FOREIGN KEY([HeadingId])
REFERENCES [dbo].[Heading] ([HeadingId])
GO
ALTER TABLE [dbo].[Heading_Category] CHECK CONSTRAINT [FK_Heading_Category_Heading]
GO
ALTER TABLE [dbo].[Heading_Heading]  WITH CHECK ADD  CONSTRAINT [FK_Heading_Heading_Heading] FOREIGN KEY([HeadingId])
REFERENCES [dbo].[Heading] ([HeadingId])
GO
ALTER TABLE [dbo].[Heading_Heading] CHECK CONSTRAINT [FK_Heading_Heading_Heading]
GO
ALTER TABLE [dbo].[Heading_Heading]  WITH CHECK ADD  CONSTRAINT [FK_Heading_Heading_Heading1] FOREIGN KEY([RelatedHeadingId])
REFERENCES [dbo].[Heading] ([HeadingId])
GO
ALTER TABLE [dbo].[Heading_Heading] CHECK CONSTRAINT [FK_Heading_Heading_Heading1]
GO
ALTER TABLE [dbo].[ServiceUrl]  WITH CHECK ADD  CONSTRAINT [FK_ServiceUrl_Service] FOREIGN KEY([ServiceId])
REFERENCES [dbo].[Service] ([ServiceId])
GO
ALTER TABLE [dbo].[ServiceUrl] CHECK CONSTRAINT [FK_ServiceUrl_Service]
GO
