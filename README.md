# DOTS_GraduationProject
Unity ECS Top-Down Shooter
Overview
This project is a top-down shooter game developed using Unity’s Entity Component System (ECS), Physics, and DOTS-based architecture. The goal is to create a performant and modular game where the player survives against waves of enemies, collects gems, and manages cooldowns, animations, and UI interactions.

Features
Entity-Component-System Architecture
Utilizes Unity ECS for scalable, performant, and data-driven gameplay.

Player Mechanics

Movement and directional animation via PlayerInputSystem and PlayerAnimationManager.

Ranged attack system using PlasmaBlastAuthoring.

Health bar and gem collection tracking in real time.

Enemy AI

Enemy entities track and move toward the player using EnemyMoveToPlayerSystem.

Attack behavior with cooldown management.

Enemies drop collectible gems upon death.

Gem Collection System

Gems can be picked up on collision.

UI updates the number of gems collected.

Camera System

Follows the player entity via a singleton-driven CameraTarget.

UI & Game Management

Pause and resume functionality via GameUIController.

Game over screen when the player dies.

Dynamic updates to collected gems and health bar.

Scripts & Components
PlayerAuthoring.cs: Sets up player data, camera, and UI references.

EnemyAuthoring.cs: Handles enemy behavior, attack, and gem dropping.

EnemySpawnerAuthoring.cs: Controls spawn logic and timing around the player.

PlasmaBlastAuthoring.cs: Defines player attacks with expiration and collision logic.

GemAuthoring.cs: Manages gem behavior and collection system.

DestroyEntitySystem.cs: Handles safe removal of entities like enemies, gems, and projectiles.

GameUIController.cs: Manages UI states such as pause, game over, and collected gem count.

PlayerAnimationManager.cs: Handles animation based on input direction.

Requirements
Unity 2022 or later with Entities package

Input System enabled

DOTS Physics enabled

Getting Started
Clone the repository.

Open the project in Unity.

Set up the scene with required GameObjects and authoring components.

Press Play and start the game.

License
This project is for educational and demonstration purposes.
