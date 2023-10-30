open System.Collections.Generic
open System
open System.Threading
open System.Threading.Tasks
open System.Threading.Channels
open System.Collections.Generic

#r "nuget: Akka, 1.5.13"

open Akka.Actor

// Function to generate a unique random number and update the set
let generateUniqueRandomNumber (maxValue: int) (mutableSet: HashSet<int>) =
    let random = Random()
    let mutable randomNumber = random.Next(1, maxValue + 1)

    // Generate a random number until it is not in the integerSet
    while mutableSet.Contains(randomNumber) do
        randomNumber <- random.Next(1, maxValue + 1)
    // Add the generated number to the integerSet
    mutableSet.Add(randomNumber) |> ignore

    randomNumber

let m = 20
let maxvalue = int32 (pown 2 m - 1) // max range of values allowed to give for a node

let mutableSet = HashSet<int>() // Used to store and tell if the random number is already been used

let myList = LinkedList<int>() //Used to store Node keys
let mutable numberOfNodes = 0
let mutable numberOfRequests = 0

let args = System.Environment.GetCommandLineArgs()
if args.Length >= 3 then // The first argument is the File name 
    numberOfNodes <- int args.[2] // The second argument is the number of nodes
    numberOfRequests <- int args.[3] // The third argument is the number of requests that need to be done 
else
    printfn "Please provide two command-line arguments: NumberOfNodes and NumberOfRequests."

// Create a CountdownEvent to coordinate starting and waiting for requests
let requestsCountdown = CountdownEvent(numberOfNodes)

type CircularListNode =
    { ID: string
      FingerTable: Dictionary<int, int>
      Data: int
      mutable Next: CircularListNode option }

// Define a global variable of type CircularListNode
let mutable globalHead : int = -1

// Dictionary to store circular linked list nodes
let mutable nodeList = Dictionary<int, CircularListNode>()

let mutable nodeData = List<int>()
// Function to create a circular linked list node
let create (actor: String) (data: int) =
    // Create a circular linked list node with the actor's name as ID
    let node = { ID = actor; FingerTable = new Dictionary<int, int>(); Data = data; Next = None }
    // Add the node to the dictionary with its ID as the key
    nodeList.[node.Data] <- node
    globalHead <- node.Data

// Function to join a new node to the circular linked list
let join (name: string) (data: int) =

    let newNode =
        {
            ID = name
            FingerTable = new Dictionary<int32, int32>()
            Data = data
            Next = None
        }
    nodeList.[newNode.Data] <- newNode 
    let keyValueSeq = nodeList |> Seq.cast<KeyValuePair<int, CircularListNode>>
    let sortedSeq = Seq.sortBy (fun (kvp : KeyValuePair<int, CircularListNode>) -> kvp.Key) keyValueSeq
    let nodeData1 = List(Dictionary(sortedSeq).Keys)
    nodeData <- nodeData1
    for i=1 to nodeData.Count-1 do
        nodeList[nodeData1.[i-1]].Next <- Some nodeList[nodeData1.[i]]
    nodeList[nodeData1.[nodeData1.Count-1]].Next <- Some nodeList[nodeData1.[0]]
    globalHead <- nodeList[nodeData1.[0]].Data

type MyActor(requestsCountdown: CountdownEvent) =
    inherit ReceiveActor()

    override this.PreStart() =
        let selfRef = this.Self

        async {
            // Start the request loop only after all actors are created
            let mutable counter = 0
            while counter < numberOfRequests do
                do! Async.Sleep(1000) // Sleep for 1 second
                printfn "%s is requesting a function call: %d" (selfRef.Path.Name) counter
                counter <- counter + 1

            printfn "%s has finished all requests" (selfRef.Path.Name)
            if requestsCountdown.Signal() then
                printfn "All actors have finished their requests."
                Environment.Exit(0)
            } |> Async.Start

// Function to create an actor with a custom name
let createActor (actorSystem: ActorSystem) (name: string) =
    actorSystem.ActorOf(Props.Create(fun () -> MyActor(requestsCountdown)), name)

let getSuccesor (key:int32) =
    let mutable successor = -1
    for k = 0 to nodeData.Count-2 do
        if key > nodeData.[k] && key <= nodeData.[k+1] && successor = -1 then
            successor <- nodeData.[k+1]
    if successor = -1 then
        successor <- nodeData.[0]
    successor

let CreateFingerTable(id:int) =
    for k=1 to m do
        let key1= id+int32 (pown 2 (k - 1))
        let value1 = getSuccesor (key1 % (int32 (pown 2 m))) 
        nodeList.[id].FingerTable.Add(key1,value1)
    printfn "Finger Table for id %d is %A" id nodeList.[id].FingerTable

let system = 
    ActorSystem.Create("MySystem")

for i in 1 .. numberOfNodes do
    let uniqueNumber = generateUniqueRandomNumber maxvalue mutableSet    // Create actors with custom names
    let actorName = string uniqueNumber
    createActor system actorName |> ignore
    if i = 1 then
        create actorName uniqueNumber |> ignore
    else
        join actorName uniqueNumber |> ignore

printfn "After adding all the nodes in the CHORD Ring"
for k in nodeData do
    CreateFingerTable k
// Wait for all actors to finish their requests
requestsCountdown.Wait()
