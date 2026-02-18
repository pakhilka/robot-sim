<!-- OPENSPEC:START -->

# OpenSpec Instructions

These instructions are for AI assistants working in this project.

Always open `openspec/AGENTS.md` when the request:
- Mentions planning/proposals/spec/change/architecture
- Introduces new features, breaking changes, or major performance/security work
- Feels ambiguous and you need the authoritative spec before coding

Use `openspec/AGENTS.md` to learn:
- How to create and apply change proposals
- Spec format and conventions
- Project structure and guardrails

Keep this managed block so `openspec update` can refresh the instructions.

<!-- OPENSPEC:END -->

# RobotSim — Agent Operating Rules (Codex entrypoint)

## Priorities
- Focus: **features** (next month)
- Bias: **70% quality/stability**, 30% speed
- Work on **code only** (NO scene editing)

## Hard Guardrails (DO NOT VIOLATE)
- **The agent may only modify project code under `Assets/Scripts/**` by default.**
- **DO NOT modify**:
  - `ProjectSettings/**`
  - `Packages/**`
  - `Assets/**/*.unity` (scenes)
  - `Assets/**/Prefabs/**` (prefabs) unless explicitly allowed (default: no)
  - Any other files under `Assets/**` that are **not** inside `Assets/Scripts/**` (art, settings, assets, input assets, etc.)
- **No secrets/keys** in the repo or in chat. Never commit credentials. Use safe storage methods.
- Before running any shell command (`git`, `dotnet`, tests, etc.) **ask for explicit confirmation**.
- **Git commands require explicit permission** (including `git status`, `git add`, `git commit`, `git push`, `git pull`, `git rebase`, etc.).

## Default response style
1) Questions/Risks (if any)
2) Contract/Spec (acceptance criteria, interfaces, DTOs impacted)
3) Only after the explicit approval keyword: **"Go!"** → implementation plan + diffs

## Workflow (OpenSpec → Beads → Code)
- OpenSpec is **mandatory** for:
  - new features
  - architectural changes
  - public API changes
- **No quick-fix mode**: even small changes must have an OpenSpec change.
- Beads is a **supporting tracker** (planning + execution), not the main communication tool.

### Approval gate (STRICT)
- **Do NOT proceed to implementation** until the user explicitly says **"Go!"**.

## OpenSpec command execution (CLI) + no-guessing rule (STRICT)
OpenSpec is used via **CLI in the terminal** (not as chat slash-commands).

Rules:
- Do NOT invent OpenSpec commands or variants (e.g., `openspec-to-beads`, `openspec-apply-change`) and do NOT suggest `PATH=...` hacks.
- If an OpenSpec command is unknown/missing, ask the human to run exactly one diagnostic command:
  - `openspec --help` (human-run)
- The agent must ask for explicit confirmation before running any terminal commands.

## Beads generation: automation may be unavailable (fallback required)
A `to-beads` automation may not exist in this environment.
If automated "to-beads" is unavailable, the agent MUST use manual fallback:
- Read `openspec/changes/<change-id>/tasks.md`
- Create one Beads Epic + many small Beads tasks via `bd create`
- Add dependencies via `bd dep add` where needed
- Continue execution via `bd ready`

Rules:
- Do NOT guess command names.
- Prefer many small tasks.

## Git Branch Naming (required)
- Features: `feature/<name>`
- Bug fixes: `bugfix/<name>`
- Infrastructure/process/tooling: `infrastructure/<name>`

Naming rules:
- Use lowercase and hyphens: `feature/tcp-reconnect`, `bugfix/motor-turning`
- Keep it short and descriptive (2–5 words).

## Session Handoff (mandatory)
At the end of each work session, provide:
- What changed (files/modules)
- What is done vs pending
- How to verify locally (commands to run; user executes them)
- Next steps / which `bd ready` task to pick next

## Landing the Plane (Session Completion)

**When ending a work session**, you MUST complete ALL steps below. Work is NOT complete until `git push` succeeds.

**MANDATORY WORKFLOW:**
1. File issues for remaining work
2. Run quality gates (if code changed)
3. Update issue status
4. PUSH TO REMOTE (MANDATORY):
   ```bash
   git pull --rebase
   bd sync
   git push
   git status  # MUST show "up to date with origin"
