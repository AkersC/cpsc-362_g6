# PIXEL RUN

---

### CPSC 362 - Group 6
### A video game for a software engineering project where you clear levels quickly while avoiding traps.

--- 

## TABLE OF CONTENTS
- [Project Description](#Project-Description)
- [Tech Stack](#Tech-Stack)
- [Features](#Features)
- [Directory Structure](#Directory-Structure)
- [Video Demo](#Video-Demo)

# Project Description
- This is a video game made in unity engine that is based on clearing levels quickly with the intent of trying multiple times to find the hidden traps and improve level clearing times.

# Tech Stack

| Technology | Purpose |
| --- | --- |
| Unity 6000.6.2f1 | Game engine and scene editor |
| C# | Gameplay, movement, traps, menus, and transitions |
| Unity 2D Physics | Rigidbody movement, collision detection, and triggers |
| Unity Tilemap and Tilemap Extras | Painting level geometry and reusable trap sections |
| Universal Render Pipeline 17.6.0 | Rendering with the project’s 2D renderer |
| Unity Input System 1.20.0 | Keyboard movement and jump actions |
| Unity UI and TextMesh Pro | Menus, buttons, and text |
| Git and GitHub | Source control and team collaboration |
| GitHub Actions / Super-Linter | C# lint checks on pushes and pull requests |


# Features 

- **Player movement:** Horizontal movement, grounded jumping, and stronger falling gravity.
- **Stationary traps:** Player contact with a trap trigger returns the player to the scene’s spawn point.
- **Moving traps:** Configurable travel offsets, speed, and pauses at each end of a path.
- **Disappearing floors:** Stepping on a tile removes its entire edge-connected section; diagonal-only neighbors stay separate.
- **Pause controls:** Pause, resume, and restart the current level.
- **Level endpoints:** Completion UI and a configurable next-scene destination.
- **Scene transitions:** An animated screen wipe when a transition controller is available.

# Directory Structure

Only the folders the team works in are shown. Every Unity asset also has a matching `.meta` file, which must be committed with it.

```
cpsc-362_g6/
├── .github/
│   └── workflows/
│       └── ci.yml              # GitHub Actions: Super-Linter for C# on every push and PR
├── Assets/                     # Everything in the game
│   ├── Animations/
│   │   └── Player/             # Idle, Run and Jump clips and the player's Animator Controller
│   ├── Art/
│   │   └── Environment/        # Kenney 1-Bit tilesheet (tiles and character sprites)
│   ├── Palettes/               # Tile Palette used to paint levels
│   ├── Prefabs/                # Reusable objects: Player and the level endpoint
│   ├── Scenes/                 # MainMenu, Gameplay (Level 1), Level2, Level3, and the UI (pause menu) prefab
│   ├── Scripts/                # All gameplay C# scripts (see table below)
│   │   └── Editor/             # Editor-only tools (e.g. the "Disappearing Floor" menu item)
│   ├── Settings/               # URP render pipeline and 2D renderer settings
│   ├── Tiles/                  # Tile assets painted into the tilemaps (ground, spikes)
│   ├── TextMesh Pro/           # TextMesh Pro fonts and shaders (Unity package content)
│   └── PlayerMovement.inputactions  # Input bindings for move and jump
├── Packages/                   # Unity package manifest (which packages the project uses)
└── ProjectSettings/            # Unity project settings, including the scene build order
```

# Video Demo
![Demo](readme/demo.gif)
