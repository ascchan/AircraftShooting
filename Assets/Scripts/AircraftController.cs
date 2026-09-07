using System.Collections.Generic;
using UnityEngine;

public class AircraftController : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Aircraft aircraftPrefab;
    [SerializeField] private Transform aircraftStartPos;
    [SerializeField] private int aircraftCount;
    
    [SerializeField] private List<Aircraft> aircraftList = new List<Aircraft>();

    private Vector3 startSpacing = new Vector3( 15f, 0f, 0f );

    private void Start()
    {
        SpawnAircraftList();
    }

    public void SpawnAircraftList()
    {  
        Vector3 basePos = aircraftStartPos != null ? aircraftStartPos.position : transform.position;
        Quaternion baseRot = aircraftStartPos != null ? aircraftStartPos.rotation : Quaternion.identity;

        for ( int i = 0; i < aircraftCount; i++ )
        {
            Vector3 startPos = basePos + ( startSpacing * i );
            Aircraft newPlane = Instantiate( aircraftPrefab, startPos, baseRot );
            newPlane.name = $"{aircraftPrefab.modelName}_{i + 1}";
            aircraftList.Add( newPlane );
        }

        // Generate the shared list of every aircraft for collision checking
        foreach ( Aircraft aircraft in aircraftList )
        {
            aircraft.CheckAircraftList( aircraftList );
        }
    }
}