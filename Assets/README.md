# Laser Puzzle (Working Title)

A portrait mobile puzzle game built in Unity, inspired by Laser Maze principles,
adapted into an original casual level-based game.

Master’s degree final project in Game Design.

Current focus:
- Strong core mechanics
- Scalable Unity architecture
- Puzzle readability
- Deterministic beam simulation
- Mobile-first interaction

----------------------------------

## Core Loop

Player:

1. Opens a level
2. Sees a 5x5 puzzle board
3. Some pieces are fixed on the board
4. Additional pieces begin in inventory tray
5. Player drags, places, moves, returns, and rotates pieces
6. Player presses Fire
7. Laser simulates
8. Failed attempts consume a laser try
9. Puzzle is solved or failed

----------------------------------

## Current MVP Rules

Board:
- 5x5 grid

Pieces:
- Entry
- Target
- Block
- Mirror
- Reflect (triangle reflector)
- Checkpoint
- Portal

Piece Sources:
- placedPieces = fixed puzzle setup
- inventoryPieces = draggable pieces

Interaction:
- Inventory -> board drag
- Board piece move
- Board piece return to inventory
- Rotate allowed pieces
- Manual laser fire button
- Rotatable pieces display interaction indicators
- Inventory pieces remain visible while used on board

Win:
- Laser reaches target
- All placed board pieces are hit by the beam
- All inventory pieces must be used

Fail:
- Timer reaches sunrise
OR
- Player runs out of laser tries

----------------------------------

## Architecture

Main Systems

GameManager
- solve validation
- level progression
- inventory return handling

LevelManager
- loads levels
- pushes data into board + inventory

BoardManager
- board state
- cell occupancy
- piece placement/movement
- board changed events

LaserControlManager
- manual laser firing
- laser clearing
- stores latest simulation result

LaserSimulationService
- pure beam logic
- behavior-driven simulation pipeline

LaserView
- beam rendering only

InventoryBarUI
- inventory tray
- used-piece visual state

BoardPiece
- lightweight runtime piece container
- stores runtime state only

BoardPieceDragHandler
- drag pieces on board
- return pieces to inventory

----------------------------------

## Project Structure

Scripts/
  Core/
  Grid/
  Pieces/
  Laser/
  Input/
  Level/
  UI/

Prefabs/
  CellPrefab
  BoardPiecePrefab
  InventoryPieceUI

ScriptableObjects/
  Levels/

----------------------------------

## Core Data

LevelData
Contains:

Placed Pieces
- piece type
- position
- direction
- rotation permission
- portal pair id (optional)

Inventory Pieces
- piece type
- portal pair id (optional)

----------------------------------

## Current Piece Runtime Permissions

Pieces use runtime permissions:

CanMove
CanRotate
CanReturnToInventory

Placed pieces:
Configured per level

Inventory placed pieces:
true true true

No "isFixed" flag used.

----------------------------------

## Laser System

Simulation handles:

- straight propagation
- reflection
- target hit
- block collision
- out-of-bounds exit
- loop prevention

Separated from visuals.

Simulation:
LaserSimulationService

Rendering:
LaserView

----------------------------------

## Piece Behavior Architecture

Beam interaction is modular and behavior-driven.

Each puzzle piece owns its own beam logic through a dedicated behavior class.

Examples:
- MirrorBeamBehavior
- ReflectBeamBehavior
- PortalBeamBehavior
- CheckpointBeamBehavior

LaserSimulationService no longer contains a large piece-type switch statement.

Instead:
- PieceBehaviorRegistry resolves the correct behavior
- Each behavior returns a BeamInteractionResult
- Simulation remains deterministic and extensible

This allows adding new puzzle mechanics without modifying simulation core logic.

----------------------------------

## Current Piece Behaviors

Mirror
- Reflects from both sides

Reflect
- Triangle reflector
- Reflects only from diagonal edge
- Other sides block

Checkpoint
- Allows beam only through matching axis

Portal
- Teleports beam to paired portal
- Only accepts beam from configured entrance direction

Block
- Stops beam

Target
- Ends simulation successfully

----------------------------------

## Visual Beam

Current beam:
LineRenderer based

Supports:
- material swap
- texture swap
- animated beam later
- impact effects later

Beam visuals should stay independent from simulation.

----------------------------------

## Interaction Readability

Rotatable pieces display animated interaction indicators.

Inventory pieces:
- remain visible while placed on board
- fade while currently in use
- restore when returned to inventory

Dragging creates temporary visual drag ghosts.

This improves readability and board state clarity.

----------------------------------

## Puzzle Pressure Systems

Current lose systems:

1. Sunrise timer
2. Limited laser tries

Laser tries are represented visually as life icons.

Incorrect laser activations consume tries.
Successful solutions do not consume tries.

----------------------------------

## UI Menu Architecture

Menus inherit from a shared BaseMenuUI class.

Current menus:
- PauseMenuUI
- GameWinPanelUI

Shared functionality:
- panel visibility
- menu state
- restart flow
- main menu navigation

Specialized behavior remains isolated per menu type.

----------------------------------

## Current Completed Milestones

[x] 5x5 board system
[x] Piece spawning
[x] Rotation
[x] Beam simulation
[x] Inventory tray
[x] Drag placement
[x] Move pieces on board
[x] Return pieces to inventory
[x] Independent level loading
[x] Solve checking
[x] Manual laser fire
[x] Laser tries system
[x] Timer lose condition
[x] Pause menu
[x] Level progression
[x] Piece behavior architecture refactor
[x] Triangle reflect logic
[x] Portal system
[x] Checkpoint system
[x] Rotatable piece indicators
[x] Persistent inventory slots

----------------------------------

## Next Planned Milestones

Priority order:

1. Auto-fire toggle
2. Hold-preview mode
3. Multi-beam simulation
4. Splitter piece
5. Beam color mechanics
6. Better solve/fail presentation
7. Audio + feedback polish
8. Level editor tooling

----------------------------------

## Design Rules

Always prefer:
- Deterministic logic
- Data driven levels
- Separation logic / visuals
- Expandable systems
- MVP before complexity

Avoid:
- Physics beam systems
- Monolithic scripts
- UI mixed into logic
- Premature polish

----------------------------------

## Adding New Pieces

Workflow:

1. Add new PieceType
2. Create a new PieceBeamBehavior class
3. Register behavior in PieceBehaviorRegistry
4. Add sprite to PieceSpriteLibrary
5. Create levels using the new piece

LaserSimulationService should remain unchanged for most new mechanics.

----------------------------------

## Tech Notes

Requires:
- EventSystem
- Physics2DRaycaster on camera
- LineRenderer for laser
- ScriptableObject levels

----------------------------------

## Author

Idan Barzzellai | Danielle Franzes | Maor Astrizki
Master's Final Project
Game Design