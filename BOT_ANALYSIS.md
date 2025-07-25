# WoW Classic Grind Bot - Technical Analysis

## How the Bot Works (Step by Step)

### 1. **Data Collection Phase**
- **Addon Integration**: Uses a modified "Happy-Pixels" addon (`DataToColor`) that reads game state and outputs colored pixels
- **Screen Capture**: `WowScreenDXGI` captures the game screen using DirectX Graphics Infrastructure (DXGI)
- **Addon Reader**: `AddonReader` parses the colored pixels from the addon to extract game data (health, mana, position, targets, etc.)
- **Minimap Analysis**: `MinimapNodeFinder` analyzes yellow dots on minimap to detect NPCs/nodes

### 2. **World State Analysis** 
- **Player State**: Reads character stats, buffs, equipment, bag contents, position
- **Environment**: Detects nearby NPCs, their names, levels, and combat status
- **Combat Status**: Tracks whether in combat, auto-attacking, casting, etc.

### 3. **Decision Making (GOAP System)**
- **Goal-Oriented Action Planning**: `GoapPlanner` uses A* pathfinding to create action sequences
- **Available Goals**: 27+ different goals like `CombatGoal`, `LootGoal`, `FollowRouteGoal`, `ApproachTargetGoal`
- **World State Evaluation**: Checks preconditions and effects for each possible action
- **Cost-Based Planning**: Selects lowest-cost path to achieve desired outcome

### 4. **Action Execution**
- **Class Configuration**: Loads JSON configs (like `Hunter_30.json`) defining combat rotations, key bindings
- **Input Simulation**: `WowProcessInput` sends keyboard/mouse commands to WoW process
- **Combat Sequence**: Executes abilities based on requirements (mana %, health %, range, cooldowns)
- **Movement**: Uses pathfinding to navigate between locations

### 5. **Pathfinding & Navigation**
- **Multi-System Support**: V1 Local (PPather), V1 Remote (PathingAPI), V3 Remote (AmeisenNavigation)
- **Route Following**: `FollowRouteGoal` follows predefined grinding paths
- **Obstacle Avoidance**: Handles getting unstuck, corpse runs
- **Zone Transitions**: Manages travel between different game zones

### 6. **Resource Management**
- **Loot Collection**: `LootGoal` automatically loots corpses
- **Vendor Interaction**: Sells items, repairs equipment, buys consumables
- **Consumable Usage**: Food/drink when health/mana low
- **Pet Management**: Hunter-specific pet feeding, healing, summoning

## Detailed Component Analysis

### Core Architecture Flow
1. **BotController** (`Core/BotController.cs`) - Main orchestration
2. **GoapAgent** (`Core/GOAP/GoapAgent.cs`) - Decision making engine
3. **AddonReader** (`Core/Addon/AddonReader.cs`) - Game state parsing
4. **WowProcessInput** (`Game/Input/WowProcessInput.cs`) - Input simulation
5. **Various Goals** (`Core/Goals/`) - Individual bot behaviors

### GOAP (Goal-Oriented Action Planning) System
- **Planning Algorithm**: Uses A* search to find optimal action sequences
- **World State**: BitVector32 representing current game conditions
- **Preconditions**: Requirements that must be met before an action can execute
- **Effects**: Changes to world state after action completion
- **Cost-Based Selection**: Chooses lowest-cost path to achieve goals
- **Dynamic Replanning**: Recalculates plan when conditions change

### Input & Screen Capture Systems
- **DXGI Screen Capture**: Hardware-accelerated screen capture using DirectX
- **Pixel Parsing**: Extracts data from addon-rendered colored pixels
- **Input Simulation**: Uses Windows Native API for keyboard/mouse events
- **Anti-Detection**: Mimics human-like input timing and patterns

### Pathfinding Implementations
- **V1 Local (PPather)**: In-process pathfinding using MPQ files
- **V1 Remote (PathingAPI)**: Out-of-process C# pathfinding service
- **V3 Remote (AmeisenNavigation)**: C++ service using Recast/Detour with MMAP files
- **Dynamic Selection**: Automatically chooses available pathfinding service

### Class Configuration System
- **JSON-Based**: Combat rotations defined in JSON files per class/level
- **Requirement System**: Complex conditions for ability usage
- **Key Binding**: Maps abilities to keyboard keys
- **Sequence Types**: Pull, Combat, Adhoc, Parallel, NPC, Wait sequences
- **Dynamic Loading**: Can switch profiles at runtime

## Key Extension Opportunities

### **High-Impact Extensions**

#### 1. **Multi-Boxing Support**
- **Implementation Points**: 
  - Extend `Core/GOAP/GoapAgent.cs` for inter-bot communication
  - Add `MultiBoxCoordinator` service
  - Enhance `FollowFocusGoal` for party following
- **Features**:
  - Coordinate multiple bot instances
  - Party/raid formation and following
  - Shared resource management
  - Synchronized combat rotations

#### 2. **Advanced Combat AI**
- **Implementation Points**:
  - Extend `Core/Goals/CombatGoal.cs` 
  - Add ML models to `Core/ClassConfig/`
  - Create `CombatAnalyzer` component
- **Features**:
  - Dynamic ability priority based on encounter type
  - Interrupt prediction and counter-casting
  - Positioning optimization (stay at max range, avoid AoE)
  - Machine learning for combat optimization

#### 3. **Economic Bot Features**
- **Implementation Points**:
  - Add `Core/Goals/AuctionHouseGoal.cs`
  - Create `EconomicAnalyzer` service
  - Extend NPC interaction system
- **Features**:
  - Auction house integration
  - Market price analysis
  - Automated trading between characters
  - Resource hoarding/distribution

#### 4. **Quest System Integration**
- **Implementation Points**:
  - Add `Core/Goals/QuestGoal.cs`, `QuestPickupGoal.cs`, `QuestTurnInGoal.cs`
  - Create `QuestTracker` service
  - Extend pathfinding for quest objectives
- **Features**:
  - Automatic quest pickup/completion
  - Quest chain following
  - Turn-in optimization
  - Experience optimization routing

### **Medium-Impact Extensions**

#### 5. **Enhanced Gathering**
- **Implementation Points**:
  - Enhance `Core/Goals/ConsumeCorpseGoal.cs`
  - Add `NodeTracker` and `RespawnPredictor` services
  - Extend minimap node detection
- **Features**:
  - Node respawn timing prediction
  - Competitive gathering (avoiding other players)
  - Route optimization based on server population
  - Cross-zone gathering coordination

#### 6. **Dungeon/Instance Support**
- **Implementation Points**:
  - Enhance `Core/PPather/` for indoor navigation
  - Add `DungeonGoal.cs` and `BossFightGoal.cs`
  - Create role-specific combat behaviors
- **Features**:
  - Indoor pathfinding enhancement
  - Boss fight mechanics
  - Group role automation (tank/heal/dps)
  - Loot distribution

#### 7. **PvP Capabilities**
- **Implementation Points**:
  - Add `Core/Goals/PvPGoal.cs` and `FleeFromPlayerGoal.cs`
  - Enhance player detection in `MinimapNodeFinder`
  - Create PvP-specific combat rotations
- **Features**:
  - Player detection and avoidance
  - Basic PvP combat routines
  - Escape mechanisms
  - Safe zone retreating

#### 8. **Advanced Monitoring**
- **Implementation Points**:
  - Extend `Frontend/Pages/` with new dashboards
  - Add analytics services to `Core/`
  - Create performance tracking components
- **Features**:
  - Performance analytics dashboard
  - Profit/hour tracking
  - Risk assessment (GM detection)
  - Remote monitoring via web interface

### **Technical Infrastructure Extensions**

#### 9. **Machine Learning Integration**
- **Implementation Points**:
  - Add ML.NET or TensorFlow.NET packages
  - Create `Core/ML/` namespace for models
  - Integrate with GOAP system for behavior learning
- **Features**:
  - Behavior pattern learning
  - Anti-detection through human-like variation
  - Performance optimization
  - Predictive pathfinding

#### 10. **Cloud/Distributed Architecture**
- **Implementation Points**:
  - Create cloud services for shared intelligence
  - Add messaging system between bot instances
  - Implement centralized configuration management
- **Features**:
  - Cloud-based pathfinding service
  - Shared intelligence between bots
  - Configuration synchronization
  - Centralized monitoring

## Key Files for Extensions

### Core System Files
- **GOAP System**: `Core/GOAP/GoapAgent.cs`, `Core/GOAP/GoapPlanner.cs`
- **Goal Implementations**: `Core/Goals/` directory (27+ goal types)
- **Bot Controller**: `Core/BotController.cs` - Main orchestration
- **Class Configs**: `Json/class/` - Combat rotations and abilities  

### Input/Output Systems
- **Input System**: `Game/Input/WowProcessInput.cs` - Keyboard/mouse simulation
- **Screen Capture**: `Core/WoWScreen/WowScreenDXGI.cs` - DirectX screen capture
- **Addon Integration**: `Core/Addon/AddonReader.cs` - Parse game state from pixels

### Navigation Systems
- **Pathfinding**: `Core/PPather/LocalPathingApi.cs` - Navigation algorithms
- **Route Following**: `Core/Goals/FollowRouteGoal.cs` - Path execution
- **Minimap Analysis**: `Core/Minimap/MinimapNodeFinder.cs` - NPC detection

### User Interface
- **Web Interface**: `Frontend/Pages/` - Blazor components for monitoring/control
- **Configuration**: `BlazorServer/` - Web server for bot management
- **Headless Mode**: `HeadlessServer/` - Command-line operation

## Technical Architecture Strengths

### Modularity
- Dependency injection throughout (`Core/DependencyInjection.cs`)
- Service-oriented design with clear interfaces
- Goal-based behavior system allows easy addition of new behaviors
- JSON-based configuration for easy customization

### Performance
- Multi-threaded design with separate threads for:
  - Addon reading (200ms intervals)
  - Screenshot capture (200ms intervals) 
  - Remote pathfinding (500ms intervals)
  - GOAP decision making
- Hardware-accelerated screen capture via DXGI
- Efficient pixel parsing and caching

### Extensibility
- GOAP system naturally supports new goal types
- Plugin-like architecture for different pathfinding systems
- Class configuration system supports any WoW class/build
- Web interface can be extended with new monitoring pages

### Anti-Detection Features
- No memory reading or DLL injection
- Human-like input timing and patterns
- Configurable delays and randomization
- Screen-based detection (like a human player would see)

The bot's architecture is well-designed for extension while maintaining stability and performance. The GOAP system is particularly powerful for adding complex behaviors without breaking existing functionality.