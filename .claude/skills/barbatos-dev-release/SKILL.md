---
name: barbatos-dev-release
description: Change the public API of the Barbatos.i18n NuGet packages safely and prepare a release - deciding whether a change is breaking, obsoleting instead of removing, versioning in Directory.Build.props, keeping README, API-REFERENCE and the consumer skills in step, and what the publishing workflow really does. Use this whenever a change adds, removes, renames or changes the signature or behaviour of a public type or member under src/, whenever asked to bump the version, write release notes, prepare or cut a release, publish to NuGet, or edit anything in .github/workflows of this repository.
---

# Public API and releases

Barbatos.i18n ships to NuGet.org. A published package can only be **unlisted, never deleted**, and anyone who
built against a member keeps calling it. So every public member is a permanent commitment, and a release is
irreversible.

## Changing public API

1. **Prefer not to.** Add an optional property or a new overload rather than changing an existing signature.
   Adding an optional parameter to an existing method is itself a binary break - callers compiled against the old
   signature fail at runtime - so add an overload instead.
2. **Never remove in place.** Keep the old member as an `[Obsolete("Use X instead.")]` forwarder for at least one
   minor release; remove it only in a major version, and only when the owner agrees.
3. **Behaviour is API too.** Which set wins, which culture decides a plural, what a missing key returns, the text
   of an exception a consumer might match - changing these breaks callers as surely as a signature does, and the
   compiler says nothing. Treat them the same way: deliberate, documented, in release notes.
4. **Mirror it.** A public addition to `Barbatos.i18n.Wpf` almost always needs its twin in `Barbatos.i18n.Maui`.
5. **Document it in the same change**: XML docs on every public member (the build has
   `GenerateDocumentationFile` on), `README.md`, `API-REFERENCE.md`, the `barbatos-i18n-*` consumer skills and, if
   an assertion encodes the old behaviour, `.claude/evals/barbatos-i18n-skills.json`. Find stale mentions with
   `grep -rn "MemberName" README.md API-REFERENCE.md .claude/skills .claude/evals samples`.
6. **Prove the surface is what you meant.** Run `barbatos-dev-verify` with `--api-baseline <last release or
   origin/main>`: no breaking changes unless intended, and every listed addition intended.

## Versioning

The version lives once, in `Directory.Build.props`: `<Version>` and `<AssemblyVersion>`, kept equal. Use
semantic versioning against the last published release: breaking change → major, new public API or new package →
minor, fixes only → patch. Bump it in its own commit with the release notes, so the bump is easy to review.

## What the publishing workflow does

`.github/workflows/barbatos-i18n-cd-nuget.yml` runs on `workflow_dispatch` **and on `release: published`** - so
publishing a GitHub Release pushes straight to NuGet.org. On `windows-latest` it writes the strong-name key from
the `STRONG_NAME_KEY` secret to `src/barbatos.snk`, logs in to NuGet with OIDC, removes `/samples/` and `/tests/`
from `Barbatos.i18n.slnx`, restores workloads, builds Release with `-p:SourceLinkEnabled=true` (which signs),
and pushes every `src/**/*.nupkg` and `*.snupkg` with `--skip-duplicate`.

**Its "Test" step runs no tests.** It calls `dotnet test --configuration Release --no-build` on the solution
*after* the tests were removed from it, so it finds no test project and succeeds. The comment above it says the
step stops a failing release; it does not. Until the workflow is fixed (that is the owner's call - point it at
the test projects, or run the tests before trimming the solution), the only gate before NuGet.org is a full local
run on Windows, including the WPF and MAUI suites.

`--skip-duplicate` means a version that is already on NuGet.org is silently not re-pushed: a fix needs a new
version number. `barbatos-i18n-republish-symbols.yml` (manual) rebuilds and re-uploads only the symbol packages,
for when NuGet.org rejected a `.snupkg`.

## What an agent does and does not do

- **Do**: prepare the change, the version bump, the release notes and a pull request; run the verification and
  report exactly what ran.
- **Do not**: create a GitHub Release, push a tag, or trigger the publishing workflow - each of those publishes
  irreversibly. Leave them to the repository owner, and say what is ready.
- **Ask first** before editing anything in `.github/workflows/`, which runs with the NuGet and signing
  credentials.
