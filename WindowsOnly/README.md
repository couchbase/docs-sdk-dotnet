# Windows-only examples

This solution singles out documentation examples that can only be compiled on
Windows, so the cross-platform compile job (`examples-build/CouchbaseDocsExamples.sln`)
stays green on Linux and macOS.

Currently it contains one project:

- **Couchbase.Examples.Logging.FullFramework** — targets full .NET Framework (`v4.7.2`)
  and demonstrates log4net integration. The project stays in place under
  `modules/howtos/examples/` so its Antora `example$` includes keep resolving; this
  solution just references it from here.

It is built by the `compile-windows-fullframework` job in
`.github/workflows/compile-samples.yml` (MSBuild + `nuget restore`).
