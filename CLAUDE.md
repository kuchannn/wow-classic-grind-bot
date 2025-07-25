# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Master Of Puppets is a World of Warcraft Classic grinding bot that supports Season of Mastery Classic, Burning Crusade Classic, Wrath of the Lich King Classic, and Cataclysm Classic. The bot uses screen capture and input simulation without memory tampering or DLL injection.

## Architecture

The project follows a modular C# architecture with the following key components:

- **Core**: Main bot logic, GOAP (Goal-Oriented Action Planning) system, combat handlers, navigation
- **BlazorServer**: Web UI for monitoring and configuration 
- **HeadlessServer**: Command-line version for production use
- **Frontend**: Razor components for the web interface
- **Game**: Input simulation and screen capture
- **Addon**: Modified Happy-Pixels addon for reading game state
- **PPather**: Local pathfinding implementation
- **PathingAPI**: Remote pathfinding service

## Build and Development Commands

### Building the Solution
```bash
# Build entire solution
dotnet build MasterOfPuppets.sln

# Build specific configuration
dotnet build -c Release
dotnet build -c Debug
```

### Running Applications
```bash
# Run BlazorServer (web UI)
cd BlazorServer
dotnet run -c Release

# Run HeadlessServer (command-line)  
cd HeadlessServer
dotnet run -c Release

# Run PathingAPI
cd PathingAPI
dotnet run
```

### Batch Files
- `BlazorServer/run.bat`: Starts web UI and opens Chrome
- `HeadlessServer/run.bat`: Runs headless version
- `PathingAPI/run.bat`: Starts pathfinding API

### Testing
```bash
# Run tests
dotnet test CoreTests/CoreTests.csproj

# Run benchmarks
dotnet run --project Benchmarks/Benchmarks.csproj -c Release
```

## Key Technical Details

### Pathfinding Systems
The bot supports multiple pathfinding approaches:
- **V3 Remote**: AmeisenNavigation (C++, uses .mmap files)
- **V1 Remote**: PathingAPI (C#, uses .mpq files)  
- **V1 Local**: In-process PPather (C#, uses .mpq files)

### GOAP System
Uses Goal-Oriented Action Planning for decision making. Goals are in `Core/Goals/` and the planner is in `Core/GOAP/`.

### Configuration
- Class configurations: `Json/class/` - Combat rotations and abilities per class
- Path configurations: `Json/path/` - Grinding routes and vendor paths
- Area data: `Json/area/` - Zone information
- Data configuration: `DataConfig/` - External data source locations

### Input System
- Uses `Game/Input/` for mouse/keyboard simulation
- Supports modifier keys and complex key combinations
- No memory injection - purely input-based

### Screen Processing
- Screen capture via `Core/WoWScreen/`
- NPC name detection via `Core/Minimap/MinimapNodeFinder.cs`
- Cursor classification for interaction feedback

## Important Files and Directories

- `Core/BotController.cs` - Main bot orchestration
- `Core/GOAP/GoapAgent.cs` - Decision making engine
- `Core/Goals/` - Individual bot behaviors
- `Core/ClassConfig/` - Combat configuration system
- `Json/` - All configuration files
- `Addons/DataToColor/` - WoW addon for game state reading

## Development Notes

- Uses .NET 9.0 with centralized package management
- Supports x86, x64, and AnyCPU platforms
- Heavy use of dependency injection via Microsoft.Extensions
- Logging via Serilog
- Real-time UI updates via SignalR

## Cursor Rules Integration

The project includes `.cursorrules` with specific coding guidelines:
- File-by-file changes with verification opportunities
- No whitespace suggestions or unnecessary summaries
- Preserve existing code structures
- Use explicit variable names
- Prioritize performance and security
- Include proper error handling and tests