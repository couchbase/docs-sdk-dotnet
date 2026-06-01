# Refactor Plan — .NET SDK Docs Code Examples

**Goal:** make the code examples clean and simple. Every example lives in a plain
`.cs` file inside a real `.csproj`, and a GitHub Actions workflow **compiles** them on
**Windows, Linux, and macOS** to guarantee they at least build against the pinned
Couchbase .NET SDK version. Runtime/live-server testing is dropped.

> Constraint honoured throughout: **don't change sample code** except to (a) rename
> for clarity, or (b) fix code that is plainly wrong / won't compile. The only
> blanket structural edit is adding a one-line file-scoped `namespace` (see §3),
> placed outside every tag region so documentation includes are unaffected.

**Target framework: `net10.0`.**

---

## 1. Current state (why it's messy)

| Area | Problem |
|------|---------|
| **Three example formats** | `.csx` dotnet-script files, loose `.cs` + a `DevGuide.csproj`, and several full standalone projects. No single way to build them. |
| **Version drift** | `#r "nuget: CouchbaseNetClient, …"` is pinned per-file at `3.2.0`, `3.4.8` (×18) and `3.9.0`. The "current" version in `antora.yml` is `3.9.0`. |
| **Tests need a live cluster** | bats + `dotnet script` run against a docker `couchbase/server-sandbox:7.1.1`, gated by `wait-for-couchbase.sh`. Heavy, slow, Linux-only, flaky (3 retries). It validates *runtime*, not *compilation*. |
| **dotnet-script dependency** | `.csx` can't be compiled with `dotnet build`; it needs the global `dotnet-script` tool. |
| **Orphaned code** | Most of `modules/devguide/examples/dotnet/*.cs` (~30 files, plus `DevGuide.sln`) is not referenced by any page. Only `Analytics.cs` and `Cloud/Cloud.cs` are used. |
| **Mixed target frameworks** | `netcoreapp3.1`, `net6`, and full-framework `v4.7.2` (log4net example — can't build on Linux/macOS). |
| **Class-name collisions** | Even within one module, names repeat: devguide declares `Program` in 4 files (+ a `Program2`, + a typo `Progam`); howtos snippets declare `HomeController` ×2 and `Startup` ×2. These would clash in any shared project. |
| **Inconsistent tags** | 170 `// tag::` vs 64 non-standard `// #tag::`. Both happen to work in Antora but it's untidy. |
| **Stray-language includes** | `managing-connections.adoc` references `ManagingConnections.java` (commented), and a page references `howtos:example$search.js`. |
| **`.cs`/`.csx` duplicate** | `hello-world/examples/StartUsing.cs` and `StartUsing.csx` both define `StartUsing`. |

---

## 2. Target architecture

```
modules/<module>/examples/…              ← example .cs files STAY in place (Antora needs this)
examples-build/                          ← NEW: build harness, references the files above
  Directory.Build.props                  ← net10.0 + single pinned SDK version for everything
  Examples.Core.csproj                   ← core-SDK snippets (most files)
  Examples.Encryption.csproj             ← snippets needing Couchbase.Extensions.Encryption
  Examples.Transactions.csproj           ← snippets needing transactions
  CouchbaseDocsExamples.sln              ← Core + Encryption + Transactions + standalone apps
WindowsOnly/
  Logging.FullFramework/                 ← full .NET Framework log4net example (moved here)
  WindowsOnly.sln
```

Standalone console apps stay where they are (retargeted to `net10.0`) and are added to
`CouchbaseDocsExamples.sln`:
`Couchbase.Examples.KV/*`, `Couchbase.Examples.SearchV2/*`,
`Couchbase.Examples.Logging.GenericHost`, `Couchbase.Examples.Logging.NoHost`.

**Why this shape**

1. **Files stay in `modules/<m>/examples/`.** Antora resolves `example$Foo` to that path,
   so leaving files put keeps all ~92 pages working with only an extension swap in the
   include directives (§7). The csproj files live in `examples-build/` and pull the
   sources in via globbed `<Compile Include="../modules/**/examples/**/*.cs" />`.

2. **Projects grouped by dependency set, not by module** (your choice). Three snippet
   projects cover every loose example:

   | Project | Extra dependency | Files |
   |---------|------------------|-------|
   | `Examples.Encryption` | `Couchbase.Extensions.Encryption` | `devguide/…/FieldEncryptionAes.cs`, `FieldEncryptionRsa.cs`, `howtos/…/EncryptingUsingSdk` |
   | `Examples.Transactions` | transactions package/API | `howtos/…/TransactionsExample.cs` |
   | `Examples.Core` | core SDK only (+ `MessagePack` for the Transcoding sample, + `FrameworkReference Microsoft.AspNetCore.App` for the DI/web snippets) | everything else (all hello-world, concept-docs, and the remaining howtos + devguide snippets) |

   Each project globs only its own file list (explicit `<Compile Include>` items, not a
   blanket glob, so a file lands in exactly one project).

3. **Standalone apps kept as-is.** They already isolate their own `Program`/`Main` and
   their own dependencies (Hosting, etc.), so leaving them separate means we don't touch
   their entry points and there are no cross-project collisions. Just retarget to net10.

4. **Version pinned once** in `examples-build/Directory.Build.props`:
   ```xml
   <Project>
     <PropertyGroup>
       <TargetFramework>net10.0</TargetFramework>
       <Nullable>disable</Nullable>
       <LangVersion>latest</LangVersion>
       <!-- single source of truth; keep in sync with antora.yml sdk_current_version -->
       <CouchbaseSdkVersion>3.9.0</CouchbaseSdkVersion>
     </PropertyGroup>
     <ItemGroup>
       <PackageReference Include="CouchbaseNetClient" Version="$(CouchbaseSdkVersion)" />
     </ItemGroup>
   </Project>
   ```
   This deletes every per-file `#r "nuget:"` line and fixes the 3.2.0/3.4.8/3.9.0 drift.
   `Examples.Encryption` / `Examples.Transactions` add their one extra `PackageReference`.

---

## 3. Migration mechanics: `.csx` → `.cs`

Per `.csx` file, a deterministic transform:

1. **Delete** the dotnet-script header comment block and the `#r "nuget: …"` line(s).
2. **Delete** the top-level invocation line (e.g. `await new Auth().ExecuteAsync();`).
   We only need compilation; dropping it removes top-level-statement entry points so
   all files coexist in one project.
3. **Add a file-scoped namespace** at the very top (outside any tag region), unique per
   file, to neutralise the class-name collisions from §1:
   ```csharp
   namespace Couchbase.Docs.Examples.Howtos.Auth;   // derived from module + file name
   ```
   The class bodies — the actual documented code and all `// tag::` regions — are kept
   **verbatim**.
4. **Rename** `.csx` → `.cs`.
5. For the handful of files that are bare top-level statements with no class, wrap the
   body in `static class <FileName>Example { … }` so it type-checks.

**Normalize tags** at the same time: `// #tag::name[]` → `// tag::name[]` and
`// #end::name[]` → `// end::name[]`. The tag *name* is unchanged, so adoc `tag=name`
references still resolve — pure cosmetic consistency.

**Resolve the `StartUsing.cs` / `StartUsing.csx` duplicate**: keep one (prefer the
newer top-level `.cs`), delete the other, and ensure the doc include points at the
survivor. Fix the `Progam` typo (plainly wrong → rename to `Program`-with-namespace).

---

## 4. Platform-specific code → `WindowsOnly/`

Only one example is platform-specific today: `Couchbase.Examples.Logging.FullFramework`
(full .NET Framework `v4.7.2`, log4net). It can't build on Linux/macOS.

- **Move** it to `WindowsOnly/Logging.FullFramework/` with its own `WindowsOnly.sln`.
- Keep it on .NET Framework (preserves the sample exactly as documented).
- It is built by a dedicated **Windows-only** CI job (§5); the cross-platform matrix
  never touches it.
- Update its `include::example$…` paths in `collecting-information-and-logging.adoc`
  to the new location.

This keeps a clean rule going forward: *anything platform-specific lives under
`WindowsOnly/` and is excluded from the cross-platform solution.*

---

## 5. New GitHub Actions workflow

Replace `.github/workflows/test-samples.yml` with two compile-only jobs:

```yaml
name: Compile .NET Code Samples
on:
  pull_request:
    branches: ["release/3.*"]
  push:
    branches: ["release/3.*"]
jobs:
  compile:
    strategy:
      fail-fast: false
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
    runs-on: ${{ matrix.os }}
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore examples-build/CouchbaseDocsExamples.sln
      - run: dotnet build examples-build/CouchbaseDocsExamples.sln --no-restore -c Release

  compile-windows-fullframework:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v4
      - uses: microsoft/setup-msbuild@v2
      - run: msbuild WindowsOnly/WindowsOnly.sln /t:Restore,Build /p:Configuration=Release
```

No docker, no live cluster, no bats — fast and deterministic.

---

## 6. Deletions (the big simplification)

Once compile-only CI is green:

- `docker-compose.yml`
- `Dockerfile`, `local-tests.Dockerfile`
- `tests/` (bats, `test.bats`, `test_helper.bash`, `wait-for-couchbase.sh`,
  `travel-sample-index.json`, `package.json`, `node_modules/`)
- old `.github/workflows/test-samples.yml`
- per-file `#r "nuget:"` directives and dotnet-script headers (folded into props)
- `DevGuide.sln`, `DevGuide.csproj`, and redundant nested csproj (`Cloud/Cloud.csproj`)

**Tradeoff (call out to stakeholders):** we lose automated *runtime* validation against a
real server. The user-stated goal is "at least COMPILES," so this is intended; if runtime
smoke-tests are wanted later they can be reintroduced as an optional, separate job.

---

## 7. Antora include compatibility — must-not-break checklist

- [ ] Every `.csx` referenced in adoc gets a matching `.cs` (same basename, same tags).
- [ ] Bulk-rewrite adoc includes: `example$Foo.csx` → `example$Foo.cs`
      (≈124 occurrences; scripted, then visually diffed).
- [ ] Tag names unchanged after `#tag::`→`tag::` normalization and after adding namespaces.
- [ ] Example files stay in their module's `examples/` dir (only `Logging.FullFramework`
      moves — its 4 includes are repointed).
- [ ] Render the site locally (Antora preview) and confirm no
      "tag not found" / "include target not found" warnings.
- [ ] Flag the stray `howtos:example$search.js` include to the docs owner (likely a
      copy-paste error; no .NET code change).

---

## 8. Risks & open decisions

1. **Examples that genuinely don't compile today** — converting `.csx`→`.cs` surfaces real
   type errors that dotnet-script tolerated. Fixing these is in-scope ("plainly wrong"),
   but each fix should be reviewed so we don't change documented behaviour. *(Surfaces in
   Phase 1.)*
2. ~~**Package availability on net10**~~ — **RESOLVED in Phase 0.** All three package sets
   restore and build on `net10.0`:
   `CouchbaseNetClient 3.9.0`, `Couchbase.Extensions.Encryption 2.0.0`,
   `Couchbase.Transactions 3.9.0`. The transactions project must pin
   `Microsoft.Extensions.DependencyInjection`/`Logging` to **10.0.1** (the versions
   CouchbaseNetClient 3.9.0 pulls) or restore fails with NU1605.
3. ~~**Transactions API location**~~ — **RESOLVED.** `TransactionsExample.cs` uses
   namespace `Couchbase.Client.Transactions`, provided by the **`Couchbase.Transactions`**
   package (v3.9.0).
4. **Encryption examples use two incompatible APIs** — `howtos/EncryptingUsingSdk` uses the
   **2.0** API (`Couchbase.Encryption.*`), while `devguide/FieldEncryptionAes.cs` /
   `FieldEncryptionRsa.cs` use a **legacy 2.x-SDK** API (`Couchbase.Configuration.Client`,
   `Couchbase.Extensions.Encryption.Providers`). The latter two are **referenced by no
   page** → excluded from the build (candidates for deletion in Phase 4). `Examples.Encryption`
   therefore pins `Couchbase.Extensions.Encryption 2.0.0` and builds only `EncryptingUsingSdk`.
5. **Version sync** — `Directory.Build.props` `CouchbaseSdkVersion` should track
   `antora.yml`'s `sdk_current_version`. Optional: a tiny CI check asserting they match.

### Phases 2–4 status — DONE
- **Phase 2:** all 124 `example$*.csx` doc includes rewritten to `.cs`; tag references
  validated (193 checked). 4 pre-existing broken includes documented below.
- **Phase 3:** the 5 cross-platform sample apps retargeted to net10 + switched from a local
  SDK source `ProjectReference` to the `CouchbaseNetClient` package, added to the solution
  (search sample's vector-query API drift fixed). `WindowsOnly/WindowsOnly.sln` isolates the
  full-framework log4net sample (kept in place so its includes still resolve).
  `.github/workflows/compile-samples.yml` added (Linux/Windows/macOS + Windows-only job).
- **Phase 4:** docker/bats live-cluster harness, old workflow, and dead build files
  (`DevGuide`/`Cloud` projects, legacy 2.x `FieldEncryption*`) deleted. README updated.

**Whole-solution build: green on net10, 0 errors.**

#### Remaining follow-ups (need a human/editorial decision)
- 4 **pre-existing** broken doc includes (tags/file that never existed): `encrypting_using_sdk_6`,
  `encrypting_using_sdk_7`, `config_warn`, and `concept-docs:example$TransactionsExample.cs`.
- `modules/hello-world/pages/platform-help.adoc` still describes the removed `dotnet script`
  flow — needs an editorial rewrite.
- `antora.yml` `sdk_current_version` is still `3.9.0` while the build pins `3.9.2`.
- NU1902/NU1904 vulnerability advisories on some transitive packages.
- ~20 orphaned (unreferenced) devguide `.cs` examples remain on disk — optional deletion.

### Phase 1 status — DONE (all 24 referenced examples compile on net10, 0 errors)
- 24 referenced `.csx`/`.cs` examples converted to plain `.cs`: script headers + `#r` stripped,
  per-file namespaces (`Couchbase.Docs.Examples.<Module>.<File>`), tags normalized.
- Tricky cases: `SubDocument`/`StartUsing` wrapped in classes; `N1qlQueries` driver block
  dropped; the 5 interdependent DI files restructured (broken `HomeControllerDI` fixed; shared
  `IMyBucketProvider` at root namespace, variants in child namespaces); `Cloud` typo + orphan tag.
- `Directory.Build.props` enables **implicit usings** (mirrors dotnet-script) — cleared the
  whole `List`/`IEnumerable`/`Task` missing-using error class.
- **SDK bumped to 3.9.2** (per request). Package pins: CouchbaseNetClient 3.9.2,
  Couchbase.Extensions.DependencyInjection 3.9.2, Couchbase.Transactions 3.9.0 (its latest),
  Couchbase.Extensions.Encryption 2.0.0-dp.1 (the preview the example targets), MessagePack 3.1.6.
- **Genuine pre-existing doc bugs fixed** (these change published snippets — review):
  - `Auth`: added `using …Authentication.Authenticators;`; 2 snippets now cast
    `((IClusterAuthenticator)cluster).Authenticator(…)` (the method is on the concrete type, not `ICluster`).
  - `Transcoding`: `Compression.None` fully-qualified; two `null` args cast to `(JsonSerializerOptions)null` (net10 STJ overload ambiguity).
  - `EncryptingUsingSdk`: NO snippet change (fixed by pinning the preview package).
  - `TransactionsExample`: `using Couchbase;` added to imports; `Main` now connects a cluster;
    init scope/collection split into two awaited calls (was a pre-existing bug); `LogOnFailure`
    given a cluster/transactions setup; custom-metadata local `cluster`→`metadataCluster`;
    `SingleQueryTransactionConfigBuilder.ExpirationTime(…)` → `.Timeout(…)` (correct 3.9 API).
- **Open follow-ups:** (a) `antora.yml` `sdk_current_version` is still 3.9.0 — decide whether to
  bump to 3.9.2 to match the build; (b) NU1902/NU1904 vulnerability advisories on some packages;
  (c) `FieldEncryptionAes/Rsa` + other orphaned devguide files still on disk (Phase 4 deletion).

### Phase 0 status — DONE
- `examples-build/` created: `Directory.Build.props` (net10.0 + pinned SDK) + the three
  project files + `CouchbaseDocsExamples.sln`.
- `dotnet restore` + `dotnet build` succeed on net10 (only `CS2008 No source files`,
  expected until Phase 1 adds the converted `.cs` files).
- Build artifacts already covered by `.gitignore` (`[Bb]in/`, `[Oo]bj/`).
- **Deferred to Phase 3:** retargeting the standalone apps (currently `net8.0` /
  `netcoreapp3.1` with old pinned package versions) and adding them to the sln — mechanical,
  done alongside the WindowsOnly split.

---

## 9. Phased execution checklist

**Phase 0 — Scaffold (no behaviour change)**
- [ ] Add `examples-build/Directory.Build.props` (net10.0 + pinned SDK version).
- [ ] Add `Examples.Core` / `Examples.Encryption` / `Examples.Transactions` csproj with
      explicit `<Compile Include>` lists; add `CouchbaseDocsExamples.sln`.
- [ ] Retarget the kept standalone apps to net10 and add them to the sln.
- [ ] Verify all packages restore on net10 (Risk §8.2).

**Phase 1 — Convert examples**
- [ ] Transform each `.csx` → `.cs` per §3 (strip `#r`/header/invocation, add per-file
      namespace, wrap bare bodies).
- [ ] Normalize `#tag::` → `tag::`; dedupe `StartUsing`; fix `Progam` typo.
- [ ] `dotnet build` locally on macOS; fix genuine compile errors (reviewed individually).

**Phase 2 — Repoint docs**
- [ ] Rewrite adoc include extensions `.csx` → `.cs`.
- [ ] Repoint the 4 `Logging.FullFramework` includes to `WindowsOnly/`.
- [ ] Build Antora preview; confirm zero include/tag warnings.

**Phase 3 — Windows-only split + new CI**
- [ ] Move `Logging.FullFramework` to `WindowsOnly/`, add `WindowsOnly.sln`.
- [ ] Add the two-job compile workflow (§5).
- [ ] Green on ubuntu/windows/macos + the windows-only full-framework job.

**Phase 4 — Cleanup**
- [ ] Delete docker/bats/tests infra, old workflow, and orphaned `DevGuide.sln`/csproj (§6).
- [ ] Update `README.md` to describe the new compile-only flow.

Each phase is independently reviewable and the docs keep rendering throughout.
```
