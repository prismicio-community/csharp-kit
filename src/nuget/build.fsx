// include Fake lib
#r @"packages/FAKE/tools/FakeLib.dll"
open Fake
open System

let authors = ["Zengularity"]

let projectName = "prismicio"
let projectDescription = "Prismic.io Development Kit for C#"
let projectSummary = "A minimal library, providing binding for C# on the .NET platform to the prismic.io REST API."

// directories
let buildDir = "../prismic/bin"
let packagingRoot = "./packaging/"
let packagingDir = packagingRoot @@ "prismic"

let buildMode = getBuildParamOrDefault "buildMode" "Release"

let releaseNotes =
    ReadFile "../../ReleaseNotes.md"
    |> ReleaseNotesHelper.parseReleaseNotes

Target "Clean" (fun _ ->
    CleanDirs [buildDir; packagingRoot; packagingDir]
)

open Fake.AssemblyInfoFile

Target "AssemblyInfo" (fun _ ->
    CreateCSharpAssemblyInfo "../SolutionInfo.cs"
      [ Attribute.Product projectName
        Attribute.Version releaseNotes.AssemblyVersion
        Attribute.FileVersion releaseNotes.AssemblyVersion
        Attribute.ComVisible false ]
)


Target "BuildApp" (fun _ ->
    MSBuild null "Build" ["Configuration", buildMode] ["../csharp-kit.sln"]
    |> Log "AppBuild-Output: "
)


Target "CreatePrismicPackage" (fun _ ->
    let libnet45 = "lib/net45/"
    let net45Dir = packagingDir @@ libnet45
    CleanDirs [net45Dir]

    CopyFile net45Dir (buildDir @@ "Release/prismicio.dll")
//    CopyFile net45Dir (buildDir @@ "Release/prismicio.dll.mdb")
    CopyFiles packagingDir ["../../README.md"; "../../ReleaseNotes.md"]

    NuGet (fun p ->
        {p with
            Authors = authors
            Project = projectName
            Description = projectDescription
            OutputPath = packagingRoot
            Summary = projectSummary
            WorkingDir = packagingDir
            Version = releaseNotes.AssemblyVersion
            ReleaseNotes = toLines releaseNotes.Notes
            Dependencies =
                ["Newtonsoft.Json", "6.0.6"]
            References =
                ["prismicio.dll"]
            Files = [
                    (libnet45@@"prismicio.dll", Some(libnet45), None)
            ]
            AccessKey = getBuildParamOrDefault "nugetkey" ""
            Publish = hasBuildParam "nugetkey" }) "prismic.nuspec"
)

Target "Default" (fun _ ->
    trace "specify target : Clean, BuildApp, CreatePrismicPackage"
    trace "CreatePrismicPackage can publish package be specifying a nugetkey parameter"
)


Target "CreatePackages" DoNothing

"Clean"
   ==> "AssemblyInfo"
       ==> "BuildApp"


"CreatePrismicPackage"
   ==> "CreatePackages"


RunTargetOrDefault "Default"
