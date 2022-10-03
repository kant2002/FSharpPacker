module FSharpPacker.Program

open System
open System.Diagnostics
open System.IO
open FSharpPacker


[<EntryPoint>]
let main args =
    let sourceFile = args[0]
    let mutable targetFramework = "net6.0"

    let mutable _foundTarget = false

    for i in 0 .. args.Length do
        if _foundTarget then
            ()
        elif args[i] = "-f" || args[i] = "--framework" then
            if i = args.Length - 1 then
                invalidArg "-f" "Please specify target framework"

            let nextArg = args[i + 1]

            if nextArg.StartsWith("-") then
                invalidArg nextArg "Please specify target framework"

            targetFramework <- nextArg
            _foundTarget <- true // 'break' substitute



    let preprocessor = FsxPreprocessor()
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
    let projectContent = $"""<Project Sdk=""Microsoft.NET.Sdk"">
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

    let additionalArguments = args[1..];

    Console.WriteLine($"Compiling generated file {tempProject}")
    let commandLineArguments = [| "publish" ; tempProject |] |> Array.append additionalArguments
    Console.WriteLine($"""Running dotnet {String.Join(" ", commandLineArguments)}""")
    let prc = Process.Start("dotnet", commandLineArguments)
    prc.WaitForExit()
    prc.ExitCode
