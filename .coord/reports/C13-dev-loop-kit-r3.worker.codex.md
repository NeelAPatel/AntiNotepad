Implemented C13-dev-loop-kit-r3. Rework scope now unions the base brief with existing rounds through `rN`; plain task names retain the single-brief scope path. The four checks run through CONTRACTS only, and failures keep their check labels.

The two A30 rows still share the memoized `BackgroundSaveAndLogChecks` result; its aggregate return value does not support independent row failures without adding check logic. The report explains this.

- Final offline gate: **PASS**, build 0 warnings, `SELFTEST PASS`, `CONTRACTS: 6 rows, 5 pass, 1 manual`. Scope listed the C13, r2, and r3 briefs; its warnings were for files outside their union.
- Plain-name gate: **PASS**, with the single-brief scope path.
- Broken fence-cache probe: failed with the expected named contract message; the assertion was restored, and the final gate passed.
- No commit or branch change.

Report: [C13-dev-loop-kit-r3.md](F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.coord/reports/C13-dev-loop-kit-r3.md)  
Gate output: [C13-dev-loop-kit-r3.gate.md](F:/LocalVSProj/Visual%20Studio%20Apps/AntiQuickPad/.coord/reports/C13-dev-loop-kit-r3.gate.md)

