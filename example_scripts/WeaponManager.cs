using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Slots")]
    public Transform weaponHolder;
    public int maxWeapons = 3;
    
    [Header("Starting Weapons")]
    public GameObject defaultPistol;
    public GameObject defaultKnife;
    
    [Header("Weapon Switching")]
    public KeyCode primaryWeaponKey = KeyCode.Alpha1;
    public KeyCode secondaryWeaponKey = KeyCode.Alpha2;
    public KeyCode meleeWeaponKey = KeyCode.Alpha3;
    public KeyCode bombKey = KeyCode.Alpha4;
    
    [Header("Audio")]
    public AudioClip weaponSwitchSound;
    public AudioClip noAmmoSound;
    
    // Current weapon system
    private List<Weapon> weapons = new List<Weapon>();
    private Weapon currentWeapon;
    private int currentWeaponIndex = 0;
    
    // Components
    private PlayerController playerController;
    private PlayerInput playerInput;
    private AudioSource audioSource;
    
    // Bomb
    private bool hasBomb = false;
    
    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerInput = GetComponent<PlayerInput>();
        audioSource = GetComponent<AudioSource>();
        
        InitializeStartingWeapons();
    }
    
    private void Update()
    {
        HandleWeaponSwitching();
        HandleWeaponInput();
    }
    
    private void InitializeStartingWeapons()
    {
        // Add default knife
        if (defaultKnife != null)
        {
            AddWeapon(defaultKnife);
        }
        
        // Add default pistol
        if (defaultPistol != null)
        {
            AddWeapon(defaultPistol);
            SwitchToWeapon(1); // Switch to pistol by default
        }
        
        // Give bomb to terrorist team
        if (playerController.team == TeamSide.Terrorist)
        {
            // Only one terrorist gets the bomb per round
            hasBomb = ShouldGiveBomb();
        }
    }
    
    private bool ShouldGiveBomb()
    {
        // Simple implementation - give to first terrorist
        // In a full implementation, this would be handled by the game mode
        PlayerController[] terrorists = FindObjectsOfType<PlayerController>();
        foreach (PlayerController terrorist in terrorists)
        {
            if (terrorist.team == TeamSide.Terrorist && terrorist != playerController)
            {
                WeaponManager otherManager = terrorist.GetComponent<WeaponManager>();
                if (otherManager != null && otherManager.hasBomb)
                {
                    return false; // Someone already has the bomb
                }
            }
        }
        return true;
    }
    
    private void HandleWeaponSwitching()
    {
        // Number key weapon switching
        if (Input.GetKeyDown(primaryWeaponKey))
        {
            TrySwitchToSlot(0);
        }
        else if (Input.GetKeyDown(secondaryWeaponKey))
        {
            TrySwitchToSlot(1);
        }
        else if (Input.GetKeyDown(meleeWeaponKey))
        {
            TrySwitchToSlot(2);
        }
        else if (Input.GetKeyDown(bombKey) && hasBomb)
        {
            // Switch to bomb for planting
            // Implementation depends on bomb system
        }
        
        // Mouse wheel weapon switching
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            SwitchToNextWeapon();
        }
        else if (scroll < 0f)
        {
            SwitchToPreviousWeapon();
        }
    }
    
    private void HandleWeaponInput()
    {
        if (currentWeapon == null) return;
        
        // Auto-fire weapons
        if (currentWeapon is AutoWeapon autoWeapon)
        {
            if (playerInput.fireHeld)
            {
                autoWeapon.TryFire();
            }
        }
        // Semi-auto weapons
        else if (playerInput.firePressed)
        {
            currentWeapon.Fire();
        }
        
        // Reload
        if (playerInput.reloadPressed)
        {
            currentWeapon.StartReload();
        }
    }
    
    public void TryFireWeapon()
    {
        if (currentWeapon == null) return;
        
        if (currentWeapon.CanFire())
        {
            currentWeapon.Fire();
            
            // Update ammo UI
            WeaponData weaponData = currentWeapon.GetWeaponData();
            UIManager.Instance.UpdateAmmo(weaponData.currentAmmo, weaponData.totalAmmo);
        }
        else
        {
            // Play no ammo sound
            if (currentWeapon.GetWeaponData().currentAmmo <= 0)
            {
                PlaySound(noAmmoSound);
            }
        }
    }
    
    public void TryReloadWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.StartReload();
        }
    }
    
    public bool AddWeapon(GameObject weaponPrefab)
    {
        if (weapons.Count >= maxWeapons)
        {
            // Drop current weapon to make room
            DropCurrentWeapon();
        }
        
        // Instantiate weapon
        GameObject weaponObj = Instantiate(weaponPrefab, weaponHolder);
        Weapon weapon = weaponObj.GetComponent<Weapon>();
        
        if (weapon != null)
        {
            weapon.Initialize(playerController);
            weapons.Add(weapon);
            
            // Hide weapon initially
            weaponObj.SetActive(false);
            
            return true;
        }
        
        return false;
    }
    
    public void RemoveWeapon(Weapon weapon)
    {
        if (weapons.Contains(weapon))
        {
            weapons.Remove(weapon);
            
            if (currentWeapon == weapon)
            {
                currentWeapon = null;
                SwitchToNextAvailableWeapon();
            }
            
            Destroy(weapon.gameObject);
        }
    }
    
    public void DropCurrentWeapon()
    {
        if (currentWeapon == null || currentWeapon is MeleeWeapon) return;
        
        // Create dropped weapon
        CreateDroppedWeapon(currentWeapon);
        
        // Remove from inventory
        RemoveWeapon(currentWeapon);
    }
    
    private void CreateDroppedWeapon(Weapon weapon)
    {
        // Create dropped weapon prefab
        GameObject droppedWeapon = Instantiate(weapon.droppedWeaponPrefab, 
            transform.position + transform.forward * 2f, transform.rotation);
        
        // Add physics
        Rigidbody rb = droppedWeapon.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(transform.forward * 5f + Vector3.up * 2f, ForceMode.Impulse);
        }
        
        // Set weapon data
        DroppedWeapon dropped = droppedWeapon.GetComponent<DroppedWeapon>();
        if (dropped != null)
        {
            dropped.SetWeaponData(weapon.GetWeaponData());
        }
    }
    
    private void TrySwitchToSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < weapons.Count)
        {
            SwitchToWeapon(slotIndex);
        }
    }
    
    private void SwitchToWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= weapons.Count) return;
        
        // Hide current weapon
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(false);
        }
        
        // Switch to new weapon
        currentWeaponIndex = weaponIndex;
        currentWeapon = weapons[currentWeaponIndex];
        currentWeapon.gameObject.SetActive(true);
        
        // Play switch sound
        PlaySound(weaponSwitchSound);
        
        // Update UI
        WeaponData weaponData = currentWeapon.GetWeaponData();
        UIManager.Instance.UpdateAmmo(weaponData.currentAmmo, weaponData.totalAmmo);
        
        // Update crosshair for weapon type
        UpdateCrosshair();
    }
    
    private void SwitchToNextWeapon()
    {
        if (weapons.Count <= 1) return;
        
        int nextIndex = (currentWeaponIndex + 1) % weapons.Count;
        SwitchToWeapon(nextIndex);
    }
    
    private void SwitchToPreviousWeapon()
    {
        if (weapons.Count <= 1) return;
        
        int prevIndex = currentWeaponIndex - 1;
        if (prevIndex < 0) prevIndex = weapons.Count - 1;
        
        SwitchToWeapon(prevIndex);
    }
    
    private void SwitchToNextAvailableWeapon()
    {
        if (weapons.Count == 0) return;
        
        // Find first available weapon
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i] != null)
            {
                SwitchToWeapon(i);
                return;
            }
        }
    }
    
    private void UpdateCrosshair()
    {
        if (currentWeapon == null) return;
        
        // Different crosshairs for different weapon types
        if (currentWeapon is SniperRifle)
        {
            // Hide crosshair for sniper rifles (they have scopes)
            UIManager.Instance.SetCrosshairVisible(false);
        }
        else
        {
            UIManager.Instance.SetCrosshairVisible(true);
            
            // Adjust crosshair size based on weapon accuracy
            float accuracy = currentWeapon.accuracy;
            UIManager.Instance.SetCrosshairSize(1f / accuracy);
        }
    }
    
    public bool HasBomb()
    {
        return hasBomb;
    }
    
    public void PlantBomb(BombSite bombSite)
    {
        if (!hasBomb) return;
        
        // Start bomb planting process
        StartCoroutine(PlantBombCoroutine(bombSite));
    }
    
    private System.Collections.IEnumerator PlantBombCoroutine(BombSite bombSite)
    {
        float plantTime = 3f;
        float timer = 0f;
        
        // Disable movement during planting
        playerController.SetMovementEnabled(false);
        
        // Show planting progress
        UIManager.Instance.ShowPlantProgress(true);
        
        while (timer < plantTime)
        {
            timer += Time.deltaTime;
            float progress = timer / plantTime;
            UIManager.Instance.UpdatePlantProgress(progress);
            
            // Check if player moved or took damage
            if (playerController.IsMoving() || playerController.health <= 0)
            {
                // Cancel planting
                UIManager.Instance.ShowPlantProgress(false);
                playerController.SetMovementEnabled(true);
                yield break;
            }
            
            yield return null;
        }
        
        // Plant successful
        hasBomb = false;
        UIManager.Instance.ShowPlantProgress(false);
        playerController.SetMovementEnabled(true);
        
        // Notify game mode
        BombDefusalGameMode gameMode = FindObjectOfType<BombDefusalGameMode>();
        if (gameMode != null)
        {
            gameMode.PlantBomb(playerController.GetComponent<NetworkPlayerController>(), bombSite.transform);
        }
    }
    
    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
    
    public List<Weapon> GetAllWeapons()
    {
        return new List<Weapon>(weapons);
    }
    
    public bool HasWeaponType(System.Type weaponType)
    {
        foreach (Weapon weapon in weapons)
        {
            if (weapon.GetType() == weaponType)
                return true;
        }
        return false;
    }
    
    public void PurchaseWeapon(string weaponId)
    {
        WeaponData weaponData = WeaponDatabase.Instance.GetWeaponById(weaponId);
        if (weaponData == null) return;
        
        // Check if player can afford it
        if (playerController.money < weaponData.price) return;
        
        // Check weapon restrictions
        if (!CanPurchaseWeapon(weaponData)) return;
        
        // Deduct money
        playerController.money -= weaponData.price;
        UIManager.Instance.UpdateMoney(playerController.money);
        
        // Add weapon to inventory
        AddWeapon(weaponData.prefab);
        
        // Switch to new weapon
        SwitchToWeapon(weapons.Count - 1);
        
        // Play purchase sound
        AudioManager.Instance.PlaySound("Purchase");
    }
    
    private bool CanPurchaseWeapon(WeaponData weaponData)
    {
        // Check if player already has this type of weapon
        switch (weaponData.type)
        {
            case WeaponType.Rifle:
            case WeaponType.SMG:
            case WeaponType.Shotgun:
            case WeaponType.SniperRifle:
                // Only one primary weapon
                return !HasPrimaryWeapon();
                
            case WeaponType.Pistol:
                // Only one secondary weapon (excluding default)
                return !HasSecondaryWeapon();
                
            default:
                return true;
        }
    }
    
    private bool HasPrimaryWeapon()
    {
        foreach (Weapon weapon in weapons)
        {
            if (weapon is AssaultRifle || weapon is SMG || weapon is Shotgun || weapon is SniperRifle)
                return true;
        }
        return false;
    }
    
    private bool HasSecondaryWeapon()
    {
        foreach (Weapon weapon in weapons)
        {
            if (weapon is Pistol && weapon != weapons[1]) // Exclude default pistol
                return true;
        }
        return false;
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Called when player picks up a weapon
    public void OnWeaponPickup(DroppedWeapon droppedWeapon)
    {
        WeaponData weaponData = droppedWeapon.GetWeaponData();
        GameObject weaponPrefab = WeaponDatabase.Instance.GetWeaponById(weaponData.id).prefab;
        
        if (AddWeapon(weaponPrefab))
        {
            // Set ammo from dropped weapon
            Weapon newWeapon = weapons[weapons.Count - 1];
            newWeapon.SetAmmo(weaponData.currentAmmo, weaponData.totalAmmo);
            
            // Switch to picked up weapon
            SwitchToWeapon(weapons.Count - 1);
            
            // Destroy dropped weapon
            Destroy(droppedWeapon.gameObject);
        }
    }
}

// Dropped weapon component
public class DroppedWeapon : MonoBehaviour, IInteractable
{
    [Header("Pickup")]
    public float pickupRange = 2f;
    public KeyCode pickupKey = KeyCode.E;
    
    private WeaponData weaponData;
    
    public void SetWeaponData(WeaponData data)
    {
        weaponData = data;
    }
    
    public WeaponData GetWeaponData()
    {
        return weaponData;
    }
    
    public void Interact(PlayerController player)
    {
        WeaponManager weaponManager = player.GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.OnWeaponPickup(this);
        }
    }
    
    private void Update()
    {
        // Simple rotation animation
        transform.Rotate(0, 30f * Time.deltaTime, 0);
    }
}