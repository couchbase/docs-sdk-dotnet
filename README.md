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

They are **compiled** (not run) to verify they build against the Couchbase .NET SDK.
`examples-build/CouchbaseDocsExamples.sln` holds three small projects, grouped by the
package an example depends on:

- `Examples.Core` — everything in the main `CouchbaseNetClient` package (KV, query,
  search, range scan, sub-document, transactions, …).
- `Examples.Encryption` — examples needing the separate `Couchbase.Extensions.Encryption`
  package.
- `Examples.Logging` — the cross-platform logging examples under
  `modules/howtos/examples/logging/`.

The Windows-only full-.NET-Framework log4net sample lives in
`modules/howtos/examples/logging/windows/` and builds separately via
`WindowsOnly/WindowsOnly.sln`. The SDK version is pinned once in
`examples-build/Directory.Build.props`.

`.github/workflows/compile-samples.yml` builds these on Linux, Windows and macOS for every
pull request. To build locally: `dotnet build examples-build/CouchbaseDocsExamples.sln`.
