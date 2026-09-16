SET NOCOUNT ON;

DECLARE @OrgA UNIQUEIDENTIFIER = NEWID();
DECLARE @OrgB UNIQUEIDENTIFIER = NEWID();
DECLARE @UserA UNIQUEIDENTIFIER = NEWID();
DECLARE @UserB UNIQUEIDENTIFIER = NEWID();
DECLARE @Province UNIQUEIDENTIFIER = NEWID();
DECLARE @District UNIQUEIDENTIFIER = NEWID();
DECLARE @Municipality UNIQUEIDENTIFIER = NEWID();
DECLARE @Ward UNIQUEIDENTIFIER = NEWID();
DECLARE @PropType UNIQUEIDENTIFIER = NEWID();
DECLARE @Purpose UNIQUEIDENTIFIER = NEWID();
DECLARE @Currency UNIQUEIDENTIFIER = NEWID();
DECLARE @AreaUnit UNIQUEIDENTIFIER = NEWID();
DECLARE @Listing UNIQUEIDENTIFIER = NEWID();

INSERT INTO MemberEntities (Id, Name, IsActive, CreatedAt) VALUES (@OrgA, N'RLS Test Org A', 1, SYSUTCDATETIME());
INSERT INTO MemberEntities (Id, Name, IsActive, CreatedAt) VALUES (@OrgB, N'RLS Test Org B', 1, SYSUTCDATETIME());

INSERT INTO Profiles (Id, Email, PasswordHash, FullName, IsActive, CreatedAt) VALUES (@UserA, N'rls-a@test.local', N'x', N'RLS Test User A', 1, SYSUTCDATETIME());
INSERT INTO Profiles (Id, Email, PasswordHash, FullName, IsActive, CreatedAt) VALUES (@UserB, N'rls-b@test.local', N'x', N'RLS Test User B', 1, SYSUTCDATETIME());

INSERT INTO EntityUsers (Id, ProfileId, MemberEntityId, IsActive, JoinedAt) VALUES (NEWID(), @UserA, @OrgA, 1, SYSUTCDATETIME());
INSERT INTO EntityUsers (Id, ProfileId, MemberEntityId, IsActive, JoinedAt) VALUES (NEWID(), @UserB, @OrgB, 1, SYSUTCDATETIME());

INSERT INTO Provinces (Id, Name, SortOrder, IsActive) VALUES (@Province, N'RLS Test Province', 0, 1);
INSERT INTO Districts (Id, ProvinceId, Name, SortOrder, IsActive) VALUES (@District, @Province, N'RLS Test District', 0, 1);
INSERT INTO Municipalities (Id, DistrictId, Name, SortOrder, IsActive) VALUES (@Municipality, @District, N'RLS Test Municipality', 0, 1);
INSERT INTO Wards (Id, MunicipalityId, Number, SortOrder, IsActive) VALUES (@Ward, @Municipality, 1, 0, 1);
INSERT INTO PropertyTypes (Id, Name, SortOrder, IsActive) VALUES (@PropType, N'RLS Test Type', 0, 1);
INSERT INTO Purposes (Id, Name, SortOrder, IsActive) VALUES (@Purpose, N'RLS Test Purpose', 0, 1);
INSERT INTO Currencies (Id, Code, Symbol, IsActive) VALUES (@Currency, N'RLT', N'R', 1);
INSERT INTO AreaUnits (Id, Name, SortOrder, IsActive) VALUES (@AreaUnit, N'RLS Test Unit', 0, 1);

-- Bypass RLS for this seeding step by acting as system admin.
EXEC sp_set_session_context 'app.is_system_admin', 1;
EXEC sp_set_session_context 'app.profile_id', @UserA;

INSERT INTO PropertyListings
    (Id, ReferenceCode, MemberEntityId, CreatedByProfileId, Title, PropertyTypeId, PurposeId,
     ProvinceId, DistrictId, MunicipalityId, WardId, CurrencyId, Price, IsPriceNegotiable,
     LandArea, AreaUnitId, HasRoadAccess, NetworkVisibility, Status, IsPublicVisible, IsDeleted, CreatedAt)
VALUES
    (@Listing, N'RLS-TEST-1', @OrgA, @UserA, N'RLS Test Listing (Org A, owner-only)', @PropType, @Purpose,
     @Province, @District, @Municipality, @Ward, @Currency, 1000000, 0,
     500, @AreaUnit, 0, 1 /* OwnerOnly */, 1 /* Draft */, 0, 0, SYSUTCDATETIME());

INSERT INTO ListingContacts (ListingId, ContactName, Phone, Email)
VALUES (@Listing, N'Org A Contact', N'9800000000', N'contact-a@test.local');

PRINT '--- STEP 1: Org A user (owner) reads their own listing + contact: expect 1 row each ---';
EXEC sp_set_session_context 'app.is_system_admin', 0;
EXEC sp_set_session_context 'app.profile_id', @UserA;
SELECT COUNT(*) AS ListingVisibleToOwner FROM PropertyListings WHERE Id = @Listing;
SELECT COUNT(*) AS ContactVisibleToOwner FROM ListingContacts WHERE ListingId = @Listing;

PRINT '--- STEP 2: Org B user reads Org A''s OwnerOnly listing + contact: expect 0 rows each ---';
EXEC sp_set_session_context 'app.profile_id', @UserB;
SELECT COUNT(*) AS ListingVisibleToOutsider FROM PropertyListings WHERE Id = @Listing;
SELECT COUNT(*) AS ContactVisibleToOutsider FROM ListingContacts WHERE ListingId = @Listing;

PRINT '--- STEP 3: Org B user tries to UPDATE Org A''s listing (should affect 0 rows, blocked/filtered) ---';
BEGIN TRY
    UPDATE PropertyListings SET Title = N'Hijacked by Org B' WHERE Id = @Listing;
    PRINT CONCAT('Rows affected by hostile update attempt: ', @@ROWCOUNT);
END TRY
BEGIN CATCH
    PRINT CONCAT('Update blocked with error: ', ERROR_MESSAGE());
END CATCH;

PRINT '--- STEP 3b: Org B user tries to DELETE Org A''s listing (should affect 0 rows, filtered out) ---';
DELETE FROM PropertyListings WHERE Id = @Listing;
PRINT CONCAT('Rows affected by hostile delete attempt (OwnerOnly, not yet visible to Org B): ', @@ROWCOUNT);

PRINT '--- STEP 4: Owner switches listing to AllMembers visibility, Org B can now READ but still cannot WRITE ---';
EXEC sp_set_session_context 'app.profile_id', @UserA;
UPDATE PropertyListings SET NetworkVisibility = 3 /* AllMembers */ WHERE Id = @Listing;

EXEC sp_set_session_context 'app.profile_id', @UserB;
SELECT COUNT(*) AS ListingVisibleAfterNetworkShare FROM PropertyListings WHERE Id = @Listing;
SELECT COUNT(*) AS ContactStillHiddenAfterNetworkShare FROM ListingContacts WHERE ListingId = @Listing;

BEGIN TRY
    UPDATE PropertyListings SET Title = N'Still hijacked?' WHERE Id = @Listing;
    PRINT CONCAT('Rows affected by hostile update attempt after read-visibility grant: ', @@ROWCOUNT);
END TRY
BEGIN CATCH
    PRINT CONCAT('Update blocked with error: ', ERROR_MESSAGE());
END CATCH;

PRINT '--- STEP 4b: Org B can now READ it but must still be unable to DELETE it (the bug this migration fixes) ---';
BEGIN TRY
    DELETE FROM PropertyListings WHERE Id = @Listing;
    PRINT CONCAT('Rows affected by hostile delete attempt after read-visibility grant (MUST be 0): ', @@ROWCOUNT);
END TRY
BEGIN CATCH
    PRINT CONCAT('Delete blocked with error: ', ERROR_MESSAGE());
END CATCH;

SELECT Title AS TitleAfterHostileAttempts FROM PropertyListings WHERE Id = @Listing;
SELECT COUNT(*) AS ListingStillExists FROM PropertyListings WHERE Id = @Listing;

PRINT '--- STEP 5: anonymous connection (no session context at all) sees nothing for a non-public listing ---';
EXEC sp_set_session_context 'app.profile_id', NULL;
EXEC sp_set_session_context 'app.is_system_admin', NULL;
SELECT COUNT(*) AS ListingVisibleToAnonymous FROM PropertyListings WHERE Id = @Listing;

PRINT '--- STEP 6: make the listing genuinely public (approved + public-visible + flag on): anonymous now sees it ---';
EXEC sp_set_session_context 'app.is_system_admin', 1;
UPDATE PropertyListings SET Status = 3 /* Approved */, IsPublicVisible = 1 WHERE Id = @Listing;
UPDATE FeatureFlags SET IsEnabled = 1 WHERE [Key] = N'public_properties_enabled';

EXEC sp_set_session_context 'app.is_system_admin', NULL;
SELECT COUNT(*) AS ListingVisibleToAnonymousOncePublic FROM PropertyListings WHERE Id = @Listing;
SELECT COUNT(*) AS ContactStillHiddenFromAnonymous FROM ListingContacts WHERE ListingId = @Listing;

PRINT '--- STEP 7: append-only audit log trigger blocks UPDATE/DELETE ---';
DECLARE @AuditId UNIQUEIDENTIFIER = NEWID();
INSERT INTO AuditLogs (Id, Action, EntityType, Summary, CreatedAt) VALUES (@AuditId, N'test.action', N'Test', N'rls test row', SYSUTCDATETIME());
BEGIN TRY
    UPDATE AuditLogs SET Summary = N'tampered' WHERE Id = @AuditId;
    PRINT 'AUDIT LOG UPDATE SUCCEEDED (THIS WOULD BE A BUG)';
END TRY
BEGIN CATCH
    PRINT CONCAT('Audit log update correctly blocked: ', ERROR_MESSAGE());
END CATCH;
BEGIN TRY
    DELETE FROM AuditLogs WHERE Id = @AuditId;
    PRINT 'AUDIT LOG DELETE SUCCEEDED (THIS WOULD BE A BUG)';
END TRY
BEGIN CATCH
    PRINT CONCAT('Audit log delete correctly blocked: ', ERROR_MESSAGE());
END CATCH;

PRINT '--- CLEANUP ---';
EXEC sp_set_session_context 'app.is_system_admin', 1;
EXEC sp_set_session_context 'app.profile_id', @UserA;
DELETE FROM ListingContacts WHERE ListingId = @Listing;
DELETE FROM PropertyListings WHERE Id = @Listing;
DELETE FROM EntityUsers WHERE ProfileId IN (@UserA, @UserB);
DELETE FROM Profiles WHERE Id IN (@UserA, @UserB);
DELETE FROM MemberEntities WHERE Id IN (@OrgA, @OrgB);
DELETE FROM Wards WHERE Id = @Ward;
DELETE FROM Municipalities WHERE Id = @Municipality;
DELETE FROM Districts WHERE Id = @District;
DELETE FROM Provinces WHERE Id = @Province;
DELETE FROM PropertyTypes WHERE Id = @PropType;
DELETE FROM Purposes WHERE Id = @Purpose;
DELETE FROM Currencies WHERE Id = @Currency;
DELETE FROM AreaUnits WHERE Id = @AreaUnit;
UPDATE FeatureFlags SET IsEnabled = 0 WHERE [Key] = N'public_properties_enabled';
PRINT 'Cleanup complete.';
