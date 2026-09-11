using UnityEngine;
using UnityEngine.UI;

public class Shooting : MonoBehaviour
{
    [SerializeField] private Rigidbody projectilePrefab;
    [SerializeField] private Transform weaponTip;

    [SerializeField] private float launchForce;
    [SerializeField] private float fireCooldown;
    [SerializeField] private GameObject WeaponTipEffect;
    [SerializeField] private float WeaponTipEffectLifeTime;
    [SerializeField] private GameManager gameManager;

    public bool allowFire;

    [SerializeField] private AircraftController aircraftController;

    private float nextFireTime;

    private bool controllerErrorReported;
 
    private void Update()
    {
        if ( Input.GetKeyDown(KeyCode.Space) && Time.time >= nextFireTime && allowFire )
        {     
            if (!aircraftController.IsFireActive)
            {
                return;
            }

            FireProjectile();
            nextFireTime = Time.time + fireCooldown;
        }

    }

    private void FireProjectile()
    {
        Rigidbody projectileInstance = Instantiate( projectilePrefab, weaponTip.position, weaponTip.rotation );
        
        gameManager.NotifyShotFired();

        projectileInstance.AddForce( weaponTip.forward * launchForce, ForceMode.VelocityChange );

        CreateWeaponTipEffect();
    }

    private void CreateWeaponTipEffect()
    {
        if (WeaponTipEffect != null)
        {
            GameObject effectInstance = Instantiate(WeaponTipEffect, weaponTip.position, weaponTip.rotation);
            Destroy(effectInstance, WeaponTipEffectLifeTime);
        }
    }

    public void AllowFireEnable()
    {
        allowFire = true;
    }
}
