#r "packages/FAKE/tools/FakeLib.dll"
open Fake
open Fake.XamarinHelper
open System.IO

//Build Directories:
let pclDir = "src/JudoDotNetXamarin/bin/Debug"
let iOSSDKDir = "src/JudoDotNetXamariniOSSDK/bin/Debug"
let droidSDKDir = "src/JudoDotNetXamarinAndroidSDK/bin/Debug"

let intTestDir = "src/JudoDotNetXamariniOS.Tests/bin/Debug"
let uiTestDir = "src/JudoXamarin.UiTests/bin/Debug"

let packageDir = "pack/"
let outputDir = "build/"

//Restore Nuget Packages and Clean
Target "Restore Packages" (fun _ ->
    RestorePackages()
)

//Build SDKs
Target "Build SDK" (fun _ ->
    !! "src/JudoDotNetXamarin/JudoDotNetXamarin.csproj"
        |> MSBuild "src/JudoDotNetXamarin/bin/Debug" "Build" [ ("Configuration", "Debug"); ("Platform", "Any CPU") ]
        |> Log "---Core build output...---"
)

Target "Build iOS SDK" (fun _ ->
    iOSBuild (fun defaults ->
        {
            defaults with ProjectPath = "src/JudoDotNetXamariniOSSDK/JudoXamarin.iOS.csproj"
                          OutputPath = "src/JudoDotNetXamariniOSSDK/bin/Debug"
                          BuildIpa = false
                          Configuration = "Debug"
                          Target = "Build"
        }) 
)

Target "Build Droid SDK" (fun _ ->
    !! "src/JudoDotNetXamarinAndroidSDK/JudoDotNetXamarinAndroidSDK.csproj"
        |> MSBuild "src/JudoDotNetXamarinAndroidSDK/bin/Debug" "Build" [ ("Configuration", "Debug"); ("Platform", "Any CPU") ]
        |> Log "----Android build output----"
)

//Build Test Projects
Target "Build Integration Test App" (fun _ -> 
    iOSBuild (fun defaults ->
        {
            defaults with ProjectPath = "src/JudoDotNetXamariniOS.Tests/JudoXamarin.IntegrationTests.csproj"
                          OutputPath = "src/JudoDotNetXamariniOS.Tests/bin/Debug"
                          BuildIpa = true
                          Configuration = "Debug"
                          Target = "Build"
        }) 
)

Target "Build UI Tests" (fun _ ->
    !! "JudoXamarin.UiTests/JudoXamarin.UiTests.csproj"
        |> MSBuild "src/JudoXamarin.UiTests/bin/Debug" "Build" [ ("Configuration", "Debug"); ("Platform", "Any CPU") ]
        |> Log "----UI Test build output----"
)


//Build Order
"Restore Packages"    
    ==> "Build SDK"    
    ==> "Build iOS SDK"
    ==> "Build Droid SDK"
    ==> "Build Integration Test App"
    ==> "Build UI Tests"

RunTargetOrDefault "Build UI Tests"
