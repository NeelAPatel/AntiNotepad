#!/usr/bin/env bash
# N2-canonical-install-and-dev-badge: keep the shell entry point on the same
# PID-safe implementation and compile-time dev identity as the PowerShell entry point.
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" >/dev/null 2>&1 && pwd -P)"
fresh_args=()
case "$#" in
  0) ;;
  1)
    if [[ "$1" == "--fresh" ]]; then
      fresh_args=(-Fresh)
    else
      printf 'Usage: %s [--fresh]\n' "$0" >&2
      exit 2
    fi
    ;;
  *)
    printf 'Usage: %s [--fresh]\n' "$0" >&2
    exit 2
    ;;
esac

if ! command -v pwsh >/dev/null 2>&1; then
  printf 'pwsh is required to run tools/dev.ps1.\n' >&2
  exit 127
fi

exec pwsh -NoLogo -NoProfile -File "$script_dir/dev.ps1" "${fresh_args[@]}"
