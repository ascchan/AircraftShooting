
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float maximumLifetime = 10f;
    [SerializeField] private bool destroyOnImpact = true;
    [SerializeField] private float destructionDelay = 0f;
    [SerializeField] private GameObject impactEffectPrefab;

    private bool hasCollided;

    private void Start()
    {
        Destroy(gameObject, maximumLifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasCollided)
        {
            return;
        }

        hasCollided = true;

        Debug.Log( gameObject.name + " hit " + collision.gameObject.name );

        Transform Aircraft = collision.transform.root;

        if (Aircraft.CompareTag("AircraftPrefab"))
        {
            Destroy(Aircraft.gameObject);
            Destroy(gameObject);
        }

        if (destroyOnImpact)
        {
            Destroy(gameObject, destructionDelay);
        }
    }
}
