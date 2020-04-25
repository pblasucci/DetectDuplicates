module DetectDuplicates.Program

open System.IO
open System.Text.RegularExpressions

let [<Literal>] Okay = 0
let [<Literal>] Fail = 1

[<EntryPoint>]
let main args =
  try
    let path = defaultArg (Array.tryHead args) (Directory.GetCurrentDirectory())
    printfn "Looking for duplicates in %s\n " path

    let duplicates = seq {
      for file in Directory.EnumerateFiles(path, "*.csproj", SearchOption.AllDirectories) do
        let packages = seq {
          for line in File.ReadLines(file) do
            let matched = Regex.Match(line, "<PackageReference\s+Include=\"([^\"]*)\"")
            if matched.Success then matched.Groups.[1].Value
        }
        for (package, occurs) in packages |> Seq.groupBy id do
          if 1 < Seq.length occurs then (file, package)
    }

    for (file, package) in duplicates do
        eprintfn "Duplicate %s found in %s" package file

    printfn "\nDuplicate detection complete!"
    if 0 < Seq.length duplicates then Fail else Okay
  with
  | x -> eprintfn "ERROR! %A" x; Fail
