using System.Diagnostics;
using FSharpPacker;

var sourceFile = args[0];
var targetFramework = "net6.0";
var preprocessor = new FsxPreprocessor()
    .WithBasePath("HomeDirectory");
preprocessor.AddSource(sourceFile);

preprocessor.Process();

var path = Path.GetTempFileName();
Console.WriteLine(path);
var sourceFiles = preprocessor.GetSources();
var sourceFilesList = string.Join(
  Environment.NewLine, 
  sourceFiles.Select(sf =>
  {
    var tempSource = Path.GetTempFileName() + ".fs";
    File.WriteAllText(tempSource, sf.ReadProducedFile());
    return $"<Compile Include=\"{tempSource}\" />";
  }));

var tempProject = path + ".fsproj";
var packageReferences = preprocessor.GetPackageReferences();
var packageReferencesList = string.Join(
  Environment.NewLine, 
  packageReferences.Select(pr => $"{($"<PackageReference Include=\"{pr.Name}\" Version=\"{pr.Version}\" />")}"));
var references = preprocessor.GetReferences();
var referencesList = string.Join(
  Environment.NewLine, 
  references.Select(pr => $"{($"<Reference Include=\"{Path.GetFileNameWithoutExtension(pr)}\"><HintPath>{pr}</HintPath></Reference>")}"));
var projectContent = @$"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <AssemblyName>{Path.GetFileNameWithoutExtension(sourceFile)}</AssemblyName>
    <OutputType>Exe</OutputType>
    <TargetFramework>{targetFramework}</TargetFramework>
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
File.WriteAllText(tempProject, projectContent);
Console.WriteLine(tempProject);
var process = Process.Start("dotnet", (new [] { "publish", tempProject }).Union(args.Skip(1)));
process.WaitForExit();
