# Chord-based Peer-to-Peer System Simulator
This program simulates a Chord-based peer-to-peer system, implementing the functionalities mentioned in the Chord paper. The key features include creating a network ring, dynamically adding nodes, performing scalable key lookups, and simulating key lookups with hop count tracking.

## System Overview
### Create Network Ring and Finger Tables
The create() function is responsible for creating the initial network ring with a specified number of nodes (numNodes). Each node in the network is associated with an integer key. The implementation should also include the creation of finger tables for each node.

### Dynamically Add Nodes
The join() function is responsible for dynamically adding nodes to the network. This function should update the finger tables with information about the new nodes that join the network.

### Scalable Key Lookup
Implement a function for scalable key lookup as described in Section 4 of the Chord paper.

### Simulator for Key Lookups
A simulator is provided to simulate key lookups in the peer-to-peer system. Each node performs a specified number of requests (numRequests). Count the number of hops required for each request made by every node, sum it up, and find the average number of hops using the formula:

```bash
Average number of hops = Sum of number of hops for all requests for all nodes / (numRequests * numNodes)
```
### Usage
The program is executed using the following command-line format:

```bash
dotnet run numNodes numRequests
```
<strong>numNodes:</strong> Number of peers to be created in the peer-to-peer system.<br>
<strong>numRequests:</strong> Number of requests each peer has to make.

Each peer sends a request per second, and the program exits when all peers have completed their specified number of requests.

Example Usage

```bash
dotnet run 10 100
This command will create a peer-to-peer system with 10 nodes, and each node will perform 100 requests.
```

### Notes
Ensure that you have the .NET runtime installed to execute the program (dotnet run).
The simulator provides insights into the performance of the Chord-based peer-to-peer system regarding key lookups and hop counts.
