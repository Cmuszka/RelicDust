# Hiperborea: Relic Dust — Codex Context

## Current goal
Second partial: evolve the prototype toward a vertical slice. Focus on playability, system depth, agency, feedback loops, and polish.

## Current design direction
Fast-paced 2D space combat with inertia-based movement, inspired by Luftrausers and The Expanse. The player pilots a configurable Black Fleet combat ship.

## Core systems to implement next
- Modular ship loadout screen
- Shared ship system framework for player and enemies
- Primary weapons: Heavy Cannon
- Secondary weapons: CIWS / machine gun
- Utility systems: Shield, Afterburner
- Targeting system shared by player/enemies

## Architecture principle
Player and enemy ships should use the same modular systems:
- Ship
- ShipMovement
- ShipHealth
- TargetingSystem
- WeaponSystem base class
- UtilitySystem base class

## Design rules
- Everything is a ship system, not a generic ability.
- Few systems, clear roles.
- Weapons and utilities should have trade-offs.
- Enemies should feel like other pilots using similar ship systems.
- Avoid overbuilding armor, damage types, subsystem failures, and roguelite progression for now.

## Future ideas
- Armor / shield / hull layers
- Kinetic, energy, and magic damage types
- Grappling hook
- Decoys
- Missile systems
- Targeting computers