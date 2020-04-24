module DetectDuplicates.Program

open System.IO
open System.Text.RegularExpressions

let [<Literal>] Okay = 0
let [<Literal>] Fail = 1

[<EntryPoint>]
let main args =
  try
    let folder =
      args
      |> Array.tryHead
      |> Option.defaultWith Directory.GetCurrentDirectory

    printfn "Looking for duplicates in %s... " folder

    let duplicates =
      Directory.EnumerateFiles(folder, "*.csproj", SearchOption.AllDirectories)
      |> Seq.collect (fun file ->
          file
          |> File.ReadLines
          |> Seq.choose (fun line ->
              let matched = Regex.Match(line, "Include=\"([^\"]*)\"")
              if matched.Success then Some matched.Groups.[1].Value else None
          )
          |> Seq.groupBy id
          |> Seq.choose (fun (package, occurs) ->
              if 1 < Seq.length occurs then
                Some {| File = file; Package = package |}
              else None
          )
      )
      |> Seq.toList

    for dup in duplicates do
        eprintfn "Duplicate %s found in %s" dup.Package dup.File

    printfn "Duplicate detection complete!"
    if 0 < Seq.length duplicates then Fail else Okay
  with
  | x -> eprintfn "ERROR! %A" x; Fail
