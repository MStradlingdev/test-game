using UnityEngine;
using System.Collections;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    public string weaponName;
    public string weaponId;
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
    public AudioClip emptySound;
    public GameObject droppedWeaponPrefab;
    
    [Header("Crosshair")]
    public Texture2D crosshairTexture;
    public Vector2 crosshairSize = Vector2.one;
    
    // Current state
    protected int currentAmmo;
    protected int currentTotalAmmo;
    protected bool isReloading;
    protected float nextFireTime;
    
    // Components
    protected AudioSource audioSource;
    protected PlayerController owner;
    protected WeaponAccuracy weaponAccuracy;
    
    protected virtual void Start()
    {
        audioSource = GetComponent<AudioSource>();
        weaponAccuracy = GetComponent<WeaponAccuracy>();
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
               owner != null &&
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
        
        // Update UI
        UIManager.Instance.UpdateAmmo(currentAmmo, currentTotalAmmo);
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
    
    protected virtual bool Raycast(out RaycastHit hit, Vector3 direction)
    {
        Ray ray = new Ray(firePoint.position, direction);
        return Physics.Raycast(ray, out hit, range, hitLayers);
    }
    
    protected virtual Vector3 GetFireDirection()
    {
        Vector3 baseDirection = firePoint.forward;
        
        // Apply weapon accuracy
        if (weaponAccuracy != null)
        {
            Vector3 accuracyOffset = weaponAccuracy.GetAccuracyOffset(owner);
            baseDirection += firePoint.right * accuracyOffset.x + firePoint.up * accuracyOffset.y;
        }
        
        return baseDirection.normalized;
    }
    
    protected virtual void ApplyDamage(RaycastHit hit)
    {
        PlayerController target = hit.collider.GetComponent<PlayerController>();
        if (target != null && target.team != owner.team)
        {
            int finalDamage = DamageSystem.CalculateWeaponDamage(this, hit, hit.distance);
            target.TakeDamage(finalDamage, owner);
            
            // Flash crosshair on hit
            UIManager.Instance.FlashCrosshair();
        }
    }
    
    public WeaponData GetWeaponData()
    {
        return new WeaponData
        {
            id = weaponId,
            name = weaponName,
            currentAmmo = currentAmmo,
            totalAmmo = currentTotalAmmo,
            isReloading = isReloading,
            damage = damage,
            fireRate = fireRate,
            accuracy = accuracy,
            price = price
        };
    }
    
    public virtual void SetAmmo(int newCurrentAmmo, int newTotalAmmo)
    {
        currentAmmo = newCurrentAmmo;
        currentTotalAmmo = newTotalAmmo;
    }
}

// Assault Rifle Implementation
public class AssaultRifle : Weapon
{
    [Header("Rifle Settings")]
    public float recoilStrength = 2f;
    public float recoilRecovery = 5f;
    public AnimationCurve recoilPattern;
    public bool isAutomatic = true;
    
    private int shotsFired = 0;
    private Vector2 currentRecoil;
    private float lastShotTime;
    
    public override void Fire()
    {
        if (!CanFire()) return;
        
        nextFireTime = Time.time + (1f / fireRate);
        currentAmmo--;
        lastShotTime = Time.time;
        
        // Apply recoil
        ApplyRecoil();
        
        // Get fire direction with accuracy
        Vector3 fireDirection = GetFireDirection();
        
        // Perform raycast
        if (Raycast(out RaycastHit hit, fireDirection))
        {
            ApplyDamage(hit);
            CreateBulletHole(hit);
        }
        
        PlayFireEffect();
        
        // Notify accuracy system
        if (weaponAccuracy != null)
            weaponAccuracy.OnWeaponFired();
        
        // Update UI
        UIManager.Instance.UpdateAmmo(currentAmmo, currentTotalAmmo);
        
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
        if (owner != null && owner.cameraTransform != null)
        {
            owner.cameraTransform.localRotation *= Quaternion.Euler(-recoil.y, recoil.x, 0);
        }
    }
    
    private void Update()
    {
        // Recoil recovery
        if (shotsFired > 0 && Time.time >= lastShotTime + 0.1f)
        {
            shotsFired = Mathf.Max(0, shotsFired - 1);
            
            // Recover from recoil
            currentRecoil = Vector2.Lerp(currentRecoil, Vector2.zero, 
                recoilRecovery * Time.deltaTime);
        }
    }
    
    private void CreateBulletHole(RaycastHit hit)
    {
        // Create bullet hole decal
        GameObject bulletHole = ObjectPool.Instance.Get("BulletHole");
        if (bulletHole != null)
        {
            bulletHole.transform.position = hit.point + hit.normal * 0.01f;
            bulletHole.transform.rotation = Quaternion.LookRotation(hit.normal);
            
            // Return to pool after delay
            ObjectPool.Instance.ReturnToPool(bulletHole, 30f);
        }
    }
}

// Pistol Implementation
public class Pistol : Weapon
{
    [Header("Pistol Settings")]
    public float recoilStrength = 1f;
    public bool isSemiAuto = true;
    
    public override void Fire()
    {
        if (!CanFire()) return;
        
        nextFireTime = Time.time + (1f / fireRate);
        currentAmmo--;
        
        // Apply light recoil
        ApplyRecoil();
        
        // Get fire direction with accuracy
        Vector3 fireDirection = GetFireDirection();
        
        // Perform raycast
        if (Raycast(out RaycastHit hit, fireDirection))
        {
            ApplyDamage(hit);
        }
        
        PlayFireEffect();
        
        // Notify accuracy system
        if (weaponAccuracy != null)
            weaponAccuracy.OnWeaponFired();
        
        // Update UI
        UIManager.Instance.UpdateAmmo(currentAmmo, currentTotalAmmo);
        
        // Auto-reload if magazine is empty
        if (currentAmmo <= 0)
        {
            StartReload();
        }
    }
    
    private void ApplyRecoil()
    {
        Vector2 recoil = new Vector2(
            Random.Range(-recoilStrength * 0.5f, recoilStrength * 0.5f),
            recoilStrength
        );
        
        // Apply recoil to camera
        if (owner != null && owner.cameraTransform != null)
        {
            owner.cameraTransform.localRotation *= Quaternion.Euler(-recoil.y, recoil.x, 0);
        }
    }
}

// Sniper Rifle Implementation
public class SniperRifle : Weapon
{
    [Header("Sniper Settings")]
    public float scopeZoomFOV = 15f;
    public float recoilStrength = 5f;
    public bool isScoped = false;
    
    private Camera playerCamera;
    private float originalFOV;
    
    protected override void Start()
    {
        base.Start();
        
        if (owner != null)
        {
            playerCamera = owner.GetComponentInChildren<Camera>();
            if (playerCamera != null)
                originalFOV = playerCamera.fieldOfView;
        }
    }
    
    public override void Fire()
    {
        if (!CanFire()) return;
        
        nextFireTime = Time.time + (1f / fireRate);
        currentAmmo--;
        
        // Apply heavy recoil
        ApplyRecoil();
        
        // Get fire direction (very accurate when scoped)
        Vector3 fireDirection = isScoped ? firePoint.forward : GetFireDirection();
        
        // Perform raycast
        if (Raycast(out RaycastHit hit, fireDirection))
        {
            // Sniper rifles do more damage
            PlayerController target = hit.collider.GetComponent<PlayerController>();
            if (target != null && target.team != owner.team)
            {
                int sniperDamage = DamageSystem.CalculateWeaponDamage(this, hit, hit.distance);
                target.TakeDamage(sniperDamage, owner);
                UIManager.Instance.FlashCrosshair();
            }
        }
        
        PlayFireEffect();
        
        // Update UI
        UIManager.Instance.UpdateAmmo(currentAmmo, currentTotalAmmo);
        
        // Auto-reload if magazine is empty
        if (currentAmmo <= 0)
        {
            StartReload();
        }
    }
    
    public void ToggleScope()
    {
        isScoped = !isScoped;
        
        if (playerCamera != null)
        {
            if (isScoped)
            {
                playerCamera.fieldOfView = scopeZoomFOV;
                UIManager.Instance.SetCrosshairVisible(false);
            }
            else
            {
                playerCamera.fieldOfView = originalFOV;
                UIManager.Instance.SetCrosshairVisible(true);
            }
        }
    }
    
    private void ApplyRecoil()
    {
        Vector2 recoil = new Vector2(
            Random.Range(-recoilStrength * 0.3f, recoilStrength * 0.3f),
            recoilStrength
        );
        
        // Apply recoil to camera
        if (owner != null && owner.cameraTransform != null)
        {
            owner.cameraTransform.localRotation *= Quaternion.Euler(-recoil.y, recoil.x, 0);
        }
    }
    
    private void Update()
    {
        // Handle right-click for scope
        if (Input.GetMouseButtonDown(1))
        {
            ToggleScope();
        }
    }
}

// Melee Weapon Implementation
public class MeleeWeapon : Weapon
{
    [Header("Melee Settings")]
    public float meleeRange = 2f;
    public float swingSpeed = 1f;
    public AnimationCurve swingCurve;
    
    private bool isSwinging = false;
    
    public override void Fire()
    {
        if (!CanFire() || isSwinging) return;
        
        StartCoroutine(SwingCoroutine());
    }
    
    private IEnumerator SwingCoroutine()
    {
        isSwinging = true;
        nextFireTime = Time.time + (1f / fireRate);
        
        float swingTime = 0f;
        float maxSwingTime = 1f / swingSpeed;
        
        while (swingTime < maxSwingTime)
        {
            swingTime += Time.deltaTime;
            float progress = swingTime / maxSwingTime;
            
            // Apply swing animation curve
            float swingValue = swingCurve.Evaluate(progress);
            
            // Check for hits during swing
            if (progress > 0.3f && progress < 0.7f)
            {
                CheckMeleeHit();
            }
            
            yield return null;
        }
        
        isSwinging = false;
    }
    
    private void CheckMeleeHit()
    {
        // Sphere cast for melee hit detection
        if (Physics.SphereCast(firePoint.position, 0.5f, firePoint.forward, out RaycastHit hit, meleeRange, hitLayers))
        {
            PlayerController target = hit.collider.GetComponent<PlayerController>();
            if (target != null && target.team != owner.team)
            {
                // Melee weapons do high damage
                target.TakeDamage(damage, owner);
                UIManager.Instance.FlashCrosshair();
            }
        }
        
        PlayFireEffect();
    }
    
    public override void StartReload()
    {
        // Melee weapons don't reload
    }
}

// Auto Weapon Interface for automatic firing
public abstract class AutoWeapon : Weapon
{
    public virtual void TryFire()
    {
        if (CanFire())
        {
            Fire();
        }
    }
}