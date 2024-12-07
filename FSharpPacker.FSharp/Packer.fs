module Packer

open System
open System.IO
open System.Text.RegularExpressions

type public NugetReference = {
    Name: string;
    Version: string;
}

type public SourceFile(fileName: string, reader: TextReader) = class
    let tempFile = Path.GetTempFileName()
    let stream = new FileStream(tempFile, FileMode.Open, FileAccess.Write, FileShare.Read)
    let writer = new StreamWriter( stream )
    let mutable additionalSearchPaths = [ Path.GetDirectoryName(Path.GetFullPath(fileName)) ]
    let mutable additionalNugetSources = [ ]

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

    member this.AddNugetSource (enumerable: seq<string>) =
        let x = Seq.map (fun (p: string) -> if Path.IsPathRooted(p) then p else Path.Combine(additionalSearchPaths[0], p)) enumerable 
        additionalNugetSources <- additionalNugetSources @ (x |> List.ofSeq);

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

let GetModuleName (sourceFile: SourceFile) =
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
    mutable nugetSources: seq<string>;
}

let public AddSource state sourceFile (content: string) =
    let m = Seq.append state.sourceFiles [new SourceFile(sourceFile, content)]
    { state with sourceFiles = m }

let public AddSourceFromFile state sourceFile =
    AddSource state sourceFile (File.ReadAllText(sourceFile))

let Unquote (data: string) = data.Trim('"')

let ParsePaths paths =
    Regex.Matches(paths, "(\"|')(?:\\\\\\1|[^\\1])*?\\1") |> Seq.map (fun x -> Unquote x.Value)

type FsxLine =
    | SourceCode of string
    | Unsupported
    | IncludePath of seq<string>
    | IncludeFiles of seq<string>
    | IncludeReference of references: seq<string> * packages: seq<NugetReference>
    | AddNugetFeed of string

let classifyLine (sourceFile: SourceFile) (normalizedLine: string) =
    if normalizedLine.StartsWith("#") then 
        if normalizedLine.StartsWith("#r") then
            let pathStrings = normalizedLine.Replace("#r ", "")
            let mutable references: seq<string> = []
            let mutable packages: seq<NugetReference> = []
            for path in ParsePaths(pathStrings) do
                let normalizedReference = Regex.Replace(path, "\\s+nuget\\s+:\\s+", "nuget:")
                if normalizedReference.StartsWith("nuget:") then
                    let packageParts = normalizedReference.Substring("nuget:".Length).Split(',')
                    let package = match packageParts with
                                    | [| name; version |] -> { Name = name.Trim(); Version = version.Trim() }
                                    | [| name |] -> { Name = name.Trim(); Version = "*" }
                                    | _ -> raise (new InvalidOperationException("Incorrect format of nuget package"))
                    packages <- Seq.append packages [package]
                else
                    let relativeReferencePath = sourceFile.ResolveRelativePath(path)
                    references <- Seq.append references [Path.GetFullPath(relativeReferencePath)]
                ()
            IncludeReference(references, packages)
        elif normalizedLine.StartsWith("#i") && not (normalizedLine.StartsWith("#if")) then
            let pathStrings = normalizedLine.Replace("#i ", "")
            let normalizedReference = Regex.Replace(pathStrings, "\\s+nuget\\s+:\\s+", "nuget:") |> Unquote
            if normalizedReference.StartsWith("nuget:") then
                let packageParts = normalizedReference.Substring("nuget:".Length).Trim()
                AddNugetFeed(packageParts)
            else
                Unsupported
        elif normalizedLine.StartsWith("#help") then
            Unsupported
        elif normalizedLine.StartsWith("#time") then
            Unsupported
        elif normalizedLine.StartsWith("#quit") then
            SourceCode("System.Environment.Exit 0")
        elif normalizedLine.StartsWith("#I") then
            let pathStrings = normalizedLine.Replace("#I ", "");
            IncludePath(ParsePaths(pathStrings))
        elif normalizedLine.StartsWith("#load") then
            let pathStrings = normalizedLine.Replace("#load ", "");
            let files = ParsePaths(pathStrings) |> Seq.map sourceFile.ResolveRelativePath
            IncludeFiles(files)
        else
            SourceCode(normalizedLine)
    else SourceCode(normalizedLine)

let rec public ProcessLine verbose  state sourceFile line =
    let normalizedLine = Regex.Replace(line, "\\s+\\#\\s+", "#")
    let lineResult = classifyLine sourceFile normalizedLine
    match lineResult with
        | SourceCode(code) -> sourceFile.WriteLine(code)
        | Unsupported -> ()
        | AddNugetFeed(code) -> state.nugetSources <- Seq.append state.nugetSources [code]
        | IncludePath(files) -> sourceFile.AddIncludePaths(files)
        | IncludeFiles(files) -> for file in files do
                                    if verbose then Console.WriteLine($"Including {file}")
                                    let innerFile = new SourceFile(file)
                                    state.sourceFiles <- Seq.append [innerFile] state.sourceFiles
                                    ProcessFile state innerFile verbose
        | IncludeReference(references, packages: seq<NugetReference>) ->
            state.references <- Seq.append state.references references
            state.packageReferences <- Seq.append state.packageReferences packages
                                                    
    ()

and public ProcessFile state sourceFile verbose =
    let worker = ProcessLine verbose state sourceFile
    let mutable insertedModule = false
    sourceFile.ReadContent() 
        |> Seq.iteri (fun i line -> 
                                    if insertedModule || (String.IsNullOrWhiteSpace(line.Trim()) || line.TrimStart().StartsWith("//")) 
                                    then worker line
                                    else 
                                        let trimmed = line.TrimStart()
                                        insertedModule <- true
                                        if not (trimmed.StartsWith("namespace ") || trimmed.StartsWith("namespace\t")) then
                                            sourceFile.WriteLine($"module {GetModuleName(sourceFile)}")
                                        worker line)
    ()
    
    
    