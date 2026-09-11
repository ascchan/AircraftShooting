using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Aircraft aircraftPrefab;
    [SerializeField] private AircraftController aircraftController;
    [SerializeField] private GameObject antiAircraftGun;
    [SerializeField] private Shooting shooting;
    [SerializeField] private GameObject UICTRL;
    [SerializeField] private TMP_InputField usernameInput;

    [SerializeField] private GameObject fireCompletePanel;
    [SerializeField] private UnityEvent onFireFinished = new UnityEvent(); 
    [SerializeField] private GameObject restartButton;
    [SerializeField] private GameObject quitButton;
    [SerializeField] private GameObject yesBtn; 
    [SerializeField] private TMP_Text storeRCDText;

    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text remainingAircraftText;
    [SerializeField] private TMP_Text inputInstructions;

    public bool fireStarted;
    public bool timerRunning;
    public bool fireFinished;
    public float firstShotTime;
    public float elapsedTime;
    public bool isGameStarted = false;
    public bool isGameFinished = false;
    public string username;
    public int RemainingAircraft => aircraftController.aircraftList.Count;

    private void Start()
    {

        EventSystem.current.SetSelectedGameObject(usernameInput.gameObject, null);
        usernameInput.ActivateInputField();
    }
    private void Awake()
    {
        if( usernameInput == null )
            usernameInput = GetComponent<TMP_InputField>();
        SetCompletionPanel( false );
        ClockTimeDisplay();
    }

    private void SetCompletionPanel( bool visible )
    {
        if( transform.IsChildOf(fireCompletePanel.transform) )
        {
            return;
        }
        fireCompletePanel.SetActive( visible );
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public void StartGame()
    {

        aircraftPrefab.enabled = true;
        aircraftController.SpawnAircraftList();
        isGameStarted = true;
    }

    public void NotifyShotFired()
    {
        if( !timerRunning )
            inputInstructions.enabled = false;

        if( !fireStarted || timerRunning || fireFinished )
            return;

        aircraftController.aircraftList.RemoveAll( aircraft => aircraft == null );
        if( aircraftController.aircraftList.Count == 0 )
            return;

        firstShotTime = Time.time;
        elapsedTime = 0;
        timerRunning = true;

        ClockTimeDisplay();
    }

    private void LateUpdate()
    {
        if( !fireStarted || fireFinished )
            return;

        // Destroy() leaves a missing Unity reference in the List.
        // Remove it in place so the collision-checking list stays current.
        aircraftController.aircraftList.RemoveAll( aircraft => aircraft == null );

        if( timerRunning )
        {
            elapsedTime = Time.time - firstShotTime;

            if( aircraftController.aircraftList.Count == 0 )
            {
                timerRunning = false;
                fireFinished = true;
                shooting.allowFire = false;
                Debug.Log( $"All aircraft down! Final time: {elapsedTime:F2} seconds");

                if( RemainingAircraft == 0 && isGameStarted )
                {
                    quitButton.SetActive( true );
                    restartButton.SetActive( true );
                    yesBtn.SetActive( true );
                    storeRCDText.gameObject.SetActive( true );
                }

                if( EventSystem.current != null )
                {
                    EventSystem.current.SetSelectedGameObject( null );
                    EventSystem.current.sendNavigationEvents = false;
                }

            }
        }

        ClockTimeDisplay();
    }

    public void ClockTimeDisplay()
    {
        if( remainingAircraftText != null )
            remainingAircraftText.text = $"Aircraft remaining: {RemainingAircraft}";

        username = UICTRL.GetComponent<UIControl>().GetUsername();
        long hundredths = (long)( elapsedTime * 100.0 );
        long minutes = hundredths / 6000;
        long seconds = ( hundredths / 100 ) % 60;
        long fraction = hundredths % 100;
        string label = fireFinished ? username + ", you completed the mission in" : "Time";

        timerText.text = $"{label}: {minutes:00}:{seconds:00}.{fraction:00}";
    }

    public void GameOver()
    {
        Time.timeScale = 0f;
        FreezePlayer();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public void FreezePlayer()
    {
        aircraftPrefab.enabled = false;
        antiAircraftGun.SetActive( false );

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public double GetElapsedTime()
    {
        return elapsedTime;
    }
}
