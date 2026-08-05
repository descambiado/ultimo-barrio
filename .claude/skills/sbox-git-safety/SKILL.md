---
name: sbox-git-safety
description: Use for every git operation in this repository - staging, committing, branching, and especially before integrating any external dependency found via sbox-integration-factory/sbox-license-auditor. Enforces small atomic commits, selective staging (never git add -A or git add .), a checkpoint branch before any external code lands, one isolated branch per external dependency, and a hard ban on reset --hard/clean/restore-global/force-push/merge-to-main without explicit validation. This project's history includes contradictory subagent commits and lost work from unsafe git operations - this skill exists to stop that from repeating.
---

# sbox-git-safety

Último Barrio's git history already shows the cost of unsafe operations: contradictory
camera/movement commits from parallel subagents, a "false 80 minute report" checkpoint,
an "unverified cloud production report" checkpoint. This skill is the discipline that
prevents the next version of that.

## Commit discipline

- **Small commits.** One logical change per commit. The camera/movement recovery in
  this session split into three commits (prefab+modifier, cursor ownership fix, QA
  tooling+editor tool fix) rather than one giant commit — each is independently
  reviewable and revertable.
- **Selective staging, always.** Never `git add -A` or `git add .`. Name files
  explicitly:
  ```
  git add "Assets/prefabs/player.prefab" "Code/UltimoBarrio/Players/PlayerMovementModifier.cs"
  ```
  This repo accumulates stray debug output at the root (`console*.json`,
  `console_fase*.txt`, `qa_output.json`, `screenshot.*`) between sessions — a broad
  `add` will stage that noise into a commit. Check `git status --short` after any
  staging step and question anything unexpected before committing.
- **Review before commit.** `git diff --cached` (or `--check` for whitespace/
  encoding issues) before every commit, not after — catching a stray file in the
  diff is much cheaper than un-committing it.
- **Write commit messages that explain *why*, not just *what*.** The identifier
  ("fix(player): ...") is what; the body is why it broke and how the fix was
  verified — future-you (or the next agent) needs the reasoning, not a restatement
  of the diff.

## Before integrating anything external

1. **Checkpoint branch first.** Before merging in code from a [[sbox-license-auditor]]
   ADOPTAR/ADAPTAR verdict:
   ```
   git branch checkpoint/pre-<dependency-name>-integration
   ```
   Never switch to it — it's a safety net, not a workspace.
2. **One isolated branch per dependency.** Integrate each external dependency on its
   own branch, not bundled with unrelated local work. This makes a bad integration
   revertable without losing everything else done in the same window, and makes the
   `git log --follow` history for any given file legible later (this project's own
   camera history was hard to audit specifically because unrelated changes were
   bundled together).
3. **Never bulk-copy a repository.** Per [[sbox-license-auditor]], only the specific
   audited files land in the tree, with an attribution comment or NOTICE entry
   pointing at source + license + commit/tag pinned in the fiche.

## Hard bans (do not use these without explicit, in-the-moment user authorization)

```
git add -A / git add .
git reset --hard
git clean (any form)
git restore . (global/unscoped restore)
git push --force (including to a checkpoint or feature branch without asking)
merge to main without validation
```

"Validation" for a merge to `main` means the target has cleared the appropriate
[[sbox-runtime-proof]] stages for whatever it contains — not just that it compiles.

## Before any destructive command

Run `git status --short` first, always — even when confident about what's in the
working tree. If it shows anything unexpected (untracked files that look like
in-progress work, modifications not attributable to the current task), stop and
figure out what they are before running anything that could discard them. This repo
has picked up unrelated pre-existing modified files between sessions before (see:
`ItemDefinition.cs`, `AutoSaveManager.cs`, `MovementProfile.cs` sitting modified at
the start of the camera-recovery session, untouched and left alone rather than
bundled into an unrelated commit) — the correct move is to leave what you don't
understand alone, not fold it into your commit or discard it.

## Branch hygiene

- Don't work directly on `main` (already a hard rule in the project's root
  `CLAUDE.md` — repeated here because it's a git-safety concern specifically).
- Checkpoint branches (`checkpoint/*`) are cheap — create one before any risky
  integration step, without hesitation. They cost nothing and have already saved
  this project's history once (`checkpoint/pre-claude-camera-recovery`).
- Prefer new commits over amending, especially across a session boundary — amending
  a commit another session or agent already built on top of rewrites history out
  from under it.
