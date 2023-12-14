using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Trivia;
using UnityEngine;
using static Trivia.GameManager;

public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerPanelController playerPanelController;
    [SerializeField] TMP_Text playerNameText;

    public string PlayerName => playerNameText.text ?? string.Empty;

    public void OnRemoveClicked()
    {
        playerPanelController.RemovePlayer(playerNameText.text);
    }
}
