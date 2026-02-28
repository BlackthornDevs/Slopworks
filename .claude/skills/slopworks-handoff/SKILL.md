---
name: slopworks-handoff
description: End a Slopworks dev session - write handoff notes, update coordination docs, commit, push, and update memory so the next session picks up seamlessly
user_invocable: true
---

# Slopworks Session Handoff

When the user says "handoff", "end session", or "wrap up", perform ALL of the following steps. This is a Slopworks-specific session end that preserves context for the next Claude session AND for Joe's Claude working in parallel.

## Steps

### 1. Write session handoff file

Create or overwrite `docs/coordination/handoff-kevin.md` with:

```markdown
# Kevin's Claude -- Session Handoff

Last updated: [YYYY-MM-DD HH:MM]
Branch: kevin/main
Last commit: [hash] [message]

## What was completed this session
- [Bullet list of everything done, with file paths]

## What's in progress (not yet committed)
- [Any uncommitted work, or "None -- all committed"]

## Next task to pick up
- [Specific next step with enough detail to start immediately]
- [Reference the plan phase/task number if applicable]

## Blockers or decisions needed
- [Any open questions, or "None"]

## Test status
- [X/Y passing, any known failures]

## Key context the next session needs
- [Anything non-obvious: workarounds, gotchas, partially built systems]
```

### 2. Update tasks-joe.md (if jawn's tasks changed)

If you assigned new tasks, completed tasks that unblock jawn, or have info jawn's Claude needs, update `docs/coordination/tasks-joe.md` with current status.

### 3. Update decisions.md (if architectural decisions were made)

If any architectural decisions were made during the session, add them to `docs/coordination/decisions.md` following the existing format.

### 4. Update auto-memory

Update `C:\Users\KevinAmditis\.claude\projects\C--Users-KevinAmditis-source-repos\memory\MEMORY.md` with:
- Any new patterns or conventions discovered
- Key file paths for new systems
- Solutions to problems encountered
- Current phase and progress

Keep it concise. Don't duplicate what's in CLAUDE.md or the handoff file.

### 5. Commit all changes

- Run `git status` to see what's outstanding
- Stage all relevant files (exclude `.claude/settings.local.json`)
- If there are uncommitted code changes, commit them first with a descriptive message
- Then commit the handoff/coordination updates separately:
  ```
  Update session handoff and coordination docs
  ```
- NEVER include Co-Authored-By lines

### 6. Push to kevin/main

- Push to `kevin/main`
- Do NOT push to `master` without explicit user permission
- Report the final commit hash

### 7. Summary to user

Print a brief summary:
- What was accomplished
- Where to pick up next session
- Whether jawn has new instructions waiting
- Final test count

## Important Notes

- NEVER push to `master` without explicit user permission
- NEVER add Co-Authored-By lines to commits
- The handoff file is the primary way the next session recovers context -- make it thorough
- Joe's Claude reads `tasks-joe.md` and `decisions.md` from master, so push coordination changes to master if they affect jawn
- If you made changes to shared code (Scripts/Core/, ScriptableObjects/, ProjectSettings/), note this prominently -- it needs to go to master for jawn to pick up
