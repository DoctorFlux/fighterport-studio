# Start FighterPort Studio from an empty folder

Prepared 16 October 2026. This is an instruction package, not an implemented application. No earlier FighterPort documents are needed. The private repository is to be created by Codex on your computer, not by downloading this file.

## 1. Use a new local folder

Create a fresh folder such as `C:\Dev\FighterPortStudioFresh`. Do not delete or reuse an older project just to start this one.

Put `CODEX_START_FROM_ZERO.md` in that folder. Keep the full filename, including `.md`.

## 2. Verify tools and GitHub login

In PowerShell, run:

```powershell
git --version
gh --version
codex --version
gh auth status --hostname github.com
```

If GitHub CLI says you are not authenticated, run:

```powershell
gh auth login --hostname github.com --git-protocol https --web
```

Complete its browser authorization yourself. Use the personal account that should own the repository. Do not paste authentication tokens into Codex or the project.

If Git has not been configured to use your GitHub CLI login, the documented helper setup command is:

```powershell
gh auth setup-git --hostname github.com
```

This configures Git's credential helper for the host. It is separate from Git commit author identity. Codex's prompt instructs it to handle missing author identity repository-locally, without guessing.

## 3. Launch Codex in the new folder

```powershell
Set-Location 'C:\Dev\FighterPortStudioFresh'
codex --search --sandbox workspace-write --ask-for-approval on-request
```

Complete Codex sign-in if needed. Keep the sandbox and required approval prompts enabled.

In Codex, enter `/model`, then select **GPT-6 Astra** and **High** for this complex first integration task. **GPT-6.1 Sol** is a lower-cost alternative; use a supported effort in the picker. Availability depends on the account and client. High is a project-specific recommendation, not a requirement to use the maximum setting.

## 4. Paste this ONE implementation instruction

```text
Read CODEX_START_FROM_ZERO.md completely and execute it in this workspace.

This is a fresh project, not a continuation of an earlier FighterPort codebase.
Create the persistent project instructions, full roadmap, application, tests,
and build scripts, then implement in tested increments.

I authorize you to create one new PRIVATE GitHub repository in my currently
authenticated personal account, initialize Git here, verify the remote is
private, and commit/push reviewed project source as specified in the document.
Do not reuse another existing repository or upload game assets or secrets.

I have no commercial game assets yet. Generate original skinned characters
and genuine native-format fixtures through verified backends so development
and native import/playback testing can proceed without them.

Do not stop after planning or repository setup. Implement the first runnable
workbench/native-integration slice, build and test it, and continue the next
ready acceptance criterion during this run. Respect required approvals.

Before ending, update project status and report the actual private repository
URL, verified build/run paths, test results, limitations, and next task.
```

Codex should discover the GitHub account itself through `gh`; no username placeholder needs editing. The prompt requires verifying privacy before pushing. If the preferred repository name is occupied, it chooses a new name instead of overwriting another project.

Browser login, required approvals, unavailable SDKs, and session limits may still require your involvement. A prompt cannot supply missing permissions or unlimited runtime. A generated fixture proves its tested subset, not every retail character.

## 5. Resume later without repeating the specification

From the same project folder:

```powershell
codex resume --last
```

Then enter:

```text
Read AGENTS.md, docs/DEVELOPMENT_STATUS.md, and docs/plans/current.md.
Implement and test the next incomplete acceptance criterion, maintain the
full master plan, and commit/push reviewed project changes only after
rechecking the intended private GitHub remote. Update the checkpoint.
```

## Official command references

- Codex CLI flags, resume and slash commands: https://developers.openai.com/codex/cli/reference
- Model and effort selection: https://learn.chatgpt.com/docs/models
- Persistent repository instructions: https://developers.openai.com/codex/agent-configuration/agents-md
- GitHub login status: https://cli.github.com/manual/gh_auth_status
- Browser login: https://cli.github.com/manual/gh_auth_login
- Git credential helper: https://cli.github.com/manual/gh_auth_setup-git
- Private repository creation: https://cli.github.com/manual/gh_repo_create
- Repository privacy verification: https://cli.github.com/manual/gh_repo_view

Commands were checked against the official documentation. They have not been executed on your Windows PC in this conversation.
