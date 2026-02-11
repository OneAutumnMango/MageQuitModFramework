# Introduction

The MageQuit Mod Framework is a lightweight, modular system for creating mods that modify spells, add custom UI, and extend game mechanics.

## What You Can Build

- **Spell Modifications** - Change damage, cooldowns, projectile speed, and more
- **Custom Game Modes** - Randomizers, balance patches, experimental features  
- **Debug Tools** - In-game testing and development utilities
- **UI Enhancements** - Custom menus and configuration interfaces

## Architecture

The framework runs as a BepInEx plugin and provides:

### Spell Modification System
Base × Mult pattern for modifying spell attributes. Changes stack additively, allowing multiple mods to coexist.

### Module System
Load/unload your mod features independently with full lifecycle management and Harmony patch integration.

### Game Data Initialization
Event-based hooks ensure your mods wait for game data to load before applying modifications.

### Dynamic Mod Menu
Press **F5** in-game to access all registered mod UIs in one unified menu.

## Why Use This Framework?

- **Clean API** - Simple, consistent interfaces for common tasks
- **Multi-mod Support** - Multiple mods can modify the same spells without conflicts
- **Easy Testing** - Load/unload modules without restarting the game
- **Helper Utilities** - Pre-built functions for IL patching, private field access, and more
- **Documentation** - Full API docs and examples
