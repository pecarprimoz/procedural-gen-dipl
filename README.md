# Procedural terrain generation
### Usage
1. Donwload the repository or clone using `git clone https://github.com/pecarprimoz/procedural-gen-dipl.git` 
2. Download the latest (stable) release of Unity.
3. Open the project in Unity, wait for the project generation.
4. Run the project, select the `ProceduralTerrain` prefab, then set the parameters trough the script `Terrain Generation` (the script auto updates when you 
run the game).
##### Additional info
You might want to debug moisture, temperature and height maps. To do that, select `TerrainDebugPlane` and set the `Plane Content` to the preffered map (kAll works with color, displays the biomes!).
If you want to have a biome map, you need to load one of the presets (this is due to older presets having wrong parameter boundaries for biomes).
##### Optional
`DebugTerrainCamera` displays the heightmap on the `DebugPlane`, if you want to see the terrain from an 
orthographic perspective, turn on `CameraTerrain`.
##### Known issues
Prefabs breaking ? Download the correct Unity version.
##### Contact
If you have any questions, feel free to email me at `pecar.primoz96@gmail.com`
### Examples WIP
##### Latest terrain generation, with biomes
![Example1](https://raw.githubusercontent.com/pecarprimoz/procedural-gen-dipl/master/Screens/wip1.png)
##### Height, moisture and temperature maps used for terrain and biome generation
![Example2](https://raw.githubusercontent.com/pecarprimoz/procedural-gen-dipl/master/Screens/height_moist_temp.png)
##### 1024x1024x64 terrain map, 250 erosion iterations, runtime texturing and object placement, less than 2 min
![Example3](https://raw.githubusercontent.com/pecarprimoz/procedural-gen-dipl/master/Screens/wip3.png)
##### 512x512x64 terrain map, 250 erosion iterations, ~5k placed objects, grass patches on 4 biomes, rock patches on 1 biome, 50k detail draw distance, ~10s total generation time
![Example4](https://raw.githubusercontent.com/pecarprimoz/procedural-gen-dipl/master/Screens/wip4.png)