# I-52 CT-DA research instrument

Research-only implementation of the V34 host fixture contract. Nothing in this directory is referenced by a
product project or by `RackCad.sln`.

## Components

- `native/`: ObjectARX 2025 x64/v143 helper loaded only into a dedicated scratch AutoCAD process.
- `harness/`: external .NET 8 controller, contract validator, tuple collector and process fence.
- `build.ps1`: builds both components without copying Autodesk SDK content into the repository.

The instrument must not run against a user's working AutoCAD process or project DWG. Runtime results are not
product evidence and cannot close a product KindContract.
