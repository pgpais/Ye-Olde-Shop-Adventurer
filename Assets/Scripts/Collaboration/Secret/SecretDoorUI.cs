using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SecretDoorUI : MonoBehaviour
{

    [SerializeField] TMP_InputField passCodeField;
    [SerializeField] Button backButton;

    SecretDoor secretDoor;

    private void Awake()
    {
        passCodeField.onEndEdit.AddListener(SubmitCode);
        backButton.onClick.AddListener(DisableAll);
        DisableAll();
    }

    private void SubmitCode(string codeText)
    {
        if (secretDoor != null)
        {
            // Parse arg0 to int
            int code = 0;
            if (int.TryParse(codeText, out code))
            {
                if (secretDoor.SubmitCode(code))
                {
                    // Success
                }
                else
                {
                    passCodeField.text = "Incorrect code.";
                }
            }
            else
            {
                passCodeField.text = "Invalid code.";
            }
        }
    }

    public void Init(SecretDoor secretDoor)
    {
        this.secretDoor = secretDoor;
        EnableAll();
    }

    //Disable every child object
    public void DisableAll()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        MoreMountains.TopDownEngine.GameManager.Instance.Paused = false;
    }

    //Enable every child object
    public void EnableAll()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        MoreMountains.TopDownEngine.GameManager.Instance.Paused = true;
    }
}