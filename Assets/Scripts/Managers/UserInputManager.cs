using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UI;

public interface IUserInputManager
{
    event Action OnAnswer1Received;
    event Action OnAnswer2Received;
    event Action OnAnswer3Received;
    event Action OnAnswer4Received;
    event Action OnSubmitReceived;
    event Action OnCancelReceived;
}

[SuppressMessage("", "IDE0051", Justification = "Methods used by Player Input SendMessages")]
// Component to sit next to PlayerInput.
[RequireComponent(typeof(PlayerInput))]
public class UserInputManager : MonoBehaviour, IUserInputManager
{
    // base class
    public static UserInputManager Instance;

    enum GamepadType { init, none, generic, playstation, xbox }

    const string GamepadScheme = "Gamepad";
    const string mouseScheme = "Keyboard&Mouse";

    string _prevControlSchema = "";
    GamepadType _curGamepad = GamepadType.init;

    public bool HasGamepad => _curGamepad > GamepadType.none;

    protected void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _buttonImages = new();

        InputSystem.onDeviceChange += (_, _) => CheckGamepads();

        CheckGamepads();
    }

    void CheckGamepads()
    {
        var result = GamepadType.none;

        for (var i = 0; i < InputSystem.devices.Count; i++)
        {
            var device = InputSystem.devices[i];

            if (device is Gamepad)
            {
                result = GamepadType.generic;
                if (device is DualShockGamepad)
                {
                    print("Playstation gamepad");
                    result = GamepadType.playstation;
                }
                else if (device is XInputController)
                {
                    print("Xbox gamepad");
                    result = GamepadType.xbox;
                }
            }
            else
            {
                print(device.ToString());
            }
        }

        if (result != _curGamepad)
        {
            print("Curren pad: "+ result);
            _curGamepad = result;
            OnGamepadChanged();
        }
    }

    /// <summary>
    /// Override this method to handle Gamepad changes.
    /// </summary>
    protected virtual void OnGamepadChanged()
    {
        foreach (var obj in _images)
            obj.SetActive(HasGamepad);

        foreach (var obj in _buttonImages)
            obj.SetActive(HasGamepad);
    }

    /// <summary>
    /// Player Input SendMessages
    /// </summary>
    void OnControlsChanged(PlayerInput input)
    {
        if (input.currentControlScheme == mouseScheme && _prevControlSchema != mouseScheme)
        {
            Cursor.visible = true;
            _prevControlSchema = mouseScheme;
        }
        else if (input.currentControlScheme == GamepadScheme && _prevControlSchema != GamepadScheme)
        {
            Cursor.visible = false;
            _prevControlSchema = GamepadScheme;
        }
    }

    [Header("PS4 Answer Buttons")]
    [SerializeField] List<Sprite> _answerButtons;

    [Header("Gamepad control Images")]
    [SerializeField] List<GameObject> _images;

    public event Action OnAnswer1Received;
    public event Action OnAnswer2Received;
    public event Action OnAnswer3Received;
    public event Action OnAnswer4Received;
    public event Action OnSubmitReceived;
    public event Action OnCancelReceived;

    List<GameObject> _buttonImages;

    void OnAnswer1()
    {
        Debug.Log("OnAnswer 1");
        OnAnswer1Received?.Invoke();
    }

    void OnAnswer2()
    {
        Debug.Log("OnAnswer 2");
        OnAnswer2Received?.Invoke();
    }

    void OnAnswer3()
    {
        Debug.Log("OnAnswer 3");
        OnAnswer3Received?.Invoke();
    }

    void OnAnswer4()
    {
        Debug.Log("OnAnswer 4");
        OnAnswer4Received?.Invoke();
    }

    void OnSubmitted()
    {
        Debug.Log("OnSubmit");
        OnSubmitReceived?.Invoke();
    }

    void OnCancelled()
    {
        Debug.Log("OnCancel");
        OnCancelReceived?.Invoke();
    }

    public void AddAnswerButton(Button button, int index)
    {
        foreach (Transform child in button.gameObject.transform)
        {
            var img = child.GetComponentInChildren<Image>(true);
            if (img != null)
            {
                img.sprite = _answerButtons[index];
                _buttonImages.Add(img.gameObject);
                OnGamepadChanged(); // Use to check if gamepad button must be displayed.
                break;
            }
        }
    }

    public void ClearAnswerButtons() => _buttonImages.Clear();

    //void SetCurrentDevice(InputDevice device)
    //{
    //    print(device);

    //    if (device is Gamepad)
    //    {
    //        if (device.description.manufacturer == "Sony Interactive Entertainment")
    //        {
    //            // CurrentDevice = CurrentDeviceType.playstation;
    //            Debug.Log("Playstation Controller Detected");
    //        }
    //        // Assumes Xbox controller, device.description.manufacturer for Xbox returns empty string
    //        else
    //        {
    //            // CurrentDevice = CurrentDeviceType.xbox;
    //            Debug.Log("Xbox Controller Detected");
    //        }
    //    }
    //}

    //public bool CurrentDeviceIsGamepad => CurrentDevice == CurrentDeviceType.playstation || CurrentDevice == CurrentDeviceType.xbox;

    //public UnityEvent<CurrentDeviceType> OnControllerChanged;

    //public CurrentDeviceType CurrentDevice
    //{
    //    get => __currentDevice; private set
    //    {
    //        __currentDevice = value;
    //        RaiseControllerChangedEvent();
    //    }
    //}
    //CurrentDeviceType __currentDevice;

    //void RaiseControllerChangedEvent()
    //{
    //    print($"Current control: {CurrentDevice}");
    //    OnControllerChanged?.Invoke(CurrentDevice);
    //    DisplayDeviceImages();
    //}

    // the controller being used by the player instantly re-constructs itself,
    // and is quickly the only device stored in InputSystem.devices[].
    //void ResetDevices()
    //{
    //    CurrentDevice = CurrentDeviceType.none;

    //    for (var i = 0; i < InputSystem.devices.Count - 1; i++)
    //    {
    //        var device = InputSystem.devices[i];

    //        if (device is Gamepad)
    //            InputSystem.RemoveDevice(device);
    //    }

    //    if (InputSystem.devices[0] == null) return;

    //    SetCurrentDevice(InputSystem.devices[0]);
    //}

}
