namespace FSharpPacker.Tests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void PassthroughFileContent()
    {
        var sourceFile = "Samples/PlainFsharp.fsx";
        var preprocessor = new FsxPreprocessor()
            .WithBasePath("HomeDirectory");
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module PlainFsharp" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
    }
    [TestMethod]
    public void HelpCommand()
    {
        var sourceFile = "Samples/HelpCommand.fsx";
        var preprocessor = new FsxPreprocessor()
            .WithBasePath("HomeDirectory");
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module HelpCommand" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
    }
    [TestMethod]
    public void TimeCommands()
    {
        var sourceFile = "Samples/TimeCommands.fsx";
        var preprocessor = new FsxPreprocessor()
            .WithBasePath("HomeDirectory");
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module TimeCommands" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
    }
    [TestMethod]
    public void QuitCommand()
    {
        var sourceFile = "Samples/QuitCommand.fsx";
        var preprocessor = new FsxPreprocessor()
            .WithBasePath("HomeDirectory");
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module QuitCommand" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine + "System.Environment.Exit 0" + Environment.NewLine, preprocessor.GetSource(sourceFile));
    }
    [TestMethod]
    public void ResolveReferences()
    {
        var sourceFile = "Samples/RegularReference.fsx";
        var preprocessor = new FsxPreprocessor()
            .WithBasePath("HomeDirectory");
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module RegularReference" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
        var assemblies = preprocessor.GetReferences();
        Assert.AreEqual(1, assemblies.Count);
        Assert.AreEqual(Path.GetFullPath("test.dll"), assemblies[0]);
    }
    [TestMethod]
    public void NugetPackageWithoutVersion()
    {
        var sourceFile = "Samples/NugetLastVersion.fsx";
        var preprocessor = new FsxPreprocessor()
            .WithBasePath("HomeDirectory");
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module NugetLastVersion" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
        var assemblies = preprocessor.GetReferences();
        Assert.AreEqual(0, assemblies.Count);
        var packages = preprocessor.GetPackageReferences();
        Assert.AreEqual(1, packages.Count);
        Assert.AreEqual("Newtonsoft.Json", packages[0].Name);
        Assert.AreEqual("*", packages[0].Version);
    }
    [TestMethod]
    public void NugetPackageWithExplicitVersion()
    {
        var sourceFile = "Samples/NugetExplicitVersion.fsx";
        var preprocessor = new FsxPreprocessor()
            .WithBasePath("HomeDirectory");
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module NugetExplicitVersion" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
        var assemblies = preprocessor.GetReferences();
        Assert.AreEqual(0, assemblies.Count);
        var packages = preprocessor.GetPackageReferences();
        Assert.AreEqual(1, packages.Count);
        Assert.AreEqual("DiffSharp-lite", packages[0].Name);
        Assert.AreEqual("1.0.0-preview-328097867", packages[0].Version);
    }
    [TestMethod]
    public void LoadFile()
    {
        var sourceFile = "Samples/LoadFile.fsx";
        var preprocessor = new FsxPreprocessor()
            .WithBasePath("HomeDirectory");
        preprocessor.AddSource(sourceFile);

        preprocessor.Process();
        
        Assert.AreEqual("module LoadFile" + Environment.NewLine + "printfn \"Hello, world\"" + Environment.NewLine, preprocessor.GetSource(sourceFile));
        var sources = preprocessor.GetSources();
        Assert.AreEqual(2, sources.Count);
    }
}