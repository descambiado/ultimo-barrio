---
name: sbox-integration-factory
description: Use before writing ANY new gameplay system, component, or content pipeline in this s&box project (inventory, items, weapons, doors, barricades, crafting, trading, vehicles, NPCs, persistence, UI panels, etc). Forces a reuse-before-rewrite search across the installed s&box API, official docs, Facepunch source, the Library Manager, the Cloud Browser, and licensed community packages BEFORE any new class or system gets written. Also applies when reviewing existing code that duplicates something the engine or an official package already provides, or when integrating a candidate found by sbox-license-auditor. Do not use for pure bug fixes to existing local code that has no native equivalent (e.g. the camera/movement fixes already done) - use this when the question is "should this be a new system at all."
---

# sbox-integration-factory

(Formerly `sbox-reuse-first`, renamed and consolidated per the "toma el control" full-stack
integration pass — `sbox-source-auditor`'s fiche discipline now lives in the companion skill
[[sbox-license-auditor]].)

Último Barrio has a documented history of subagents writing parallel, contradictory
systems from scratch (duplicate camera solutions, three inventory-adjacent components,
cube weapons) because nobody checked what already existed first. Concrete evidence found
in this project: `Code/UltimoBarrio/Combat/BaseInventoryComponent.cs`,
`UltimoBarrioWeaponAdapter.cs`, `WeaponEquipper.cs`, `WeaponPickup.cs`, `PistolWeapon.cs`,
`RifleWeapon.cs` were a parallel inventory/weapon system with **zero external references** —
the remnants of an earlier, abandoned attempt at exactly the kind of migration this skill
exists to prevent from happening carelessly again. This skill is the discipline that stops
the next version of that.

## The rule

**Reuse before rewrite.** Every new system starts with a search, not a blank file.
No exceptions for "it'll just take five minutes to write."

## The 9-step order

Work through these in order. Do not skip to step 9 because steps 1-8 feel slow.

1. **Encontrar el sistema existente.** Grep the actually-installed engine, not memory of a
   past s&box version:
   ```
   C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.Engine.xml
   C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.System.xml
   C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.Tools.xml
   ```
   These are XML doc comments extracted from the real compiled API — they say what
   exists *in this install*, not what existed in a blog post from two years ago.
   Search for the noun first (`Inventory`, `Door`, `Weapon`, `NavMesh`, `Joint`),
   then read every member's summary before concluding nothing fits. Also check the local
   codebase itself — this project's own systems (`InventoryComponent`, `AIBase`,
   `BaseCombatWeapon`) are frequently more complete than a fresh search would assume.

2. **Buscar ejemplo oficial actual.** Check:
   ```
   C:\Program Files (x86)\Steam\steamapps\common\sbox\templates\
   C:\Program Files (x86)\Steam\steamapps\common\sbox\samples\
   C:\Program Files (x86)\Steam\steamapps\common\sbox\addons\
   ```
   The `game.playercontroller` template, `sweeper` sample, and any addon source
   (`menu`, `tools`) are real first-party code using the current API shape — more
   reliable than any web search result, which may predate a breaking API change.

3. **Buscar Library o paquete con fuente.** s&box packages with visible source live under
   the engine's package cache and inside any addon that mounts them. Check the project's
   `.sbproj` `PackageReferences` and `Assets/asset-registry.yml` — packages already pulled
   in or already evaluated may already solve this (e.g. `facepunch/sboxweapons` is already
   registered there, pending verification, for the weapon-asset track).

4. **Buscar repositorios compatibles.** Use WebSearch/WebFetch against `sbox.game` package
   listings and GitHub for packages whose source is visible and whose last commit targets a
   current engine build (check API surface used, not just recency of the commit date).

5. **Verificar licencia.** Hand off to [[sbox-license-auditor]] for the full check — never
   skip straight to copying code because a repo is public. A "source-available" repo
   (e.g. `dxura/dxrp`, confirmed all-rights-reserved) is not the same as open source.

6. **Verificar fecha y API utilizada.** Cross-reference the candidate's use of s&box APIs
   against what step 1 confirmed exists *now*. A package built against a pre-`NavMeshAgent`
   or pre-`Sandbox.PlayerController` era will not compile, or worse, will compile against
   stale interop and misbehave silently.

7. **Comparar con la arquitectura local.** Read the actual current local component — not a
   memory of what it does. Does the candidate's data model match `ItemDefinition`/
   `ItemRegistry`? Does it assume single-player where Último Barrio requires
   host-authoritative networking and per-apartment access policies?

8. **Decidir: adoptar, adaptar, extraer patrón, solo assets, o descartar.** Write the
   verdict down in an [[sbox-license-auditor]] fiche before touching any code.
   "Extraer patrón" means writing new code, but the design decisions (data shape, event
   flow, edge case handling) come from the reference, not from scratch.

9. **Escribir código nuevo solo para la diferencia específica.** If steps 1-8 found 90% of
   the system, the new code is the missing 10%, not a fresh reimplementation of the whole
   thing "to be safe" or "to match our style." Adapt call sites to match local conventions;
   don't duplicate the reference's logic under a new name.

## Prohibiciones (hard rules)

- **No copiar un gamemode completo.** Extract the specific files/patterns needed, not a
  whole addon or gamemode — a full-repo copy drags in its bugs and its license terms for
  code paths you'll never call. `Softsplit/sandbox-plus-plus` is GPL-3.0: pattern reference
  only, its code does not get copied into this MIT-adjacent project without a licensing
  conversation the user hasn't had yet.
- **No inventar API.** If you're not sure a property/method exists, grep the XML docs
  (step 1) before writing the call.
- **No inventar rutas de assets.** Verify with `asset_search`/`asset_find_by_file` (scene
  MCP toolset) or `Get-ChildItem` before referencing a model, material, or sound path.
- **No cubos como armas o contenido final.** `models/dev/*`, error checkerboards, and
  placeholder primitives (e.g. the current `BarricadeAnchor` using `models/dev/box.vmdl`)
  are a documented, tracked gap — not something to leave undocumented or wave away as done.
- **No sistemas paralelos duplicados.** If `InventoryComponent` already owns inventory,
  nothing else gets to also own it. If `Sandbox.PlayerController` already owns look/move/
  camera, nothing else gets to also own it.
- **No declarar funcional porque compile.** Compiling proves the C# is syntactically valid
  against present types. It proves nothing about runtime behavior — see
  [[ultimo-barrio-runtime-proof]].

## When this skill says "write new code anyway"

Not everything has a native or reusable equivalent. Local, narrative-specific systems
(apartment ownership persistence, the day/night raid cycle, Último Barrio's specific
crafting recipes) are legitimately novel — the search in steps 1-4 will come back empty
or partial, and that's a valid, *documented* outcome, not a failure to search hard enough.
Record the empty result in the auditor fiche anyway: it's the evidence that skipping
straight to new code was the right call, not a shortcut. Two searches this session came
back empty for real reasons and are recorded as such: "In This House" (NPC AI reference —
not locatable; `AIBase`/`PerceptionComponent` already implement the needed pattern) and
"Modular Inventory System" (moot once the inventory-migration question was settled).
