#!/usr/bin/env bash
# Route one headless Codex run: model by role, failover on usage limits, log + ledger.
# Usage: .coord/tools/codex-run.sh <quick|worker|terra|director|backup|reviewer> <task-id> <task-file> [extra codex args]
# Env:   CODEX_BUDGET_TAG  ledger tag (default: today). CODEX_EFFORT overrides the role's effort. CODEX_REVIEW_EFFORT (medium)
#        CODEX_TIMEOUT_MIN per-run wall limit (default 45). CODEX_WORKDIR (default: repo root). CODEX_DRY=1 prints the plan only.
#        CODEX_SANDBOX workspace-write (default) or read-only.
set -u
ROOT="$(cd "$(dirname "$0")/../.." && pwd -W)"
role="${1:?role: quick|worker|terra|director|backup|reviewer}"; task="${2:?task id, e.g. M9-function-contract-separation}"; file="${3:?task file}"; shift 3
EXTRA=("$@")
SANDBOX="${CODEX_SANDBOX-workspace-write}"
case "$SANDBOX" in
  workspace-write|read-only) ;;
  *) echo "invalid CODEX_SANDBOX '$SANDBOX': expected workspace-write or read-only" >&2; exit 2;;
esac
if [[ ! "$task" =~ ^[A-Z][A-Za-z0-9]*(-[a-z0-9]+)+$ ]]; then
  echo "invalid task name '$task': expected <ID>-<slug> using ASCII letters, digits, and hyphens (slug lowercase)" >&2
  exit 2
fi
WORKDIR="${CODEX_WORKDIR:-$ROOT}"; TAG="${CODEX_BUDGET_TAG:-$(date +%F)}"; TMIN="${CODEX_TIMEOUT_MIN:-45}"
# Ledger and logs live in the main checkout, so they cover every worktree's runs and outlive worktree removal.
MAIN="$(cd "$(git -C "$ROOT" rev-parse --path-format=absolute --git-common-dir 2>/dev/null)/.." 2>/dev/null && pwd -W)" || MAIN="$ROOT"
[ -n "$MAIN" ] || MAIN="$ROOT"; LOGS="$MAIN/.coord/logs"
LEDGER="$LOGS/usage.tsv"; mkdir -p "$LOGS" "$ROOT/.coord/reports"; touch "$LEDGER"

# Only Codex's own usage limits cap runs; the ledger records each run's time for planning.
tier() { case "$1" in
  quick)    echo "gpt-6-luna ${CODEX_EFFORT:-medium}";;
  worker)   echo "gpt-6-luna ${CODEX_EFFORT:-xhigh}";;
  terra)    echo "gpt-6.1-terra ${CODEX_EFFORT:-medium}";;
  director) echo "gpt-6.1-sol ${CODEX_EFFORT:-medium}";;
  backup)   echo "gpt-5.6-luna ${CODEX_EFFORT:-xhigh}";;
  reviewer) echo "gpt-6.1-sol ${CODEX_REVIEW_EFFORT:-medium}";;
  *) return 1;; esac; }
next() { case "$1" in quick|worker|reviewer) echo backup;; terra|director) echo worker;; *) echo "";; esac; }  # failover chain
tier "$role" >/dev/null || { echo "unknown role $role (expected quick|worker|terra|director|backup|reviewer)" >&2; exit 2; }

attempt() {  # $1=role -> 0 ok, 4 limit-type failure (try next tier), else codex exit code
  local r="$1" model effort log out start rc
  read -r model effort <<<"$(tier "$r")"
  log="$LOGS/$task.$r.log"; out="$LOGS/$task.$r.last.md"
  echo "PLAN: role=$r model=$model effort=$effort sandbox=$SANDBOX workdir=$WORKDIR task=$file log=$log report=$out timeout=${TMIN}m"
  [ "${CODEX_DRY:-0}" = 1 ] && return 0
  start=$(date +%s)
  [ -n "${CODEX_TRACE:-}" ] && set -x
  # ANTINOTEPAD_CODEX lets the user's shell profile skip prompt setup (starship) in Codex's own commands.
  ANTINOTEPAD_CODEX=1 timeout "${TMIN}m" codex exec -m "$model" -c model_reasoning_effort="$effort" -c 'windows.sandbox="unelevated"' \
    -C "$WORKDIR" --sandbox "$SANDBOX" --output-last-message "$out" "${EXTRA[@]}" - < "$file" > "$log" 2>&1
  rc=$?
  printf '%s\t%s\t%s\t%s\t%s\t%s\t%ss\n' "$TAG" "$(date +%FT%T)" "$r" "$model" "$task" "exit=$rc" "$(( $(date +%s) - start ))" >> "$LEDGER"
  [ $rc -eq 0 ] && return 0
  [ $rc -eq 124 ] && { echo "TIMEOUT after ${TMIN}m ($r)" >&2; return 124; }
  grep -qiE "usage limit|rate limit|quota|model is at capacity|try again (at|in)|limit reached" "$log" && return 4
  return $rc
}

r="$role"
while :; do
  attempt "$r"; rc=$?
  case $rc in
    0) echo "DONE via $r. Last message: $LOGS/$task.$r.last.md  Log: $LOGS/$task.$r.log"; exit 0;;
    4) n="$(next "$r")"; [ -z "$n" ] && { echo "STOP: no further tier after $r (rc=$rc). Write HANDOFF.md, do not loop." >&2; exit $rc; }
         echo "FAILOVER: $r -> $n" >&2; r="$n";;
    *) echo "FAIL: $r exit=$rc. Not retrying (see log). Revert or fix manually." >&2; exit $rc;;
  esac
done
