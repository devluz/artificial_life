# First try

1. Open the project in Unity 2018.4 LTS
2. Open scene Scenes/map
3. Press start button. The simulation will start using a preset exampe
4. Click on "world" in the hierachy  
5. Change "Time Factor" to a large value to speed things up (don't go to far up immediately it might reduce the FPS to < 1)
6. Watch the simulation
7. You will slow the capsules slowly turning blue (better view distance) / red (higher speed)
8. After ~100 generations they will turn ping as they will both have high view distance and speed. There is no cost to this yet so high speed & high view distance always wins (at some point they there is no benifit to inrease the view distance anymore and the speed actually might cause them to fall off the platform)

# Current setting & how to customize them
* Food: You can change the number of food values via the "world" Game object
* Mutation / selection: Done in C# via Simulation.cs in the method EndRound 
* Movement / behaviour of the Artificial Life: Done via ArtificialLife.cs in SimulationStep and OnCollisionEnter
