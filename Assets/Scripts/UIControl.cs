using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text userAndTime;

    protected string username;

    void Start()
    {
        ConfirmUsername(usernameInput.text.Trim());

        usernameInput.onValueChanged.AddListener(ConfirmUsername);
    }

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

    public string GetUsername()
    {
        return usernameInput.text.Trim();
    }
}
