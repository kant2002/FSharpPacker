extern alias fsharp;
using System.Text.RegularExpressions;

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
            ProcessFile(sourceFile);
        }
    }

    private void ProcessFile(SourceFile sourceFile)
    {
        sourceFile.WriteLine($"module {GetModuleName(sourceFile)}");
        foreach (var line in sourceFile.ReadContent())
        {
            var normalizedLine = Regex.Replace(line, "\\s+\\#\\s+", "#");
            if (normalizedLine.StartsWith("#"))
            {
                if (normalizedLine.StartsWith("#r"))
                {
                    var pathStrings = normalizedLine.Replace("#r ", string.Empty);
                    foreach (var path in ParsePaths(pathStrings))
                    {
                        var normalizedReference = Regex.Replace(path, "\\s+nuget\\s+:\\s+", "nuget:");
                        if (normalizedReference.StartsWith("nuget:"))
                        {
                            var packageParts = normalizedReference.Substring("nuget:".Length).Split(',');
                            var name = packageParts[0].Trim();
                            var version = packageParts.ElementAtOrDefault(1)?.Trim() ?? "*";
                            this.packageReferences.Add(new(name, version));
                        }
                        else
                        {
                            var relativeReferencePath = sourceFile.ResolveRelativePath(path);
                            this.references.Add(Path.GetFullPath(relativeReferencePath));
                        }
                    }
                }
                else if (normalizedLine.StartsWith("#help"))
                {
                    // Help command make no sense.
                    continue;
                }
                else if (normalizedLine.StartsWith("#quit"))
                {
                    sourceFile.WriteLine("System.Environment.Exit 0");
                }
                else if (normalizedLine.StartsWith("#I"))
                {
                    var pathStrings = normalizedLine.Replace("#I ", string.Empty);
                    sourceFile.AddIncludePaths(ParsePaths(pathStrings));
                }
                else if (normalizedLine.StartsWith("#load"))
                {
                    var pathStrings = normalizedLine.Replace("#load ", string.Empty);
                    foreach (var path in ParsePaths(pathStrings))
                    {
                        var relativeReferencePath = sourceFile.ResolveRelativePath(path);
                        Console.WriteLine($"Including {relativeReferencePath}");
                        var innerFile = new SourceFile(relativeReferencePath);
                        this.sourceFiles.Insert(0, innerFile);
                        ProcessFile(innerFile);
                    }
                }
            }
            else
            {
                sourceFile.WriteLine(line);
            }
        }
    }

    private static IEnumerable<string> ParsePaths(string paths) => fsharp::Packer.ParsePaths(paths);

    private static string GetModuleName(SourceFile sourceFile) => fsharp::Packer.GetModuleName(sourceFile);

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