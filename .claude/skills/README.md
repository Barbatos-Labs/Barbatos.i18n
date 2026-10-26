# Barbatos skills

Claude Code discovers every `<name>/SKILL.md` in this folder when it opens the repository. It lists each skill's
`description` up front and loads the body only when a task calls for it. Every skill is prefixed `barbatos-` so it
is distinguishable from third-party skills at a glance. There are two kinds, for two different readers.

## For working on the library: `barbatos-dev-*`

Procedures an agent follows when changing this repository. The invariants of each part of the code are not here
but in [`../rules/`](../rules), which load automatically when a matching file is opened; [`CLAUDE.md`](../../CLAUDE.md)
maps both.

| Skill | Covers |
|---|---|
| [`barbatos-dev-verify`](barbatos-dev-verify/SKILL.md) | Choosing and running the test suites, the WPF and MAUI suites from Linux (`scripts/verify-linux.sh`), checking the public API with ApiCompat |
| [`barbatos-dev-add-file-format`](barbatos-dev-add-file-format/SKILL.md) | A new `Barbatos.i18n.<Format>` package: layout, entry points, parser, tests, wiring, docs |
| [`barbatos-dev-release`](barbatos-dev-release/SKILL.md) | Changing public API safely, versioning, and what the publishing workflow really does |

## For apps that use the packages: `barbatos-i18n-*`

These teach an agent how to *consume* the Barbatos.i18n NuGet packages correctly. They are part of the product:
they are distributed to users, and inside this repository they are the reference for samples, the README and
"how should this behave" questions - not instructions for changing `src/`.

| Skill | Covers |
|---|---|
| [`barbatos-i18n-setup`](barbatos-i18n-setup/SKILL.md) | Picking packages, registering providers, switching language. The entry point; routes to the rest |
| [`barbatos-i18n-xaml`](barbatos-i18n-xaml/SKILL.md) | WPF and MAUI markup extensions, keys from bindings, plurals, live language switching |
| [`barbatos-i18n-aspnetcore`](barbatos-i18n-aspnetcore/SKILL.md) | Servers and concurrent requests — read before writing any server-side localization |
| [`barbatos-i18n-resources`](barbatos-i18n-resources/SKILL.md) | Authoring JSON, YAML, INI, CSV and RESX locale files |
| [`barbatos-i18n-troubleshooting`](barbatos-i18n-troubleshooting/SKILL.md) | Symptom-first diagnosis when text is wrong, missing or stale |

The split is by **consumption surface** rather than by feature, because the surfaces have genuinely different
hazards. The server skill exists as its own entry for that reason: the library's default configuration keeps
one culture per process, which is correct for a desktop app and silently wrong for a web API, and an agent
writing server code needs that in front of it before it writes the registration.

### Giving them to users

- **Claude Code, in an app's repository**: copy the five `barbatos-i18n-*` folders into that repository's
  `.claude/skills/` (or `~/.claude/skills/` for every project). Do not copy the `barbatos-dev-*` ones; they are
  about this repository.
- **claude.ai / Claude Desktop**: zip each `barbatos-i18n-*` folder separately and upload it under Settings →
  Skills. Upload all five, since they route to each other. An uploaded copy does not update itself: re-upload
  after a change here.

## Keeping them accurate

A skill that documents an API that no longer exists, or behaviour the library no longer has, is worse than no
skill, since an agent will follow it confidently. When changing public API or observable behaviour in `src/`,
search for it in the same change:

```bash
grep -rn "MethodName" .claude/skills .claude/evals README.md API-REFERENCE.md
```

Every C# snippet in the consumer skills has been compiled and run against the library, and every XAML property
named in them was checked against the source. Keep it that way — prefer verifying a snippet over trusting that it
still looks right. The prompts and assertions used to evaluate the consumer skills are in
[`../evals/barbatos-i18n-skills.json`](../evals/barbatos-i18n-skills.json); an assertion that encodes old
behaviour grades a correct answer as wrong, so update it along with the skill.
