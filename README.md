# NiGHTSImporter
Importing 3d models from the Sega Saturn game NiGHTS into Dreams into Unity3d

# Usage and info
- Insert your NiGHTS into Dreams (US final) into your CD-Rom drive or mount it as a virtual drive (from .iso or .cue)
- Open the project in Unity with the SampleScene
- Select the import object in the scene and setup your import parameters
- Start game in unity editor and wait for import to be finished
- Export models by right clicking the object in the scene and selecting "Export to FBX..."
- Textures will be outputed to the Textures folder (from root)
- A simple uv-mapped texture atlas named "ModelTexture.png" will be created for each level model objjects 

# Known problems and limitations
- color ram dump fles are used for color bank textures (should be replaced by reading the colors from the files)
- some textures might have wrong colors due to not all color ram dumps being provided in the repo
