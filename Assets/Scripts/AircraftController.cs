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

    [SerializeField] private Transform aircraftStartPos;
    [SerializeField] private int aircraftCount;
    
    [SerializeField] private List<Aircraft> aircraftList = new List<Aircraft>();

    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text remainingAircraftText;
    [SerializeField] private TMP_Text inputInstructions;

    [SerializeField] private GameObject fireCompletePanel;
    [SerializeField] private UnityEvent onFireFinished = new UnityEvent();
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button yesBtn; 
    [SerializeField] private TMP_Text storeRCDText;


    [SerializeField] private Shooting shooting;

    public bool IsFireActive => fireStarted && !fireFinished && aircraftList.Count > 0;

    [SerializeField] private Vector3 startSpacing;

    private bool fireStarted;
    private bool timerRunning;
    private bool fireFinished;
    private double firstShotTime;
    private double elapsedTime;

    private bool isGameStarted = false;
    private bool isGameFinished = false;
    private string username;

    public int RemainingAircraft => aircraftList.Count;

    
    private void Awake()
    {
        SetCompletionPanel(false);
        ClockTimeDisplay();
    }

    private void SetCompletionPanel(bool visible)
    {
        if (transform.IsChildOf(fireCompletePanel.transform))
        {
            return;
        }
        fireCompletePanel.SetActive(visible);
    }  
 
    public void StartGame()
    {
        aircraftPrefab.enabled = true;
        SpawnAircraftList();
        isGameStarted = true;
    }

    private void Update()
    {

    }

    public void SpawnAircraftList()
    {
        foreach (Aircraft aircraft in aircraftList)
        {
            if (aircraft != null)
            {
                aircraft.gameObject.SetActive(false);
                Destroy(aircraft.gameObject);
            }

        }

        aircraftList.Clear();
        fireStarted = false;
        timerRunning = false;
        fireFinished = false;
        firstShotTime = 0;
        elapsedTime = 0;

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

        fireStarted = aircraftList.Count > 0;
        ClockTimeDisplay();
    }

    public void NotifyShotFired()
    {
        if(!timerRunning)
            inputInstructions.enabled = false;

        if (!fireStarted || timerRunning || fireFinished)
            return;

        aircraftList.RemoveAll(aircraft => aircraft == null);
        if (aircraftList.Count == 0)
            return;

        firstShotTime = Time.timeAsDouble;
        elapsedTime = 0;
        timerRunning = true;

        ClockTimeDisplay();
    }

    private void LateUpdate()
    {
        if (!fireStarted || fireFinished)
            return;

        // Destroy() leaves a missing Unity reference in the List.
        // Remove it in place so the collision-checking list stays current.
        aircraftList.RemoveAll(aircraft => aircraft == null);

        if (timerRunning)
        {
            elapsedTime = Time.timeAsDouble - firstShotTime;

            if (aircraftList.Count == 0)
            {
                timerRunning = false;
                fireFinished = true;
                shooting.allowFire = false;
                Debug.Log($"All aircraft down! Final time: {elapsedTime:F2} seconds", this);
                
                if (RemainingAircraft == 0 && isGameStarted)
                {
                    quitButton.gameObject.SetActive(true);
                    restartButton.gameObject.SetActive(true);
                    yesBtn.gameObject.SetActive(true);
                    storeRCDText.gameObject.SetActive(true);
                }

                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.sendNavigationEvents = false;
                }
                
            }
        }

        ClockTimeDisplay();
    }

    public void ClockTimeDisplay()
    {
        if (remainingAircraftText != null)
            remainingAircraftText.text = $"Aircraft remaining: {RemainingAircraft}";

        username = UICTRL.GetComponent<UIControl>().GetUsername();
        long hundredths = (long)(elapsedTime * 100.0);
        long minutes = hundredths / 6000;
        long seconds = (hundredths / 100) % 60;
        long fraction = hundredths % 100;
        string label = fireFinished ? username + ", you completed the mission in" : "Time";

        timerText.text = $"{label}: {minutes:00}:{seconds:00}.{fraction:00}";
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        FreezePlayer();
    }

    public void FreezePlayer()
    {
        aircraftPrefab.enabled = false;
        antiAircraftGun.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public double GetElapsedTime()
    {
        return elapsedTime;
    }
}