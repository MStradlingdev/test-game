# Complete Guide to Creating a Counter-Strike Style Game

## Table of Contents
1. [Project Overview](#project-overview)
2. [Phase 1: Development Environment Setup](#phase-1-development-environment-setup)
3. [Phase 2: Basic Game Framework](#phase-2-basic-game-framework)
4. [Phase 3: Player Movement and Controls](#phase-3-player-movement-and-controls)
5. [Phase 4: Weapons System](#phase-4-weapons-system)
6. [Phase 5: Combat Mechanics](#phase-5-combat-mechanics)
7. [Phase 6: Multiplayer Networking](#phase-6-multiplayer-networking)
8. [Phase 7: Game Modes and Round System](#phase-7-game-modes-and-round-system)
9. [Phase 8: Map Design and Level Creation](#phase-8-map-design-and-level-creation)
10. [Phase 9: User Interface and HUD](#phase-9-user-interface-and-hud)
11. [Phase 10: Audio System](#phase-10-audio-system)
12. [Phase 11: Polish and Optimization](#phase-11-polish-and-optimization)
13. [Advanced Features](#advanced-features)
14. [Testing and Deployment](#testing-and-deployment)

## Project Overview

### Game Concept
We'll be creating a tactical first-person shooter similar to Counter-Strike with:
- **Teams**: Terrorists vs Counter-Terrorists
- **Objective-based gameplay**: Bomb defusal, hostage rescue
- **Round-based matches**: Best of 30 rounds
- **Economy system**: Buy weapons and equipment
- **Tactical gameplay**: No respawning during rounds

### Technology Stack
- **Engine**: Unity 3D (recommended) or Unreal Engine 4/5
- **Programming**: C# (Unity) or C++ (Unreal)
- **Networking**: Mirror Networking (Unity) or built-in multiplayer (Unreal)
- **Audio**: FMOD or Wwise
- **Version Control**: Git with Git LFS for assets

### Development Time Estimate
- **Solo Developer**: 2-3 years
- **Small Team (3-5 people)**: 1-2 years
- **Experienced Team**: 6-12 months

---

## Phase 1: Development Environment Setup

### Step 1.1: Install Development Tools

#### Unity Setup (Recommended for Beginners)
1. **Download Unity Hub**
   - Go to https://unity.com/download
   - Install Unity Hub
   - Create Unity account

2. **Install Unity Editor**
   - Open Unity Hub
   - Go to "Installs" tab
   - Click "Install Editor"
   - Choose Unity 2022.3 LTS (Long Term Support)
   - Include modules:
     - WebGL Build Support
     - Windows Build Support
     - Linux Build Support
     - Visual Studio Community (Windows)

3. **Setup IDE**
   - **Windows**: Visual Studio Community 2022
   - **Mac**: Visual Studio for Mac or JetBrains Rider
   - **Linux**: JetBrains Rider or VSCode with C# extension

#### Alternative: Unreal Engine Setup
1. **Epic Games Launcher**
   - Download from https://www.epicgames.com/store/download
   - Install Epic Games Launcher
   - Create Epic Games account

2. **Install Unreal Engine**
   - Open Epic Games Launcher
   - Go to "Unreal Engine" tab
   - Install Unreal Engine 5.1 or later
   - Include: Visual Studio integration

### Step 1.2: Version Control Setup

1. **Install Git**
   ```bash
   # Linux
   sudo apt install git git-lfs
   
   # Windows - download from git-scm.com
   # Mac - install via Homebrew: brew install git git-lfs
   ```

2. **Setup Git LFS** (for large assets)
   ```bash
   git lfs install
   ```

3. **Create Repository Structure**
   ```
   CounterStrikeGame/
   ├── .gitignore
   ├── .gitattributes
   ├── README.md
   ├── Assets/           # Unity assets
   ├── ProjectSettings/  # Unity project settings
   ├── Packages/        # Package manager
   └── Documentation/   # Design documents
   ```

### Step 1.3: Project Initialization

#### Unity Project Setup
1. **Create New Project**
   - Open Unity Hub
   - Click "New Project"
   - Select "3D" template
   - Name: "CounterStrikeGame"
   - Location: Choose your workspace

2. **Import Essential Packages**
   - Window → Package Manager
   - Install packages:
     - ProBuilder (for level design)
     - ProGrids (for precise placement)
     - Post Processing (for visual effects)
     - Cinemachine (for cameras)

3. **Setup Project Structure**
   ```
   Assets/
   ├── Scripts/
   │   ├── Player/
   │   ├── Weapons/
   │   ├── Networking/
   │   ├── UI/
   │   ├── Audio/
   │   └── Managers/
   ├── Prefabs/
   ├── Materials/
   ├── Textures/
   ├── Models/
   ├── Audio/
   ├── Scenes/
   └── Resources/
   ```

---

## Phase 2: Basic Game Framework

### Step 2.1: Game Manager System

Create the core game management system:

#### GameManager.cs
```csharp
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public int maxRounds = 30;
    public float roundTime = 115f;
    public float freezeTime = 15f;
    public float bombTimer = 40f;
    
    [Header("Teams")]
    public Team terroristTeam;
    public Team counterTerroristTeam;
    
    public GameState currentState = GameState.WaitingForPlayers;
    public int currentRound = 0;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeGame()
    {
        terroristTeam = new Team("Terrorists", TeamSide.Terrorist);
        counterTerroristTeam = new Team("Counter-Terrorists", TeamSide.CounterTerrorist);
    }
    
    public void StartRound()
    {
        currentRound++;
        currentState = GameState.FreezeTime;
        
        // Reset player positions
        RespawnAllPlayers();
        
        // Start freeze time countdown
        Invoke(nameof(StartRoundActive), freezeTime);
    }
    
    private void StartRoundActive()
    {
        currentState = GameState.RoundActive;
        // Enable player movement
        // Start round timer
    }
    
    public void EndRound(TeamSide winner, RoundEndReason reason)
    {
        currentState = GameState.RoundEnd;
        
        if (winner == TeamSide.Terrorist)
            terroristTeam.score++;
        else
            counterTerroristTeam.score++;
            
        // Award money based on performance
        CalculateRoundRewards(winner, reason);
        
        // Check if match is over
        if (IsMatchOver())
        {
            EndMatch();
        }
        else
        {
            Invoke(nameof(StartRound), 5f);
        }
    }
    
    private bool IsMatchOver()
    {
        int roundsToWin = (maxRounds / 2) + 1;
        return terroristTeam.score >= roundsToWin || 
               counterTerroristTeam.score >= roundsToWin;
    }
}

public enum GameState
{
    WaitingForPlayers,
    FreezeTime,
    RoundActive,
    RoundEnd,
    MatchEnd
}

public enum TeamSide
{
    Terrorist,
    CounterTerrorist
}

public enum RoundEndReason
{
    TimeExpired,
    BombExploded,
    BombDefused,
    EliminateEnemies,
    HostagesRescued
}

[System.Serializable]
public class Team
{
    public string name;
    public TeamSide side;
    public int score;
    public List<PlayerController> players;
    
    public Team(string teamName, TeamSide teamSide)
    {
        name = teamName;
        side = teamSide;
        score = 0;
        players = new List<PlayerController>();
    }
}
```

### Step 2.2: Player Framework

#### PlayerController.cs
```csharp
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4.5f;
    public float runSpeed = 6f;
    public float crouchSpeed = 2f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;
    
    [Header("Look")]
    public float mouseSensitivity = 100f;
    public Transform cameraTransform;
    public float maxLookAngle = 80f;
    
    // Components
    private CharacterController controller;
    private PlayerInput input;
    private Camera playerCamera;
    
    // Movement variables
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private bool isRunning;
    
    // Look variables
    private float xRotation = 0f;
    
    // Player state
    public PlayerState state = PlayerState.Alive;
    public TeamSide team;
    public int money = 800;
    public int health = 100;
    public int armor = 0;
    
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void Update()
    {
        if (state != PlayerState.Alive) return;
        
        HandleMovement();
        HandleLook();
        HandleInput();
    }
    
    private void HandleMovement()
    {
        // Ground check
        isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        // Get input
        Vector2 moveInput = input.moveInput;
        
        // Calculate move direction
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        
        // Apply movement speed
        float currentSpeed = GetCurrentSpeed();
        controller.Move(move * currentSpeed * Time.deltaTime);
        
        // Jumping
        if (input.jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    private float GetCurrentSpeed()
    {
        if (isCrouching) return crouchSpeed;
        if (isRunning) return runSpeed;
        return walkSpeed;
    }
    
    private void HandleLook()
    {
        Vector2 lookInput = input.lookInput * mouseSensitivity * Time.deltaTime;
        
        // Rotate the player horizontally
        transform.Rotate(Vector3.up * lookInput.x);
        
        // Rotate the camera vertically
        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    private void HandleInput()
    {
        // Crouching
        if (input.crouchPressed)
        {
            ToggleCrouch();
        }
        
        // Running
        isRunning = input.runHeld && !isCrouching;
        
        // Weapon actions
        if (input.firePressed)
        {
            // Fire weapon
        }
        
        if (input.reloadPressed)
        {
            // Reload weapon
        }
    }
    
    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        
        if (isCrouching)
        {
            controller.height = 1f;
            cameraTransform.localPosition = new Vector3(0, 0.5f, 0);
        }
        else
        {
            controller.height = 2f;
            cameraTransform.localPosition = new Vector3(0, 1.6f, 0);
        }
    }
    
    public void TakeDamage(int damage, PlayerController attacker = null)
    {
        if (state != PlayerState.Alive) return;
        
        // Apply armor reduction
        int actualDamage = CalculateDamageWithArmor(damage);
        
        health -= actualDamage;
        
        if (health <= 0)
        {
            Die(attacker);
        }
    }
    
    private int CalculateDamageWithArmor(int damage)
    {
        if (armor <= 0) return damage;
        
        int armorDamage = Mathf.RoundToInt(damage * 0.5f);
        int healthDamage = damage - armorDamage;
        
        armor = Mathf.Max(0, armor - armorDamage);
        
        return healthDamage;
    }
    
    private void Die(PlayerController killer = null)
    {
        state = PlayerState.Dead;
        
        // Disable movement
        enabled = false;
        
        // Drop weapon
        // Award kill to killer
        // Switch to spectator camera
        
        GameManager.Instance.CheckRoundEnd();
    }
}

public enum PlayerState
{
    Alive,
    Dead,
    Spectating
}
```

---

## Phase 3: Player Movement and Controls

### Step 3.1: Advanced Movement System

#### PlayerInput.cs
```csharp
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Input Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode runKey = KeyCode.LeftShift;
    public KeyCode reloadKey = KeyCode.R;
    public KeyCode useKey = KeyCode.E;
    
    // Input properties
    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool jumpPressed { get; private set; }
    public bool crouchPressed { get; private set; }
    public bool runHeld { get; private set; }
    public bool firePressed { get; private set; }
    public bool fireHeld { get; private set; }
    public bool reloadPressed { get; private set; }
    public bool usePressed { get; private set; }
    
    private void Update()
    {
        // Movement input
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
        
        // Look input
        lookInput = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );
        
        // Button inputs
        jumpPressed = Input.GetKeyDown(jumpKey);
        crouchPressed = Input.GetKeyDown(crouchKey);
        runHeld = Input.GetKey(runKey);
        
        firePressed = Input.GetMouseButtonDown(0);
        fireHeld = Input.GetMouseButton(0);
        
        reloadPressed = Input.GetKeyDown(reloadKey);
        usePressed = Input.GetKeyDown(useKey);
    }
}
```

### Step 3.2: Counter-Strike Style Movement

Enhance the PlayerController with CS-style movement mechanics:

```csharp
// Add to PlayerController.cs

[Header("Advanced Movement")]
public float accelerationSpeed = 10f;
public float maxSpeed = 7.5f;
public float friction = 4f;
public float airAcceleration = 2f;
public float stopSpeed = 1.5f;

private Vector3 horizontalVelocity;

private void HandleAdvancedMovement()
{
    Vector2 inputDir = input.moveInput.normalized;
    Vector3 inputVector = (transform.right * inputDir.x + transform.forward * inputDir.y);
    
    if (isGrounded)
    {
        GroundMovement(inputVector);
    }
    else
    {
        AirMovement(inputVector);
    }
    
    // Apply horizontal velocity
    controller.Move(horizontalVelocity * Time.deltaTime);
}

private void GroundMovement(Vector3 inputVector)
{
    float currentSpeed = horizontalVelocity.magnitude;
    
    if (inputVector.magnitude > 0)
    {
        // Acceleration
        float acceleration = accelerationSpeed * GetCurrentSpeed() / maxSpeed;
        Vector3 targetVelocity = inputVector * GetCurrentSpeed();
        
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, 
            acceleration * Time.deltaTime);
    }
    else
    {
        // Friction
        if (currentSpeed > stopSpeed)
        {
            float drop = currentSpeed * friction * Time.deltaTime;
            horizontalVelocity = horizontalVelocity.normalized * Mathf.Max(currentSpeed - drop, 0);
        }
        else
        {
            horizontalVelocity = Vector3.zero;
        }
    }
}

private void AirMovement(Vector3 inputVector)
{
    if (inputVector.magnitude > 0)
    {
        float acceleration = airAcceleration;
        Vector3 targetVelocity = inputVector * GetCurrentSpeed();
        
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, 
            acceleration * Time.deltaTime);
    }
}
```

---

## Phase 4: Weapons System

### Step 4.1: Base Weapon System

#### Weapon.cs
```csharp
using UnityEngine;
using System.Collections;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    public string weaponName;
    public int damage;
    public float fireRate;
    public float range;
    public float accuracy;
    public int magazineSize;
    public int totalAmmo;
    public float reloadTime;
    public int price;
    
    [Header("Weapon Settings")]
    public LayerMask hitLayers = -1;
    public Transform firePoint;
    public ParticleSystem muzzleFlash;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    
    // Current state
    protected int currentAmmo;
    protected int currentTotalAmmo;
    protected bool isReloading;
    protected float nextFireTime;
    
    // Components
    protected AudioSource audioSource;
    protected PlayerController owner;
    
    protected virtual void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentAmmo = magazineSize;
        currentTotalAmmo = totalAmmo;
    }
    
    public virtual void Initialize(PlayerController player)
    {
        owner = player;
    }
    
    public virtual bool CanFire()
    {
        return !isReloading && 
               currentAmmo > 0 && 
               Time.time >= nextFireTime &&
               owner.state == PlayerState.Alive;
    }
    
    public abstract void Fire();
    
    public virtual void StartReload()
    {
        if (isReloading || currentAmmo == magazineSize || currentTotalAmmo <= 0)
            return;
            
        StartCoroutine(ReloadCoroutine());
    }
    
    protected virtual IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        
        // Play reload sound
        if (reloadSound != null)
            audioSource.PlayOneShot(reloadSound);
            
        yield return new WaitForSeconds(reloadTime);
        
        // Calculate ammo to reload
        int ammoNeeded = magazineSize - currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, currentTotalAmmo);
        
        currentAmmo += ammoToLoad;
        currentTotalAmmo -= ammoToLoad;
        
        isReloading = false;
    }
    
    protected virtual void PlayFireEffect()
    {
        // Muzzle flash
        if (muzzleFlash != null)
            muzzleFlash.Play();
            
        // Fire sound
        if (fireSound != null)
            audioSource.PlayOneShot(fireSound);
    }
    
    protected virtual bool Raycast(out RaycastHit hit)
    {
        Ray ray = new Ray(firePoint.position, firePoint.forward);
        return Physics.Raycast(ray, out hit, range, hitLayers);
    }
    
    public WeaponData GetWeaponData()
    {
        return new WeaponData
        {
            name = weaponName,
            currentAmmo = currentAmmo,
            totalAmmo = currentTotalAmmo,
            isReloading = isReloading
        };
    }
}

[System.Serializable]
public class WeaponData
{
    public string name;
    public int currentAmmo;
    public int totalAmmo;
    public bool isReloading;
}
```

### Step 4.2: Rifle Implementation

#### AssaultRifle.cs
```csharp
using UnityEngine;

public class AssaultRifle : Weapon
{
    [Header("Rifle Settings")]
    public float recoilStrength = 2f;
    public float recoilRecovery = 5f;
    public AnimationCurve recoilPattern;
    
    private int shotsFired = 0;
    private Vector2 currentRecoil;
    
    public override void Fire()
    {
        if (!CanFire()) return;
        
        nextFireTime = Time.time + (1f / fireRate);
        currentAmmo--;
        
        // Apply recoil
        ApplyRecoil();
        
        // Perform raycast
        if (Raycast(out RaycastHit hit))
        {
            // Calculate damage based on distance and body part
            int finalDamage = CalculateDamage(hit);
            
            // Apply damage to target
            PlayerController target = hit.collider.GetComponent<PlayerController>();
            if (target != null && target.team != owner.team)
            {
                target.TakeDamage(finalDamage, owner);
            }
            
            // Create bullet hole
            CreateBulletHole(hit);
        }
        
        PlayFireEffect();
        
        // Auto-reload if magazine is empty
        if (currentAmmo <= 0)
        {
            StartReload();
        }
    }
    
    private void ApplyRecoil()
    {
        shotsFired++;
        
        // Calculate recoil based on shots fired
        float recoilMultiplier = recoilPattern.Evaluate((float)shotsFired / 10f);
        
        Vector2 recoil = new Vector2(
            Random.Range(-recoilStrength, recoilStrength) * recoilMultiplier,
            recoilStrength * recoilMultiplier
        );
        
        currentRecoil += recoil;
        
        // Apply recoil to camera
        owner.cameraTransform.localRotation *= Quaternion.Euler(-recoil.y, recoil.x, 0);
    }
    
    private void Update()
    {
        // Recoil recovery
        if (shotsFired > 0 && Time.time >= nextFireTime + 0.1f)
        {
            shotsFired = Mathf.Max(0, shotsFired - 1);
            
            // Recover from recoil
            currentRecoil = Vector2.Lerp(currentRecoil, Vector2.zero, 
                recoilRecovery * Time.deltaTime);
        }
    }
    
    private int CalculateDamage(RaycastHit hit)
    {
        int baseDamage = damage;
        
        // Distance falloff
        float distance = Vector3.Distance(firePoint.position, hit.point);
        float damageMultiplier = Mathf.Lerp(1f, 0.7f, distance / range);
        
        // Body part multiplier
        if (hit.collider.CompareTag("Head"))
            damageMultiplier *= 4f;
        else if (hit.collider.CompareTag("Body"))
            damageMultiplier *= 1f;
        else if (hit.collider.CompareTag("Leg"))
            damageMultiplier *= 0.75f;
            
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }
    
    private void CreateBulletHole(RaycastHit hit)
    {
        // Create bullet hole decal
        GameObject bulletHole = ObjectPool.Instance.Get("BulletHole");
        bulletHole.transform.position = hit.point + hit.normal * 0.01f;
        bulletHole.transform.rotation = Quaternion.LookRotation(hit.normal);
        
        // Return to pool after delay
        ObjectPool.Instance.ReturnToPool(bulletHole, 30f);
    }
}
```

---

## Phase 5: Combat Mechanics

### Step 5.1: Damage System

#### DamageSystem.cs
```csharp
using UnityEngine;

public static class DamageSystem
{
    public static int CalculateWeaponDamage(Weapon weapon, RaycastHit hit, float distance)
    {
        int baseDamage = weapon.damage;
        
        // Distance falloff
        float falloffMultiplier = CalculateDistanceFalloff(weapon, distance);
        
        // Body part multiplier
        float bodyMultiplier = GetBodyPartMultiplier(hit.collider);
        
        // Material penetration
        float penetrationMultiplier = GetPenetrationMultiplier(hit.collider);
        
        int finalDamage = Mathf.RoundToInt(baseDamage * falloffMultiplier * 
                                         bodyMultiplier * penetrationMultiplier);
        
        return Mathf.Max(1, finalDamage);
    }
    
    private static float CalculateDistanceFalloff(Weapon weapon, float distance)
    {
        // Different weapons have different falloff curves
        if (weapon is SniperRifle)
            return Mathf.Lerp(1f, 0.9f, distance / weapon.range);
        else if (weapon is AssaultRifle)
            return Mathf.Lerp(1f, 0.7f, distance / weapon.range);
        else if (weapon is Pistol)
            return Mathf.Lerp(1f, 0.5f, distance / weapon.range);
        else
            return 1f;
    }
    
    private static float GetBodyPartMultiplier(Collider hitCollider)
    {
        switch (hitCollider.tag)
        {
            case "Head": return 4f;
            case "Chest": return 1f;
            case "Stomach": return 1.25f;
            case "Arm": return 0.75f;
            case "Leg": return 0.75f;
            default: return 1f;
        }
    }
    
    private static float GetPenetrationMultiplier(Collider hitCollider)
    {
        // Check if bullet went through armor/helmet
        PlayerController player = hitCollider.GetComponentInParent<PlayerController>();
        if (player != null && player.armor > 0)
        {
            if (hitCollider.CompareTag("Head"))
                return 0.5f; // Helmet protection
            else
                return 0.7f; // Body armor protection
        }
        
        return 1f;
    }
}
```

### Step 5.2: Weapon Accuracy System

#### WeaponAccuracy.cs
```csharp
using UnityEngine;

[System.Serializable]
public class AccuracySettings
{
    [Header("Base Accuracy")]
    public float standingAccuracy = 0.1f;
    public float crouchingAccuracy = 0.05f;
    public float movingAccuracy = 0.3f;
    public float jumpingAccuracy = 0.8f;
    
    [Header("Recoil")]
    public float recoilIncrease = 0.1f;
    public float recoilDecay = 2f;
    public float maxRecoil = 1f;
    
    [Header("First Shot")]
    public bool hasFirstShotAccuracy = true;
    public float firstShotMultiplier = 0.1f;
}

public class WeaponAccuracy : MonoBehaviour
{
    public AccuracySettings settings;
    
    private float currentInaccuracy = 0f;
    private bool isFirstShot = true;
    private float lastShotTime;
    
    public Vector3 GetAccuracyOffset(PlayerController player)
    {
        float inaccuracy = CalculateCurrentInaccuracy(player);
        
        // Generate random spread within accuracy cone
        Vector2 randomOffset = Random.insideUnitCircle * inaccuracy;
        
        return new Vector3(randomOffset.x, randomOffset.y, 0);
    }
    
    private float CalculateCurrentInaccuracy(PlayerController player)
    {
        float baseInaccuracy = GetBaseInaccuracy(player);
        
        // Add recoil inaccuracy
        float totalInaccuracy = baseInaccuracy + currentInaccuracy;
        
        // First shot accuracy
        if (isFirstShot && settings.hasFirstShotAccuracy)
        {
            totalInaccuracy *= settings.firstShotMultiplier;
        }
        
        return totalInaccuracy;
    }
    
    private float GetBaseInaccuracy(PlayerController player)
    {
        // Check player state
        if (!player.controller.isGrounded)
            return settings.jumpingAccuracy;
        else if (player.controller.velocity.magnitude > 1f)
            return settings.movingAccuracy;
        else if (player.isCrouching)
            return settings.crouchingAccuracy;
        else
            return settings.standingAccuracy;
    }
    
    public void OnWeaponFired()
    {
        // Increase inaccuracy from recoil
        currentInaccuracy = Mathf.Min(settings.maxRecoil, 
            currentInaccuracy + settings.recoilIncrease);
        
        isFirstShot = false;
        lastShotTime = Time.time;
    }
    
    private void Update()
    {
        // Decay recoil over time
        if (Time.time - lastShotTime > 0.2f)
        {
            currentInaccuracy = Mathf.Max(0, 
                currentInaccuracy - settings.recoilDecay * Time.deltaTime);
                
            // Reset first shot after delay
            if (Time.time - lastShotTime > 1f)
            {
                isFirstShot = true;
            }
        }
    }
}
```

---

## Phase 6: Multiplayer Networking

### Step 6.1: Network Setup (Using Mirror Networking)

#### Install Mirror Networking
1. Open Package Manager in Unity
2. Add package from git URL: `https://github.com/vis2k/Mirror.git`
3. Import Mirror samples

#### NetworkManager Setup
```csharp
using Mirror;
using UnityEngine;

public class CSNetworkManager : NetworkManager
{
    [Header("CS Game Settings")]
    public GameObject playerPrefab;
    public Transform[] spawnPointsT;
    public Transform[] spawnPointsCT;
    
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Assign player to team
        TeamSide team = AssignPlayerToTeam();
        
        // Get spawn point
        Transform spawnPoint = GetSpawnPoint(team);
        
        // Spawn player
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Setup team
        NetworkPlayerController networkPlayer = player.GetComponent<NetworkPlayerController>();
        networkPlayer.team = team;
        
        NetworkServer.AddPlayerForConnection(conn, player);
    }
    
    private TeamSide AssignPlayerToTeam()
    {
        int tCount = 0, ctCount = 0;
        
        foreach (NetworkPlayerController player in FindObjectsOfType<NetworkPlayerController>())
        {
            if (player.team == TeamSide.Terrorist) tCount++;
            else ctCount++;
        }
        
        return tCount <= ctCount ? TeamSide.Terrorist : TeamSide.CounterTerrorist;
    }
    
    private Transform GetSpawnPoint(TeamSide team)
    {
        Transform[] spawnPoints = team == TeamSide.Terrorist ? spawnPointsT : spawnPointsCT;
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
```

### Step 6.2: Network Player Controller

#### NetworkPlayerController.cs
```csharp
using Mirror;
using UnityEngine;

public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Networking")]
    [SyncVar(hook = nameof(OnTeamChanged))]
    public TeamSide team;
    
    [SyncVar(hook = nameof(OnHealthChanged))]
    public int health = 100;
    
    [SyncVar]
    public int money = 800;
    
    [SyncVar]
    public bool isAlive = true;
    
    private PlayerController playerController;
    private Weapon currentWeapon;
    
    public override void OnStartLocalPlayer()
    {
        // Setup local player specific components
        playerController = GetComponent<PlayerController>();
        playerController.enabled = true;
        
        // Enable camera for local player only
        Camera playerCamera = GetComponentInChildren<Camera>();
        playerCamera.enabled = true;
        
        // Disable camera for other players
        if (!isLocalPlayer)
        {
            playerCamera.enabled = false;
        }
    }
    
    [Command]
    public void CmdFireWeapon(Vector3 origin, Vector3 direction)
    {
        // Validate shot on server
        if (!isAlive || currentWeapon == null || !currentWeapon.CanFire())
            return;
            
        // Perform raycast on server
        Ray ray = new Ray(origin, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, currentWeapon.range))
        {
            // Check if hit player
            NetworkPlayerController hitPlayer = hit.collider.GetComponent<NetworkPlayerController>();
            if (hitPlayer != null && hitPlayer.team != team)
            {
                int damage = DamageSystem.CalculateWeaponDamage(currentWeapon, hit, hit.distance);
                hitPlayer.TakeDamage(damage, this);
            }
        }
        
        // Notify all clients of the shot
        RpcFireWeapon(origin, direction);
    }
    
    [ClientRpc]
    public void RpcFireWeapon(Vector3 origin, Vector3 direction)
    {
        // Play effects on all clients
        if (currentWeapon != null)
        {
            currentWeapon.PlayFireEffect();
        }
    }
    
    [Command]
    public void CmdTakeDamage(int damage, uint attackerId)
    {
        if (!isAlive) return;
        
        health = Mathf.Max(0, health - damage);
        
        if (health <= 0)
        {
            isAlive = false;
            RpcPlayerDied();
            
            // Award kill to attacker
            NetworkPlayerController attacker = NetworkIdentity.spawned[attackerId].GetComponent<NetworkPlayerController>();
            if (attacker != null)
            {
                attacker.OnKillPlayer(this);
            }
        }
    }
    
    [ClientRpc]
    public void RpcPlayerDied()
    {
        // Handle death on all clients
        playerController.enabled = false;
        
        if (isLocalPlayer)
        {
            // Switch to spectator camera
            SpectatorCamera.Instance.StartSpectating();
        }
    }
    
    public void TakeDamage(int damage, NetworkPlayerController attacker)
    {
        CmdTakeDamage(damage, attacker.netId);
    }
    
    public void OnKillPlayer(NetworkPlayerController victim)
    {
        // Award money for kill
        money += 300;
        
        // Update kill stats
        // Play kill sound
    }
    
    private void OnTeamChanged(TeamSide oldTeam, TeamSide newTeam)
    {
        // Update player appearance based on team
        UpdatePlayerAppearance();
    }
    
    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        // Update health UI
        if (isLocalPlayer)
        {
            UIManager.Instance.UpdateHealth(newHealth);
        }
    }
    
    private void UpdatePlayerAppearance()
    {
        // Change player model/materials based on team
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Color teamColor = team == TeamSide.Terrorist ? Color.red : Color.blue;
        
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = teamColor;
        }
    }
}
```

---

## Phase 7: Game Modes and Round System

### Step 7.1: Bomb Defusal Game Mode

#### BombDefusalGameMode.cs
```csharp
using UnityEngine;
using Mirror;
using System.Collections;

public class BombDefusalGameMode : NetworkBehaviour, IGameMode
{
    [Header("Bomb Settings")]
    public GameObject bombPrefab;
    public Transform[] bombSites;
    public float bombTimer = 40f;
    public float defuseTime = 10f;
    
    [SyncVar]
    public bool bombPlanted = false;
    
    [SyncVar]
    public bool bombDefused = false;
    
    [SyncVar]
    public float bombTimeRemaining;
    
    private GameObject activeBomb;
    private Transform plantedSite;
    
    public void StartRound()
    {
        // Reset bomb state
        bombPlanted = false;
        bombDefused = false;
        bombTimeRemaining = 0f;
        
        if (activeBomb != null)
        {
            NetworkServer.Destroy(activeBomb);
            activeBomb = null;
        }
        
        // Give bomb to random terrorist
        GiveBombToTerrorist();
    }
    
    private void GiveBombToTerrorist()
    {
        NetworkPlayerController[] terrorists = FindPlayersOfTeam(TeamSide.Terrorist);
        if (terrorists.Length > 0)
        {
            NetworkPlayerController bombCarrier = terrorists[Random.Range(0, terrorists.Length)];
            bombCarrier.GetComponent<Inventory>().AddItem("C4 Bomb");
        }
    }
    
    [Server]
    public void PlantBomb(NetworkPlayerController planter, Transform bombSite)
    {
        if (bombPlanted) return;
        
        bombPlanted = true;
        plantedSite = bombSite;
        bombTimeRemaining = bombTimer;
        
        // Spawn bomb object
        activeBomb = Instantiate(bombPrefab, bombSite.position, bombSite.rotation);
        NetworkServer.Spawn(activeBomb);
        
        // Start bomb timer
        StartCoroutine(BombTimerCoroutine());
        
        // Award money for plant
        planter.money += 300;
        
        RpcBombPlanted(bombSite.name);
    }
    
    [ClientRpc]
    public void RpcBombPlanted(string siteName)
    {
        // Play bomb plant sound
        AudioManager.Instance.PlaySound("BombPlant");
        
        // Show bomb planted message
        UIManager.Instance.ShowMessage($"Bomb planted at {siteName}!");
        
        // Update HUD
        UIManager.Instance.ShowBombTimer(true);
    }
    
    private IEnumerator BombTimerCoroutine()
    {
        while (bombTimeRemaining > 0 && !bombDefused)
        {
            yield return new WaitForSeconds(1f);
            bombTimeRemaining--;
            
            // Warning sounds
            if (bombTimeRemaining <= 10)
            {
                RpcPlayBombWarning();
            }
        }
        
        if (!bombDefused)
        {
            ExplodeBomb();
        }
    }
    
    [ClientRpc]
    public void RpcPlayBombWarning()
    {
        AudioManager.Instance.PlaySound("BombWarning");
    }
    
    [Server]
    public void StartDefuse(NetworkPlayerController defuser)
    {
        if (!bombPlanted || bombDefused) return;
        
        StartCoroutine(DefuseCoroutine(defuser));
    }
    
    private IEnumerator DefuseCoroutine(NetworkPlayerController defuser)
    {
        float defuseProgress = 0f;
        
        RpcStartDefuse(defuser.netId);
        
        while (defuseProgress < defuseTime && bombTimeRemaining > 0)
        {
            // Check if defuser is still alive and in range
            if (!defuser.isAlive || 
                Vector3.Distance(defuser.transform.position, activeBomb.transform.position) > 2f)
            {
                RpcStopDefuse();
                yield break;
            }
            
            defuseProgress += Time.deltaTime;
            yield return null;
        }
        
        if (defuseProgress >= defuseTime)
        {
            DefuseBomb(defuser);
        }
    }
    
    [ClientRpc]
    public void RpcStartDefuse(uint defuserId)
    {
        AudioManager.Instance.PlaySound("DefuseStart");
        UIManager.Instance.ShowDefuseProgress(true);
    }
    
    [ClientRpc]
    public void RpcStopDefuse()
    {
        AudioManager.Instance.StopSound("DefuseStart");
        UIManager.Instance.ShowDefuseProgress(false);
    }
    
    [Server]
    private void DefuseBomb(NetworkPlayerController defuser)
    {
        bombDefused = true;
        
        // Award money for defuse
        defuser.money += 300;
        
        // Award money to CT team
        AwardTeamMoney(TeamSide.CounterTerrorist, 500);
        
        // End round
        GameManager.Instance.EndRound(TeamSide.CounterTerrorist, RoundEndReason.BombDefused);
        
        RpcBombDefused();
    }
    
    [ClientRpc]
    public void RpcBombDefused()
    {
        AudioManager.Instance.PlaySound("BombDefused");
        UIManager.Instance.ShowMessage("Bomb defused! Counter-Terrorists win!");
        UIManager.Instance.ShowBombTimer(false);
    }
    
    [Server]
    private void ExplodeBomb()
    {
        // Award money to terrorist team
        AwardTeamMoney(TeamSide.Terrorist, 800);
        
        // End round
        GameManager.Instance.EndRound(TeamSide.Terrorist, RoundEndReason.BombExploded);
        
        RpcBombExploded();
    }
    
    [ClientRpc]
    public void RpcBombExploded()
    {
        AudioManager.Instance.PlaySound("BombExplosion");
        UIManager.Instance.ShowMessage("Bomb exploded! Terrorists win!");
        UIManager.Instance.ShowBombTimer(false);
        
        // Screen shake effect
        CameraShake.Instance.Shake(2f, 1f);
    }
    
    public bool CheckRoundEndCondition()
    {
        // Check if time expired
        if (GameManager.Instance.roundTimeRemaining <= 0 && !bombPlanted)
        {
            GameManager.Instance.EndRound(TeamSide.CounterTerrorist, RoundEndReason.TimeExpired);
            return true;
        }
        
        // Check if all players eliminated
        if (CountAlivePlayers(TeamSide.Terrorist) == 0)
        {
            GameManager.Instance.EndRound(TeamSide.CounterTerrorist, RoundEndReason.EliminateEnemies);
            return true;
        }
        
        if (CountAlivePlayers(TeamSide.CounterTerrorist) == 0)
        {
            GameManager.Instance.EndRound(TeamSide.Terrorist, RoundEndReason.EliminateEnemies);
            return true;
        }
        
        return false;
    }
    
    private NetworkPlayerController[] FindPlayersOfTeam(TeamSide team)
    {
        return System.Array.FindAll(FindObjectsOfType<NetworkPlayerController>(), 
            p => p.team == team);
    }
    
    private int CountAlivePlayers(TeamSide team)
    {
        return System.Array.FindAll(FindObjectsOfType<NetworkPlayerController>(), 
            p => p.team == team && p.isAlive).Length;
    }
    
    private void AwardTeamMoney(TeamSide team, int amount)
    {
        NetworkPlayerController[] players = FindPlayersOfTeam(team);
        foreach (NetworkPlayerController player in players)
        {
            player.money += amount;
        }
    }
}

public interface IGameMode
{
    void StartRound();
    bool CheckRoundEndCondition();
}
```

### Step 7.2: Economy System

#### EconomySystem.cs
```csharp
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EconomySettings
{
    [Header("Starting Money")]
    public int startingMoney = 800;
    public int maxMoney = 16000;
    
    [Header("Round Rewards")]
    public int winBonus = 3250;
    public int lossBonus = 1400;
    public int consecutiveLossBonus = 500;
    public int maxConsecutiveLossBonus = 3400;
    
    [Header("Action Rewards")]
    public int killReward = 300;
    public int bombPlantReward = 300;
    public int bombDefuseReward = 300;
    public int hostageRescueReward = 200;
}

public class EconomySystem : MonoBehaviour
{
    public EconomySettings settings;
    
    private Dictionary<TeamSide, int> consecutiveLosses = new Dictionary<TeamSide, int>();
    
    private void Start()
    {
        consecutiveLosses[TeamSide.Terrorist] = 0;
        consecutiveLosses[TeamSide.CounterTerrorist] = 0;
    }
    
    public void CalculateRoundRewards(TeamSide winner, RoundEndReason reason)
    {
        // Award winning team
        AwardTeamMoney(winner, settings.winBonus);
        consecutiveLosses[winner] = 0;
        
        // Award losing team with loss bonus
        TeamSide loser = winner == TeamSide.Terrorist ? TeamSide.CounterTerrorist : TeamSide.Terrorist;
        consecutiveLosses[loser]++;
        
        int lossBonus = CalculateLossBonus(loser);
        AwardTeamMoney(loser, lossBonus);
        
        // Additional rewards based on round end reason
        switch (reason)
        {
            case RoundEndReason.BombExploded:
                // Bomb exploded - terrorists get extra money
                AwardTeamMoney(TeamSide.Terrorist, 300);
                break;
                
            case RoundEndReason.BombDefused:
                // Bomb defused - CTs get extra money
                AwardTeamMoney(TeamSide.CounterTerrorist, 300);
                break;
        }
    }
    
    private int CalculateLossBonus(TeamSide team)
    {
        int losses = consecutiveLosses[team];
        int bonus = settings.lossBonus + (losses * settings.consecutiveLossBonus);
        return Mathf.Min(bonus, settings.maxConsecutiveLossBonus);
    }
    
    private void AwardTeamMoney(TeamSide team, int amount)
    {
        NetworkPlayerController[] players = FindPlayersOfTeam(team);
        foreach (NetworkPlayerController player in players)
        {
            player.money = Mathf.Min(settings.maxMoney, player.money + amount);
        }
    }
    
    public void AwardKillMoney(NetworkPlayerController killer, NetworkPlayerController victim)
    {
        if (killer.team != victim.team)
        {
            killer.money = Mathf.Min(settings.maxMoney, killer.money + settings.killReward);
        }
    }
    
    public void AwardBombPlantMoney(NetworkPlayerController planter)
    {
        planter.money = Mathf.Min(settings.maxMoney, planter.money + settings.bombPlantReward);
    }
    
    public void AwardBombDefuseMoney(NetworkPlayerController defuser)
    {
        defuser.money = Mathf.Min(settings.maxMoney, defuser.money + settings.bombDefuseReward);
    }
    
    private NetworkPlayerController[] FindPlayersOfTeam(TeamSide team)
    {
        return System.Array.FindAll(FindObjectsOfType<NetworkPlayerController>(), 
            p => p.team == team);
    }
}
```

---

## Phase 8: Map Design and Level Creation

### Step 8.1: Level Design Principles

#### Counter-Strike Map Design Guidelines
1. **Three-Lane Structure**: Most CS maps follow a three-lane layout
   - Long range lane (for snipers)
   - Medium range lane (for rifles)
   - Close quarters lane (for SMGs/shotguns)

2. **Bomb Site Design**
   - Multiple entry points (2-4 routes)
   - Defensive positions for CTs
   - Clear sight lines but with cover
   - Balanced distance from T spawn

3. **Timing and Rotations**
   - T's should reach bomb sites slightly before CTs
   - Rotation times between sites should be balanced
   - Mid-map control should be valuable

### Step 8.2: Using ProBuilder for Level Creation

#### Setting up ProBuilder
```csharp
// Install ProBuilder from Package Manager
// Window → ProBuilder → ProBuilder Window

// Basic ProBuilder workflow:
// 1. Create shapes with ProBuilder
// 2. Edit geometry with ProBuilder tools
// 3. Apply materials
// 4. Add collision
// 5. Set up lighting
```

#### Level Creation Script
```csharp
using UnityEngine;
using UnityEngine.ProBuilder;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Settings")]
    public Vector3 mapSize = new Vector3(100, 20, 100);
    public Material wallMaterial;
    public Material floorMaterial;
    public Material ceilingMaterial;
    
    [Header("Bomb Sites")]
    public Transform bombSiteA;
    public Transform bombSiteB;
    
    [Header("Spawn Points")]
    public Transform[] terroristSpawns;
    public Transform[] counterTerroristSpawns;
    
    public void GenerateBasicLevel()
    {
        CreateFloor();
        CreateWalls();
        CreateBombSites();
        SetupLighting();
        SetupNavMesh();
    }
    
    private void CreateFloor()
    {
        GameObject floor = ShapeGenerator.CreateShape(ShapeType.Cube);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(mapSize.x, 1, mapSize.z);
        floor.transform.position = new Vector3(0, -0.5f, 0);
        
        // Apply material
        ProBuilderMesh mesh = floor.GetComponent<ProBuilderMesh>();
        Material[] materials = new Material[mesh.faces.Count];
        for (int i = 0; i < materials.Length; i++)
            materials[i] = floorMaterial;
        mesh.renderer.materials = materials;
    }
    
    private void CreateWalls()
    {
        // Create perimeter walls
        CreateWall(new Vector3(-mapSize.x/2, mapSize.y/2, 0), new Vector3(1, mapSize.y, mapSize.z));
        CreateWall(new Vector3(mapSize.x/2, mapSize.y/2, 0), new Vector3(1, mapSize.y, mapSize.z));
        CreateWall(new Vector3(0, mapSize.y/2, -mapSize.z/2), new Vector3(mapSize.x, mapSize.y, 1));
        CreateWall(new Vector3(0, mapSize.y/2, mapSize.z/2), new Vector3(mapSize.x, mapSize.y, 1));
    }
    
    private void CreateWall(Vector3 position, Vector3 scale)
    {
        GameObject wall = ShapeGenerator.CreateShape(ShapeType.Cube);
        wall.name = "Wall";
        wall.transform.position = position;
        wall.transform.localScale = scale;
        
        // Apply material
        ProBuilderMesh mesh = wall.GetComponent<ProBuilderMesh>();
        Material[] materials = new Material[mesh.faces.Count];
        for (int i = 0; i < materials.Length; i++)
            materials[i] = wallMaterial;
        mesh.renderer.materials = materials;
        
        // Add collision
        wall.AddComponent<BoxCollider>();
    }
    
    private void CreateBombSites()
    {
        // Create bomb site A
        CreateBombSite(bombSiteA.position, "Bomb Site A");
        
        // Create bomb site B
        CreateBombSite(bombSiteB.position, "Bomb Site B");
    }
    
    private void CreateBombSite(Vector3 position, string siteName)
    {
        GameObject site = new GameObject(siteName);
        site.transform.position = position;
        
        // Add bomb site component
        BombSite bombSiteComponent = site.AddComponent<BombSite>();
        bombSiteComponent.siteName = siteName;
        
        // Add trigger collider
        BoxCollider trigger = site.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(8, 4, 8);
    }
    
    private void SetupLighting()
    {
        // Create directional light
        GameObject light = new GameObject("Directional Light");
        Light lightComponent = light.AddComponent<Light>();
        lightComponent.type = LightType.Directional;
        lightComponent.intensity = 1.5f;
        light.transform.rotation = Quaternion.Euler(45, 45, 0);
        
        // Setup lightmapping
        Lightmapping.BakeAsync();
    }
    
    private void SetupNavMesh()
    {
        // Mark surfaces as walkable
        NavMeshSurface navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
    }
}

[System.Serializable]
public class BombSite : MonoBehaviour
{
    public string siteName;
    public bool canPlantBomb = true;
    
    private void OnTriggerEnter(Collider other)
    {
        NetworkPlayerController player = other.GetComponent<NetworkPlayerController>();
        if (player != null && player.team == TeamSide.Terrorist)
        {
            // Show plant bomb UI
            UIManager.Instance.ShowPlantBombUI(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        NetworkPlayerController player = other.GetComponent<NetworkPlayerController>();
        if (player != null)
        {
            UIManager.Instance.ShowPlantBombUI(false);
        }
    }
}
```

### Step 8.3: Optimization and LOD System

#### Level Optimization Script
```csharp
using UnityEngine;

public class LevelOptimizer : MonoBehaviour
{
    [Header("LOD Settings")]
    public float lodDistances = { 50f, 100f, 200f };
    public Material simplifiedMaterial;
    
    [Header("Culling")]
    public LayerMask cullingMask = -1;
    public float cullingDistance = 500f;
    
    private void Start()
    {
        SetupLODGroups();
        SetupOcclusionCulling();
    }
    
    private void SetupLODGroups()
    {
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer.gameObject.layer == LayerMask.NameToLayer("Environment"))
            {
                CreateLODGroup(renderer.gameObject);
            }
        }
    }
    
    private void CreateLODGroup(GameObject obj)
    {
        LODGroup lodGroup = obj.AddComponent<LODGroup>();
        
        LOD[] lods = new LOD[3];
        
        // LOD 0 - High detail
        lods[0] = new LOD(0.5f, new Renderer[] { obj.GetComponent<Renderer>() });
        
        // LOD 1 - Medium detail (simplified mesh)
        GameObject lodMedium = CreateSimplifiedMesh(obj, 0.5f);
        lods[1] = new LOD(0.2f, new Renderer[] { lodMedium.GetComponent<Renderer>() });
        
        // LOD 2 - Low detail (very simplified)
        GameObject lodLow = CreateSimplifiedMesh(obj, 0.1f);
        lods[2] = new LOD(0.05f, new Renderer[] { lodLow.GetComponent<Renderer>() });
        
        lodGroup.SetLODs(lods);
    }
    
    private GameObject CreateSimplifiedMesh(GameObject original, float quality)
    {
        GameObject simplified = Instantiate(original, original.transform.parent);
        simplified.name = original.name + "_LOD";
        
        // Apply simplified material
        Renderer renderer = simplified.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = simplifiedMaterial;
        }
        
        return simplified;
    }
    
    private void SetupOcclusionCulling()
    {
        // Mark objects as occluders/occludees
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer.gameObject.layer == LayerMask.NameToLayer("Environment"))
            {
                renderer.gameObject.isStatic = true;
            }
        }
        
        // Bake occlusion culling
        #if UNITY_EDITOR
        UnityEditor.StaticOcclusionCulling.Compute();
        #endif
    }
}
```

---

## Phase 9: User Interface and HUD

### Step 9.1: HUD System

#### UIManager.cs
```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("HUD Elements")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI armorText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI roundTimeText;
    public TextMeshProUGUI roundText;
    public TextMeshProUGUI bombTimerText;
    
    [Header("Crosshair")]
    public Image crosshair;
    public Color defaultCrosshairColor = Color.white;
    public Color hitCrosshairColor = Color.red;
    
    [Header("Team Panels")]
    public GameObject terroristPanel;
    public GameObject counterTerroristPanel;
    public Transform terroristPlayerList;
    public Transform ctPlayerList;
    
    [Header("Buy Menu")]
    public GameObject buyMenu;
    public Transform weaponGrid;
    public Button buyMenuButton;
    
    [Header("Death Screen")]
    public GameObject deathScreen;
    public TextMeshProUGUI killerText;
    public Button spectateButton;
    
    [Header("Message System")]
    public GameObject messagePanel;
    public TextMeshProUGUI messageText;
    
    [Header("Bomb UI")]
    public GameObject bombTimerPanel;
    public GameObject plantBombUI;
    public GameObject defuseProgressBar;
    public Slider defuseSlider;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetupUI();
    }
    
    private void SetupUI()
    {
        // Hide all panels initially
        buyMenu.SetActive(false);
        deathScreen.SetActive(false);
        bombTimerPanel.SetActive(false);
        plantBombUI.SetActive(false);
        defuseProgressBar.SetActive(false);
        
        // Setup buy menu button
        buyMenuButton.onClick.AddListener(ToggleBuyMenu);
        
        // Setup spectate button
        spectateButton.onClick.AddListener(StartSpectating);
    }
    
    public void UpdateHealth(int health)
    {
        healthText.text = health.ToString();
        
        // Change color based on health
        if (health <= 20)
            healthText.color = Color.red;
        else if (health <= 50)
            healthText.color = Color.yellow;
        else
            healthText.color = Color.white;
    }
    
    public void UpdateArmor(int armor)
    {
        armorText.text = armor.ToString();
        armorText.gameObject.SetActive(armor > 0);
    }
    
    public void UpdateMoney(int money)
    {
        moneyText.text = "$" + money.ToString();
    }
    
    public void UpdateAmmo(int currentAmmo, int totalAmmo)
    {
        ammoText.text = $"{currentAmmo} / {totalAmmo}";
    }
    
    public void UpdateRoundTime(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        roundTimeText.text = $"{minutes:00}:{seconds:00}";
    }
    
    public void UpdateRoundInfo(int round, int tScore, int ctScore)
    {
        roundText.text = $"Round {round} - T: {tScore} CT: {ctScore}";
    }
    
    public void ShowBombTimer(bool show)
    {
        bombTimerPanel.SetActive(show);
    }
    
    public void UpdateBombTimer(float timeRemaining)
    {
        bombTimerText.text = timeRemaining.ToString("F1");
        
        // Pulse effect when low time
        if (timeRemaining <= 10f)
        {
            float scale = 1f + Mathf.Sin(Time.time * 10f) * 0.1f;
            bombTimerText.transform.localScale = Vector3.one * scale;
        }
    }
    
    public void ShowPlantBombUI(bool show)
    {
        plantBombUI.SetActive(show);
    }
    
    public void ShowDefuseProgress(bool show)
    {
        defuseProgressBar.SetActive(show);
    }
    
    public void UpdateDefuseProgress(float progress)
    {
        defuseSlider.value = progress;
    }
    
    public void ShowMessage(string message, float duration = 3f)
    {
        messageText.text = message;
        messagePanel.SetActive(true);
        
        Invoke(nameof(HideMessage), duration);
    }
    
    private void HideMessage()
    {
        messagePanel.SetActive(false);
    }
    
    public void ShowDeathScreen(string killerName)
    {
        deathScreen.SetActive(true);
        killerText.text = $"Killed by {killerName}";
    }
    
    public void HideDeathScreen()
    {
        deathScreen.SetActive(false);
    }
    
    public void FlashCrosshair()
    {
        StartCoroutine(CrosshairFlash());
    }
    
    private System.Collections.IEnumerator CrosshairFlash()
    {
        crosshair.color = hitCrosshairColor;
        yield return new WaitForSeconds(0.1f);
        crosshair.color = defaultCrosshairColor;
    }
    
    private void ToggleBuyMenu()
    {
        buyMenu.SetActive(!buyMenu.activeSelf);
        
        if (buyMenu.activeSelf)
        {
            SetupBuyMenu();
        }
    }
    
    private void SetupBuyMenu()
    {
        // Clear existing items
        foreach (Transform child in weaponGrid)
        {
            Destroy(child.gameObject);
        }
        
        // Add weapon buttons
        WeaponData[] weapons = WeaponDatabase.Instance.GetAvailableWeapons();
        foreach (WeaponData weapon in weapons)
        {
            CreateWeaponButton(weapon);
        }
    }
    
    private void CreateWeaponButton(WeaponData weapon)
    {
        GameObject buttonObj = Instantiate(weaponButtonPrefab, weaponGrid);
        Button button = buttonObj.GetComponent<Button>();
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        
        buttonText.text = $"{weapon.name}\n${weapon.price}";
        
        button.onClick.AddListener(() => PurchaseWeapon(weapon));
        
        // Disable if can't afford
        NetworkPlayerController localPlayer = FindLocalPlayer();
        if (localPlayer.money < weapon.price)
        {
            button.interactable = false;
        }
    }
    
    private void PurchaseWeapon(WeaponData weapon)
    {
        NetworkPlayerController localPlayer = FindLocalPlayer();
        localPlayer.CmdPurchaseWeapon(weapon.id);
        
        buyMenu.SetActive(false);
    }
    
    private void StartSpectating()
    {
        SpectatorCamera.Instance.StartSpectating();
        deathScreen.SetActive(false);
    }
    
    private NetworkPlayerController FindLocalPlayer()
    {
        NetworkPlayerController[] players = FindObjectsOfType<NetworkPlayerController>();
        foreach (NetworkPlayerController player in players)
        {
            if (player.isLocalPlayer)
                return player;
        }
        return null;
    }
}
```

### Step 9.2: Buy Menu System

#### WeaponDatabase.cs
```csharp
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "CS Game/Weapon Database")]
public class WeaponDatabase : ScriptableObject
{
    public static WeaponDatabase Instance { get; private set; }
    
    [Header("Pistols")]
    public WeaponData[] pistols;
    
    [Header("Rifles")]
    public WeaponData[] rifles;
    
    [Header("SMGs")]
    public WeaponData[] smgs;
    
    [Header("Shotguns")]
    public WeaponData[] shotguns;
    
    [Header("Sniper Rifles")]
    public WeaponData[] sniperRifles;
    
    [Header("Equipment")]
    public EquipmentData[] equipment;
    
    private void OnEnable()
    {
        Instance = this;
    }
    
    public WeaponData[] GetAvailableWeapons()
    {
        List<WeaponData> allWeapons = new List<WeaponData>();
        allWeapons.AddRange(pistols);
        allWeapons.AddRange(rifles);
        allWeapons.AddRange(smgs);
        allWeapons.AddRange(shotguns);
        allWeapons.AddRange(sniperRifles);
        
        return allWeapons.ToArray();
    }
    
    public WeaponData GetWeaponById(string id)
    {
        WeaponData[] allWeapons = GetAvailableWeapons();
        foreach (WeaponData weapon in allWeapons)
        {
            if (weapon.id == id)
                return weapon;
        }
        return null;
    }
}

[System.Serializable]
public class WeaponData
{
    public string id;
    public string name;
    public int price;
    public int damage;
    public float fireRate;
    public float accuracy;
    public int magazineSize;
    public int totalAmmo;
    public float reloadTime;
    public WeaponType type;
    public GameObject prefab;
    public Sprite icon;
}

[System.Serializable]
public class EquipmentData
{
    public string id;
    public string name;
    public int price;
    public EquipmentType type;
    public GameObject prefab;
    public Sprite icon;
}

public enum WeaponType
{
    Pistol,
    Rifle,
    SMG,
    Shotgun,
    SniperRifle
}

public enum EquipmentType
{
    Armor,
    Helmet,
    Grenade,
    Flashbang,
    Smoke,
    Defuser
}
```

---

## Phase 10: Audio System

### Step 10.1: Audio Manager

#### AudioManager.cs
```csharp
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource uiSource;
    public AudioSource announcerSource;
    
    [Header("Audio Settings")]
    public float masterVolume = 1f;
    public float musicVolume = 0.7f;
    public float sfxVolume = 1f;
    public float voiceVolume = 1f;
    
    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip[] roundStartMusic;
    public AudioClip[] bombPlantedMusic;
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;
    
    [Header("UI Sounds")]
    public AudioClip buttonHover;
    public AudioClip buttonClick;
    public AudioClip purchaseSound;
    public AudioClip errorSound;
    
    [Header("Game Sounds")]
    public AudioClip roundStartSound;
    public AudioClip bombPlantedSound;
    public AudioClip bombDefusedSound;
    public AudioClip bombExplodedSound;
    
    [Header("Voice Lines")]
    public AudioClip[] terroristSpawnLines;
    public AudioClip[] ctSpawnLines;
    public AudioClip[] roundWinLines;
    public AudioClip[] roundLoseLines;
    
    private Dictionary<string, AudioClip> soundLibrary;
    private List<AudioSource> audioSourcePool;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeAudioManager()
    {
        soundLibrary = new Dictionary<string, AudioClip>();
        audioSourcePool = new List<AudioSource>();
        
        // Create audio source pool
        for (int i = 0; i < 20; i++)
        {
            CreatePooledAudioSource();
        }
        
        // Build sound library
        BuildSoundLibrary();
        
        // Apply volume settings
        ApplyVolumeSettings();
    }
    
    private void CreatePooledAudioSource()
    {
        GameObject audioObj = new GameObject("PooledAudioSource");
        audioObj.transform.SetParent(transform);
        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        audioSourcePool.Add(source);
    }
    
    private void BuildSoundLibrary()
    {
        // Add all sounds to library for easy access
        soundLibrary["ButtonHover"] = buttonHover;
        soundLibrary["ButtonClick"] = buttonClick;
        soundLibrary["Purchase"] = purchaseSound;
        soundLibrary["Error"] = errorSound;
        soundLibrary["RoundStart"] = roundStartSound;
        soundLibrary["BombPlanted"] = bombPlantedSound;
        soundLibrary["BombDefused"] = bombDefusedSound;
        soundLibrary["BombExploded"] = bombExplodedSound;
    }
    
    public void PlaySound(string soundName, float volume = 1f)
    {
        if (soundLibrary.ContainsKey(soundName))
        {
            PlaySound(soundLibrary[soundName], volume);
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found in library!");
        }
    }
    
    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableAudioSource();
        if (source != null)
        {
            source.clip = clip;
            source.volume = volume * sfxVolume * masterVolume;
            source.Play();
        }
    }
    
    public void PlaySoundAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        
        AudioSource source = GetAvailableAudioSource();
        if (source != null)
        {
            source.transform.position = position;
            source.clip = clip;
            source.volume = volume * sfxVolume * masterVolume;
            source.spatialBlend = 1f; // 3D sound
            source.maxDistance = 50f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.Play();
        }
    }
    
    public void PlayRandomVoiceLine(AudioClip[] voiceLines)
    {
        if (voiceLines.Length > 0)
        {
            AudioClip randomClip = voiceLines[Random.Range(0, voiceLines.Length)];
            announcerSource.clip = randomClip;
            announcerSource.volume = voiceVolume * masterVolume;
            announcerSource.Play();
        }
    }
    
    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicClip == null) return;
        
        musicSource.clip = musicClip;
        musicSource.loop = loop;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }
    
    public void StopMusic()
    {
        musicSource.Stop();
    }
    
    public void FadeOutMusic(float duration)
    {
        StartCoroutine(FadeAudioSource(musicSource, 0f, duration));
    }
    
    public void FadeInMusic(AudioClip musicClip, float duration)
    {
        musicSource.clip = musicClip;
        musicSource.volume = 0f;
        musicSource.Play();
        StartCoroutine(FadeAudioSource(musicSource, musicVolume * masterVolume, duration));
    }
    
    private System.Collections.IEnumerator FadeAudioSource(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            yield return null;
        }
        
        source.volume = targetVolume;
        
        if (targetVolume == 0f)
        {
            source.Stop();
        }
    }
    
    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource source in audioSourcePool)
        {
            if (!source.isPlaying)
            {
                // Reset source properties
                source.spatialBlend = 0f; // 2D by default
                source.pitch = 1f;
                return source;
            }
        }
        
        // If no available source, create a new one
        CreatePooledAudioSource();
        return audioSourcePool[audioSourcePool.Count - 1];
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume;
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
    
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        announcerSource.volume = voiceVolume * masterVolume;
    }
    
    private void ApplyVolumeSettings()
    {
        musicSource.volume = musicVolume * masterVolume;
        uiSource.volume = sfxVolume * masterVolume;
        announcerSource.volume = voiceVolume * masterVolume;
    }
    
    // Game event handlers
    public void OnRoundStart(TeamSide playerTeam)
    {
        PlaySound("RoundStart");
        
        if (playerTeam == TeamSide.Terrorist)
            PlayRandomVoiceLine(terroristSpawnLines);
        else
            PlayRandomVoiceLine(ctSpawnLines);
            
        if (roundStartMusic.Length > 0)
        {
            AudioClip randomMusic = roundStartMusic[Random.Range(0, roundStartMusic.Length)];
            PlayMusic(randomMusic, false);
        }
    }
    
    public void OnBombPlanted()
    {
        PlaySound("BombPlanted");
        
        if (bombPlantedMusic.Length > 0)
        {
            AudioClip tensionMusic = bombPlantedMusic[Random.Range(0, bombPlantedMusic.Length)];
            FadeOutMusic(1f);
            Invoke(nameof(() => PlayMusic(tensionMusic)), 1f);
        }
    }
    
    public void OnBombDefused()
    {
        PlaySound("BombDefused");
        StopMusic();
    }
    
    public void OnBombExploded()
    {
        PlaySound("BombExploded");
        StopMusic();
    }
    
    public void OnRoundEnd(bool playerTeamWon)
    {
        if (playerTeamWon)
        {
            PlayMusic(victoryMusic, false);
            PlayRandomVoiceLine(roundWinLines);
        }
        else
        {
            PlayMusic(defeatMusic, false);
            PlayRandomVoiceLine(roundLoseLines);
        }
    }
}
```

### Step 10.2: Dynamic Music System

#### MusicController.cs
```csharp
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [Header("Music Layers")]
    public AudioSource baseLayer;
    public AudioSource tensionLayer;
    public AudioSource actionLayer;
    
    [Header("Settings")]
    public float fadeSpeed = 2f;
    public float tensionThreshold = 30f; // Bomb timer threshold
    public float actionThreshold = 10f;  // Low health threshold
    
    private float basevolume = 0.5f;
    private float tensionVolume = 0f;
    private float actionVolume = 0f;
    
    private void Update()
    {
        UpdateMusicLayers();
    }
    
    private void UpdateMusicLayers()
    {
        // Get game state
        bool bombPlanted = GameManager.Instance.bombPlanted;
        float bombTimer = GameManager.Instance.bombTimeRemaining;
        int playerHealth = GetLocalPlayerHealth();
        
        // Calculate target volumes
        float targetBaseVolume = basevolume;
        float targetTensionVolume = 0f;
        float targetActionVolume = 0f;
        
        if (bombPlanted)
        {
            if (bombTimer <= actionThreshold)
            {
                targetActionVolume = 1f;
                targetTensionVolume = 0.3f;
                targetBaseVolume = 0.1f;
            }
            else if (bombTimer <= tensionThreshold)
            {
                targetTensionVolume = 0.7f;
                targetBaseVolume = 0.3f;
            }
        }
        
        if (playerHealth <= 20)
        {
            targetActionVolume = Mathf.Max(targetActionVolume, 0.5f);
        }
        
        // Smoothly transition volumes
        tensionVolume = Mathf.MoveTowards(tensionVolume, targetTensionVolume, fadeSpeed * Time.deltaTime);
        actionVolume = Mathf.MoveTowards(actionVolume, targetActionVolume, fadeSpeed * Time.deltaTime);
        
        // Apply volumes
        baseLayer.volume = targetBaseVolume * AudioManager.Instance.musicVolume;
        tensionLayer.volume = tensionVolume * AudioManager.Instance.musicVolume;
        actionLayer.volume = actionVolume * AudioManager.Instance.musicVolume;
    }
    
    private int GetLocalPlayerHealth()
    {
        NetworkPlayerController localPlayer = FindObjectOfType<NetworkPlayerController>();
        return localPlayer != null ? localPlayer.health : 100;
    }
}
```

---

## Phase 11: Polish and Optimization

### Step 11.1: Performance Optimization

#### PerformanceManager.cs
```csharp
using UnityEngine;
using UnityEngine.Rendering;

public class PerformanceManager : MonoBehaviour
{
    [Header("Quality Settings")]
    public bool enableDynamicQuality = true;
    public float targetFrameRate = 60f;
    public float qualityAdjustThreshold = 5f;
    
    [Header("LOD Settings")]
    public float lodBias = 1f;
    public int maximumLODLevel = 0;
    
    [Header("Shadow Settings")]
    public ShadowQuality shadowQuality = ShadowQuality.All;
    public ShadowResolution shadowResolution = ShadowResolution.Medium;
    public float shadowDistance = 100f;
    
    private float frameRateHistory = 60f;
    private float lastQualityAdjustTime = 0f;
    private int currentQualityLevel = 2; // 0=Low, 1=Medium, 2=High
    
    private void Start()
    {
        ApplyOptimalSettings();
        Application.targetFrameRate = (int)targetFrameRate;
    }
    
    private void Update()
    {
        if (enableDynamicQuality)
        {
            MonitorPerformance();
        }
    }
    
    private void MonitorPerformance()
    {
        // Calculate average frame rate
        frameRateHistory = Mathf.Lerp(frameRateHistory, 1f / Time.deltaTime, Time.deltaTime);
        
        // Adjust quality if needed
        if (Time.time - lastQualityAdjustTime > 2f) // Check every 2 seconds
        {
            if (frameRateHistory < targetFrameRate - qualityAdjustThreshold && currentQualityLevel > 0)
            {
                DecreaseQuality();
                lastQualityAdjustTime = Time.time;
            }
            else if (frameRateHistory > targetFrameRate + qualityAdjustThreshold && currentQualityLevel < 2)
            {
                IncreaseQuality();
                lastQualityAdjustTime = Time.time;
            }
        }
    }
    
    private void ApplyOptimalSettings()
    {
        // Detect hardware capabilities
        bool hasDiscreteGPU = SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2 &&
                             SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3;
        
        bool hasHighEndCPU = SystemInfo.processorCount >= 4 && SystemInfo.processorFrequency > 2000;
        
        // Set initial quality level based on hardware
        if (hasDiscreteGPU && hasHighEndCPU && SystemInfo.systemMemorySize > 8000)
        {
            currentQualityLevel = 2; // High
        }
        else if (hasDiscreteGPU || hasHighEndCPU)
        {
            currentQualityLevel = 1; // Medium
        }
        else
        {
            currentQualityLevel = 0; // Low
        }
        
        ApplyQualitySettings();
    }
    
    private void IncreaseQuality()
    {
        currentQualityLevel = Mathf.Min(2, currentQualityLevel + 1);
        ApplyQualitySettings();
        Debug.Log($"Quality increased to: {GetQualityName()}");
    }
    
    private void DecreaseQuality()
    {
        currentQualityLevel = Mathf.Max(0, currentQualityLevel - 1);
        ApplyQualitySettings();
        Debug.Log($"Quality decreased to: {GetQualityName()}");
    }
    
    private void ApplyQualitySettings()
    {
        switch (currentQualityLevel)
        {
            case 0: // Low Quality
                QualitySettings.SetQualityLevel(0);
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.shadowDistance = 50f;
                QualitySettings.lodBias = 0.5f;
                QualitySettings.maximumLODLevel = 2;
                QualitySettings.particleRaycastBudget = 16;
                break;
                
            case 1: // Medium Quality
                QualitySettings.SetQualityLevel(3);
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Medium;
                QualitySettings.shadowDistance = 75f;
                QualitySettings.lodBias = 0.75f;
                QualitySettings.maximumLODLevel = 1;
                QualitySettings.particleRaycastBudget = 64;
                break;
                
            case 2: // High Quality
                QualitySettings.SetQualityLevel(5);
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.High;
                QualitySettings.shadowDistance = 100f;
                QualitySettings.lodBias = 1f;
                QualitySettings.maximumLODLevel = 0;
                QualitySettings.particleRaycastBudget = 256;
                break;
        }
    }
    
    private string GetQualityName()
    {
        string[] names = { "Low", "Medium", "High" };
        return names[currentQualityLevel];
    }
}
```

### Step 11.2: Object Pooling System

#### ObjectPool.cs
```csharp
using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }
    
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }
    
    [Header("Pools")]
    public Pool[] pools;
    
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        
        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                objectPool.Enqueue(obj);
            }
            
            poolDictionary.Add(pool.tag, objectPool);
        }
    }
    
    public GameObject Get(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist!");
            return null;
        }
        
        Queue<GameObject> pool = poolDictionary[tag];
        
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // Pool is empty, create new object
            Pool poolData = System.Array.Find(pools, p => p.tag == tag);
            if (poolData != null)
            {
                GameObject obj = Instantiate(poolData.prefab);
                return obj;
            }
        }
        
        return null;
    }
    
    public void ReturnToPool(GameObject obj, string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Destroy(obj);
            return;
        }
        
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolDictionary[tag].Enqueue(obj);
    }
    
    public void ReturnToPool(GameObject obj, float delay)
    {
        StartCoroutine(ReturnAfterDelay(obj, delay));
    }
    
    private System.Collections.IEnumerator ReturnAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Try to find which pool this object belongs to
        foreach (Pool pool in pools)
        {
            if (obj.name.Contains(pool.prefab.name))
            {
                ReturnToPool(obj, pool.tag);
                yield break;
            }
        }
        
        // If not found, just destroy it
        Destroy(obj);
    }
}
```

---

## Advanced Features

### Spectator Camera System

#### SpectatorCamera.cs
```csharp
using UnityEngine;

public class SpectatorCamera : MonoBehaviour
{
    public static SpectatorCamera Instance { get; private set; }
    
    [Header("Spectator Settings")]
    public float moveSpeed = 10f;
    public float lookSensitivity = 2f;
    public float zoomSpeed = 5f;
    public KeyCode spectateKey = KeyCode.Space;
    
    private Camera spectatorCam;
    private bool isSpectating = false;
    private NetworkPlayerController[] alivePlayers;
    private int currentSpectatedIndex = 0;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            spectatorCam = GetComponent<Camera>();
            spectatorCam.enabled = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (isSpectating)
        {
            HandleSpectatorInput();
        }
    }
    
    public void StartSpectating()
    {
        isSpectating = true;
        spectatorCam.enabled = true;
        
        // Disable player camera
        NetworkPlayerController localPlayer = FindLocalPlayer();
        if (localPlayer != null)
        {
            Camera playerCam = localPlayer.GetComponentInChildren<Camera>();
            if (playerCam != null)
                playerCam.enabled = false;
        }
        
        UpdateAlivePlayers();
        SpectateNextPlayer();
    }
    
    public void StopSpectating()
    {
        isSpectating = false;
        spectatorCam.enabled = false;
    }
    
    private void HandleSpectatorInput()
    {
        if (Input.GetKeyDown(spectateKey))
        {
            SpectateNextPlayer();
        }
        
        // Free camera movement when not spectating a player
        if (currentSpectatedIndex == -1)
        {
            HandleFreeCameraMovement();
        }
    }
    
    private void SpectateNextPlayer()
    {
        UpdateAlivePlayers();
        
        if (alivePlayers.Length == 0)
        {
            // No alive players, use free camera
            currentSpectatedIndex = -1;
            return;
        }
        
        currentSpectatedIndex = (currentSpectatedIndex + 1) % alivePlayers.Length;
        SpectatePlayer(alivePlayers[currentSpectatedIndex]);
    }
    
    private void SpectatePlayer(NetworkPlayerController player)
    {
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            transform.position = playerCamera.transform.position;
            transform.rotation = playerCamera.transform.rotation;
        }
    }
    
    private void HandleFreeCameraMovement()
    {
        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;
        
        transform.Rotate(-mouseY, mouseX, 0);
        
        // Movement
        Vector3 movement = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W)) movement += transform.forward;
        if (Input.GetKey(KeyCode.S)) movement -= transform.forward;
        if (Input.GetKey(KeyCode.A)) movement -= transform.right;
        if (Input.GetKey(KeyCode.D)) movement += transform.right;
        if (Input.GetKey(KeyCode.Q)) movement -= transform.up;
        if (Input.GetKey(KeyCode.E)) movement += transform.up;
        
        transform.position += movement.normalized * moveSpeed * Time.deltaTime;
        
        // Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        spectatorCam.fieldOfView = Mathf.Clamp(spectatorCam.fieldOfView - scroll * zoomSpeed, 20f, 100f);
    }
    
    private void UpdateAlivePlayers()
    {
        NetworkPlayerController[] allPlayers = FindObjectsOfType<NetworkPlayerController>();
        System.Collections.Generic.List<NetworkPlayerController> alive = new System.Collections.Generic.List<NetworkPlayerController>();
        
        foreach (NetworkPlayerController player in allPlayers)
        {
            if (player.isAlive && !player.isLocalPlayer)
                alive.Add(player);
        }
        
        alivePlayers = alive.ToArray();
    }
    
    private NetworkPlayerController FindLocalPlayer()
    {
        NetworkPlayerController[] players = FindObjectsOfType<NetworkPlayerController>();
        foreach (NetworkPlayerController player in players)
        {
            if (player.isLocalPlayer)
                return player;
        }
        return null;
    }
}
```

---

## Testing and Deployment

### Step 1: Testing Framework

#### GameTester.cs
```csharp
using UnityEngine;
using System.Collections;

public class GameTester : MonoBehaviour
{
    [Header("Test Settings")]
    public bool enableTesting = true;
    public KeyCode runTestsKey = KeyCode.F1;
    
    private void Update()
    {
        if (enableTesting && Input.GetKeyDown(runTestsKey))
        {
            StartCoroutine(RunAllTests());
        }
    }
    
    private IEnumerator RunAllTests()
    {
        Debug.Log("Starting game tests...");
        
        yield return StartCoroutine(TestPlayerMovement());
        yield return StartCoroutine(TestWeaponSystem());
        yield return StartCoroutine(TestNetworking());
        yield return StartCoroutine(TestGameModes());
        
        Debug.Log("All tests completed!");
    }
    
    private IEnumerator TestPlayerMovement()
    {
        Debug.Log("Testing player movement...");
        
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Vector3 startPos = player.transform.position;
            
            // Simulate movement input
            yield return new WaitForSeconds(1f);
            
            // Check if player can move
            bool canMove = Vector3.Distance(startPos, player.transform.position) > 0.1f;
            Debug.Log($"Player movement test: {(canMove ? "PASS" : "FAIL")}");
        }
        else
        {
            Debug.Log("Player movement test: FAIL - No player found");
        }
    }
    
    private IEnumerator TestWeaponSystem()
    {
        Debug.Log("Testing weapon system...");
        
        Weapon[] weapons = FindObjectsOfType<Weapon>();
        bool allWeaponsWorking = true;
        
        foreach (Weapon weapon in weapons)
        {
            if (!weapon.CanFire())
            {
                allWeaponsWorking = false;
                break;
            }
        }
        
        Debug.Log($"Weapon system test: {(allWeaponsWorking ? "PASS" : "FAIL")}");
        yield return null;
    }
    
    private IEnumerator TestNetworking()
    {
        Debug.Log("Testing networking...");
        
        bool networkingWorking = Mirror.NetworkManager.singleton != null &&
                                Mirror.NetworkManager.singleton.isNetworkActive;
        
        Debug.Log($"Networking test: {(networkingWorking ? "PASS" : "FAIL")}");
        yield return null;
    }
    
    private IEnumerator TestGameModes()
    {
        Debug.Log("Testing game modes...");
        
        BombDefusalGameMode gameMode = FindObjectOfType<BombDefusalGameMode>();
        bool gameModeWorking = gameMode != null;
        
        Debug.Log($"Game mode test: {(gameModeWorking ? "PASS" : "FAIL")}");
        yield return null;
    }
}
```

### Step 2: Build and Deployment

#### BuildManager.cs
```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class BuildManager : MonoBehaviour
{
    [MenuItem("CS Game/Build All Platforms")]
    public static void BuildAllPlatforms()
    {
        BuildWindows();
        BuildLinux();
        BuildMac();
    }
    
    [MenuItem("CS Game/Build Windows")]
    public static void BuildWindows()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetScenePaths();
        buildPlayerOptions.locationPathName = "Builds/Windows/CounterStrikeGame.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;
        
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log($"Windows build result: {report.summary.result}");
    }
    
    [MenuItem("CS Game/Build Linux")]
    public static void BuildLinux()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetScenePaths();
        buildPlayerOptions.locationPathName = "Builds/Linux/CounterStrikeGame";
        buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
        buildPlayerOptions.options = BuildOptions.None;
        
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log($"Linux build result: {report.summary.result}");
    }
    
    [MenuItem("CS Game/Build Mac")]
    public static void BuildMac()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetScenePaths();
        buildPlayerOptions.locationPathName = "Builds/Mac/CounterStrikeGame.app";
        buildPlayerOptions.target = BuildTarget.StandaloneOSX;
        buildPlayerOptions.options = BuildOptions.None;
        
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log($"Mac build result: {report.summary.result}");
    }
    
    private static string[] GetScenePaths()
    {
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenes;
    }
}
#endif
```

---

## Final Notes and Next Steps

### Performance Considerations
1. **Network Optimization**: Use client-side prediction and lag compensation
2. **Graphics Optimization**: Implement LOD system and occlusion culling
3. **Memory Management**: Use object pooling for frequently created/destroyed objects
4. **Audio Optimization**: Use compressed audio formats and audio streaming

### Additional Features to Consider
1. **Anti-cheat System**: Implement server-side validation for all actions
2. **Replay System**: Record and playback game matches
3. **Statistics Tracking**: Track player performance and match history
4. **Customization**: Allow weapon skins and player customization
5. **Matchmaking**: Implement skill-based matchmaking system

### Legal Considerations
- This guide is for educational purposes
- Ensure you have rights to all assets used
- Consider trademark issues when naming your game
- Implement proper age ratings and content warnings

### Community and Support
- Set up dedicated servers for multiplayer
- Create documentation for modding support
- Establish community forums and support channels
- Plan for regular updates and bug fixes

This comprehensive guide provides the foundation for creating a Counter-Strike style game. Each system can be expanded and refined based on your specific requirements and player feedback.