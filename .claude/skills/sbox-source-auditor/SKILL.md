---
name: sbox-source-auditor
description: Use whenever a candidate external dependency (an s&box package, a GitHub repository, a Cloud Browser asset pack, or an official Facepunch sample) is being considered for adoption, adaptation, or pattern extraction into Último Barrio. Produces one structured fiche per candidate with a explicit ADOPTAR/ADAPTAR/EXTRAER PATRÓN/SOLO ASSETS/DESCARTAR verdict. Never treat a repository as reusable just because it is public - this skill is the license/compatibility/risk gate that sbox-reuse-first hands off to at step 5.
---

# sbox-source-auditor

A public repository is not automatically safe to use. This skill produces one
auditable fiche per candidate dependency so decisions are traceable later, and so
"we found it on GitHub" never substitutes for an actual license and compatibility
check.

## When to run this

Every time [[sbox-reuse-first]] step 5 onward is reached for a real candidate — i.e.
something specific enough to point at (a named repo, a named package, a named Cloud
Browser asset), not a vague "maybe something exists" hunch.

## The fiche format

Create one file per candidate under `.research/` (see [[sbox-reuse-first]] — never
commit the candidate's code itself there, only the fiche). Filename:
`.research/<short-slug>.md`. Use this exact structure:

```markdown
# <Candidate Name>

- **Nombre:** 
- **Fuente:** (URL — repo, package listing, or Cloud Browser link)
- **Commit/tag/revisión:** (exact SHA or version pinned, not "latest" — "latest" rots)
- **Última actualización:** (date of that commit/tag, not today's date)
- **Licencia:** (name + link to the actual license file/text you read — not assumed)
- **Código fuente disponible:** Sí / No / Parcial
- **Sistema que aporta:** (one sentence — inventory base? door constraint? vehicle physics?)
- **API de s&box utilizada:** (concrete types/members it calls — cross-check against
  the installed XML docs per sbox-reuse-first step 6)
- **Compatibilidad probable:** Alta / Media / Baja — with the one-line reason
- **Archivos concretos útiles:** (exact paths inside the candidate, not "the whole repo")
- **Dependencias:** (what it requires that Último Barrio doesn't already have)
- **Conflictos con nuestro proyecto:** (naming collisions, architecture mismatches,
  assumes singleplayer where we need host-authoritative, etc.)
- **Riesgo de networking:** Ninguno / Bajo / Medio / Alto — does it assume
  authority models incompatible with our host-authoritative rule?
- **Riesgo de persistencia:** Ninguno / Bajo / Medio / Alto — does adopting it risk
  existing save data (apartment claims, inventories already on disk)?
- **Trabajo de integración:** (rough scope — "drop-in", "needs adapter component",
  "needs data migration")
- **Veredicto:** ADOPTAR / ADAPTAR / EXTRAER PATRÓN / SOLO ASSETS / DESCARTAR
```

## Verdict definitions

- **ADOPTAR** — use as-is, essentially unmodified, wired directly into the project.
  Reserve for cases with a compatible license, current API usage, and no networking/
  persistence conflicts.
- **ADAPTAR** — the core logic is sound but needs modification to fit local data
  shapes (e.g. `ItemDefinition`/`ItemRegistry`) or the host-authority model. Specify
  what changes in "Trabajo de integración."
- **EXTRAER PATRÓN** — don't take the code, take the design decision. Write new code
  locally that follows the same approach (e.g. "constraint-based door hinges" as a
  concept, reimplemented against our own component model).
- **SOLO ASSETS** — the code isn't usable (wrong license, wrong architecture, wrong
  API era) but included models/materials/sounds are separately licensed for reuse.
  Verify the asset license independently — code and asset licensing in the same repo
  are often different.
- **DESCARTAR** — none of the above apply. Record *why* — this is what proves the
  search in sbox-reuse-first step 1-4 was actually done, not skipped.

## License verification — do not skip this

"It's on GitHub" is not a license. Before any verdict other than DESCARTAR:

1. Find the actual license file (`LICENSE`, `LICENSE.md`, repo sidebar) or explicit
   statement in the README. No file = no license = treat as All Rights Reserved and
   DESCARTAR unless you can get explicit permission from the author.
2. Read what the license actually permits for a commercial or eventually-monetized
   game (Último Barrio's status may change — check current project intent, don't
   assume "hobby project" permissions forever apply). GPL-family licenses may require
   this project's source to be shared under matching terms — flag this explicitly in
   the fiche if relevant, don't bury it.
3. For s&box-specific Cloud Browser assets, the platform's own asset licensing terms
   apply — check the listing page itself, not just an assumption that "packages on
   sbox.game are all free to use however."
4. Facepunch's own official samples/templates (shipped with the engine install) carry
   whatever license Facepunch ships them under — check
   `C:\Program Files (x86)\Steam\steamapps\common\sbox\templates\` and any LICENSE
   file there rather than assuming MIT.

## Storage discipline

- Fiches go in `.research/` — tracked in git, reviewable, never deleted once written
  (a DESCARTAR fiche is valuable: it stops the next person from re-researching the
  same dead end).
- The candidate's actual source code does **not** get copied into `.research/`,
  `Code/`, `Assets/`, or `Libraries/` at fiche-writing time. Only after a fiche
  reaches ADOPTAR/ADAPTAR and the integration is actually being done does code
  movement happen, and it happens with attribution (a comment or NOTICE entry
  pointing at source + license), matching [[sbox-git-safety]]'s per-dependency
  branch isolation.
