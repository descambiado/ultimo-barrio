---
name: ultimo-barrio-runtime-proof
description: Use before declaring ANY feature, fix, or integration "done", "working", or "fixed" in this project. Defines the 10-stage proof ladder (DESCUBIERTO through MULTIJUGADOR) and forbids skipping from COMPILA straight to FUNCIONA. Also defines that QA verification must exercise real inputs/interfaces (E to pick up, UI to trade, physical equip/fire, Stop/Play for persistence, a second client for host authorization) rather than commands that artificially produce the result they're meant to verify. Use this every time before writing a completion report, a STATE.md update, or a commit message that claims something works.
---

# ultimo-barrio-runtime-proof

(Formerly `sbox-runtime-proof`, renamed and expanded per the "toma el control" full-stack
integration pass to add the discovery/audit/import stages ahead of writing code, and an
explicit multiplayer stage.)

This project has a documented history of "validated" reports that weren't. Trust current
code, git, and a physical test — never a prior "validado" claim. This skill generalizes
that rule to every feature, not just the camera fix that first prompted it.

## The 10 stages — never skip one

```
DESCUBIERTO   → a candidate system/asset was found (native API, official sample, or
                 licensed package) via sbox-integration-factory's search
AUDITADO       → sbox-license-auditor produced a fiche with a real verdict
IMPORTADO      → the code/asset actually landed in Code/Assets/Libraries (if adopted/
                 adapted — skip this stage entirely for locally-written systems)
ESCRITO        → the code exists in a file
COMPILA        → dotnet build reports 0 errors
CARGA          → the scene/prefab loads without broken references, TypeLibrary warnings,
                 or JSON parse errors
VISIBLE        → the object/UI/effect actually appears where expected
INTERACTUABLE  → a real input can target it (E prompt appears, button is clickable,
                 weapon can be aimed at)
FUNCIONA       → the interaction produces the correct state change, observed via
                 read_console/QA state dump, not assumed
MULTIJUGADOR   → a second client (or at minimum host-authority checks confirmed) produces
                 the same correct result under networking rules, and PERSISTE — Stop/Play
                 (or a real restart) preserves the state through the exact save/load path
                 used in prod, not a QA shortcut that bypasses it
```

**The most common failure mode this skill exists to prevent:** reporting "FUNCIONA"
because `dotnet build` printed "0 Errores. 0 Advertencias." COMPILA is stage 5 of 10.
It proves the C# is syntactically valid. It proves nothing else.

## Verification must use real inputs, not QA shortcuts

QA commands (`ConCmd`-tagged methods, the `ub_qa_*`/`ub_test_*` family) are for
**observing** state, never for **producing** the result they're meant to verify.
Concretely:

| Feature | Verify with | Not with |
|---|---|---|
| Item pickup | Look at item, press E, watch inventory in HUD change | `qa_give_scrap` (a debug tool, not proof E works) |
| Trading | Open trader UI, click buy, watch wallet/inventory change | Directly calling `Wallet.Deposit()` from a QA command |
| Crafting | Open crafting UI, click craft button, watch output slot | Directly calling the recipe's output method |
| Equip/fire weapon | Physical input (hotbar click or keybind), then physical fire input | Spawning the weapon prefab directly into `HeldItems` |
| Barricade placement | Select kit, aim at anchor, physical place input | Instantiating the barricade GameObject via QA |
| Claim apartment | Craft door kit for real, walk to door, hold E | Calling `ApartmentClaimService.RequestClaim` from a console command |
| Persistence | Actual Stop → Play in editor (or real client disconnect/reconnect) | Reading the save file's on-disk JSON without going through load code |
| Host authorization | A second client attempting the action, checking it's rejected/accepted correctly | Only ever testing as host |

Static validators that only **consult state and pure logic** without mutating the scene
(`ub_test_all` and its sub-suites: `ItemRegistryTests`, `MeleeLogicTests`, `AITests`, etc.)
are a legitimate, different category — they check data integrity (broken item/recipe
references, invalid `SpawnZone` config) and are correctly run after any catalog/recipe
change. They are not a substitute for the INTERACTUABLE/FUNCIONA/MULTIJUGADOR stages of an
actual gameplay feature. One of these tests currently has a real bug worth knowing about:
`AITests.cs`'s `(Retreat, Attack, false)` row is tautological — `bool passes = legal`
checks the test's own hardcoded input against itself, not the real `SaqueadorBrain` FSM
(which never has that transition in its actual code). A failing "FAIL: transición ilegal"
log line from this specific row is a test bug, not a gameplay regression — verify against
the actual state machine code before treating any test failure as meaningful.

## The physical validation checklist (adapt per feature)

For any player-facing system, before calling it done, confirm — physically, in a running
Play Mode session:

- The prompt/UI/effect that signals interactivity actually appears
- The exact input specified in the design (E, click, keybind) triggers it — not a QA
  equivalent
- The host validates preconditions (distance, ownership, resource availability, kit
  consumption) and visibly rejects invalid attempts, not just silently no-ops
- The resulting state change is visible somewhere a player can see it
- For anything with persistence claims: Stop, then Play, then confirm the state survived
- For anything with authority claims: a second client attempts the privileged action and
  is correctly rejected/accepted

## Reporting discipline

State which of the 10 stages was actually reached, not just "it works." If only
DESCUBIERTO through CARGA were verified, say that plainly. Never write "debería funcionar"
as a completion claim — either it was observed working at the appropriate stage, or the
report says what stage it actually reached and what's still unverified. This applies to
the final ENTREGA report format too: every field (Pickup E, Inventario, Crafting, Door
kit, Claim, Stash, Barricadas, Armas, Enemigos, ...) gets its real stage, not a blanket
"funciona."
