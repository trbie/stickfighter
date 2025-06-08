using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InputDisplayHelper : MonoBehaviour
{
    public TMP_SpriteAsset keyboardSpriteAsset;
    public TMP_SpriteAsset gamepadSpriteAsset;
    public TextMeshProUGUI[] textObjects;

    void Awake()
    {
        PlayerInput input = FindFirstObjectByType<PlayerInput>();
        UpdateSpriteAssets(input.currentControlScheme);
    }

    void OnEnable()
    {
        InputUser.onChange += OnInputDeviceChange;
    }

    void OnDisable()
    {
        InputUser.onChange -= OnInputDeviceChange;
    }

    void OnInputDeviceChange(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change == InputUserChange.ControlSchemeChanged)
        {
            UpdateSpriteAssets(user.controlScheme.Value.name);
        }
    }

    void UpdateSpriteAssets(string schemeName)
    {
        TMP_SpriteAsset asset = schemeName == "Gamepad" ? gamepadSpriteAsset : keyboardSpriteAsset;
        foreach (var textObject in textObjects)
        {
            if (textObject != null)
            {
                textObject.spriteAsset = asset;
            }
        }
    }
}
