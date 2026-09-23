# I-52 CT-DA native external build

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
  /m /p:Configuration=Release /p:Platform=x64 `
  /p:ObjectArxSdkRoot=D:\Downloads\CDROM1 /v:minimal
```

Expected output: `eng/research/I52Ctda/native/bin/x64/Release/I52CtdaNative.arx`.

Record MSBuild, `cl.exe`, `link.exe`, Windows SDK, ObjectARX headers, libraries, command line, output byte count and SHA-256. The actual linker command determines linked-library provenance.

## Split tuple

`BUILD_MACHINE_TOOLCHAIN_TUPLE` identifies the compiler machine. `HOST_EXECUTION_TUPLE` identifies the AutoCAD machine. They may differ. The exact native-helper SHA-256 bridges the tuples; a build-machine success supplies no CTDA host result.
