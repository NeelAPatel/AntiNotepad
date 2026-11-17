# A23-paste-keep-structure implementation report

Changed: Editor/PasteEngine.cs, Editor/PadView.cs, SelfTest/Paste.cs, SelfTest/Contracts.cs and tools/clip-dump.ps1. The engine includes image and file stubs, list conversion and safeguards; fragment offsets count UTF-8 bytes.

Avalonia 12.1.4 calls: GetDataFormatsAsync, TryGetTextAsync, TryGetFilesAsync, and TryGetValueAsync with DataFormat.CreateBytesPlatformFormat("HTML Format"). HTML is decoded as UTF-8. No Win32 fallback.

Offline build into build/A23-check passed with zero warnings; --selftest passed including captured list, markdown, mixed lists, entities, empty items, UTF-8 offsets, guards, malformed HTML and image/file stubs.

The standard offline gate failed Scope because the card uses abbreviated product paths the gate cannot recognize. Other checks passed. The card was not edited. The loose-scope offline gate passed with a scope warning. Git status was manually checked against the five approved files. Existing loop budget and PadView size warnings remain.

Not verified: live clipboard decoding, browser and AI sources, editor code, one-step Undo, menu/keyboard integration and checklist/numbered note behavior. These need the planned dev-copy hand test. No clipboard, browser, or normal app launch was used. No normal web-page capture was supplied.
