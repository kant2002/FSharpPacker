FSharp Packer
=============

This tool allows package FSX files as self-contained executables.

Installation:

```shell
dotnet tool install --global FSharpPacker
```

### [Nix](https://nixos.org/manual/nix/stable/) (Nix/NixOS)

Nix w/ [Flakes](https://nixos.wiki/wiki/Flakes)
```
nix profile install github:kant2002/FSharpPacker
```

Run without installing
```
nix run github:kant2002/FSharpPacker -- --help
```

# Usage

USAGE: `fspack <file> [--help] [--framework <framework>] [--verbose] [--noselfcontained] [--aot] `

FILE:

    <file>                .fsx file to convert to executable file

OPTIONS:

    --framework, -f <framework>
                          Specify target framework (e.g. net6.0)
    --verbose, -v         Verbose output
    --noselfcontained, -nsc
                          Don't publish as self-contained (with dotnet runtime included)
    --singefile, -sf      Produce single file
    --aot, -aot           Enable AOT-compilation
    --projectref [<projectReferences>...]
                          Add project references to the script, so you can use classes from them
    --help                display this list of options.


**Please note that the app is produced as self-contained by default.**



Simple usage:

```shell
fspack fsx-file.fsx [<additional arguments to dotnet publish>]
```

For example:
```shell
fspack FSharpPacker.Tests\Samples\LoadFile.fsx -o test -r win-x64
test\LoadFile.exe
```

for AOT build
```shell
fspack FSharpPacker.Tests\Samples\LoadFile.fsx -aot -o test-aot -r win-x64 -f net7.0
test-aot\LoadFile.exe
```

Self-contained with dotnet 7 and a single-file executable:
```shell
fspack FSharpPacker.Tests/Samples/LoadFile.fsx  -o test-single-file -r win-x64 -f net7.0 -sf -p:EnableCompressionInSingleFile=true
test-single-file\LoadFile.exe
```

## Extensions

You can specify project reference using `--projectref` option. There can be more then one project references.
Also script now supported [DependencyManager.FsProj](https://github.com/ThisFunctionalTom/DependencyManager.FsProj) extension which allow you specify `#r "fsproj: test.fsproj"` inside FSX file.

# Supported FSX directives

| Directive | Status  | Notes |
| --------- | ------- | ----- 
| #load     | :white_check_mark: | |
| #r "path\file.dll"     | :white_check_mark: | |
| #r "nuget: package"     | :white_check_mark: | |
| #r "nuget: package, version"     | :white_check_mark: | |
| #r "fsproj: custom-path-search-hint"     | :white_large_square: | |
| #load     | :white_check_mark: | |
| #i "nuget: source-feed"     | :white_check_mark: | |
| #quit     | :white_check_mark: | |
| #r "custom: custom-path"     | :white_large_square: | This is tricky and require deep involvement with FSharp.Compiler.Services |
| #I "custom: custom-path-search-hint"     | :white_large_square: | |

# Ignored FSX directives

| Directive | 
| --------- | 
| #help     | 
| #time     | 

# Producing Nuget package

```shell
dotnet pack FSharpPacker.FSharp -c Release
dotnet tool install FSharpPacker --global --add-source FSharpPacker.FSharp\bin\Release\ 
```

```shell
dotnet tool uninstall -g FSharpPacker
```