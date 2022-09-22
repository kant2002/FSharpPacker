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
fspack FSharpPacker.Tests\Samples\LoadFile.fsx --self-contained -o test
test\LoadFile.exe
```

for AOT build (need to hack Program.cs and change to `var targetFramework = "net7.0";` and rebuild and reinstall tool)
```
fspack FSharpPacker.Tests\Samples\LoadFile.fsx --self-contained -o test-aot -r win-x64 /p:PublishAot=true
test-aot\LoadFile.exe
```

# Producing Nuget package

```
dotnet pack FSharpPacker -c Release
dotnet tool install --global --add-source FSharpPacker\bin\Release\ FSharpPacker
```

```
dotnet tool uninstall -g FSharpPacker
```