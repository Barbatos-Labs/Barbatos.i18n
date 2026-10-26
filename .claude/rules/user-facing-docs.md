---
paths:
  - ".claude/skills/barbatos-i18n-*/**"
  - ".claude/evals/**"
  - "README.md"
  - "API-REFERENCE.md"
  - "samples/**"
---

# User-facing documentation: README, API reference, samples and consumer skills

These describe the library to the people who install it from NuGet. They are product, not notes, and an agent or a
developer will follow them confidently - so a statement about an API that no longer behaves that way is worse than
no statement.

- **The `barbatos-i18n-*` skills are for consumers of the packages**, not instructions for changing the library.
  They are distributed outside this repository (zipped and uploaded to claude.ai, or copied into an app's
  `.claude/skills/`), so a change here only reaches users once they are re-uploaded. Keep their names stable:
  `.claude/evals/barbatos-i18n-skills.json` and any uploaded copies refer to them by name.
- **Every C# snippet compiles and runs, and every XAML property exists.** Verify a snippet you add or change
  against the library - a scratch console project referencing `src/` is enough - rather than trusting that it
  still looks right. Check XAML property names against the extension classes in `src/`.
- **Describe behaviour as the code has it today.** When a fix changes observable behaviour (the plural rule for
  zero, which culture decides it, set precedence), search these files for the old statement in the same change:
  `grep -rn "<term>" README.md API-REFERENCE.md .claude/skills .claude/evals samples`.
- **Evals.** `.claude/evals/barbatos-i18n-skills.json` holds prompts and assertions for the consumer skills. An
  assertion that encodes old behaviour grades a correct answer as a failure; update it with the behaviour.
- **Samples are read as examples.** Comments in English, the recommended APIs (`BindText` over
  `LocalizeConverter`, `ICompositeStringLocalizer`, `UseAmbientCulture` on the server), and no workaround that a
  consumer would copy for the wrong reason.
