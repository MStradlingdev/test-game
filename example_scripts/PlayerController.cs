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
    
    [Header("Advanced Movement")]
    public float accelerationSpeed = 10f;
    public float maxSpeed = 7.5f;
    public float friction = 4f;
    public float airAcceleration = 2f;
    public float stopSpeed = 1.5f;
    
    [Header("Look")]
    public float mouseSensitivity = 100f;
    public Transform cameraTransform;
    public float maxLookAngle = 80f;
    
    [Header("Audio")]
    public AudioClip[] footstepSounds;
    public AudioClip jumpSound;
    public AudioClip landSound;
    
    // Components
    private CharacterController controller;
    private PlayerInput input;
    private Camera playerCamera;
    private AudioSource audioSource;
    
    // Movement variables
    private Vector3 velocity;
    private Vector3 horizontalVelocity;
    private bool isGrounded;
    private bool wasGrounded;
    public bool isCrouching { get; private set; }
    private bool isRunning;
    private bool movementEnabled = true;
    
    // Look variables
    private float xRotation = 0f;
    
    // Player state
    public PlayerState state = PlayerState.Alive;
    public TeamSide team;
    public int money = 800;
    public int health = 100;
    public int armor = 0;
    
    // Footstep system
    private float footstepTimer = 0f;
    private float footstepInterval = 0.5f;
    
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
        playerCamera = GetComponentInChildren<Camera>();
        audioSource = GetComponent<AudioSource>();
        
        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
        
        // Initialize horizontal velocity
        horizontalVelocity = Vector3.zero;
    }
    
    private void Update()
    {
        if (state != PlayerState.Alive || !movementEnabled) return;
        
        CheckGrounded();
        HandleMovement();
        HandleLook();
        HandleInput();
        HandleFootsteps();
    }
    
    private void CheckGrounded()
    {
        wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;
        
        // Landing sound
        if (!wasGrounded && isGrounded && velocity.y < -5f)
        {
            PlaySound(landSound);
        }
    }
    
    private void HandleMovement()
    {
        // Apply gravity
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        // Get input
        Vector2 moveInput = input.moveInput;
        
        // Use advanced movement system
        HandleAdvancedMovement();
        
        // Jumping
        if (input.jumpPressed && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            PlaySound(jumpSound);
        }
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        
        // Combine horizontal and vertical movement
        Vector3 finalMovement = horizontalVelocity * Time.deltaTime + velocity * Time.deltaTime;
        controller.Move(finalMovement);
    }
    
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
        isRunning = input.runHeld && !isCrouching && input.moveInput.magnitude > 0;
        
        // Weapon actions
        WeaponManager weaponManager = GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            if (input.firePressed || input.fireHeld)
            {
                weaponManager.TryFireWeapon();
            }
            
            if (input.reloadPressed)
            {
                weaponManager.TryReloadWeapon();
            }
        }
        
        // Use/interact
        if (input.usePressed)
        {
            TryInteract();
        }
    }
    
    private void HandleFootsteps()
    {
        // Only play footsteps when moving on ground
        if (!isGrounded || horizontalVelocity.magnitude < 1f) return;
        
        footstepTimer -= Time.deltaTime;
        
        if (footstepTimer <= 0f)
        {
            // Adjust interval based on movement speed
            float speedMultiplier = horizontalVelocity.magnitude / walkSpeed;
            footstepInterval = isCrouching ? 0.8f : (0.6f / speedMultiplier);
            
            footstepTimer = footstepInterval;
            
            // Play random footstep sound
            if (footstepSounds.Length > 0)
            {
                AudioClip footstep = footstepSounds[Random.Range(0, footstepSounds.Length)];
                PlaySound(footstep, 0.7f);
            }
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
            // Check if there's room to stand up
            if (CanStandUp())
            {
                controller.height = 2f;
                cameraTransform.localPosition = new Vector3(0, 1.6f, 0);
            }
            else
            {
                isCrouching = true; // Stay crouched
            }
        }
    }
    
    private bool CanStandUp()
    {
        Vector3 capsuleBottom = transform.position + controller.center - Vector3.up * (controller.height / 2);
        Vector3 capsuleTop = capsuleBottom + Vector3.up * 2f; // Full standing height
        
        return !Physics.CheckCapsule(capsuleBottom, capsuleTop, controller.radius * 0.9f);
    }
    
    private void TryInteract()
    {
        // Raycast for interactable objects
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(this);
            }
            
            // Check for bomb sites
            BombSite bombSite = hit.collider.GetComponent<BombSite>();
            if (bombSite != null && team == TeamSide.Terrorist)
            {
                WeaponManager weaponManager = GetComponent<WeaponManager>();
                if (weaponManager != null && weaponManager.HasBomb())
                {
                    weaponManager.PlantBomb(bombSite);
                }
            }
        }
    }
    
    public void TakeDamage(int damage, PlayerController attacker = null)
    {
        if (state != PlayerState.Alive) return;
        
        // Apply armor reduction
        int actualDamage = CalculateDamageWithArmor(damage);
        
        health -= actualDamage;
        
        // Update UI
        UIManager.Instance.UpdateHealth(health);
        UIManager.Instance.UpdateArmor(armor);
        
        if (health <= 0)
        {
            Die(attacker);
        }
        else
        {
            // Play hurt sound/effect
            PlayHurtEffect();
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
        movementEnabled = false;
        
        // Show death screen
        string killerName = killer != null ? killer.name : "Unknown";
        UIManager.Instance.ShowDeathScreen(killerName);
        
        // Drop weapon
        WeaponManager weaponManager = GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.DropCurrentWeapon();
        }
        
        // Award kill to killer
        if (killer != null && killer.team != team)
        {
            EconomySystem.Instance.AwardKillMoney(killer.GetComponent<NetworkPlayerController>(), 
                                                 GetComponent<NetworkPlayerController>());
        }
        
        // Check round end
        GameManager.Instance.CheckRoundEnd();
    }
    
    public void Respawn()
    {
        state = PlayerState.Alive;
        health = 100;
        movementEnabled = false; // Will be enabled when round starts
        
        // Reset position
        Transform spawnPoint = GetSpawnPoint();
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
        
        // Reset camera
        xRotation = 0f;
        cameraTransform.localRotation = Quaternion.identity;
        
        // Reset movement
        velocity = Vector3.zero;
        horizontalVelocity = Vector3.zero;
        
        // Hide death screen
        UIManager.Instance.HideDeathScreen();
        
        // Update UI
        UIManager.Instance.UpdateHealth(health);
        UIManager.Instance.UpdateMoney(money);
    }
    
    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
    }
    
    private Transform GetSpawnPoint()
    {
        CSNetworkManager networkManager = FindObjectOfType<CSNetworkManager>();
        if (networkManager != null)
        {
            Transform[] spawnPoints = team == TeamSide.Terrorist ? 
                                    networkManager.spawnPointsT : networkManager.spawnPointsCT;
            
            if (spawnPoints.Length > 0)
            {
                return spawnPoints[Random.Range(0, spawnPoints.Length)];
            }
        }
        
        return null;
    }
    
    private void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
    
    private void PlayHurtEffect()
    {
        // Screen flash effect
        CameraEffects cameraEffects = GetComponent<CameraEffects>();
        if (cameraEffects != null)
        {
            cameraEffects.PlayDamageEffect();
        }
        
        // Crosshair flash
        UIManager.Instance.FlashCrosshair();
    }
    
    // Getters for other systems
    public bool IsMoving()
    {
        return horizontalVelocity.magnitude > 0.1f;
    }
    
    public bool IsRunning()
    {
        return isRunning;
    }
    
    public Vector3 GetVelocity()
    {
        return horizontalVelocity;
    }
}

public enum PlayerState
{
    Alive,
    Dead,
    Spectating
}

public interface IInteractable
{
    void Interact(PlayerController player);
}