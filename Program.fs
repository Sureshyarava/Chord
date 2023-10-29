open System
open System.Diagnostics



let runScriptWithInput numberOfNodes numberOfRequests =
    let scriptPath = "./Chord.fsx" // Replace with the actual path to your FSX script
    let arguments = sprintf "%d %d" numberOfNodes numberOfRequests
    let startInfo = new ProcessStartInfo("dotnet", sprintf "fsi %s %s" scriptPath arguments)
    startInfo.RedirectStandardInput <- true
    let process = new Process()
    process.StartInfo <- startInfo
    process.Start()
    process.StandardInput.WriteLine(arguments)
    process.WaitForExit()


let processInput numberOfNodes numberOfRequests =
    // Your logic to process the input goes here
    runScriptWithInput numberOfNodes numberOfRequests

[<EntryPoint>]
let main argv =
    if argv.Length = 2 then
        let numberOfNodes, numberOfRequests = 
            try
                int argv.[0], int argv.[1] // Get the first and second command-line arguments as integers
            with
            | :? System.FormatException ->
                printfn "Invalid input. Please enter valid integers for NumberOfNodes and NumberOfRequests."
                0,0 // Set the exit code to 1 to indicate an error
        processInput numberOfNodes numberOfRequests
    else
        printfn "Please provide two command-line arguments: NumberOfNodes and NumberOfRequests."

    0 // Return an integer exit code (0 means success)