#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.XamarinHelper

let buildDir = "samples/Samples/bin/Debug"

Target "Clean" (fun _ ->
    CleanDir buildDir
)

Target "Test" (fun _ ->
    trace "Testing stuff..."
)

Target "Build-Pcl" (fun _ ->
    RestorePackages()

    !! "samples/Samples/Samples.csproj"
        |> MSBuild "samples/Samples/bin/Debug" "Build"  [ ("Configuration", "Debug"); ("Platform", "Any CPU") ] 
        |> Log "---PCL build output---"
)

Target "Build-iOS" (fun _ ->
    iOSBuild (fun defaults -> 
        { 
            defaults with ProjectPath = "samples/iOS/Samples.iOS.csproj"
                          OutputPath = "samples/iOS/bin/iPhoneSimulator/Debug"
                          Configuration = "Debug|iPhoneSimulator"
                          Target = "Build"
        })
)

Target "Build-Droid" (fun _ ->
    !! "samples/Droid/Samples.Droid.csproj"
        |> MSBuild "samples/Droid/bin/Debug" "Build" [ ("Configuration", "Debug"); ("Platform", "Any CPU") ]
        |> Log "----Android build output----"
)

"Clean"
  ==> "Build-Pcl"
  ==> "Test"

"Clean"
  ==> "Build-Pcl"
  ==> "Build-iOS"
  ==> "Build-Droid"

RunTargetOrDefault "Test"
