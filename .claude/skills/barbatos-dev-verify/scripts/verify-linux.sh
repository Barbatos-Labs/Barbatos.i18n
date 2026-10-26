#!/usr/bin/env bash
# Verifies a change to Barbatos.i18n on a machine without Windows.
#
# Runs the core, DependencyInjection and file-format test suites, compiles the WPF library and its tests with
# Windows targeting enabled (they cannot run), and runs the MAUI tests through a throwaway harness that compiles
# the real package and test sources against the plain net10.0 assets of Microsoft.Maui.Controls. Optionally
# compares every package's public API with a git ref through ApiCompat.
#
# Usage: verify-linux.sh [options] [suite...]
#   suites                 core di formats maui wpf (default: all of them)
#   --all-tfms             test every target framework, not just net10.0
#   --api-baseline REF     compare each package's public API with REF (for example origin/main)
#   --api-only             with --api-baseline, skip the test suites
#   --install-sdk          install the .NET 10 SDK and the 8.0/9.0 runtimes into ~/.dotnet when dotnet is missing
#   --work-dir DIR         reuse DIR for the harness, the baseline build and logs (default: a new temp directory)
#   -h, --help             show this help
#
# Exit status is 0 when every step passed or failed only in a known, environment-specific way.

set -uo pipefail

# Tests that fail on Linux for environmental reasons and pass on Windows. Keep this list short and explained.
#   ICU formats the vi-VN short date as 14/6/2026; the test expects Windows' 14/06/2026.
KNOWN_LINUX_FAILURES=(
  "Barbatos.i18n.UnitTests.LocalizationCultureManagerTests.SetCulture_ShouldUseCustomCulture_WhenCustomBuilderIsProvided"
)

usage() { sed -n '2,18p' "$0" | sed 's/^# \{0,1\}//'; }

SUITES=()
ALL_TFMS=false
API_BASELINE=""
INSTALL_SDK=false
API_ONLY=false
WORK=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --all-tfms) ALL_TFMS=true ;;
    --api-baseline) API_BASELINE="${2:?--api-baseline needs a git ref}"; shift ;;
    --api-only) API_ONLY=true ;;
    --install-sdk) INSTALL_SDK=true ;;
    --work-dir) WORK="${2:?--work-dir needs a directory}"; shift ;;
    -h|--help) usage; exit 0 ;;
    core|di|formats|maui|wpf) SUITES+=("$1") ;;
    *) echo "Unknown argument: $1" >&2; usage >&2; exit 2 ;;
  esac
  shift
done
[[ ${#SUITES[@]} -eq 0 ]] && SUITES=(core di formats maui wpf)
if $API_ONLY; then
  [[ -n "$API_BASELINE" ]] || { echo "--api-only needs --api-baseline REF." >&2; exit 2; }
  SUITES=()
fi

REPO="$(git -C "$(dirname "$0")" rev-parse --show-toplevel)" || { echo "Not inside the repository." >&2; exit 2; }
WORK="${WORK:-$(mktemp -d "${TMPDIR:-/tmp}/barbatos-verify.XXXXXX")}"
mkdir -p "$WORK/logs"

export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

# --- Locate or install the SDK -------------------------------------------------------------------------------
if ! command -v dotnet >/dev/null 2>&1; then
  if [[ -x "$HOME/.dotnet/dotnet" ]]; then
    export PATH="$HOME/.dotnet:$PATH"
  elif $INSTALL_SDK; then
    installer="$WORK/dotnet-install.sh"
    curl -sSL https://dot.net/v1/dotnet-install.sh -o "$installer" && chmod +x "$installer" || exit 2
    "$installer" --channel 10.0 --install-dir "$HOME/.dotnet" >"$WORK/logs/install.log" 2>&1 &&
      "$installer" --channel 9.0 --runtime dotnet --install-dir "$HOME/.dotnet" >>"$WORK/logs/install.log" 2>&1 &&
      "$installer" --channel 8.0 --runtime dotnet --install-dir "$HOME/.dotnet" >>"$WORK/logs/install.log" 2>&1 ||
      { echo "SDK install failed; see $WORK/logs/install.log" >&2; exit 2; }
    export PATH="$HOME/.dotnet:$PATH"
  else
    echo "dotnet is not installed. Re-run with --install-sdk, or install the .NET 10 SDK plus the 8.0 and 9.0" >&2
    echo "runtimes (https://dot.net/v1/dotnet-install.sh)." >&2
    exit 2
  fi
fi
DOTNET_ROOT="$(dirname "$(readlink -f "$(command -v dotnet)")")"
export DOTNET_ROOT

TEST_TFM_ARGS=(-f net10.0)
$ALL_TFMS && TEST_TFM_ARGS=()

RESULTS=()
FAILED=false
record() {
  RESULTS+=("$(printf '%-34s %s' "$1" "$2")")
  if [[ "$2" == FAIL* ]]; then FAILED=true; fi
}

has_suite() { local s; for s in "${SUITES[@]}"; do [[ "$s" == "$1" ]] && return 0; done; return 1; }

is_known_failure() { local k; for k in "${KNOWN_LINUX_FAILURES[@]}"; do [[ "$k" == "$1" ]] && return 0; done; return 1; }

# Runs one test project and classifies the outcome: PASS, KNOWN (only known environmental failures) or FAIL.
run_tests() {
  local label="$1" project="$2" log="$WORK/logs/$1.log"
  echo "== $label"
  dotnet test "$project" "${@:3}" >"$log" 2>&1
  local rc=$?
  grep -E '^(Passed|Failed)!' "$log" | sed 's/^/   /'
  if [[ $rc -eq 0 ]]; then record "$label" "PASS"; return; fi

  local failed unknown=() name
  failed="$(grep -oE '^[[:space:]]+Failed [^ ]+' "$log" | awk '{print $2}' | sort -u)"
  for name in $failed; do is_known_failure "$name" || unknown+=("$name"); done

  if [[ -n "$failed" && ${#unknown[@]} -eq 0 ]]; then
    record "$label" "KNOWN (environmental failure only)"
  elif [[ -z "$failed" ]]; then
    record "$label" "FAIL (build error - see $log)"
    grep -E 'error [A-Z]+[0-9]+' "$log" | sort -u | head -10 | sed 's/^/   /'
  else
    record "$label" "FAIL (${unknown[*]})"
  fi
}

run_build() {
  local label="$1" project="$2" log="$WORK/logs/$1.log"
  echo "== $label"
  if dotnet build "$project" "${@:3}" >"$log" 2>&1; then
    local warnings
    warnings="$(grep -E 'warning [A-Z]+[0-9]+' "$log" | grep -v NU1902 | sort -u | wc -l)"
    record "$label" "PASS ($warnings warnings, NU1902 excluded)"
  else
    record "$label" "FAIL (see $log)"
    grep -E 'error [A-Z]+[0-9]+' "$log" | sort -u | head -10 | sed 's/^/   /'
  fi
}

package_version() {
  sed -n "s/.*PackageVersion Include=\"$1\" Version=\"\([^\"]*\)\".*/\1/p" "$REPO/Directory.Packages.props" | head -1
}

# Writes a harness that compiles the MAUI package sources of SOURCE_ROOT for plain net10.0, plus a test project
# that compiles the MAUI tests of the current checkout against it.
write_maui_harness() {
  local source_root="$1" dir="$2"
  mkdir -p "$dir/lib" "$dir/tests"
  cat >"$dir/Directory.Build.props" <<EOF
<Project>
  <PropertyGroup>
    <LangVersion>14.0</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <NoWarn>\$(NoWarn);NU1902;NU1903</NoWarn>
  </PropertyGroup>
</Project>
EOF
  cat >"$dir/Directory.Packages.props" <<EOF
<Project>
  <PropertyGroup><ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally></PropertyGroup>
</Project>
EOF
  cat >"$dir/lib/Barbatos.i18n.Maui.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <!-- Generated by verify-linux.sh: the real MAUI package sources, compiled against plain net10.0 assets. -->
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyName>Barbatos.i18n.Maui</AssemblyName>
    <RootNamespace>Barbatos.i18n.Maui</RootNamespace>
    <IsTrimmable>true</IsTrimmable>
    <EnableTrimAnalyzer>true</EnableTrimAnalyzer>
    <EnableAotAnalyzer>true</EnableAotAnalyzer>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="$source_root/src/Barbatos.i18n.Maui/**/*.cs" Exclude="$source_root/src/Barbatos.i18n.Maui/obj/**;$source_root/src/Barbatos.i18n.Maui/bin/**" />
    <PackageReference Include="Microsoft.Maui.Controls" Version="${MAUI_VERSION:-10.0.*}" />
    <ProjectReference Include="$source_root/src/Barbatos.i18n/Barbatos.i18n.csproj" />
  </ItemGroup>
</Project>
EOF
  cat >"$dir/tests/MauiHarness.Tests.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsTestProject>true</IsTestProject>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="$REPO/tests/Barbatos.i18n.Maui.UnitTests/**/*.cs" Exclude="$REPO/tests/Barbatos.i18n.Maui.UnitTests/obj/**;$REPO/tests/Barbatos.i18n.Maui.UnitTests/bin/**" />
    <Using Include="Xunit" />
    <PackageReference Include="AwesomeAssertions" Version="$(package_version AwesomeAssertions)" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="$(package_version Microsoft.NET.Test.Sdk)" />
    <PackageReference Include="xunit" Version="$(package_version xunit)" />
    <PackageReference Include="xunit.runner.visualstudio" Version="$(package_version xunit.runner.visualstudio)" />
    <PackageReference Include="Microsoft.Maui.Controls" Version="${MAUI_VERSION:-10.0.*}" />
    <ProjectReference Include="../lib/Barbatos.i18n.Maui.csproj" />
  </ItemGroup>
</Project>
EOF
}

echo "Repository: $REPO"
echo "Work dir:   $WORK"
echo "SDK:        $(dotnet --version)"

# --- Test suites --------------------------------------------------------------------------------------------
has_suite core && run_tests "core tests" "$REPO/tests/Barbatos.i18n.UnitTests/Barbatos.i18n.UnitTests.csproj" "${TEST_TFM_ARGS[@]}"
has_suite di && run_tests "di tests" "$REPO/tests/Barbatos.i18n.DependencyInjection.UnitTests/Barbatos.i18n.DependencyInjection.UnitTests.csproj" "${TEST_TFM_ARGS[@]}"
if has_suite formats; then
  for format in Json Ini Csv; do
    run_tests "$(tr '[:upper:]' '[:lower:]' <<<"$format") tests" "$REPO/tests/Barbatos.i18n.$format.UnitTests/Barbatos.i18n.$format.UnitTests.csproj" "${TEST_TFM_ARGS[@]}"
  done
fi
if has_suite maui; then
  write_maui_harness "$REPO" "$WORK/maui-harness"
  run_tests "maui tests (net10.0 harness)" "$WORK/maui-harness/tests/MauiHarness.Tests.csproj"
fi
if has_suite wpf; then
  WPF_TFM_ARGS=(-f net10.0-windows)
  $ALL_TFMS && WPF_TFM_ARGS=()
  run_build "wpf library (compile only)" "$REPO/src/Barbatos.i18n.Wpf/Barbatos.i18n.Wpf.csproj" -p:EnableWindowsTargeting=true "${WPF_TFM_ARGS[@]}"
  run_build "wpf tests (compile only)" "$REPO/tests/Barbatos.i18n.Wpf.UnitTests/Barbatos.i18n.Wpf.UnitTests.csproj" -p:EnableWindowsTargeting=true "${WPF_TFM_ARGS[@]}"
fi

# --- Public API ---------------------------------------------------------------------------------------------
# Builds every package at REF and at the working tree, then runs ApiCompat twice per package: once for breaking
# changes (must be clean) and once in strict mode, which also lists additions so they are made on purpose.
if [[ -n "$API_BASELINE" ]]; then
  echo "== public API vs $API_BASELINE"
  baseline="$WORK/baseline"
  git -C "$REPO" worktree remove --force "$baseline" >/dev/null 2>&1
  if ! git -C "$REPO" worktree add --detach "$baseline" "$API_BASELINE" >"$WORK/logs/worktree.log" 2>&1; then
    record "public API" "FAIL (cannot check out $API_BASELINE)"
  else
    trap 'git -C "$REPO" worktree remove --force "$baseline" >/dev/null 2>&1' EXIT
    apicompat="$WORK/tools/apicompat"
    [[ -x "$apicompat" ]] || dotnet tool install Microsoft.DotNet.ApiCompat.Tool --tool-path "$WORK/tools" >"$WORK/logs/apicompat-install.log" 2>&1
    write_maui_harness "$baseline" "$WORK/maui-harness-baseline"
    [[ -f "$WORK/maui-harness/lib/Barbatos.i18n.Maui.csproj" ]] || write_maui_harness "$REPO" "$WORK/maui-harness"

    api_ok=true
    for package in Barbatos.i18n Barbatos.i18n.DependencyInjection Barbatos.i18n.Json Barbatos.i18n.Ini Barbatos.i18n.Csv Barbatos.i18n.Wpf Barbatos.i18n.Maui; do
      case "$package" in
        Barbatos.i18n.Wpf) tfm=net10.0-windows ;;
        *) tfm=net10.0 ;;
      esac
      if [[ "$package" == Barbatos.i18n.Maui ]]; then
        left_project="$WORK/maui-harness-baseline/lib/Barbatos.i18n.Maui.csproj"
        right_project="$WORK/maui-harness/lib/Barbatos.i18n.Maui.csproj"
      else
        left_project="$baseline/src/$package/$package.csproj"
        right_project="$REPO/src/$package/$package.csproj"
      fi
      if [[ ! -f "$left_project" ]]; then
        echo "   $package: not present at $API_BASELINE (new package) - skipped"
        continue
      fi
      for side in left right; do
        project_var="${side}_project"
        dotnet build "${!project_var}" -f "$tfm" -p:EnableWindowsTargeting=true \
          >"$WORK/logs/api-$side-$package.log" 2>&1 || { api_ok=false; echo "   $package: $side build failed"; continue 2; }
      done
      left_dll="$(dirname "$left_project")/bin/Debug/$tfm/$package.dll"
      right_dll="$(dirname "$right_project")/bin/Debug/$tfm/$package.dll"

      if "$apicompat" --left-assembly "$left_dll" --right-assembly "$right_dll" >"$WORK/logs/api-$package.log" 2>&1; then
        additions="$("$apicompat" --left-assembly "$left_dll" --right-assembly "$right_dll" --strict-mode 2>&1 | grep -cE '^CP[0-9]+')"
        echo "   $package: no breaking changes, $additions addition(s)"
        [[ "$additions" -gt 0 ]] && "$apicompat" --left-assembly "$left_dll" --right-assembly "$right_dll" --strict-mode 2>&1 | grep -E '^CP[0-9]+' | sed 's/^/      /'
      else
        api_ok=false
        echo "   $package: BREAKING"
        grep -E '^CP[0-9]+' "$WORK/logs/api-$package.log" | sed 's/^/      /'
      fi
    done
    if $api_ok; then
      record "public API vs $API_BASELINE" "PASS"
    else
      record "public API vs $API_BASELINE" "FAIL (see above)"
    fi
  fi
fi

# --- Summary ------------------------------------------------------------------------------------------------
echo
echo "Summary (logs in $WORK/logs):"
printf '  %s\n' "${RESULTS[@]}"
if has_suite wpf; then
  echo "  Note: the WPF tests were compiled, not run. Run them on Windows before calling a WPF change verified:"
  echo "        dotnet test tests/Barbatos.i18n.Wpf.UnitTests/Barbatos.i18n.Wpf.UnitTests.csproj -f net10.0-windows"
fi
$FAILED && exit 1
exit 0
