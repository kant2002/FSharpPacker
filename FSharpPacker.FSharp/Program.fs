module FSharpPacker.Program

open System
open System.Diagnostics
open System.IO
open FSharpPacker
open Argu

type CliArguments =
    | [<AltCommandLine("-f")>] Framework of framework:string
    | [<AltCommandLine("-v")>] Verbose
    | [<AltCommandLine("-nsc")>] NoSelfContained
    | [<AltCommandLine("-aot")>] AOT
    | [<MainCommand; ExactlyOnce; First>] File of file:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Framework _ -> "Specify target framework (e.g. net6.0)"
            | Verbose _ -> "Verbose output"
            | File _ -> ".fsx file to convert to executable file"
            | AOT _ -> "Enable AOT-compilation"
            | NoSelfContained _ -> "Don't publish as self-contained (with dotnet runtime included)"

[<EntryPoint>]
let main argv =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<CliArguments>(programName = "fspack", errorHandler = errorHandler)

    let results = parser.ParseCommandLine (inputs=argv, ignoreUnrecognized = true)


    let sourceFile = results.GetResult(CliArguments.File)
    let targetFramework = results.GetResult(CliArguments.Framework, defaultValue = "net6.0")
    let verbose =  match results.TryGetResult(CliArguments.Verbose) with | Some _ -> true |None -> false
    let preprocessor = FsxPreprocessor(verbose = verbose)
    preprocessor.AddSource(sourceFile)
    preprocessor.Process()

    let sourceFiles = preprocessor.GetSources()

    let sourceFilesList =
        sourceFiles
            |> Array.map (fun sf ->
                let tempSource = Path.GetTempFileName() + ".fs"
                File.WriteAllText(tempSource, sf.ReadProducedFile())
                $"<Compile Include=\"{tempSource}\" />")
            |> fun x -> String.Join(Environment.NewLine, x)
    
    let packageReferences = preprocessor.GetPackageReferences()
    let packageReferencesList =
        packageReferences
            |> Array.map (fun pr ->
                $"<PackageReference Include=\"{pr.Name}\" Version=\"{pr.Version}\" />"
               )
            |> fun x -> String.Join(Environment.NewLine, x)
    let references = preprocessor.GetReferences()
    let referencesList =
        references
            |> Array.map (fun pr ->
                $"<Reference Include=\"{Path.GetFileNameWithoutExtension(pr)}\"><HintPath>{pr}</HintPath></Reference>"
               )
            |> fun x -> String.Join(Environment.NewLine, x)
    let defineInteractive = true;
    let projectContent = $"""<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>{Path.GetFileNameWithoutExtension(sourceFile)}</AssemblyName>
    <OutputType>Exe</OutputType>
    <TargetFramework>{targetFramework}</TargetFramework>
    {(if defineInteractive then  "<DefineConstants>$(DefineConstants);INTERACTIVE</DefineConstants>" else String.Empty)}
  </PropertyGroup>

  <ItemGroup>
    {packageReferencesList}
  </ItemGroup>

  <ItemGroup>
    {referencesList}
  </ItemGroup>

  <ItemGroup>
    {sourceFilesList}
  </ItemGroup>

</Project>
"""
    
    let path = Path.GetTempFileName()
    let tempProject = path + ".fsproj";
    File.WriteAllText(tempProject, projectContent);

    let selfContained = match results.TryGetResult(CliArguments.NoSelfContained) with | Some _ -> false |None -> true
    let doAot = match results.TryGetResult(CliArguments.AOT) with | Some _ -> true |None -> false
    let additionalArguments = results.UnrecognizedCliParams

    if verbose then Console.WriteLine($"Compiling generated file {tempProject}")
    let commandLineArguments =  Array.append  [| "publish"
                                                 tempProject
                                                 "-c"
                                                 "Release"
                                                 if doAot then "/p:PublishAot=true" else ""
                                                 if selfContained then "--self-contained" else "--no-self-contained"|]
                                               (additionalArguments |> List.toArray)
    if verbose then Console.WriteLine($"""Running dotnet {String.Join(" ", commandLineArguments)}""")
    let prc = Process.Start("dotnet", commandLineArguments)
    prc.WaitForExit()
    prc.ExitCode
