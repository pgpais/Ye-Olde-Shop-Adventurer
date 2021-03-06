using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SecretDoorUI : MonoBehaviour
{

    [SerializeField] TMP_InputField passCodeField;
    [SerializeField] Button backButton;
    [Space]
    [SerializeField] TMP_Text secretDoorMessage;
    [SerializeField] StringKey secretDoorMessageStringKey;

    SecretDoor secretDoor;
    bool active;

    private void Awake()
    {
        passCodeField.onEndEdit.AddListener(SubmitCode);
        backButton.onClick.AddListener(DisableAll);
    }

    private void Update()
    {
        if (active && Input.GetKeyDown(KeyCode.Escape))
        {
            DisableAll();
        }
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
                    LogsManager.SendLogDirectly(new Log(
                        LogType.CorrectCodeInserted,
                        null
                    ));

                    DisableAll();
                }
                else
                {
                    // Failed
                    LogsManager.SendLogDirectly(new Log(
                        LogType.WrongCodeInserted,
                        null
                    ));

                    passCodeField.text = "";
                }
            }
            else
            {
                LogsManager.SendLogDirectly(new Log(
                    LogType.WrongCodeInserted,
                    null
                ));
                passCodeField.text = "";
            }
        }
    }

    public void Init(SecretDoor secretDoor)
    {
        this.secretDoor = secretDoor;
        EnableAll();

        secretDoorMessage.text = Localisation.Get(secretDoorMessageStringKey);

        EventSystem.current.SetSelectedGameObject(passCodeField.gameObject);
    }

    //Disable every child object
    public void DisableAll()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        active = false;
        MoreMountains.TopDownEngine.GameManager.Instance.Paused = false;
    }

    //Enable every child object
    public void EnableAll()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        active = true;
        MoreMountains.TopDownEngine.GameManager.Instance.Paused = true;
    }
}