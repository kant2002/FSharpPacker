namespace FSharpPacker

open System
open System.IO
open Packer

type FsxPreprocessor() =
    
    let mutable sourceFiles = ResizeArray<SourceFile>()
    
    let mutable references = ResizeArray<string>()
    let mutable packageReferences = ResizeArray<NugetReference>()
    
    member _.AddSource (sourceFile:string, content:string) =
        sourceFiles.Add(SourceFile(sourceFile, content))
        
    member _.AddSource (sourceFile:string) =
        sourceFiles.Add(SourceFile(sourceFile, File.ReadAllText(sourceFile)));

    member _.Process () =
        
        for sourceFile in sourceFiles do
            Console.WriteLine($"Processing {sourceFile.FileName}")
            let state: FsxProgramState = { sourceFiles = sourceFiles; references = references; packageReferences = packageReferences }
            Packer.ProcessFile state sourceFile
            sourceFiles <- ResizeArray(state.sourceFiles)
            references <- ResizeArray(state.references)
            packageReferences <- ResizeArray(state.packageReferences)
        
    member _.GetSource(mainFsx: string) =
        sourceFiles
        |> Seq.tryFind (fun x -> x.FileName = mainFsx)
        |> Option.map (fun x -> x.ReadProducedFile())
        |> Option.defaultValue ""
        
    
    member _.GetSources() =
        sourceFiles.ToArray()
    
    member _.GetReferences() =
        references.ToArray()
    
    member _.GetPackageReferences() =
        packageReferences.ToArray()
    