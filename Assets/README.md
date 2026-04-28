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
6. Laser simulates
7. If beam satisfies puzzle goals -> level solved

----------------------------------

## Current MVP Rules

Board:
- 5x5 grid

Pieces:
- Entry
- Target
- Block
- Reflect

Piece Sources:
- placedPieces = fixed puzzle setup
- inventoryPieces = draggable pieces

Interaction:
- Inventory -> board drag
- Board piece move
- Board piece return to inventory
- Rotate movable pieces
- Laser updates after board changes

Win:
- Laser reaches target
- All required pieces are hit by beam

Fail:
- No fail state.
- Player remains in puzzle until solved.

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
- triggers simulation
- listens to board changes

LaserSimulationService
- pure beam logic

LaserView
- beam rendering only

InventoryBarUI
- inventory tray

BoardPiece
- piece runtime behavior

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
- required

Inventory Pieces
- piece type
- direction
- required

----------------------------------

## Current Piece Runtime Permissions

Pieces use runtime permissions:

CanMove
CanRotate
CanReturnToInventory

Placed pieces:
false false false

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

## Reflection Rules

Current reflect piece uses two mirror states:

/ mirror
\ mirror

determined by rotation.

Can be extended later.

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

----------------------------------

## Next Planned Milestones

Priority order:

1. Manual fire mode
2. Auto-fire toggle
3. Hold-preview mode
4. Solve UI
5. Level progression UI

Then:

6. Splitter piece
7. Portal piece
8. More puzzle mechanics
9. Level editor tooling

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

Every new piece must define:

1. Beam entry behavior
2. Beam output behavior
3. Rotatable?
4. Movable?
5. Affects win condition?

----------------------------------

## Tech Notes

Requires:
- EventSystem
- Physics2DRaycaster on camera
- LineRenderer for laser
- ScriptableObject levels

----------------------------------

## Author

Lior Tzabari
Master's Final Project
Game Design