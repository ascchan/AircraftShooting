using System.Collections.Generic;
using UnityEngine;

public class Aircraft : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float turnSpeed;

    [SerializeField] private float minFloorAltitude;
    [SerializeField] private float groundClearance;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float avoidanceRadius;
    [SerializeField] private float avoidancePriority;

    [SerializeField] private Vector3 boundsCenter;
    [SerializeField] private Vector3 boundsSize;

    [SerializeField] private float posChangeInterval;

    private Vector3 randomNextPos;
    private float nextPosTimer;
    private List<Aircraft> checkAircraftList;

    public string modelName;


    private void Start()
    {
        PickNewRandomNextPos();
        nextPosTimer = Random.Range(0f, posChangeInterval);
    }

    public void CheckAircraftList( List<Aircraft> aircraftList )
    {
        checkAircraftList = aircraftList;
    }

    private void Update()
    {
        HandleNextPosTimer();
        Fly();
        FlyAboveGround();
    }

    private void HandleNextPosTimer()
    {
        nextPosTimer += Time.deltaTime;
        if( nextPosTimer >= posChangeInterval )
        {
            PickNewRandomNextPos();
            nextPosTimer = 0f;
        }
    }

    private void PickNewRandomNextPos()
    {
        float minY = Mathf.Max(
            boundsCenter.y - boundsSize.y * 0.5f, 
            minFloorAltitude + 5f
        );
        float maxY = boundsCenter.y + boundsSize.y * 0.5f;

        randomNextPos = new Vector3(
            Random.Range( 
                boundsCenter.x - boundsSize.x * 0.5f, 
                boundsCenter.x + boundsSize.x * 0.5f 
            ),
            Random.Range( minY, maxY ),
            Random.Range( 
                boundsCenter.z - boundsSize.z * 0.5f, 
                boundsCenter.z + boundsSize.z * 0.5f 
            )
        );
    }

    private void Fly()
    {
        Vector3 desiredDirection = ( randomNextPos - transform.position ).normalized;

        if( !IsWithinBounds(transform.position) )
        {
            desiredDirection = ( boundsCenter - transform.position ).normalized * 2f;
        }

        Vector3 fleetAvoidance = ComputeCollisionAvoidance();
        desiredDirection += fleetAvoidance * avoidancePriority;

        Vector3 groundUpForce = ComputeGroundAvoidance();
        desiredDirection += groundUpForce * 5f;

        if ( desiredDirection != Vector3.zero )
        {
            Quaternion targetRotation = Quaternion.LookRotation( 
                                            desiredDirection.normalized 
                                        );
            transform.rotation = Quaternion.Slerp( 
                                    transform.rotation, 
                                    targetRotation, 
                                    turnSpeed * Time.deltaTime 
                                 );
        }

        transform.Translate( Vector3.forward * (speed * Time.deltaTime), Space.Self );
    }

    private Vector3 ComputeGroundAvoidance()
    {
        Vector3 upForce = Vector3.zero;

        Vector3 sensorDirection = ( transform.forward + Vector3.down * 0.5f ).normalized;

        if( Physics.Raycast(
                transform.position, 
                sensorDirection, 
                out RaycastHit ground, 
                groundCheckDistance, 
                groundLayer
            ) )
        {
            float groundDist = ground.distance;

            float prior = 1f - Mathf.Clamp01( groundDist / groundCheckDistance );
            upForce = Vector3.up * prior;
        }
        else if( transform.position.y < minFloorAltitude + groundClearance )
        {
            float underAltitude = ( minFloorAltitude + groundClearance ) - transform.position.y;
            upForce = Vector3.up * ( underAltitude / groundClearance );
        }

        return upForce;
    }

    private void FlyAboveGround()
    {
        if( transform.position.y < minFloorAltitude )
        {
            Vector3 aboveGroundPos = transform.position;
            aboveGroundPos.y = minFloorAltitude;
            transform.position = aboveGroundPos;

            if( transform.forward.y < 0 )
            {
                Vector3 moveForward = new Vector3( 
                    transform.forward.x, 
                    0.1f, transform.forward.z 
                ).normalized;
                
                transform.rotation = Quaternion.LookRotation( moveForward );
            }
        }
    }

    // Reference from the Reynolds' Boids flocking algorithm
    // Artificial life simulation developed by Craig Reynolds
    // published in his paper, SIGGRAPH'87, Anaheim, July 27-31, 1987
    // "Flocks, Herds, and Schools: A Distributed Behavioral Model"
    //
    private Vector3 ComputeCollisionAvoidance()
    {
        if( checkAircraftList == null ) 
            return Vector3.zero;

        Vector3 push = Vector3.zero;
        int nearbyCount = 0;

        for ( int i = 0; i < checkAircraftList.Count; i++ )
        {
            Aircraft other = checkAircraftList[i];
            if (other == null || other == this) continue;

            Vector3 diff = transform.position - other.transform.position;
            float dist = diff.magnitude;

            if( dist < avoidanceRadius && dist > 0.001f )
            {
                push += ( diff / (dist * dist) );
                nearbyCount++;
            }
        }

        return nearbyCount > 0 ? push.normalized : Vector3.zero;
    }

    private bool IsWithinBounds( Vector3 pos )
    {
        Vector3 min = boundsCenter - boundsSize * 0.5f;
        Vector3 max = boundsCenter + boundsSize * 0.5f;
        return pos.x >= min.x && pos.x <= max.x &&
               pos.y >= min.y && pos.y <= max.y &&
               pos.z >= min.z && pos.z <= max.z;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 sensorDir = ( transform.forward + Vector3.down * 0.5f ).normalized;
        Gizmos.DrawRay( transform.position, sensorDir * groundCheckDistance );

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube( new Vector3(
            transform.position.x, 
            minFloorAltitude, 
            transform.position.z), 
            new Vector3( 30f, 0.2f, 30f ) 
        );
    }
}