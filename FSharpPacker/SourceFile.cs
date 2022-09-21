namespace FSharpPacker;

public class SourceFile
{
    private TextReader reader;
    private TextWriter writer;
    private string tempFile;
    public SourceFile(string fileName, string content)
    {
        this.FileName = fileName;
        this.reader = new StringReader(content);
        tempFile = Path.GetTempFileName();
        this.writer = new StreamWriter(new FileStream(tempFile, FileMode.Open, FileAccess.Write, FileShare.Read));
    }
    public SourceFile(string fileName)
    {
        this.FileName = fileName;
        this.reader = new StreamReader(fileName);
        tempFile = Path.GetTempFileName();
        this.writer = new StreamWriter(new FileStream(tempFile, FileMode.Open, FileAccess.Write, FileShare.Read));
    }

    public string FileName { get; }

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
}