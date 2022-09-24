FSharp Packer
=============

This tool allows package FSX files as self-contained executables.

Install it.

```shell
dotnet tool install --global FSharpPacker
```

Usage is following

```shell
fspack fsx-file.fsx [<additional arguments to dotnet publish>]
```

For example:
```shell
fspack FSharpPacker.Tests\Samples\LoadFile.fsx --self-contained -o test
test\LoadFile.exe
```

for AOT build
```shell
fspack FSharpPacker.Tests\Samples\LoadFile.fsx --self-contained -o test-aot -r win-x64 --framework net7.0 /p:PublishAot=true
test-aot\LoadFile.exe
```

# Supported FSX directives

| Directive | Status  | Notes |
| --------- | ------- | ----- 
| #load     | :white_check_mark: | |
| #r "path\file.dll"     | :white_check_mark: | |
| #r "nuget: package"     | :white_check_mark: | |
| #r "nuget: package, version"     | :white_check_mark: | |
| #load     | :white_check_mark: | |
| #I "nuget: source-feed"     | :white_large_square: | Only because I'm lazy. File an issue if needed. |
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
dotnet pack FSharpPacker -c Release
dotnet tool install --global --add-source FSharpPacker\bin\Release\ FSharpPacker
```

```shell
dotnet tool uninstall -g FSharpPacker
```