---
name: sbox-reuse-first
description: Use before writing ANY new gameplay system, component, or content pipeline in this s&box project (inventory, items, weapons, doors, barricades, crafting, trading, vehicles, NPCs, persistence, UI panels, etc). Forces a reuse-before-rewrite search across the installed s&box API, official docs, Facepunch source, the Library Manager, the Cloud Browser, and licensed community packages BEFORE any new class or system gets written. Also applies when reviewing existing code that duplicates something the engine or an official package already provides. Do not use for pure bug fixes to existing local code that has no native equivalent (e.g. the camera/movement fixes already done) - use this when the question is "should this be a new system at all."
---

# sbox-reuse-first

Último Barrio has a documented history of subagents writing parallel, contradictory
systems from scratch (duplicate camera solutions, three inventory-adjacent components,
cube weapons) because nobody checked what already existed first. This skill exists to
stop that pattern before it produces another one.

## The rule

**Reuse before rewrite.** Every new system starts with a search, not a blank file.
No exceptions for "it'll just take five minutes to write."

## The 9-step order

Work through these in order. Do not skip to step 9 because steps 1-8 feel slow.

1. **Search the installed API.** Grep the actually-installed engine, not memory of a
   past s&box version:
   ```
   C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.Engine.xml
   C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.System.xml
   C:\Program Files (x86)\Steam\steamapps\common\sbox\bin\managed\Sandbox.Tools.xml
   ```
   These are XML doc comments extracted from the real compiled API — they say what
   exists *in this install*, not what existed in a blog post from two years ago.
   Search for the noun first (`Inventory`, `Door`, `Weapon`, `NavMesh`, `Constraint`),
   then read every member's summary before concluding nothing fits.

2. **Search for an official current example.** Check:
   ```
   C:\Program Files (x86)\Steam\steamapps\common\sbox\templates\
   C:\Program Files (x86)\Steam\steamapps\common\sbox\samples\
   C:\Program Files (x86)\Steam\steamapps\common\sbox\addons\
   ```
   The `game.playercontroller` template, `sweeper` sample, and any addon source
   (`menu`, `tools`) are real first-party code using the current API shape — more
   reliable than any web search result, which may predate a breaking API change.

3. **Search Library Manager / installed packages.** s&box packages with visible
   source live under the engine's package cache and inside any addon that mounts
   them. Check the project's `.sbproj` `PackageReferences` and look for `Libraries/`
   in the repo — packages already pulled in may already solve this.

4. **Search sbox.game and GitHub for compatible repositories.** Use WebSearch /
   WebFetch against `sbox.game` package listings and GitHub for packages whose source
   is visible and whose last commit targets a current engine build (check API surface
   used, not just recency of the commit date — an old commit using still-current API
   beats a recent commit using a removed one).

5. **Verify the license.** A public GitHub repo is not automatically reusable. See
   [[sbox-source-auditor]] for the full check — never skip straight to copying code
   because a repo is public.

6. **Verify date and API used.** Cross-reference the candidate's use of s&box APIs
   against what step 1 confirmed exists *now*. A package built against a
   pre-`Sandbox.PlayerController` era will not compile, or worse, will compile against
   stale interop and misbehave silently.

7. **Compare against local architecture.** Read the actual current local component
   (`InventoryComponent`, `HeldItemController`, etc — not a memory of what it does).
   Does the candidate's data model match `ItemDefinition`/`ItemRegistry`? Does it
   assume single-player where Último Barrio requires host-authoritative networking?

8. **Decide: adopt, adapt, extract pattern, or discard.** Write the verdict down in
   an [[sbox-source-auditor]] fiche before touching any code. "Extract pattern" means
   you write new code, but the design decisions (data shape, event flow, edge case
   handling) come from the reference, not from scratch.

9. **Only write new code for the specific gap.** If steps 1-8 found 90% of the
   system, the new code is the missing 10%, not a fresh reimplementation of the whole
   thing "to be safe" or "to match our style." Adapt call sites to match local
   conventions; don't duplicate the reference's logic under a new name.

## Hard rules

- **No inventing API.** If you're not sure a property/method exists, grep the XML
  docs (step 1) before writing the call. A build error from a hallucinated API wastes
  a cycle; a runtime silent-failure from a *slightly* wrong hallucinated overload
  wastes a debugging session.
- **No inventing asset paths.** Verify with `asset_search` / `asset_find_by_file`
  (scene MCP toolset) or `Get-ChildItem` before referencing a model, material, or
  sound path in code or a prefab. A missing asset reference fails silently at runtime
  (`TypeLibrary could not find...` / broken reference warnings), not at compile time.
- **No dev/error models as shipped content.** `models/dev/*`, error checkerboards,
  and placeholder primitives do not go into anything described as "implemented" —
  they're allowed only as a temporary marker explicitly logged as such.
- **No parallel system if a native one exists.** If `Sandbox.PlayerController`
  already owns look/move/camera, nothing else gets to also own it (this is exactly
  the bug the camera fix just resolved — do not reintroduce the pattern elsewhere).
- **No copying whole repositories without audit.** Extract the specific files/patterns
  needed; a full-repo copy drags in its bugs, its license terms for parts you don't
  need, and its API assumptions for code paths you'll never call.
- **Compiling is not integration.** "0 errors, 0 warnings" proves the C# is
  syntactically valid against present types. It proves nothing about whether the
  system is wired to real data, reachable from real input, or produces the intended
  runtime behavior. See [[sbox-runtime-proof]] for what "done" actually requires.

## When this skill says "write new code anyway"

Not everything has a native or reusable equivalent. Local, narrative-specific systems
(apartment ownership persistence, the day/night raid cycle, Último Barrio's specific
crafting recipes) are legitimately novel — the search in steps 1-4 will come back
empty or partial, and that's a valid, *documented* outcome, not a failure to search
hard enough. Record the empty result in the auditor fiche anyway: it's the evidence
that skipping straight to new code was the right call, not a shortcut.
