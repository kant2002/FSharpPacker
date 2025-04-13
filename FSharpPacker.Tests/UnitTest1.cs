namespace FSharpPacker.Tests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void PassthroughFileContent()
    {
        var sourceFile = "Samples/PlainFsharp.fsx";
        var preprocessor = new FsxPreprocessor(verbose:false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module PlainFsharp" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
    }
    [TestMethod]
    public void HelpCommand()
    {
        var sourceFile = "Samples/HelpCommand.fsx";
        var preprocessor = new FsxPreprocessor(verbose:false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module HelpCommand" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
    }
    [TestMethod]
    public void TimeCommands()
    {
        var sourceFile = "Samples/TimeCommands.fsx";
        var preprocessor = new FsxPreprocessor(verbose:false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module TimeCommands" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
    }
    [TestMethod]
    public void QuitCommand()
    {
        var sourceFile = "Samples/QuitCommand.fsx";
        var preprocessor = new FsxPreprocessor(verbose:false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module QuitCommand" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine + "System.Environment.Exit 0" + Environment.NewLine, preprocessor.GetSource(sourceFile));
    }
    [TestMethod]
    public void ResolveReferences()
    {
        var sourceFile = "Samples/RegularReference.fsx";
        var preprocessor = new FsxPreprocessor(verbose:false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module RegularReference" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
        var assemblies = preprocessor.GetReferences();
        Assert.AreEqual(1, assemblies.Length);
        Assert.AreEqual(Path.GetFullPath("Samples/fake_dll.fsx"), assemblies[0]);
    }
    [TestMethod]
    public void NugetPackageWithoutVersion()
    {
        var sourceFile = "Samples/NugetLastVersion.fsx";
        var preprocessor = new FsxPreprocessor(verbose:false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module NugetLastVersion" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
        var assemblies = preprocessor.GetReferences();
        Assert.AreEqual(0, assemblies.Length);
        var packages = preprocessor.GetPackageReferences();
        Assert.AreEqual(1, packages.Length);
        Assert.AreEqual("Newtonsoft.Json", packages[0].Name);
        Assert.AreEqual("*", packages[0].Version);
    }
    [TestMethod]
    public void NugetPackageWithExplicitVersion()
    {
        var sourceFile = "Samples/NugetExplicitVersion.fsx";
        var preprocessor = new FsxPreprocessor(verbose:false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module NugetExplicitVersion" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
        var assemblies = preprocessor.GetReferences();
        Assert.AreEqual(0, assemblies.Length);
        var packages = preprocessor.GetPackageReferences();
        Assert.AreEqual(1, packages.Length);
        Assert.AreEqual("DiffSharp-lite", packages[0].Name);
        Assert.AreEqual("1.0.0-preview-328097867", packages[0].Version);
    }
    [TestMethod]
    public void NugetCustomFeed()
    {
        var sourceFile = "Samples/NugetCustomFeed.fsx";
        var preprocessor = new FsxPreprocessor(verbose:false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module NugetCustomFeed" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
        var assemblies = preprocessor.GetReferences();
        Assert.AreEqual(0, assemblies.Length);
        var packageSources = preprocessor.GetPackageSources();
        Assert.AreEqual(1, packageSources.Length);
        Assert.AreEqual("https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet8/nuget/v3/index.json", packageSources[0]);
    }
    [TestMethod]
    public void FsProjReferenceExtension()
    {
        var sourceFile = "Samples/FsProjReferenceExtension.fsx";
        var preprocessor = new FsxPreprocessor(verbose: false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();

        Assert.AreEqual("module FsProjReferenceExtension" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
        var assemblies = preprocessor.GetReferences();
        Assert.AreEqual(0, assemblies.Length);
        var projectReferences = preprocessor.GetProjectReferences();
        Assert.AreEqual(1, projectReferences.Length);
        Assert.AreEqual("test.fsproj", projectReferences[0]);
    }
    [TestMethod]
    public void LoadFile()
    {
        var sourceFile = "Samples/LoadFile.fsx";
        var preprocessor = new FsxPreprocessor(verbose:false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module LoadFile" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
        var sources = preprocessor.GetSources();
        Assert.AreEqual(2, sources.Length);
    }

    [TestMethod]
    public void MultipleLoadFile()
    {
        var sourceFile = "Samples/MultipleLoadFile.fsx";
        var preprocessor = new FsxPreprocessor(verbose:false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();

        Assert.AreEqual("module MultipleLoadFile" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
        var sources = preprocessor.GetSources();
        Assert.AreEqual(3, sources.Length);
    }

    [TestMethod]
    public void LowercaseScriptWithDashesInName()
    {
        var sourceFile = "Samples/lowercase-script.fsx";
        var preprocessor = new FsxPreprocessor(verbose:false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();

        var sources = preprocessor.GetSources();
        Assert.AreEqual(2, sources.Length);
        // we expect module name of test in lowercase should be in pascal case
        Assert.AreEqual("module Testscript" + Environment.NewLine + "open System" + Environment.NewLine + "let testScript() = Console.WriteLine 1" + Environment.NewLine, sources[0].ReadProducedFile());
        // we expect module name of test in kebabcase should be escaped correctly
        Assert.AreEqual("module ``lowercase-script``" + Environment.NewLine + "open Testscript" + Environment.NewLine + "testScript()" + Environment.NewLine, sources[1].ReadProducedFile());
    }
    [TestMethod]
    public void IncludePath()
    {
        var sourceFile = "Samples/IncludePath.fsx";
        var preprocessor = new FsxPreprocessor(verbose:false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();

        Assert.AreEqual("module IncludePath" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
        var sources = preprocessor.GetSources();
        Assert.AreEqual(2, sources.Length);
    }
    [TestMethod]
    public void ConditionalCompilation()
    {
        var sourceFile = "Samples/ConditionalCompilation.fsx";
        var preprocessor = new FsxPreprocessor(verbose: false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();

        Assert.AreEqual("module ConditionalCompilation" + Environment.NewLine + "#if INTERACTIVE" + Environment.NewLine + "printfn \"INTERACTIVE\"" + Environment.NewLine + "#else" + Environment.NewLine + "printfn \"NOT INTERACTIVE\"" + Environment.NewLine + "#endif" + Environment.NewLine, preprocessor.GetSource(sourceFile));
        var sources = preprocessor.GetSources();
        Assert.AreEqual(1, sources.Length);
    }
    [TestMethod]
    public void NamespacesInLoadFiles()
    {
        var sourceFile = "Samples/Namespaces.fsx";
        var preprocessor = new FsxPreprocessor(verbose: true);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();

        Assert.AreEqual(@"module Namespaces
printfn ""Hello, world""
".ReplaceLineEndings(), preprocessor.GetSource(sourceFile));

        Assert.AreEqual(@"namespace Asfaload.Collector
module Queue=
    let f (s:string) = s
".ReplaceLineEndings(), preprocessor.FindSource(_ => _.EndsWith("Level1/WithNamespace.fsx")));
    }
    [TestMethod]
    public void NamespacesWithCommentsInLoadFiles()
    {
        var sourceFile = "Samples/NamespacesWithComments.fsx";
        var preprocessor = new FsxPreprocessor(verbose: true);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();

        Assert.AreEqual(@"module NamespacesWithComments
printfn ""Hello, world""
".ReplaceLineEndings(), preprocessor.GetSource(sourceFile));

        Assert.AreEqual(@"// Use this
namespace Asfaload.Collector
module Queue=
    let f (s:string) = s
".ReplaceLineEndings(), preprocessor.FindSource(_ => _.EndsWith("Level1/WithNamespaceAndComments.fsx")));
    }
    [TestMethod]
    public void MultipleLoadDirectivesFile()
    {
        var sourceFile = "Samples/MultipleLoadDirectivesFile.fsx";
        var preprocessor = new FsxPreprocessor(verbose: true);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();

        CollectionAssert.AreEqual(new string[] { "WithNamespace.fsx", "WithNamespaceAndComments.fsx", "Loader.fsx", "MultipleLoadDirectivesFile.fsx" }, preprocessor.GetSources().Select(_ => Path.GetFileName(_.FileName)).ToArray());
    }
    [TestMethod]
    public void Shebang()
    {
        var sourceFile = "Samples/Shebang.fsx";
        var preprocessor = new FsxPreprocessor(verbose: false);
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();

        Assert.AreEqual("module Shebang" + Environment.NewLine + Environment.NewLine + "printfn \"Hello, world!\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
    }
}