# Code examples build harness

The example `.cs` files live under `modules/<module>/examples/` (Antora resolves
`include::example$…` from there). The projects here compile them — they are never run.

## Where to add a new `.cs` file

| Put the file in… | Compiled by | Pickup |
|---|---|---|
| `modules/hello-world/examples/` | `Examples.Core` | automatic (glob) |
| `modules/concept-docs/examples/` | `Examples.Core` | automatic (glob) |
| `modules/howtos/examples/` | `Examples.Core` | automatic (glob) |
| `modules/howtos/examples/logging/` | `Examples.Logging` | automatic (glob) |
| `modules/devguide/examples/dotnet/` | `Examples.Core` | **manual** — add a `<Compile Include>` line |
| `modules/howtos/examples/logging/windows/` | `WindowsOnly` | **manual** — add a `<Compile Include>` line |

The globs are non-recursive: a file only joins a project if it sits directly in that
folder. Subfolders and `devguide` are listed file-by-file.

Two files need a non-core dependency set, so they are routed explicitly:

- `Examples.Encryption` — `howtos/examples/EncryptingUsingSdk.cs` (excluded from `Core`).
- `WindowsOnly` — the full-.NET-Framework log4net sample (Windows-only, builds separately).

If a new file needs a package other than the core SDK, exclude it from `Examples.Core`
and add it to the project with the right dependencies.

## Versions

The SDK version is pinned once in `Directory.Build.props`
(keep it in sync with `antora.yml`'s `sdk_current_version`).

## What CI builds

`.github/workflows/compile-samples.yml`:

- `compile` — `dotnet build CouchbaseDocsExamples.sln` on Linux, Windows and macOS
  (`Examples.Core`, `Examples.Encryption`, `Examples.Logging`).
- `compile-windows-fullframework` — `WindowsOnly/WindowsOnly.sln` via MSBuild, Windows only.

Build locally: `dotnet build examples-build/CouchbaseDocsExamples.sln`
