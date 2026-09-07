using UnityEngine;

public class Shooting : MonoBehaviour
{
    [SerializeField] private Rigidbody projectilePrefab;
    [SerializeField] private Transform weaponTip;

    [SerializeField] private float launchForce = 40f;
    [SerializeField] private float fireCooldown = 0.5f;

    [SerializeField] private bool allowLeftMouseButton = true;

    private void Update()
    {
        if ( Input.GetKeyDown(KeyCode.Space) )
        {
            FireProjectile();
        }
    }

    private void FireProjectile()
    {
        Rigidbody projectileInstance = Instantiate( projectilePrefab, weaponTip.position, weaponTip.rotation );

        projectileInstance.AddForce( weaponTip.forward * launchForce, ForceMode.Impulse );
    }
}
