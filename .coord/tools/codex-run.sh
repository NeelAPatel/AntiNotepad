#!/usr/bin/env bash
# Route one headless Codex run: model tier, usage cap, failover, log + ledger.
# Usage: .coord/tools/codex-run.sh <director|worker|backup> <task-id> <task-file> [extra codex args]
# Env:   CODEX_BUDGET_TAG  budget bucket (default: today). CODEX_MAX_DIRECTOR (20), CODEX_MAX_WORKER (10), CODEX_MAX_BACKUP (5)
#        CODEX_TIMEOUT_MIN per-run wall limit (default 45). CODEX_WORKDIR (default: repo root). CODEX_DRY=1 prints the plan only.
set -u
ROOT="$(cd "$(dirname "$0")/../.." && pwd -W)"
role="${1:?role: director|worker|backup}"; task="${2:?task id, e.g. M9-function-contract-separation}"; file="${3:?task file}"; shift 3
EXTRA=("$@")
WORKDIR="${CODEX_WORKDIR:-$ROOT}"; TAG="${CODEX_BUDGET_TAG:-$(date +%F)}"; TMIN="${CODEX_TIMEOUT_MIN:-45}"
LEDGER="$ROOT/.coord/logs/usage.tsv"; mkdir -p "$ROOT/.coord/logs" "$ROOT/.coord/reports"; touch "$LEDGER"

tier() { case "$1" in
  director) echo "gpt-6.1-sol medium ${CODEX_MAX_DIRECTOR:-20}";;
  worker)   echo "gpt-6-luna xhigh ${CODEX_MAX_WORKER:-10}";;
  backup)   echo "gpt-5.6-luna xhigh ${CODEX_MAX_BACKUP:-5}";;
  *) echo "unknown role $1" >&2; return 1;; esac; }
next() { case "$1" in director) echo worker;; worker) echo backup;; *) echo "";; esac; }  # failover chain

attempt() {  # $1=role -> 0 ok, 3 cap reached, 4 limit-type failure (try next tier), else codex exit code
  local r="$1" model effort cap used log out start rc
  read -r model effort cap <<<"$(tier "$r")" || return 2
  used=$(awk -F'\t' -v t="$TAG" -v r="$r" '$1==t && $3==r' "$LEDGER" | wc -l)
  if [ "$used" -ge "$cap" ]; then echo "CAP: $r runs used $used/$cap for tag $TAG" >&2; return 3; fi
  log="$ROOT/.coord/logs/$task.$r.log"; out="$ROOT/.coord/reports/$task.$r.codex.md"
  echo "PLAN: role=$r model=$model effort=$effort run=$((used+1))/$cap workdir=$WORKDIR task=$file log=$log report=$out timeout=${TMIN}m"
  [ "${CODEX_DRY:-0}" = 1 ] && return 0
  start=$(date +%s)
  [ -n "${CODEX_TRACE:-}" ] && set -x
  timeout "${TMIN}m" codex exec -m "$model" -c model_reasoning_effort="$effort" -c 'windows.sandbox="unelevated"' \
    -C "$WORKDIR" --sandbox workspace-write --output-last-message "$out" "${EXTRA[@]}" - < "$file" > "$log" 2>&1
  rc=$?
  printf '%s\t%s\t%s\t%s\t%s\t%s\t%ss\n' "$TAG" "$(date +%FT%T)" "$r" "$model" "$task" "exit=$rc" "$(( $(date +%s) - start ))" >> "$LEDGER"
  [ $rc -eq 0 ] && return 0
  [ $rc -eq 124 ] && { echo "TIMEOUT after ${TMIN}m ($r)" >&2; return 124; }
  grep -qiE "usage limit|rate limit|quota|try again (at|in)|limit reached" "$log" && return 4
  return $rc
}

r="$role"
while :; do
  attempt "$r"; rc=$?
  case $rc in
    0) echo "DONE via $r. Report: .coord/reports/$task.$r.codex.md  Log: .coord/logs/$task.$r.log"; exit 0;;
    3|4) n="$(next "$r")"; [ -z "$n" ] && { echo "STOP: no further tier after $r (rc=$rc). Write HANDOFF.md, do not loop." >&2; exit $rc; }
         echo "FAILOVER: $r -> $n" >&2; r="$n";;
    *) echo "FAIL: $r exit=$rc. Not retrying (see log). Revert or fix manually." >&2; exit $rc;;
  esac
done
