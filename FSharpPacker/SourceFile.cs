namespace FSharpPacker;

public class SourceFile
{
    private TextReader reader;
    private TextWriter writer;
    private string tempFile;
    private List<string> additionalSearchPaths = new();

    public SourceFile(string fileName, string content)
        : this(fileName, new StringReader(content))
    {
    }
    public SourceFile(string fileName)
        : this(fileName, new StreamReader(fileName))
    {
    }
    private SourceFile(string fileName, TextReader reader)
    {
        this.FileName = fileName;
        this.reader = reader;
        tempFile = Path.GetTempFileName();
        this.writer = new StreamWriter(new FileStream(tempFile, FileMode.Open, FileAccess.Write, FileShare.Read));

        additionalSearchPaths.Add(Path.GetDirectoryName(Path.GetFullPath(FileName))!);
    }

    public string FileName { get; }

    public string ResolveRelativePath(string path)
    {
        foreach (var basePath in additionalSearchPaths)
        {
            string resolvedPath = Path.Combine(basePath, path);
            resolvedPath = Path.GetFullPath(resolvedPath);
            if (File.Exists(resolvedPath))
            {
                return resolvedPath;
            }
        }

        throw new InvalidOperationException($"Cannot resolve file {path}");
    }

    public string? ReadLine()
    {
        return this.reader.ReadLine();
    }

    public IEnumerable<string> ReadContent()
    {
        while (reader.ReadLine() is { } line)
        {
            yield return line;
        }
    }

    public void WriteLine(string line)
    {
        this.writer.WriteLine(line);
        this.writer.Flush();
    }

    public string ReadProducedFile()
    {
        this.writer.Close();
        return File.ReadAllText(tempFile);
    }

    internal void AddIncludePaths(IEnumerable<string> enumerable)
    {
        this.additionalSearchPaths.AddRange(enumerable.Select(_ =>
        {
            if (Path.IsPathRooted(_))
            {
                return _;
            }

            return Path.Combine(additionalSearchPaths[0], _);
        }));
    }
}