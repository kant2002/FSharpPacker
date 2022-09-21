FSharp Packer
=============

This tool allows package FSX files as self-contained executables.

Usage is following

```shell
dotnet run fsx-file.fsx [<additional arguments to dotnet publish>]
```

Currently produced application requires .net 7, but that's strictly because 
I have install broken .NET 7.0. Change csproj and Program.cs and that's it. 

For example:
```
cd FSharpPacker
dotnet run --project FSharpPacker FSharpPacker.Tests\Samples\LoadFile.fsx --self-contained -o test
```

for AOT build
```
dotnet run --project FSharpPacker FSharpPacker.Tests\Samples\LoadFile.fsx --self-contained -o test-aot -- -r win-x64 /p:PublishAot=true
```
