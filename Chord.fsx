open System.Collections.Generic
open System

#r "nuget: Akka, 1.5.6"

open Akka.Actor

let m = 20
let mutable numberOfNodes = 0
let mutable numberOfRequests = 0

let create (actor) =
    printfn "Actor created %A" ,actor


let args = System.Environment.GetCommandLineArgs()

if args.Length >= 3 then // here the first argument is the File name 
    numberOfNodes <- int args.[2] // second argument is number of nodes
    numberOfRequests <- int args.[3] // third argument is number of requests that need to be done 
else
    printfn "Please provide two command-line arguments: NumberOfNodes and NumberOfRequests."


printfn "Number of Nodes: %d" numberOfNodes
printfn "Number of Requests: %d" numberOfRequests


// Function to generate a unique random number and update the set
let generateUniqueRandomNumber (maxValue: int) (integerSet: HashSet<int>) =
    let random = Random()
    let mutable randomNumber = 1

    // Generate a random number until it is not in the integerSet
    while integerSet.Contains(randomNumber) do
        randomNumber <- random.Next(1, maxValue + 1)
    
    // Add the generated number to the integerSet
    integerSet.Add(randomNumber)

    randomNumber

// Example usage:
let maxValue = int32 (pown 2 20 - 1)

let mutableSet = HashSet<int>()

for _ in 1..numberOfNodes do
    let number = generateUniqueRandomNumber maxValue mutableSet
    printfn "Generated number: %d" number
