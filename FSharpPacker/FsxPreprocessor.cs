extern alias fsharp;

namespace FSharpPacker;

using NugetReference = fsharp::Packer.NugetReference;
using SourceFile = fsharp::Packer.SourceFile;

public class FsxPreprocessor
{
    private List<SourceFile> sourceFiles = new();
    private List<string> references = new();
    private List<NugetReference> packageReferences = new();

    public void AddSource(string sourceFile, string content)
    {
        sourceFiles.Add(new SourceFile(sourceFile, content));
    }

    public void AddSource(string sourceFile)
    {
        sourceFiles.Add(new SourceFile(sourceFile, File.ReadAllText(sourceFile)));
    }

    public void Process()
    {
        foreach (var sourceFile in sourceFiles.ToList())
        {
            Console.WriteLine($"Processing {sourceFile.FileName}");
            var state = new fsharp::Packer.FsxProgramState(sourceFiles, references, packageReferences);
            fsharp::Packer.ProcessFile(state, sourceFile);
            sourceFiles = state.sourceFiles.ToList();
            references = state.references.ToList();
            packageReferences = state.packageReferences.ToList();
        }
    }

    public string GetSource(string mainFsx)
    {
        return sourceFiles.First(_ => _.FileName == mainFsx).ReadProducedFile();
    }

    public IReadOnlyList<SourceFile> GetSources()
    {
        return sourceFiles;
    }

    public IReadOnlyList<string> GetReferences() => references;

    public IReadOnlyList<NugetReference> GetPackageReferences() => packageReferences;
}