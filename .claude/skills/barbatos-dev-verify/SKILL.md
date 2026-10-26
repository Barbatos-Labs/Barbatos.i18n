---
name: barbatos-dev-verify
description: Verify a change to the Barbatos.i18n library itself before calling it done - pick and run the right test suites, compile WPF and run the MAUI tests even from Linux where their test projects cannot run, and prove the NuGet public API did not change by accident with ApiCompat against a git ref. Use this whenever you have modified anything under src/ or tests/ of this repository, before committing, pushing or opening a pull request, when asked to run the tests, build it, check nothing broke or confirm a refactoring is behaviour-preserving, and whenever a dotnet build or test here fails for lack of Windows, the MAUI workload or an SDK.
allowed-tools:
  - Bash(${CLAUDE_SKILL_DIR}/scripts/verify-linux.sh)
  - Bash(${CLAUDE_SKILL_DIR}/scripts/verify-linux.sh *)
---

# Verifying a change to Barbatos.i18n

Nothing else will catch a mistake: **no CI runs on pull requests**, and the only workflow publishes to NuGet.org,
where a package can never be deleted - while its test step currently runs no tests (see `barbatos-dev-release`).
Verification happens here, before the push.

## 1. Choose what to run

Run the suites for what you touched, plus everything downstream of it:

| Changed | Run |
|---|---|
| `src/Barbatos.i18n/` | everything - every package depends on the core |
| `src/Barbatos.i18n.DependencyInjection/` | `di` |
| `src/Barbatos.i18n.{Json,Ini,Csv}/` | `formats` |
| `src/Barbatos.i18n.Wpf/` or `.Maui/` | `wpf` **and** `maui` - the packages mirror each other, so a change to one usually has a twin |
| public types or members anywhere | add the public API check (section 3) |

## 2. Run it

**On Windows**, with the MAUI workload restored (`dotnet workload restore`), run the projects directly and pass
`-f` so the multi-targeted projects do not build for every platform:

```bash
dotnet test tests/Barbatos.i18n.UnitTests/Barbatos.i18n.UnitTests.csproj -f net10.0
dotnet test tests/Barbatos.i18n.Wpf.UnitTests/Barbatos.i18n.Wpf.UnitTests.csproj -f net10.0-windows
dotnet test tests/Barbatos.i18n.Maui.UnitTests/Barbatos.i18n.Maui.UnitTests.csproj -f net10.0-windows10.0.19041.0
```

**On Linux** use the bundled script, `scripts/verify-linux.sh` in this skill's folder
(`.claude/skills/barbatos-dev-verify/`). It needs only the .NET 10 SDK, which it can install. It has not been tried
on macOS.

```bash
${CLAUDE_SKILL_DIR}/scripts/verify-linux.sh                          # all suites, net10.0
${CLAUDE_SKILL_DIR}/scripts/verify-linux.sh maui wpf                 # just the UI packages
${CLAUDE_SKILL_DIR}/scripts/verify-linux.sh --all-tfms               # net8.0 too, before a PR
${CLAUDE_SKILL_DIR}/scripts/verify-linux.sh --api-baseline origin/main
${CLAUDE_SKILL_DIR}/scripts/verify-linux.sh --install-sdk            # when dotnet is missing
```

What it does that plain `dotnet test` cannot:

- **MAUI**: the real test project needs Windows and the workload. The script writes a throwaway harness that
  compiles the real `src/Barbatos.i18n.Maui` sources and the real MAUI test files against the plain `net10.0`
  assets of `Microsoft.Maui.Controls`, and runs them. All MAUI tests pass that way, because they stay at the
  converter/extension level by design.
- **WPF**: compiles the library and the test project with `-p:EnableWindowsTargeting=true`. The tests **cannot
  run** off Windows - say so when you report, and ask for a Windows run of the WPF suite before a WPF change is
  called verified.
- **Known failures**: it recognises the one environmental failure (below) and reports it as `KNOWN` rather than
  failing the run. Any other failure fails it.

The script takes about two minutes, keeps its logs in its work directory (printed at the start; pass
`--work-dir` to reuse a harness between runs), and exits non-zero on any real failure.

## 3. Check the public API

Every public member is a permanent commitment to NuGet consumers, and the compiler is happy with most breaking
changes - a removed overload, a changed default, a type moved to another namespace. Compare the build with the
branch you started from:

```bash
${CLAUDE_SKILL_DIR}/scripts/verify-linux.sh --api-baseline origin/main --api-only
```

It builds all seven packages at that ref and in the working tree and runs ApiCompat (installed into the work
directory, not globally) twice per package: once for breaking changes, which must be clean, and once in strict
mode, which lists additions. A refactoring should report **no breaking changes and 0 additions** for every
package. A feature may add members - check each listed one is intended, documented with XML docs, and worth
supporting forever. The same check runs on Windows: install `Microsoft.DotNet.ApiCompat.Tool` and pass
`--left-assembly`/`--right-assembly` for each package's `net10.0` (WPF: `net10.0-windows`) build.

## 4. Read the results honestly

Expected noise, which is not caused by your change:

- `NU1902` on every project: an advisory for `Microsoft.Build.Tasks.Git`, pulled in by SourceLink.
- `LocalizationCultureManagerTests.SetCulture_ShouldUseCustomCulture_WhenCustomBuilderIsProvided` fails on
  Linux only: it expects Windows' vi-VN short date `14/06/2026`, ICU formats `14/6/2026`.
- Test-project warnings that predate current work: `CS8625` in the DI tests, `CA1063` (4 lines) in the WPF tests.
  The libraries themselves build with **0** warnings; keep it that way.

Not noise:

- `LocalizationBuilderTests.FromResource_ShouldAllowLookupUsingTestResourcePropertyDirectly` failing is a real
  regression in RESX satellite loading. Never weaken its assertion.
- A test that fails only on `net8.0` is a real failure; the packages ship for net8.0 too.

When you report, state exactly what ran and what only compiled - for example "core, DI, formats and MAUI tests
ran and passed; WPF compiled, its tests were not run (Linux); public API unchanged against origin/main". Do not
call a change verified on the strength of a build.

## If you are refactoring

Behaviour-preserving changes need a net that predates them. Before restructuring code whose behaviour is only
implicitly covered, write characterization tests, run them on the unchanged code, and commit them first. The
parity tests (`OneOffPathParityTests`, `FallbackParityTests`) and `LookupPrecedenceTests` are examples of
the shape that works here: run two paths, or several localizers, over the same inputs and compare.
