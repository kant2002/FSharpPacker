using System.Diagnostics;
using FSharpPacker;

var sourceFile = args[0];
var targetFramework = "net6.0";
var preprocessor = new FsxPreprocessor();
preprocessor.AddSource(sourceFile);

preprocessor.Process();

var sourceFiles = preprocessor.GetSources();
var sourceFilesList = string.Join(
  Environment.NewLine, 
  sourceFiles.Select(sf =>
  {
    var tempSource = Path.GetTempFileName() + ".fs";
    File.WriteAllText(tempSource, sf.ReadProducedFile());
    return $"<Compile Include=\"{tempSource}\" />";
  }));

var packageReferences = preprocessor.GetPackageReferences();
var packageReferencesList = string.Join(
  Environment.NewLine, 
  packageReferences.Select(pr => $"{($"<PackageReference Include=\"{pr.Name}\" Version=\"{pr.Version}\" />")}"));
var references = preprocessor.GetReferences();
var referencesList = string.Join(
  Environment.NewLine, 
  references.Select(pr => $"{($"<Reference Include=\"{Path.GetFileNameWithoutExtension(pr)}\"><HintPath>{pr}</HintPath></Reference>")}"));
var defineInteractive = true;
var projectContent = @$"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <AssemblyName>{Path.GetFileNameWithoutExtension(sourceFile)}</AssemblyName>
    <OutputType>Exe</OutputType>
    <TargetFramework>{targetFramework}</TargetFramework>
    {(defineInteractive ? "<DefineConstants>$(DefineConstants);INTERACTIVE</DefineConstants>" : string.Empty)}
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
";
var path = Path.GetTempFileName();
var tempProject = path + ".fsproj";
File.WriteAllText(tempProject, projectContent);

var additionalArguments = args.Skip(1);

Console.WriteLine($"Compiling generated file {tempProject}");
var commandLineArguments = (new[] { "publish", tempProject }).Union(additionalArguments);
Console.WriteLine($"Running dotnet {string.Join(" ", commandLineArguments)}");
var process = Process.Start("dotnet", commandLineArguments);
process.WaitForExit();
