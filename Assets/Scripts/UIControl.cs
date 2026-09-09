using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text userAndTime;
   // [SerializeField] private TMP_Text enterUsernameRemind;

    private string username;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ConfirmUsername(usernameInput.text.Trim());
        //ConfirmUsername(usernameInput.text);

        usernameInput.onValueChanged.AddListener(ConfirmUsername);
    }

    // Update is called once per frame
    void Update()
    {
        ConfirmUsername(usernameInput.text.Trim());
    }

    private void ConfirmUsername(string usernameText)
    {
        if (string.IsNullOrEmpty(usernameText))
        {
            startButton.interactable = false;
        }
        else
        {
            startButton.interactable = true;
        }
    }

    private void OnDestroy()
    {
        usernameInput.onValueChanged.RemoveListener(ConfirmUsername);
    }

}
