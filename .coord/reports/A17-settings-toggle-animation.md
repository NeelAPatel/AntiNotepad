# A17-settings-toggle-animation

1. Summary: Implemented shared Settings toggle motion; completion blocked by the pinned design snapshot.
2. Change list: Added toggle size tokens and interruptible knob, fill and outline motion.
3. Behavior changes: Hover and press resize the knob; clicking slides it and fades the track. Reduced motion snaps and keeps the knob at rest size, including when enabled by this toggle.
4. Out-of-scope files: No product files changed beyond the two listed. The design snapshot baseline in SelfTest/Design.cs needs review and an update for the new control tree.
5. Plan deviations: Selftest and gate fail on the design snapshot hash; the approved Files list does not include its baseline. Gate warns SettingsParts.cs crossed 600 lines.
6. Contracts: None new; click callback and accessibility name retained.
7. Lens review: Accept after fixing the reduced-motion enabling edge; pointer and visual behavior require hand checks.
8. Checks run: Offline build passed with 0 warnings; selftest failed on design hash. Gate failed on the same selftest; build, seam, tokens, format and scope passed. Cleanup of the check folder was rejected by the execution policy; gate kept its folders on failure.
9. Hand checks: Once checks pass, use the dev copy to check every Settings page, hover, press, release and rapid double click, with Reduce motion on and off.
