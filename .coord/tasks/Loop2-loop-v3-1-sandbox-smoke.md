# Loop2-loop-v3-1-sandbox-smoke: read-only sandbox smoke test

Role: worker. You run in a read-only sandbox on purpose.

1. Read `AGENTS.md` (repo root) and state its first heading.
2. Try to create the file `build/sandbox-smoke.txt` containing the word `smoke`.
3. Report in your final message, in this form: `READ: ok|failed`, `WRITE: refused|succeeded` (with the error text if
   refused). Do not retry the write by other means, do not edit any other file, do not run builds.
