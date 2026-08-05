---
name: sbox-license-auditor
description: Use whenever a candidate external dependency (an s&box package, a GitHub repository, a Cloud Browser asset pack, or an official Facepunch sample) is being considered for adoption, adaptation, or pattern extraction into Último Barrio. Produces one structured fiche per candidate with an explicit ADOPTAR/ADAPTAR/EXTRAER PATRÓN/SOLO ASSETS/REFERENCIA/DESCARTAR verdict. Never treat a repository as reusable just because it is public - this skill is the license/compatibility/risk gate that sbox-integration-factory hands off to at step 5.
---

# sbox-license-auditor

(Formerly `sbox-source-auditor`, renamed and expanded per the "toma el control" full-stack
integration pass — adds the `REFERENCIA` verdict and explicit asset-license/attribution
fields.)

A public repository is not automatically safe to use. This skill produces one auditable
fiche per candidate dependency so decisions are traceable later, and so "we found it on
GitHub" never substitutes for an actual license and compatibility check.

## When to run this

Every time [[sbox-integration-factory]] step 5 onward is reached for a real candidate —
i.e. something specific enough to point at (a named repo, a named package, a named Cloud
Browser asset), not a vague "maybe something exists" hunch.

## The fiche format

Create one file per candidate under `.research/` (never commit the candidate's code
itself there, only the fiche — vendored clones for inspection go in `.research/vendor/`,
which is gitignored). Filename: `.research/<short-slug>.md`. Use this exact structure:

```markdown
# <Candidate Name>

- **Nombre:**
- **URL:** (repo, package listing, or Cloud Browser link)
- **Revisión exacta:** (commit SHA or tag pinned, not "latest" — "latest" rots)
- **Fecha:** (date of that commit/tag, not today's date)
- **Licencia de código:** (name + link to the actual LICENSE file/text you read — not assumed)
- **Licencia de assets:** (models/materials/sounds bundled may carry a *different* license
  than the code — check separately, never assume they match)
- **Atribución:** (what credit/notice is required if used — quote the exact clause)
- **Código fuente disponible:** Sí / No / Parcial
- **Sistema que aporta:** (one sentence — inventory base? door constraint? vehicle physics?)
- **Dependencias:** (what it requires that Último Barrio doesn't already have)
- **Archivos concretos útiles:** (exact paths inside the candidate, not "the whole repo")
- **Riesgos:** (networking-authority mismatch, persistence/save-data risk, GPL-family
  copyleft obligations, naming collisions with local code)
- **Veredicto:** ADOPTAR / ADAPTAR / EXTRAER PATRÓN / SOLO ASSETS / REFERENCIA / DESCARTAR
```

## Verdict definitions

- **ADOPTAR** — use as-is, essentially unmodified, wired directly into the project.
  Reserve for cases with a compatible license, current API usage, and no networking/
  persistence conflicts.
- **ADAPTAR** — the core logic is sound but needs modification to fit local data shapes
  or the host-authority model.
- **EXTRAER PATRÓN** — don't take the code, take the design decision. Write new code
  locally that follows the same approach. This is the mandatory verdict for anything under
  a copyleft license (GPL-family) whose code we do not want to relicense under — e.g.
  `Softsplit/sandbox-plus-plus` (GPL-3.0): its constraint/placement UX is worth learning
  from, its code is not worth copying.
- **SOLO ASSETS** — the code isn't usable (wrong license, wrong architecture, wrong API
  era) but included models/materials/sounds are separately licensed for reuse. Verify the
  asset license independently — code and asset licensing in the same repo are often
  different, which is exactly why this fiche format separates the two fields.
- **REFERENCIA** — not adopted, adapted, or extracted at all; kept only as reading material
  to understand current official patterns (e.g. `Facepunch/sbox-hc1`, which has **no
  LICENSE file** — confirmed 404 on `/blob/main/LICENSE` — meaning all-rights-reserved by
  default. Reading it to learn how Facepunch structures weapon/networking code is fine;
  copying from it is not).
- **DESCARTAR** — none of the above apply, or the license forbids reuse outright (e.g.
  `dxura/dxrp`: confirmed proprietary, "all rights reserved except for the limited
  permissions expressly granted," redistribution and commercial use explicitly prohibited
  without prior written permission — do not clone this into `.research/vendor/` at all,
  there is nothing legally usable to extract even for pattern reference beyond what's
  already public in its own marketing).

## License verification — do not skip this

"It's on GitHub" is not a license. Before any verdict other than DESCARTAR:

1. Find the actual license file (`LICENSE`, `LICENSE.md`, repo sidebar) or explicit
   statement in the README. No file = no license = treat as all-rights-reserved and
   DESCARTAR/REFERENCIA-only unless you can get explicit permission from the author.
2. Read what the license actually permits for a commercial or eventually-monetized game —
   check current project intent, don't assume "hobby project" permissions forever apply.
   GPL-family licenses may require this project's source to be shared under matching
   terms — flag this explicitly in the fiche, don't bury it (see `sandbox-plus-plus` above).
3. For s&box-specific Cloud Browser assets, the platform's own asset licensing terms apply
   — check the listing page itself, not just an assumption that "packages on sbox.game are
   all free to use however."
4. Facepunch's own official samples/templates (shipped with the engine install) carry
   whatever license Facepunch ships them under — check
   `C:\Program Files (x86)\Steam\steamapps\common\sbox\templates\` and any LICENSE file
   there rather than assuming MIT. Facepunch's *public GitHub* repos are not automatically
   licensed either — `sbox-hc1` has none, confirmed.

## Storage discipline

- Fiches go in `.research/` — tracked in git, reviewable, never deleted once written (a
  DESCARTAR fiche is valuable: it stops the next person from re-researching the same dead
  end, and re-cloning something already ruled out for legal reasons).
- Vendored clones for inspection go in `.research/vendor/` (gitignored — never committed).
  Only after a fiche reaches ADOPTAR/ADAPTAR and the integration is actually being done
  does code move into `Code/`/`Assets/`/`Libraries/`, and it moves with attribution (a
  comment or `docs/legal/third-party-code.md` entry pointing at source + license + pinned
  commit/tag), matching [[sbox-git-safety]]'s per-dependency branch isolation.
- Never guess or fabricate a repository URL. If a name given in a task ("Vehicle Physics
  Kit", "S-box-Field-Guide/vehicle-prototyping", "In This House", "Modular Inventory
  System") does not resolve to a real, findable repository after an honest search, record
  that plainly and substitute a verified alternative or leave the slot open — do not invent
  a URL to fill the gap.
