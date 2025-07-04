#  Unity ECS Top-Down Shooter

Welcome to a modular and high-performance **top-down shooter** built with **Unity’s ECS (Entity Component System)** and **DOTS (Data-Oriented Technology Stack)**. This project is a perfect example of scalable gameplay where systems, components, and entities work together to deliver dynamic game mechanics.


![image](https://github.com/user-attachments/assets/4215138d-9eb9-45d3-ad5f-41e4f1239333)
![image](https://github.com/user-attachments/assets/f043d57f-9365-4a80-a8f8-907f32ab49d2)



<h2 align="center">🎥 Gameplay Video</h2>

<p align="center">
  <a href="https://www.youtube.com/watch?v=UyF7lLAoHXM" target="_blank">
    <img src="https://img.youtube.com/vi/UyF7lLAoHXM/0.jpg" alt="Watch the video"  />
  </a>
</p>



---

##  Features

 **Entity-Component Architecture**  
 Modular systems that separate logic and data for better performance and flexibility.

 **Player Mechanics**
- WASD or joystick-based movement
- Direction-based animation
- Ranged plasma blast attack
- Health and gem UI integration

 **Enemy AI**
- Automatically tracks and moves toward the player
- Attack with cooldown system
- Drops gems when defeated

 **Gem Collection**
- Pick up system with collision detection
- Real-time UI update

 **Camera System**
- Smooth player-following using singleton-based camera reference

 **Game UI**
- Pause/resume system
- Game over screen
- Dynamic gem and health UI

---

##  Key Components

| Script | Description |
|--------|-------------|
| `PlayerAuthoring.cs` | Sets up player ECS components and attack logic |
| `EnemyAuthoring.cs` | Defines enemy behavior and damage logic |
| `EnemySpawnerAuthoring.cs` | Controls periodic spawning of enemies |
| `PlasmaBlastAuthoring.cs` | Player's ranged projectile system |
| `GemAuthoring.cs` | Logic for collectible gems |
| `DestroyEntitySystem.cs` | Central cleanup system for ECS entities |
| `GameUIController.cs` | Manages UI interactions, pause/game over |
| `PlayerAnimationManager.cs` | Controls directional movement animations |

---

##  Requirements

- **Unity 2022+**
- **Entities Package**
- **Input System Package**
- **DOTS Physics Enabled**

---

##  Getting Started

1. **Clone** the repository.
2. **Open** the project in Unity.
3. **Ensure** all systems (Player, UI, Camera, Enemy, etc.) are placed and referenced correctly in the scene.
4. Press **Play** and survive as long as you can!

---
