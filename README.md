# Procedural terrain generation
### Usage
1. Donwload the repository or clone using `git clone https://github.com/pecarprimoz/procedural-gen-dipl.git` 
2. Download [Unity 2018.2.18f 
(PC)](https://unity3d.com/get-unity/download?thank-you=update&download_nid=59169&os=Win) / [Unity 2018.2.18f 
(MAC)](https://unity3d.com/get-unity/download?thank-you=update&download_nid=59169&os=Mac)
3. Open the project in Unity, wait for the project generation.
4. Set the starting parameters in the prefab `ProceduralTerrain`, trough the script `Terrain Generation`. You 
can also edit these properties when the project is running.
##### Optional
`DebugTerrainCamera` displays the heightmap on the `DebugPlane`, if you want to see the terrain from an 
orthographic perspective, turn on `CameraTerrain`.
##### Known issues
Prefabs breaking ? Download the correct Unity version.
Terraing painting isn't working currenlty, need to figure out splat mapping.
##### Contact
If you have any questions, feel free to email me at `pecar.primoz96@gmail.com`
### Examples WIP
##### Thermal erosion at 50 iterations
![Example1](https://raw.githubusercontent.com/pecarprimoz/procedural-gen-dipl/master/Screens/ex1.png)
##### Thermal erosion at 50 iterations, orthographic
![Example2](https://raw.githubusercontent.com/pecarprimoz/procedural-gen-dipl/master/Screens/ex2.png)
##### Hydraulic erosion at 200 iterations
![Example3](https://raw.githubusercontent.com/pecarprimoz/procedural-gen-dipl/master/Screens/ex3.png)