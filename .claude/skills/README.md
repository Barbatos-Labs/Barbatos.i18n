# Barbatos skills

Skills that teach an AI coding agent how to consume the Barbatos.i18n NuGet packages correctly. Every skill in
this repository is prefixed `barbatos-` so it is distinguishable from third-party skills at a glance.

These are written for the *consumer* of the packages, not for someone working on the library itself —
contributors should read [`CLAUDE.md`](../CLAUDE.md) at the repository root instead.

| Skill | Covers |
|---|---|
| [`barbatos-i18n-setup`](barbatos-i18n-setup/SKILL.md) | Picking packages, registering providers, switching language. The entry point; routes to the rest |
| [`barbatos-i18n-xaml`](barbatos-i18n-xaml/SKILL.md) | WPF and MAUI markup extensions, keys from bindings, plurals, live language switching |
| [`barbatos-i18n-aspnetcore`](barbatos-i18n-aspnetcore/SKILL.md) | Servers and concurrent requests — read before writing any server-side localization |
| [`barbatos-i18n-resources`](barbatos-i18n-resources/SKILL.md) | Authoring JSON, YAML, INI, CSV and RESX locale files |
| [`barbatos-i18n-troubleshooting`](barbatos-i18n-troubleshooting/SKILL.md) | Symptom-first diagnosis when text is wrong, missing or stale |

## Using them

Copy the folders into the agent's skills directory, or point the agent at this directory. Each skill is a
self-contained `SKILL.md`: YAML frontmatter naming the skill and describing when it applies, then the
instructions.

The split is by **consumption surface** rather than by feature, because the surfaces have genuinely different
hazards. The server skill exists as its own entry for that reason: the library's default configuration keeps
one culture per process, which is correct for a desktop app and silently wrong for a web API, and an agent
writing server code needs that in front of it before it writes the registration.

## Keeping them accurate

A skill that documents an API that no longer exists is worse than no skill, since an agent will follow it
confidently. When changing public API in `src/`, check whether any skill mentions it:

```bash
grep -rn "MethodName" skills/
```

Every C# snippet in these skills has been compiled and run against the library, and every XAML property named
in them was checked against the source. Keep it that way — prefer verifying a snippet over trusting that it
still looks right.
