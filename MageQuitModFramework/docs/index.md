---
_layout: landing
---

# MageQuit Mod Framework

A lightweight, modular framework for creating MageQuit mods with BepInEx. Provides structured systems for spell modifications, module lifecycle management, UI integration, and game data access.

## What is This?

MageQuit Mod Framework simplifies mod development for the game MageQuit by providing:

- **Spell Modification System** - Centralized modifier registration with Base × Multiplier pattern
- **Module System** - Structured lifecycle hooks (Initialize, InitializeAfterGameData)
- **Game Data Access** - Event-driven system for safe access to game configuration
- **UI Components** - Dynamic mod menu and reusable UI building blocks
- **Helper Utilities** - Common operations for game state and element manipulation

The framework handles the complexity of Harmony patching, initialization timing, and mod coordination so you can focus on implementing your mod's unique features.

## Installation

### For Mod Developers

1. **Clone or download** this repository
2. **Build the framework:**
   ```bash
   dotnet build MageQuitModFramework/MageQuitModFramework.csproj
   ```
3. **Copy** `bin/Debug/net472/MageQuitModFramework.dll` to your MageQuit `BepInEx/plugins/` folder
4. **Reference** the framework DLL in your mod project

### For Mod Users

1. Install BepInEx for MageQuit (if not already installed)
2. Download `MageQuitModFramework.dll` from releases
3. Place it in `MageQuit/BepInEx/plugins/`
4. Install any mods that depend on this framework

## Project Structure

- **MageQuitModFramework/** - Core framework library
  - `src/Data/` - Game data initialization and access
  - `src/Modding/` - Module system and registration
  - `src/Spells/` - Spell modification system
  - `src/UI/` - Dynamic menu and UI components
  - `src/Utilities/` - Helper functions
- **ExampleMod/** - Sample mod demonstrating framework usage
- **MageQuitModFramework.Tests/** - Unit tests

## Requirements

- .NET Framework 4.7.2
- BepInEx 5.x
- HarmonyX (included with BepInEx)
- MageQuit game

## License

This project is open source. See LICENSE file for details.