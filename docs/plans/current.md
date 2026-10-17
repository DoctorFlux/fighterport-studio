# Next executable acceptance criterion

R2 next: resolve loose Brawl companions explicitly through the actual native workspace.
Use original fixtures with MDL0/material in one BRRES, its RGBA8 texture in another
resource, and CHR0 in a separate motion folder. Let the user select dependencies;
report missing/ambiguous matches and record selected paths, identities and hashes.
Persist that resolution and recover it after restarting the application. Do not
silently guess companions or alter either source folder.

Acceptance: native editor visibly previews the resolved texture and plays the selected
motion; an edit saves to a separate Brawl-native output and reopens with the expected
texture/motion; all original hashes match before/after and after process restart.
Add missing/ambiguous dependency fixtures and keep unsupported native data explicit.
This Brawl-specific path must not become the intermediate format for other games.

The restart baseline, independent identity/safety and first Open Source slice have
passed their documented build/native/GUI gates. After this single next task, continue
installation/dump/image/container providers, game-neutral projects and measured
cross-rig/contact/bake work, then the configured Goku-to-installed-Rivals-Modkit
import/state/attack/build/playtest slice in MASTER_PLAN.md. No publication is authorized.
