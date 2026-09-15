# I-56 — integration tag protection

## Result

```text
Observation UTC = 2026-09-15T23:00:56Z
Repository      = marioap-afk/Calculadora_de_racks
Ruleset ID      = 23505961
Ruleset name    = Protect integration/* tags
Target          = tag
Enforcement     = active
Pattern         = refs/tags/integration/*
Conclusion      = PASS
```

This is the administrative prerequisite authorized by Proposal V4, ADR-0045 and Owner selection
`OWN-H = A`. It does not start the activation pause, create an integration tag, declare a Candidate,
integrate I-56 or make Workflow V2 effective.

## Preflight

The preflight ran `git fetch --all --prune` before changing GitHub configuration.

| Check | Observed result |
|---|---|
| Branch | `docs/initiative-workflow-v2` |
| Worktree | clean |
| Local HEAD | `f1b60391cf22fd3371c50e9cfba787812b13c14b` |
| Upstream HEAD | `f1b60391cf22fd3371c50e9cfba787812b13c14b` |
| `origin/main` after fetch | `dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093` |
| `git ls-remote origin refs/heads/main` | `dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093` |
| Existing repository rulesets | none |
| Existing matching `integration/*` tags | none |
| Legacy tag-protection endpoint | HTTP 404; GitHub directs current protection to repository rulesets |

No equivalent active protection pre-existed, so one ruleset was created. No duplicate was created.

## Effective configuration

The created repository ruleset has this effective state:

| Requirement | API fact | Verdict |
|---|---|---|
| Ruleset identity | ID `23505961`; `Protect integration/* tags` | PASS |
| Enforced state | `enforcement: active` | PASS |
| Correct target | `target: tag` | PASS |
| Exact namespace | include `refs/tags/integration/*`; no exclusions | PASS |
| Update/movement restriction | rule `update` | PASS |
| Deletion restriction | rule `deletion` | PASS |
| Direct bypass | `bypass_actors: []`; `current_user_can_bypass: never` | PASS |
| New-tag creation | no `creation` rule; authenticated integrator has repository push permission | PASS |

GitHub defines `update` and `deletion` rules as allowing those operations only to configured bypass
actors. This ruleset has no such actors. Conversely, GitHub restricts creation only when a `creation`
rule is present; this ruleset does not contain one. An authorized integrator with tag-push permission
can therefore create a new `integration/<unit>` or `integration/<unit>-corr<N>`, while an existing
matching ref cannot ordinarily be moved, force-updated or deleted.

Verification was non-destructive. It did not create a fake tag, update a tag, delete a tag or move a
real tag.

## Bypass and administrative limitation

There is **no configured operational bypass** in the ruleset, including no RepositoryRole, User,
Integration, Team or deploy-key actor. The API explicitly reports that the authenticated repository
Owner cannot directly bypass the ref rules.

The repository Owner has `admin: true`. GitHub administrators with permission to edit repository
rules can modify, disable or delete a repository ruleset. Therefore the Owner retains an
**administrative capability to remove or weaken protection first**; GitHub does not make this
repository-level configuration unchangeable by its administrator. This report does not claim
protection against that administrative action.

The authenticated API response includes `bypass_actors`, so bypass visibility was sufficient for
this verification. No credential, token or sensitive administrative value is recorded.

## Reproducible inspection

The conclusion can be reproduced without mutating refs:

```powershell
git fetch --all --prune
git status --short --branch
git rev-parse HEAD
git rev-parse '@{u}'
git rev-parse origin/main
git ls-remote origin refs/heads/main

gh api 'repos/marioap-afk/Calculadora_de_racks/rulesets?includes_parents=true&targets=tag'
gh api 'repos/marioap-afk/Calculadora_de_racks/rulesets/23505961'
gh api 'repos/marioap-afk/Calculadora_de_racks' --jq '{full_name,permissions,owner:.owner.login}'
git ls-remote --tags origin 'refs/tags/integration/*'
```

The exact ruleset response used for verification, with no credentials, was:

```json
{
  "id": 23505961,
  "name": "Protect integration/* tags",
  "target": "tag",
  "source_type": "Repository",
  "source": "marioap-afk/Calculadora_de_racks",
  "enforcement": "active",
  "conditions": {
    "ref_name": {
      "exclude": [],
      "include": ["refs/tags/integration/*"]
    }
  },
  "rules": [
    {"type": "update"},
    {"type": "deletion"}
  ],
  "bypass_actors": [],
  "current_user_can_bypass": "never",
  "created_at": "2026-09-15T17:00:15.350-06:00",
  "updated_at": "2026-09-15T17:00:15.376-06:00",
  "_links": {
    "self": {
      "href": "https://api.github.com/repos/marioap-afk/Calculadora_de_racks/rulesets/23505961"
    },
    "html": {
      "href": "https://github.com/marioap-afk/Calculadora_de_racks/rules/23505961"
    }
  }
}
```

## Gate status

```text
G13A TAG PROTECTION = PASS
ACTIVATION PRECONDITION = SATISFIED

ACTIVATION PAUSE = NOT STARTED
WORKFLOW V2 = NOT EFFECTIVE
WORKFLOW_V2_EFFECTIVE_SHA = DOES NOT EXIST
```
