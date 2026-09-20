#!/usr/bin/env bash
# Verifies the MemberAdmin invitation-scoping rules added to InvitationsController.Create:
#   1. A MemberAdmin CAN invite MemberStaff into their OWN org.
#   2. A MemberAdmin CANNOT invite MemberStaff into an org they don't administer (403).
#   3. A MemberAdmin CANNOT invite another MemberAdmin, even into their own org (403).
# SuperAdmin's own unrestricted invite capability is unaffected by this and isn't re-tested here.
#
# Usage: ./verify-invitation-scoping.sh <member-admin-email> <member-admin-password>
# The MemberAdmin account must administer "Softsaro PTV LTD" specifically (this dev DB's fixture
# org used for cross-org comparison against "Dalevai PVT LTD"). Re-resolves org/role IDs by name
# at runtime via a SuperAdmin login, rather than hardcoding GUIDs, so it stays valid across
# database resets.

set -euo pipefail

API_BASE="${API_BASE:-http://localhost:5080}"
BOOTSTRAP_ADMIN_EMAIL="${REAK_BOOTSTRAP_ADMIN_EMAIL:-dev-admin@reak.local}"
BOOTSTRAP_ADMIN_PASSWORD="${REAK_BOOTSTRAP_ADMIN_PASSWORD:?Set REAK_BOOTSTRAP_ADMIN_PASSWORD (the SuperAdmin password) before running}"

MEMBER_ADMIN_EMAIL="${1:?Usage: $0 <member-admin-email> <member-admin-password>}"
MEMBER_ADMIN_PASSWORD="${2:?Usage: $0 <member-admin-email> <member-admin-password>}"

PASS=0
FAIL=0

check() {
  local label="$1" expected="$2" actual="$3"
  if [ "$expected" = "$actual" ]; then
    echo "PASS: $label (got $actual)"
    PASS=$((PASS + 1))
  else
    echo "FAIL: $label (expected $expected, got $actual)"
    FAIL=$((FAIL + 1))
  fi
}

login() {
  curl -s -X POST "$API_BASE/api/auth/login" -H "Content-Type: application/json" \
    -d "{\"email\":\"$1\",\"password\":\"$2\"}" | python3 -c "import sys,json; print(json.load(sys.stdin)['accessToken'])"
}

echo "--- Resolving org/role IDs via SuperAdmin ---"
ADMIN_TOKEN=$(login "$BOOTSTRAP_ADMIN_EMAIL" "$BOOTSTRAP_ADMIN_PASSWORD")

SOFTSARO_ID=$(curl -s "$API_BASE/api/member-entities" -H "Authorization: Bearer $ADMIN_TOKEN" \
  | python3 -c "import sys,json; print(next(m['id'] for m in json.load(sys.stdin) if m['name']=='Softsaro PTV LTD'))")
DALEVAI_ID=$(curl -s "$API_BASE/api/member-entities" -H "Authorization: Bearer $ADMIN_TOKEN" \
  | python3 -c "import sys,json; print(next(m['id'] for m in json.load(sys.stdin) if m['name']=='Dalevai PVT LTD'))")
MEMBERSTAFF_ROLE_ID=$(curl -s "$API_BASE/api/reference/roles" -H "Authorization: Bearer $ADMIN_TOKEN" \
  | python3 -c "import sys,json; print(next(r['id'] for r in json.load(sys.stdin) if r['name']=='MemberStaff'))")
MEMBERADMIN_ROLE_ID=$(curl -s "$API_BASE/api/reference/roles" -H "Authorization: Bearer $ADMIN_TOKEN" \
  | python3 -c "import sys,json; print(next(r['id'] for r in json.load(sys.stdin) if r['name']=='MemberAdmin'))")

echo "Softsaro=$SOFTSARO_ID  Dalevai=$DALEVAI_ID  MemberStaff=$MEMBERSTAFF_ROLE_ID  MemberAdmin=$MEMBERADMIN_ROLE_ID"

echo "--- Logging in as $MEMBER_ADMIN_EMAIL ---"
MA_TOKEN=$(login "$MEMBER_ADMIN_EMAIL" "$MEMBER_ADMIN_PASSWORD")

invite() {
  local email="$1" memberEntityId="$2" roleId="$3"
  curl -s -o /dev/null -w "%{http_code}" -X POST "$API_BASE/api/invitations" \
    -H "Authorization: Bearer $MA_TOKEN" -H "Content-Type: application/json" \
    -d "{\"email\":\"$email\",\"memberEntityId\":\"$memberEntityId\",\"roleId\":\"$roleId\"}"
}

echo "--- Test 1: invite MemberStaff into own org (Softsaro) ---"
STATUS=$(invite "scoping-test-own-org@example.test" "$SOFTSARO_ID" "$MEMBERSTAFF_ROLE_ID")
check "own-org MemberStaff invite succeeds" "200" "$STATUS"

echo "--- Test 2: invite MemberStaff into a DIFFERENT org (Dalevai) ---"
STATUS=$(invite "scoping-test-cross-org@example.test" "$DALEVAI_ID" "$MEMBERSTAFF_ROLE_ID")
check "cross-org MemberStaff invite is forbidden" "403" "$STATUS"

echo "--- Test 3: invite a MemberAdmin (even into own org) ---"
STATUS=$(invite "scoping-test-memberadmin@example.test" "$SOFTSARO_ID" "$MEMBERADMIN_ROLE_ID")
check "inviting MemberAdmin is forbidden" "403" "$STATUS"

echo
echo "=== $PASS passed, $FAIL failed ==="
[ "$FAIL" -eq 0 ]
