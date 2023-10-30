open System.Collections.Generic
open System
open System.Threading
open System.Threading.Tasks
open System.Threading.Channels
open System.Collections.Generic

#r "nuget: Akka"

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

let keyList =List<int>()

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

type chord_Node =
    { ID: IActorRef
      FingerTable: Dictionary<int, int>
      Data: int
      mutable Next: chord_Node option }

// Define a global variable of type CircularListNode
let mutable globalHead : int = -1

// Dictionary to store circular linked list nodes
let mutable nodeList = Dictionary<int, chord_Node>()

let mutable nodeData = List<int>()
// Function to create a circular linked list node
let create (actor: IActorRef) (data: int) =
    // Create a circular linked list node with the actor's name as ID
    let node = { ID = actor; FingerTable = new Dictionary<int, int>(); Data = data; Next = None }
    // Add the node to the dictionary with its ID as the key
    nodeList.[node.Data] <- node
    node.Next <- Some node
    globalHead <- node.Data
    nodeData <- List<int> nodeList.Keys

// Function to join a new node to the circular linked list
let join (name: IActorRef) (data: int) =

    let newNode =
        {
            ID = name
            FingerTable = new Dictionary<int32, int32>()
            Data = data
            Next = None
        }
    nodeList.[newNode.Data] <- newNode 
    if data < nodeData[0] || data > nodeData[nodeData.Count-1] then
        let temp=nodeList[nodeData[nodeData.Count-1]].Next
        newNode.Next<-temp
        nodeList[nodeData[nodeData.Count-1]].Next<- Some newNode
        if data < nodeData[0] then
            nodeData.Insert(0,data)
        else
            nodeData.Add(data)
    else
        let mutable temp1=true
        for i=1 to nodeData.Count-1 do
            if temp1 && nodeData[i-1]<data && data <nodeData[i] then
                let temp= nodeList[nodeData[i-1]].Next
                newNode.Next<-temp
                nodeList[nodeData[i-1]].Next <- Some newNode
                nodeData.Insert(i,data)
                temp1 <- false

let calculateNumberOfHops (node1: int)(key : int) =
    let hops=0.1
    hops

type MyActor(requestsCountdown: CountdownEvent) =
    inherit ReceiveActor()

    override this.PreStart() =
        let selfRef = this.Self

        async {
            // Start the request loop only after all actors are created
            let mutable counter = 0
            while counter < numberOfRequests do
                do! Async.Sleep(1000) // Sleep for 1 second
                let nodeValue = int selfRef.Path.Name
                let key=calculateNumberOfHops nodeValue keyList[counter]
                printfn "number of hops from %s %f is " (selfRef.Path.Name)  key 
                counter <- counter + 1

            printfn "%s has finished all requests" (selfRef.Path.Name)
            if requestsCountdown.Signal() then
                printfn "All actors have finished their requests."
                Environment.Exit(0)
            } |> Async.Start

// Function to create an actor with a custom name
let createActor (actorSystem: ActorSystem) (name: string) =
    let actorref = actorSystem.ActorOf(Props.Create(fun () -> MyActor(requestsCountdown)), name)
    actorref

for i=1 to numberOfRequests do
    let key=generateUniqueRandomNumber maxvalue mutableSet
    keyList.Add(key)

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

let system = 
    ActorSystem.Create("MySystem")

for i in 1 .. numberOfNodes do
    let uniqueNumber = generateUniqueRandomNumber maxvalue mutableSet    // Create actors with custom names
    let actorName = string uniqueNumber
    let actor = createActor system actorName
    if i = 1 then
        create actor uniqueNumber 
    else
        join actor uniqueNumber 

for k in nodeData do
    CreateFingerTable k

printfn "The NodeList is %A" nodeList
// Wait for all actors to finish their requests
requestsCountdown.Wait()