
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float maximumLifetime;
    [SerializeField] private bool destroyOnImpact;
    [SerializeField] private float destructionDelay;
    [SerializeField] private GameObject impactEffectPrefab;

    private bool hasCollided;

    private void Start()
    {
        Destroy( gameObject, maximumLifetime );
    }

    private void OnCollisionEnter( Collision collision )
    {
        if( hasCollided )
        {
            return;
        }

        hasCollided = true;

        Debug.Log( gameObject.name + " hit " + collision.gameObject.name );

        GameObject aircraft = collision.transform.root.gameObject;

        if( aircraft.CompareTag("AircraftPrefab") )
        {
            hasCollided = true;
            ContactPoint contact = collision.GetContact(0);

            Instantiate( impactEffectPrefab, contact.point, Quaternion.LookRotation(contact.normal) );
        }

        Destroy(aircraft);
        Destroy(gameObject);

        if( destroyOnImpact )
        {
            Destroy( gameObject, destructionDelay );
        }
    }
}
