Shield: [![CC BY-NC-SA 4.0][cc-by-nc-sa-shield]][cc-by-nc-sa]

This work is licensed under a
[Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License][cc-by-nc-sa].

[![CC BY-NC-SA 4.0][cc-by-nc-sa-image]][cc-by-nc-sa]

[cc-by-nc-sa]: http://creativecommons.org/licenses/by-nc-sa/4.0/
[cc-by-nc-sa-image]: https://licensebuttons.net/l/by-nc-sa/4.0/88x31.png
[cc-by-nc-sa-shield]: https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-lightgrey.svg

This repository hosts the documentation source for the Couchbase .NET SDK.

## Code examples

Documentation code examples live under `modules/<module>/examples/` as plain `.cs`
files and are pulled into the pages via Antora `include::example$…` tag regions.

They are **compiled** (not run) to verify they build against the Couchbase .NET SDK:

- `examples-build/CouchbaseDocsExamples.sln` — the cross-platform examples, grouped into
  projects by dependency (`Examples.Core`, `Examples.Encryption`, `Examples.Transactions`)
  plus the standalone sample apps.
- `WindowsOnly/WindowsOnly.sln` — the full-.NET-Framework log4net sample (Windows only).
- The SDK version is pinned once in `examples-build/Directory.Build.props`.

`.github/workflows/compile-samples.yml` builds these on Linux, Windows and macOS for every
pull request. To build locally: `dotnet build examples-build/CouchbaseDocsExamples.sln`.
