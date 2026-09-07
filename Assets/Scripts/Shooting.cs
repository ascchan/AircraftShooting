using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField] private Rigidbody projectilePrefab;
    [SerializeField] private Transform weaponTip;

    [SerializeField] private float launchForce;
    [SerializeField] private float fireCooldown;
    [SerializeField] private GameObject WeaponTipEffect;
    [SerializeField] private float WeaponTipEffectLifeTime;

    private float nextFireTime;

    private void Update()
    {
        if ( Input.GetKeyDown(KeyCode.Space) && Time.time >= nextFireTime )
        {
            FireProjectile();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    private void FireProjectile()
    {
        Rigidbody projectileInstance = Instantiate( projectilePrefab, weaponTip.position, weaponTip.rotation );
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
}
