using System;
using System.Collections;
using System.Linq;
using TMPro;
using Trivia;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using static Trivia.GameManager;


public class NewPlayerController : MonoBehaviour
{
    [SerializeField] PlayerPanelController playerPanelController;
    [SerializeField] TMP_InputField playerNameText;
    [SerializeField] TMP_Dropdown avatarDropdown;
    [SerializeField] Button saveButton;

    void Start() => Initialize();


    void OnEnable() => ClearInput();


    void ClearInput()
    {
        saveButton.interactable = true;
        playerNameText.text = "";

        var values = Enum.GetValues(typeof(Enums.AvatarType));
        var randomVal = Random.Range(0, values.Length);
        avatarDropdown.value = randomVal;
    }

    void Initialize()
    {
        ClearInput();

        var list = Enum.GetNames(typeof(Enums.AvatarType)).ToList();

        if (list.Count > 0)
        {
            avatarDropdown.ClearOptions();
            avatarDropdown.AddOptions(list);
        }
    }

    public void SaveButonClick()
    {
        saveButton.interactable = false;
        var avatarName = avatarDropdown.options[avatarDropdown.value].text;
        if (Enum.TryParse<Enums.AvatarType>(avatarName, out var type))
            playerPanelController.AddPlayer(playerNameText.text, type);

        Instance.HideAddPlayerWindow();
        saveButton.interactable = true;
    }

}
