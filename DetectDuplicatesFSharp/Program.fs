module DetectDuplicates.Program

open System.IO
open System.Text.RegularExpressions

let [<Literal>] Okay = 0
let [<Literal>] Fail = 1

let findDuplicates file packages =
  packages
  |> Seq.groupBy id
  |> Seq.choose (fun (package, occurs) ->
      if 1 < Seq.length occurs then
        Some {| FileName = file; Package = package |}
      else None
  )

let getPackages file =
  let packages = seq {
    for line in File.ReadLines(file) do
      let matched = Regex.Match(line, "PackageReference Include=\"([^\"]*)\"")
      if matched.Success then matched.Groups.[1].Value
  }
  {| File = file; Packages = packages |}

[<EntryPoint>]
let main args =
  try
    let folder =
      args
      |> Array.tryHead
      |> Option.defaultWith Directory.GetCurrentDirectory

    printfn "Looking for duplicates in %s... " folder

    let files =
      Directory.EnumerateFiles(folder, "*.csproj", SearchOption.AllDirectories)

    let duplicates = seq {
      for entry in files |> Seq.map getPackages do
        yield! findDuplicates entry.File entry.Packages
    }

    printfn "Duplicate detection complete!"

    if 1 < Seq.length duplicates then
      for dup in duplicates do
        eprintfn "Duplicate %s found in %s" dup.Package dup.FileName
      Fail
    else
      Okay
  with
  | x -> eprintfn "ERROR! %A" x; Fail
