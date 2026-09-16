-- Reverses RowLevelSecurity.sql, in dependency order.

DROP TRIGGER IF EXISTS dbo.trg_AuditLogs_PreventModification;
GO

DROP SECURITY POLICY IF EXISTS Security.CollaborationWorkspacePolicy;
DROP SECURITY POLICY IF EXISTS Security.DemandContactPolicy;
DROP SECURITY POLICY IF EXISTS Security.ListingContactPolicy;
DROP SECURITY POLICY IF EXISTS Security.DemandPolicy;
DROP SECURITY POLICY IF EXISTS Security.ListingPolicy;
GO

DROP FUNCTION IF EXISTS Security.fn_CollaborationParticipantPredicate;
DROP FUNCTION IF EXISTS Security.fn_DemandContactWritePredicate;
DROP FUNCTION IF EXISTS Security.fn_DemandContactReadPredicate;
DROP FUNCTION IF EXISTS Security.fn_ListingContactWritePredicate;
DROP FUNCTION IF EXISTS Security.fn_ListingContactReadPredicate;
DROP FUNCTION IF EXISTS Security.fn_DemandWritePredicate;
DROP FUNCTION IF EXISTS Security.fn_DemandReadPredicate;
DROP FUNCTION IF EXISTS Security.fn_ListingWritePredicate;
DROP FUNCTION IF EXISTS Security.fn_ListingReadPredicate;
GO

DROP SCHEMA IF EXISTS Security;
GO
