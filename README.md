# Counter-Strike Style Game Development Project

A comprehensive guide and codebase for creating a tactical first-person shooter game similar to Counter-Strike using Unity 3D.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Project Structure](#project-structure)
- [Development Phases](#development-phases)
- [Quick Start Guide](#quick-start-guide)
- [Documentation](#documentation)
- [Contributing](#contributing)
- [Legal Notice](#legal-notice)

## 🎮 Overview

This project provides a complete step-by-step guide and implementation for creating a Counter-Strike style tactical shooter. The game features team-based combat, round-based gameplay, economy system, and realistic weapon mechanics.

### Game Features

- **Team-based Gameplay**: Terrorists vs Counter-Terrorists
- **Objective-based Rounds**: Bomb defusal and elimination modes
- **Economy System**: Buy weapons and equipment between rounds
- **Realistic Weapon Mechanics**: Recoil patterns, damage falloff, and accuracy systems
- **Multiplayer Networking**: Support for up to 10 players
- **Advanced Movement**: Counter-Strike style movement with bunny hopping prevention
- **Audio System**: 3D positional audio and dynamic music
- **Spectator Mode**: Free camera and player following

## 🔧 Requirements

### Software Requirements
- **Unity 2022.3 LTS** or newer
- **Visual Studio 2022** (Windows) or **Visual Studio Code** (Cross-platform)
- **Git** with Git LFS support

### Hardware Requirements
- **Minimum**: Intel i5-4590 / AMD FX 8350, 8GB RAM, GTX 960 / RX 470
- **Recommended**: Intel i7-8700K / AMD Ryzen 5 3600, 16GB RAM, GTX 1060 / RX 580

### Unity Packages
- Mirror Networking
- ProBuilder
- Post Processing Stack v2
- Cinemachine
- TextMeshPro

## 🚀 Installation

### 1. Clone the Repository
```bash
git clone https://github.com/your-username/counterstrike-game.git
cd counterstrike-game
```

### 2. Setup Unity Project
1. Open Unity Hub
2. Click "Open" and select the project folder
3. Unity will automatically import all required packages

### 3. Install Dependencies
The project will automatically install required packages through Unity's Package Manager.

### 4. Setup Networking
1. Open the NetworkManager scene
2. Configure Mirror Networking settings
3. Build and test locally or set up dedicated server

## 📁 Project Structure

```
CounterStrikeGame/
├── Assets/
│   ├── Scripts/
│   │   ├── Player/              # Player movement and controls
│   │   ├── Weapons/             # Weapon systems and mechanics
│   │   ├── Networking/          # Multiplayer networking
│   │   ├── UI/                  # User interface systems
│   │   ├── Audio/               # Audio management
│   │   ├── Managers/            # Core game managers
│   │   └── GameModes/           # Game mode implementations
│   ├── Prefabs/                 # Game object prefabs
│   ├── Materials/               # Materials and shaders
│   ├── Textures/                # Texture assets
│   ├── Models/                  # 3D models
│   ├── Audio/                   # Sound effects and music
│   ├── Scenes/                  # Game scenes
│   └── Resources/               # Loadable resources
├── Documentation/               # Detailed documentation
├── example_scripts/            # Example implementations
└── README.md                   # This file
```

## 📚 Development Phases

The project is organized into 11 development phases:

### Phase 1: Development Environment Setup
- Unity installation and configuration
- Version control setup
- Project structure creation

### Phase 2: Basic Game Framework
- Core game manager
- Player framework
- Team system

### Phase 3: Player Movement and Controls
- First-person controller
- Advanced movement mechanics
- Input system

### Phase 4: Weapons System
- Base weapon classes
- Rifle, pistol, and melee implementations
- Accuracy and recoil systems

### Phase 5: Combat Mechanics
- Damage calculation
- Armor system
- Hit detection

### Phase 6: Multiplayer Networking
- Mirror Networking integration
- Client-server architecture
- Network synchronization

### Phase 7: Game Modes and Round System
- Bomb defusal mode
- Economy system
- Round management

### Phase 8: Map Design and Level Creation
- ProBuilder integration
- Level optimization
- Bomb site creation

### Phase 9: User Interface and HUD
- In-game HUD
- Buy menu system
- Spectator interface

### Phase 10: Audio System
- 3D positional audio
- Dynamic music system
- Voice lines and sound effects

### Phase 11: Polish and Optimization
- Performance optimization
- Object pooling
- Quality settings

## 🎯 Quick Start Guide

### 1. Basic Setup
```csharp
// 1. Create a new Unity 3D project
// 2. Import the Core Scripts from example_scripts/
// 3. Set up the basic scene with GameManager
```

### 2. Player Setup
```csharp
// Add these components to your player prefab:
// - CharacterController
// - PlayerController
// - PlayerInput
// - WeaponManager
// - AudioSource
```

### 3. Weapon Configuration
```csharp
// Create weapon prefabs with:
// - Weapon script (AssaultRifle, Pistol, etc.)
// - WeaponAccuracy component
// - Audio source for weapon sounds
// - Muzzle flash particle system
```

### 4. Network Setup
```csharp
// 1. Add Mirror Networking to the project
// 2. Create NetworkManager with CSNetworkManager
// 3. Set up spawn points for both teams
// 4. Configure player prefab with NetworkPlayerController
```

### 5. Testing
```csharp
// 1. Build the project
// 2. Run multiple instances for testing
// 3. Use the GameTester component for automated testing
```

## 📖 Documentation

Detailed documentation is available in the following files:

- **[Complete Development Guide](Counter_Strike_Game_Development_Guide.md)** - Step-by-step implementation guide
- **[API Reference](Documentation/API_Reference.md)** - Code documentation
- **[Architecture Overview](Documentation/Architecture.md)** - System design explanation
- **[Networking Guide](Documentation/Networking.md)** - Multiplayer implementation details
- **[Performance Guide](Documentation/Performance.md)** - Optimization best practices

### Key Scripts

| Script | Purpose | Location |
|--------|---------|----------|
| `GameManager.cs` | Core game state management | `example_scripts/` |
| `PlayerController.cs` | Player movement and input | `example_scripts/` |
| `WeaponManager.cs` | Weapon system management | `example_scripts/` |
| `UIManager.cs` | User interface management | See guide |
| `AudioManager.cs` | Audio system management | See guide |

## 🎨 Customization

### Adding New Weapons
1. Create a new weapon class inheriting from `Weapon`
2. Configure weapon stats in the inspector
3. Add to the `WeaponDatabase` ScriptableObject
4. Create weapon prefab with appropriate components

### Creating New Maps
1. Use ProBuilder to create level geometry
2. Set up bomb sites with `BombSite` component
3. Place spawn points for both teams
4. Configure lighting and post-processing
5. Bake navigation mesh and occlusion culling

### Modifying Game Rules
1. Extend the `IGameMode` interface
2. Implement custom round logic
3. Configure economy rewards
4. Add to the game mode selection system

## 🤝 Contributing

We welcome contributions to improve the project! Please follow these guidelines:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/new-feature`
3. **Commit changes**: `git commit -am 'Add new feature'`
4. **Push to branch**: `git push origin feature/new-feature`
5. **Submit a Pull Request**

### Contribution Areas
- Bug fixes and optimizations
- New weapon implementations
- Additional game modes
- Map creation tools
- Performance improvements
- Documentation updates

## 🏗️ Building and Deployment

### Development Build
```bash
# Open Build Settings in Unity
# Select target platform
# Enable Development Build
# Build and Run
```

### Production Build
```bash
# Disable Development Build
# Enable optimizations
# Configure player settings
# Build for target platforms
```

### Dedicated Server
```bash
# Enable Server Build in build settings
# Remove client-only components
# Configure headless mode
# Deploy to server infrastructure
```

## 🔧 Troubleshooting

### Common Issues

**Network Connection Problems**
- Check firewall settings
- Verify port configuration
- Ensure Mirror Networking is properly configured

**Performance Issues**
- Enable dynamic quality scaling
- Check LOD configuration
- Monitor object pooling usage

**Audio Problems**
- Verify audio source configurations
- Check 3D audio settings
- Ensure proper audio mixing

**Movement Issues**
- Check CharacterController settings
- Verify input system configuration
- Test ground detection

## 📝 Changelog

### Version 1.0.0 (Current)
- Complete game framework implementation
- All 11 development phases documented
- Example scripts and prefabs
- Comprehensive documentation
- Multiplayer networking support

## 📄 Legal Notice

### Educational Purpose
This project is created for **educational purposes only**. It demonstrates game development techniques and is not intended for commercial distribution.

### Asset Rights
- Ensure you have proper licenses for all assets used
- The code framework is provided under MIT license
- Audio and visual assets are not included and must be sourced separately

### Trademark Notice
Counter-Strike is a trademark of Valve Corporation. This project is not affiliated with or endorsed by Valve Corporation.

### License
```
MIT License

Copyright (c) 2024 Counter-Strike Style Game Project

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## 🌟 Acknowledgments

- Unity Technologies for the Unity Engine
- Mirror Networking team for networking solution
- Counter-Strike community for inspiration and reference
- Game development community for tutorials and resources

## 📞 Support

For questions, bug reports, or feature requests:

1. **Check the documentation** first
2. **Search existing issues** on GitHub
3. **Create a new issue** with detailed information
4. **Join the community Discord** for real-time help

---

**Happy Game Development! 🎮**

*"The best games are built one line of code at a time."*