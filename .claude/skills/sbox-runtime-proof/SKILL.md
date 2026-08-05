---
name: sbox-runtime-proof
description: Use before declaring ANY feature, fix, or integration "done", "working", or "fixed" in this project. Defines the 8-stage proof ladder (WRITTEN through PERSISTS) and forbids skipping from COMPILES straight to WORKS. Also defines that QA verification must exercise real inputs/interfaces (E to pick up, UI to trade, physical equip/fire, Stop/Play for persistence, a second client for host authorization) rather than commands that artificially produce the result they're meant to verify. Use this every time before writing a completion report, a STATE.md update, or a commit message that claims something works.
---

# sbox-runtime-proof

This project has a documented history of "validated" reports that weren't. The prior
instruction for this session was explicit: *no confíes en informes anteriores de
"validado"* — trust current code, git, and a physical test in editor. This skill
generalizes that rule to every feature going forward, not just the camera fix.

## The 8 stages — never skip one

```
ESCRITO              → the code exists in a file
COMPILA               → dotnet build reports 0 errors
CARGA EN EDITOR        → the scene/prefab loads without broken references,
                          TypeLibrary warnings, or JSON parse errors
VISIBLE                → the object/UI/effect actually appears where expected
INTERACTUABLE          → a real input can target it (E prompt appears, button
                          is clickable, weapon can be aimed at)
FUNCIONA EN RUNTIME     → the interaction produces the correct state change,
                          observed via read_console / QA state dump, not assumed
FUNCIONA MULTIJUGADOR   → a second client (or at minimum host-authority checks
                          confirmed) produces the same correct result under
                          networking rules, not just on a listen-server host alone
PERSISTE                → Stop/Play (or a real restart) preserves the state;
                          the data survives the exact save/load path used in prod,
                          not a QA shortcut that bypasses it
```

**The most common failure mode this skill exists to prevent:** reporting
"FUNCIONA" because `dotnet build` printed "0 Errores. 0 Advertencias." COMPILA is
stage 2 of 8. It proves the C# is syntactically valid. It proves nothing else.

## Verification must use real inputs, not QA shortcuts

QA commands (`ConCmd`-tagged methods, the `ub_qa_*` family) are for **observing**
state, never for **producing** the result they're meant to verify. Concretely:

| Feature | Verify with | Not with |
|---|---|---|
| Item pickup | Look at item, press E, watch inventory in HUD change | `qa_give_scrap` (that's a cheat/debug tool, not proof E works) |
| Trading | Open trader UI, click buy, watch wallet/inventory change | Directly calling `Wallet.Deposit()` from a QA command |
| Crafting | Open crafting UI, click craft button, watch output slot | Directly calling the recipe's output method |
| Equip/fire weapon | Physical input (hotbar click or keybind), then physical fire input | Spawning the weapon prefab directly into `HeldItems` |
| Barricade placement | Select kit, aim at anchor, physical place input | Instantiating the barricade GameObject via QA |
| Persistence | Actual Stop → Play in editor (or real client disconnect/reconnect) | Reading the save file's on-disk JSON without going through load code |
| Host authorization | A second client attempting the action, checking it's rejected/accepted correctly | Only ever testing as host — the authority check itself is what's under test |

QA commands like `ub_qa_camera_state` are correctly used for *reading* current
`EyeAngles`/`WorldPosition`/etc — they never once produced camera motion themselves;
a human moved the mouse and the QA command only reported what happened. That's the
right pattern: QA observes, real input acts.

## The physical validation checklist (adapt per feature)

For any player-facing system, before calling it done, confirm — physically, in a
running Play Mode session, not by reading the code and reasoning it should work:

- The prompt/UI/effect that signals interactivity actually appears
- The exact input specified in the design (E, click, keybind) triggers it — not a
  QA equivalent
- The host validates preconditions (distance, ownership, resource availability) and
  visibly rejects invalid attempts, not just silently no-ops
- The resulting state change is visible somewhere a player can see it (HUD,
  inventory, world geometry) — an internal field changing with no visible
  consequence is not a completed feature
- For anything with persistence claims: Stop, then Play, then confirm the state
  survived — every persistence claim in this project gets this specific test

## Reporting discipline

When writing a completion report, a `STATE.md` update, or a commit message: state
which of the 8 stages was actually reached, not just "it works." If only COMPILA
and CARGA EN EDITOR were verified, say that plainly — a partial result honestly
reported is useful; a stage skipped and reported as "done" is what caused the
runtime being broken in the first place (see the original camera/movement recovery
task this session started with, which existed *because* of exactly this pattern).

Never write "debería funcionar" (should work) as a completion claim. Either it was
observed working at the appropriate stage, or the report says what stage it actually
reached and what's still unverified.
