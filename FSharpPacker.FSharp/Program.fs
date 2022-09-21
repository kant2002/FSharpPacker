module Packer

open System
open System.IO

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