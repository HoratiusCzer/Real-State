using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace REAK.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRowLevelSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"-- REAK row-level security (spec §17). SQL Server's native RLS (CREATE SECURITY POLICY +
-- predicate functions) is this stack's equivalent of Postgres RLS — enforced by the database
-- engine itself, not just application code, so it holds even against a direct query that
-- bypasses the API layer.
--
-- Session context this depends on (set once per connection by the API layer — wired up in
-- Stage 4 once JWT auth exists to know who the caller is; until then these predicates simply
-- deny everything for connections that never set the context, which is the fail-closed default):
--   app.profile_id       UNIQUEIDENTIFIER  -- the authenticated caller's Profile.Id
--   app.is_system_admin  BIT               -- 1 if caller holds a SuperAdmin/AssociationAdmin role
--
-- Read vs write predicates are deliberately separate. A listing that is publicly/network
-- visible is readable by more people than are allowed to modify it — collapsing the two into
-- one predicate would let a network-visibility read-grant double as a write-grant, which is
-- wrong. Only ownership (or system admin) ever authorizes INSERT/UPDATE.

CREATE SCHEMA Security AUTHORIZATION dbo;");

            migrationBuilder.Sql(@"-- ===========================================================================================
-- Property listings
-- ===========================================================================================

CREATE FUNCTION Security.fn_ListingReadPredicate(
    @Id UNIQUEIDENTIFIER,
    @MemberEntityId UNIQUEIDENTIFIER,
    @Status INT,
    @NetworkVisibility INT,
    @IsPublicVisible BIT,
    @IsDeleted BIT,
    @ExpiresAt DATETIME2
)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
SELECT 1 AS AccessResult
WHERE CAST(SESSION_CONTEXT(N'app.is_system_admin') AS BIT) = 1
   OR EXISTS (
        SELECT 1 FROM dbo.EntityUsers eu
        WHERE eu.MemberEntityId = @MemberEntityId
          AND eu.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
          AND eu.IsActive = 1
   )
   OR (
        @NetworkVisibility = 3 /* AllMembers */
        AND EXISTS (
            SELECT 1 FROM dbo.EntityUsers eu2
            WHERE eu2.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
              AND eu2.IsActive = 1
        )
   )
   OR (
        @NetworkVisibility = 2 /* SelectedMembers */
        AND EXISTS (
            SELECT 1 FROM dbo.ListingVisibilityMembers lvm
            JOIN dbo.EntityUsers eu3 ON eu3.MemberEntityId = lvm.MemberEntityId
            WHERE lvm.ListingId = @Id
              AND eu3.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
              AND eu3.IsActive = 1
        )
   )
   OR (
        -- Public projection gate (spec §16): Approved AND public-visible AND active member AND
        -- not deleted AND not expired AND the public_properties_enabled flag is on. This branch
        -- does not depend on app.profile_id, so it also covers genuinely anonymous connections.
        @Status = 3 /* Approved */
        AND @IsPublicVisible = 1
        AND @IsDeleted = 0
        AND (@ExpiresAt IS NULL OR @ExpiresAt > SYSUTCDATETIME())
        AND EXISTS (SELECT 1 FROM dbo.MemberEntities me WHERE me.Id = @MemberEntityId AND me.IsActive = 1)
        AND EXISTS (SELECT 1 FROM dbo.FeatureFlags ff WHERE ff.[Key] = N'public_properties_enabled' AND ff.IsEnabled = 1)
   );");

            migrationBuilder.Sql(@"CREATE FUNCTION Security.fn_ListingWritePredicate(@MemberEntityId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
SELECT 1 AS AccessResult
WHERE CAST(SESSION_CONTEXT(N'app.is_system_admin') AS BIT) = 1
   OR EXISTS (
        SELECT 1 FROM dbo.EntityUsers eu
        WHERE eu.MemberEntityId = @MemberEntityId
          AND eu.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
          AND eu.IsActive = 1
   );");

            migrationBuilder.Sql(@"CREATE SECURITY POLICY Security.ListingPolicy
    ADD FILTER PREDICATE Security.fn_ListingReadPredicate(Id, MemberEntityId, Status, NetworkVisibility, IsPublicVisible, IsDeleted, ExpiresAt)
        ON dbo.PropertyListings,
    ADD BLOCK PREDICATE Security.fn_ListingWritePredicate(MemberEntityId)
        ON dbo.PropertyListings AFTER INSERT,
    ADD BLOCK PREDICATE Security.fn_ListingWritePredicate(MemberEntityId)
        ON dbo.PropertyListings AFTER UPDATE,
    ADD BLOCK PREDICATE Security.fn_ListingWritePredicate(MemberEntityId)
        ON dbo.PropertyListings BEFORE DELETE
    WITH (STATE = ON);");

            migrationBuilder.Sql(@"-- ===========================================================================================
-- Demands (no public path — spec §9 has no public demand projection)
-- ===========================================================================================

CREATE FUNCTION Security.fn_DemandReadPredicate(
    @Id UNIQUEIDENTIFIER,
    @MemberEntityId UNIQUEIDENTIFIER,
    @NetworkVisibility INT
)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
SELECT 1 AS AccessResult
WHERE CAST(SESSION_CONTEXT(N'app.is_system_admin') AS BIT) = 1
   OR EXISTS (
        SELECT 1 FROM dbo.EntityUsers eu
        WHERE eu.MemberEntityId = @MemberEntityId
          AND eu.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
          AND eu.IsActive = 1
   )
   OR (
        @NetworkVisibility = 3 /* AllMembers */
        AND EXISTS (
            SELECT 1 FROM dbo.EntityUsers eu2
            WHERE eu2.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
              AND eu2.IsActive = 1
        )
   )
   OR (
        @NetworkVisibility = 2 /* SelectedMembers */
        AND EXISTS (
            SELECT 1 FROM dbo.DemandVisibilityMembers dvm
            JOIN dbo.EntityUsers eu3 ON eu3.MemberEntityId = dvm.MemberEntityId
            WHERE dvm.DemandId = @Id
              AND eu3.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
              AND eu3.IsActive = 1
        )
   );");

            migrationBuilder.Sql(@"CREATE FUNCTION Security.fn_DemandWritePredicate(@MemberEntityId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
SELECT 1 AS AccessResult
WHERE CAST(SESSION_CONTEXT(N'app.is_system_admin') AS BIT) = 1
   OR EXISTS (
        SELECT 1 FROM dbo.EntityUsers eu
        WHERE eu.MemberEntityId = @MemberEntityId
          AND eu.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
          AND eu.IsActive = 1
   );");

            migrationBuilder.Sql(@"CREATE SECURITY POLICY Security.DemandPolicy
    ADD FILTER PREDICATE Security.fn_DemandReadPredicate(Id, MemberEntityId, NetworkVisibility)
        ON dbo.Demands,
    ADD BLOCK PREDICATE Security.fn_DemandWritePredicate(MemberEntityId)
        ON dbo.Demands AFTER INSERT,
    ADD BLOCK PREDICATE Security.fn_DemandWritePredicate(MemberEntityId)
        ON dbo.Demands AFTER UPDATE,
    ADD BLOCK PREDICATE Security.fn_DemandWritePredicate(MemberEntityId)
        ON dbo.Demands BEFORE DELETE
    WITH (STATE = ON);");

            migrationBuilder.Sql(@"-- ===========================================================================================
-- Contact isolation (spec §2.3, §8.4, §9.1) — the headline guarantee. Never joined into normal
-- listing/demand queries; read access requires owning-org membership OR an explicit, unrevoked
-- CollaborationContactDisclosure scoped to the specific listing/demand via the collaboration's
-- originating match. Write access is owning-org-only, full stop — a disclosure grant only ever
-- unlocks reading, never editing someone else's contact info.
-- ===========================================================================================

CREATE FUNCTION Security.fn_ListingContactReadPredicate(@ListingId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
SELECT 1 AS AccessResult
WHERE CAST(SESSION_CONTEXT(N'app.is_system_admin') AS BIT) = 1
   OR EXISTS (
        SELECT 1 FROM dbo.PropertyListings pl
        JOIN dbo.EntityUsers eu ON eu.MemberEntityId = pl.MemberEntityId
        WHERE pl.Id = @ListingId
          AND eu.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
          AND eu.IsActive = 1
   )
   OR EXISTS (
        SELECT 1
        FROM dbo.CollaborationContactDisclosures ccd
        JOIN dbo.CollaborationWorkspaces cw ON cw.Id = ccd.CollaborationWorkspaceId
        JOIN dbo.CollaborationRequests cr ON cr.Id = cw.CollaborationRequestId
        JOIN dbo.Matches m ON m.Id = cr.MatchId
        JOIN dbo.EntityUsers eu2 ON eu2.MemberEntityId = ccd.ReceivingMemberEntityId
        WHERE m.ListingId = @ListingId
          AND ccd.DataType = 1 /* ListingContact */
          AND ccd.RevokedAt IS NULL
          AND eu2.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
          AND eu2.IsActive = 1
   );");

            migrationBuilder.Sql(@"CREATE FUNCTION Security.fn_ListingContactWritePredicate(@ListingId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
SELECT 1 AS AccessResult
WHERE CAST(SESSION_CONTEXT(N'app.is_system_admin') AS BIT) = 1
   OR EXISTS (
        SELECT 1 FROM dbo.PropertyListings pl
        JOIN dbo.EntityUsers eu ON eu.MemberEntityId = pl.MemberEntityId
        WHERE pl.Id = @ListingId
          AND eu.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
          AND eu.IsActive = 1
   );");

            migrationBuilder.Sql(@"CREATE SECURITY POLICY Security.ListingContactPolicy
    ADD FILTER PREDICATE Security.fn_ListingContactReadPredicate(ListingId)
        ON dbo.ListingContacts,
    ADD BLOCK PREDICATE Security.fn_ListingContactWritePredicate(ListingId)
        ON dbo.ListingContacts AFTER INSERT,
    ADD BLOCK PREDICATE Security.fn_ListingContactWritePredicate(ListingId)
        ON dbo.ListingContacts AFTER UPDATE,
    ADD BLOCK PREDICATE Security.fn_ListingContactWritePredicate(ListingId)
        ON dbo.ListingContacts BEFORE DELETE
    WITH (STATE = ON);");

            migrationBuilder.Sql(@"CREATE FUNCTION Security.fn_DemandContactReadPredicate(@DemandId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
SELECT 1 AS AccessResult
WHERE CAST(SESSION_CONTEXT(N'app.is_system_admin') AS BIT) = 1
   OR EXISTS (
        SELECT 1 FROM dbo.Demands d
        JOIN dbo.EntityUsers eu ON eu.MemberEntityId = d.MemberEntityId
        WHERE d.Id = @DemandId
          AND eu.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
          AND eu.IsActive = 1
   )
   OR EXISTS (
        SELECT 1
        FROM dbo.CollaborationContactDisclosures ccd
        JOIN dbo.CollaborationWorkspaces cw ON cw.Id = ccd.CollaborationWorkspaceId
        JOIN dbo.CollaborationRequests cr ON cr.Id = cw.CollaborationRequestId
        JOIN dbo.Matches m ON m.Id = cr.MatchId
        JOIN dbo.EntityUsers eu2 ON eu2.MemberEntityId = ccd.ReceivingMemberEntityId
        WHERE m.DemandId = @DemandId
          AND ccd.DataType = 2 /* DemandContact */
          AND ccd.RevokedAt IS NULL
          AND eu2.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
          AND eu2.IsActive = 1
   );");

            migrationBuilder.Sql(@"CREATE FUNCTION Security.fn_DemandContactWritePredicate(@DemandId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
SELECT 1 AS AccessResult
WHERE CAST(SESSION_CONTEXT(N'app.is_system_admin') AS BIT) = 1
   OR EXISTS (
        SELECT 1 FROM dbo.Demands d
        JOIN dbo.EntityUsers eu ON eu.MemberEntityId = d.MemberEntityId
        WHERE d.Id = @DemandId
          AND eu.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
          AND eu.IsActive = 1
   );");

            migrationBuilder.Sql(@"CREATE SECURITY POLICY Security.DemandContactPolicy
    ADD FILTER PREDICATE Security.fn_DemandContactReadPredicate(DemandId)
        ON dbo.DemandContacts,
    ADD BLOCK PREDICATE Security.fn_DemandContactWritePredicate(DemandId)
        ON dbo.DemandContacts AFTER INSERT,
    ADD BLOCK PREDICATE Security.fn_DemandContactWritePredicate(DemandId)
        ON dbo.DemandContacts AFTER UPDATE,
    ADD BLOCK PREDICATE Security.fn_DemandContactWritePredicate(DemandId)
        ON dbo.DemandContacts BEFORE DELETE
    WITH (STATE = ON);");

            migrationBuilder.Sql(@"-- ===========================================================================================
-- Collaboration workspace + children (spec §13.1) — accessible only to participants. Read and
-- write share one predicate here: a participant may both read and post into their own
-- workspace; that's a materially different situation from listing/demand visibility, where
-- ""can see it"" and ""can edit it"" are deliberately different audiences.
-- ===========================================================================================

CREATE FUNCTION Security.fn_CollaborationParticipantPredicate(@CollaborationWorkspaceId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
SELECT 1 AS AccessResult
WHERE CAST(SESSION_CONTEXT(N'app.is_system_admin') AS BIT) = 1
   OR EXISTS (
        SELECT 1 FROM dbo.CollaborationParticipants cp
        WHERE cp.CollaborationWorkspaceId = @CollaborationWorkspaceId
          AND cp.ProfileId = CAST(SESSION_CONTEXT(N'app.profile_id') AS UNIQUEIDENTIFIER)
   );");

            migrationBuilder.Sql(@"CREATE SECURITY POLICY Security.CollaborationWorkspacePolicy
    ADD FILTER PREDICATE Security.fn_CollaborationParticipantPredicate(Id) ON dbo.CollaborationWorkspaces,
    ADD BLOCK PREDICATE Security.fn_CollaborationParticipantPredicate(Id) ON dbo.CollaborationWorkspaces AFTER INSERT,
    ADD BLOCK PREDICATE Security.fn_CollaborationParticipantPredicate(Id) ON dbo.CollaborationWorkspaces AFTER UPDATE,

    ADD FILTER PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationParticipants,
    ADD BLOCK PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationParticipants AFTER INSERT,

    ADD FILTER PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationMessages,
    ADD BLOCK PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationMessages AFTER INSERT,

    ADD FILTER PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationFiles,
    ADD BLOCK PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationFiles AFTER INSERT,

    ADD FILTER PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationNotes,
    ADD BLOCK PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationNotes AFTER INSERT,

    ADD FILTER PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationTasks,
    ADD BLOCK PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationTasks AFTER INSERT,

    ADD FILTER PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationViewings,
    ADD BLOCK PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationViewings AFTER INSERT,

    ADD FILTER PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationActivities,
    ADD BLOCK PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationActivities AFTER INSERT,

    ADD FILTER PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationContactDisclosures,
    ADD BLOCK PREDICATE Security.fn_CollaborationParticipantPredicate(CollaborationWorkspaceId) ON dbo.CollaborationContactDisclosures AFTER INSERT
    WITH (STATE = ON);");

            migrationBuilder.Sql(@"-- ===========================================================================================
-- Audit logs: append-only (spec §19). Normal users — and the application's own connection —
-- cannot modify or delete a row once written, enforced at the engine level via an INSTEAD OF
-- trigger rather than a convention the application code has to remember to honor.
-- ===========================================================================================

CREATE TRIGGER dbo.trg_AuditLogs_PreventModification
ON dbo.AuditLogs
INSTEAD OF UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    RAISERROR('AuditLogs is append-only: UPDATE and DELETE are not permitted.', 16, 1);
    ROLLBACK TRANSACTION;
END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"-- Reverses RowLevelSecurity.sql, in dependency order.

DROP TRIGGER IF EXISTS dbo.trg_AuditLogs_PreventModification;");

            migrationBuilder.Sql(@"DROP SECURITY POLICY IF EXISTS Security.CollaborationWorkspacePolicy;
DROP SECURITY POLICY IF EXISTS Security.DemandContactPolicy;
DROP SECURITY POLICY IF EXISTS Security.ListingContactPolicy;
DROP SECURITY POLICY IF EXISTS Security.DemandPolicy;
DROP SECURITY POLICY IF EXISTS Security.ListingPolicy;");

            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS Security.fn_CollaborationParticipantPredicate;
DROP FUNCTION IF EXISTS Security.fn_DemandContactWritePredicate;
DROP FUNCTION IF EXISTS Security.fn_DemandContactReadPredicate;
DROP FUNCTION IF EXISTS Security.fn_ListingContactWritePredicate;
DROP FUNCTION IF EXISTS Security.fn_ListingContactReadPredicate;
DROP FUNCTION IF EXISTS Security.fn_DemandWritePredicate;
DROP FUNCTION IF EXISTS Security.fn_DemandReadPredicate;
DROP FUNCTION IF EXISTS Security.fn_ListingWritePredicate;
DROP FUNCTION IF EXISTS Security.fn_ListingReadPredicate;");

            migrationBuilder.Sql(@"DROP SCHEMA IF EXISTS Security;");
        }
    }
}
