module Packer

open System
open System.IO
open System.Text.RegularExpressions

printfn "Hello from F#"

type public NugetReference = {
    Name: string;
    Version: string;
}

type public SourceFile(fileName: string, reader: TextReader) = class
    let tempFile = Path.GetTempFileName()
    let stream = new FileStream(tempFile, FileMode.Open, FileAccess.Write, FileShare.Read)
    let writer = new StreamWriter( stream )
    let mutable additionalSearchPaths = [ Path.GetDirectoryName(Path.GetFullPath(fileName)) ]

    new (fileName, content) = SourceFile(fileName, new StringReader(content))
    new (fileName: string) = SourceFile(fileName, new StreamReader(fileName))

    member this.FileName = fileName
    member this.ReadLine() = reader.ReadLine
    member this.WriteLine(line: string) =
        writer.WriteLine line
        writer.Flush()

    member this.ReadProducedFile() =
        writer.Close()
        File.ReadAllText tempFile

    member this.AddIncludePaths (enumerable: seq<string>) =
        let x = Seq.map (fun (p: string) -> if Path.IsPathRooted(p) then p else Path.Combine(additionalSearchPaths[0], p)) enumerable 
        additionalSearchPaths <- additionalSearchPaths @ (x |> List.ofSeq);

    member this.ResolveRelativePath(path: string) =
        let resolve basePath =
            let resolvedPath = Path.Combine(basePath, path)
            if File.Exists resolvedPath then Some(resolvedPath) else None

        match Seq.tryPick resolve additionalSearchPaths with
            | Some(path) -> path
            | None -> raise (InvalidOperationException($"Cannot resolve file {path}"))

    member this.ReadContent() =seq {
        let mutable line = reader.ReadLine()
        while line <> null do
            yield line
            line <- reader.ReadLine()
    }

end

let public GetModuleName (sourceFile: SourceFile) =
    let fileName = Path.GetFileNameWithoutExtension(sourceFile.FileName)
    let isIdentifier = Regex.IsMatch (fileName, "^[_a-zA-Z][_a-zA-Z0-9]{0,30}$")
    if not isIdentifier
        then "``" + fileName + "``"
        else if Char.IsLower(fileName[0]) then Char.ToUpper(fileName[0]).ToString() + fileName.Substring(1)
        else fileName

type public FsxProgramState = {
    mutable sourceFiles: seq<SourceFile>;
    mutable references: seq<string>;
    mutable packageReferences: seq<NugetReference>;
}

let public AddSource state sourceFile (content: string) =
    let m = Seq.append state.sourceFiles [new SourceFile(sourceFile, content)]
    { state with sourceFiles = m }

let public AddSourceFromFile state sourceFile =
    AddSource state sourceFile (File.ReadAllText(sourceFile))

let Unquote (data: string) = data.Trim('"')

let public ParsePaths paths =
    Regex.Matches(paths, "(\"|')(?:\\\\\\1|[^\\1])*?\\1") |> Seq.map (fun x -> Unquote x.Value)