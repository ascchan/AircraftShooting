using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AircraftController : MonoBehaviour
{
    [SerializeField] private Aircraft aircraftPrefab;
    [SerializeField] private GameObject antiAircraftGun;
    [SerializeField] private GameObject UICTRL;
    [SerializeField] private GameManager gameManager;

    [SerializeField] private Transform aircraftStartPos;
    [SerializeField] private int aircraftCount;
    
    [SerializeField] public List<Aircraft> aircraftList = new List<Aircraft>();

    public bool IsFireActive => gameManager.fireStarted && !gameManager.fireFinished && aircraftList.Count > 0;

    [SerializeField] private Vector3 startSpacing;


    public int RemainingAircraft => aircraftList.Count;

    public void SpawnAircraftList()
    {
        foreach( Aircraft aircraft in aircraftList )
        {
            if( aircraft != null )
            {
                aircraft.gameObject.SetActive( false );
                Destroy( aircraft.gameObject );
            }

        }

        aircraftList.Clear();
        gameManager.fireStarted = false;
        gameManager.timerRunning = false;
        gameManager.fireFinished = false;
        gameManager.firstShotTime = 0;
        gameManager.elapsedTime = 0;

        Vector3 basePos = aircraftStartPos != null ? aircraftStartPos.position : transform.position;
        Quaternion baseRot = aircraftStartPos != null ? aircraftStartPos.rotation : Quaternion.identity;

        for( int i = 0; i < aircraftCount; i++ )
        {
            Vector3 startPos = basePos + ( startSpacing * i );
            Aircraft newPlane = Instantiate( aircraftPrefab, startPos, baseRot );
            newPlane.name = $"{aircraftPrefab.modelName}_{i + 1}";
            aircraftList.Add( newPlane );
        }

        // Generate the shared list of every aircraft for collision checking
        foreach( Aircraft aircraft in aircraftList )
        {
            aircraft.CheckAircraftList( aircraftList );
        }

        gameManager.fireStarted = aircraftList.Count > 0;
        gameManager.ClockTimeDisplay();
    }

}