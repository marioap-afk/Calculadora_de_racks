# I-52 CT-DA native external build

R3 (frozen V35): the canonical build is `eng/research/I52Ctda/build-r3.ps1 -SourceSha <committed sha>`. It refuses a
dirty tree or a HEAD that differs from the SHA, verifies the V35 freeze and the generated authority, rebuilds
`native/I52CtdaNative.vcxproj` and `payload/I52CtdaPayload.vcxproj` (Release|x64, v143, `PreferredToolArchitecture=x64`,
VC tools `14.44.35207`, ObjectARX `25.0.58.0`), builds the managed observer and publishes the harness, records dumpbin
headers/imports/exports and the linker read tlogs, computes the new build tuple (`harness tuple-v35`) and writes a new
versioned package with `SHA256SUMS` and `TRANSFER-METADATA.json` outside the repository. Existing packages are never
overwritten. The sections below describe the historical V34 single-helper build.

This package definition contains references and instructions only. It does not redistribute Autodesk or Microsoft files.

## Required build-machine tuple

- repository commit containing the R1B milestone;
- Windows x64;
- Visual Studio 2022 or compatible MSBuild with `PlatformToolset=v143`;
- MSVC v143 Hostx64/x64 compiler and linker;
- installed Windows 10/11 SDK;
- legitimate ObjectARX 2025 SDK `25.0.58.0` with `inc`, `inc-x64` and `lib-x64` beneath `ObjectArxSdkRoot`.

## Build

```powershell
msbuild eng/research/I52Ctda/native/I52CtdaNative.vcxproj `
  /m /p:Configuration=Release /p:Platform=x64 /p:PlatformToolset=v143 `
  /p:PreferredToolArchitecture=x64 `
  /p:ObjectArxSdkRoot=D:\CDROM1 /v:minimal
```

`PreferredToolArchitecture=x64` selects the `Hostx64\x64` compiler and linker; without it MSBuild may pick
`HostX86\x64`. Pass the SDK root explicitly; the project default is only a fallback.

The ObjectARX `inc` and `inc-x64` directories are external headers (`/external:W0`): SDK header warnings
such as C4201 in `AcString.h` are not RackCad findings, while `/W4 /WX` still governs the helper sources.
The fixture's `AcGeMatrix3d::translation` requires `acge25.lib` in addition to the four ObjectARX core libraries.

Expected output: `eng/research/I52Ctda/native/bin/x64/Release/I52CtdaNative.arx`.

Record MSBuild, `cl.exe`, `link.exe`, Windows SDK, ObjectARX headers, libraries, command line, output byte count and SHA-256. The actual linker command determines linked-library provenance.

## Split tuple

`BUILD_MACHINE_TOOLCHAIN_TUPLE` identifies the compiler machine. `HOST_EXECUTION_TUPLE` identifies the AutoCAD machine. They may differ. The exact native-helper SHA-256 bridges the tuples; a build-machine success supplies no CTDA host result.
