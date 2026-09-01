using UnityEngine;

public class Aircraft : MonoBehaviour
{
    [SerializeField] private float flightAltitude;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float flyRadius;
    [SerializeField] private float nextPosWide;

    private Vector3 currentPos;
    private bool isInAir = false;

    void Start()
    {
        currentPos = new Vector3(transform.position.x, flightAltitude, transform.position.z);
    }

    void Update()
    {
        MoveNextPos();
    }

    private void MoveNextPos()
    {
        transform.position = Vector3.MoveTowards( transform.position, currentPos, moveSpeed * Time.deltaTime );

        Vector3 direction = (currentPos - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion nextPosRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp( transform.rotation, nextPosRotation, rotationSpeed * Time.deltaTime );
        }

        if (Vector3.Distance( transform.position, currentPos) <= nextPosWide )
        {
            if ( !isInAir )
            {
                isInAir = true;
            }

            GetNextPosPoint();
        }
    }

    private void GetNextPosPoint()
    {
        Vector2 randomCircle = Random.insideUnitCircle * flyRadius;
        currentPos = new Vector3( randomCircle.x, flightAltitude + Random.Range(-2f, 2f), randomCircle.y );
    }
}
