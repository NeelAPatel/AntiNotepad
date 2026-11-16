# A14-search-open-fade-up build report

- Added reusable Windows 11 timing and easing tokens and cubic-bezier easing, retaining the existing default for other animations.
- All overlay hosts fade and rise on open; close restores state immediately while the non-interactive host fades and slides down. Interruptions cancel the active animation and clean up the host.
- Recorded the approved motion reference and Microsoft Learn sources in docs/decisions.md.
- Selftest checks curve endpoints, monotonicity, and start values. The specified enter curve evaluates to approximately 0.4463304 at 0.1, so the requested > 0.5 assertion is mathematically incompatible with the approved curve; the test checks the actual value instead.
- Offline gate: PASS, including build (zero warnings/errors), --selftest, seam, tokens, format, scope, and size. Details: .coord/reports/A14-search-open-fade-up.gate.md. NuGet audit was disabled for the offline build.
- Not verified: live animation, focus, reduced motion, rapid interruption, themes, or Search entry points. No app was launched; hand testing remains.
