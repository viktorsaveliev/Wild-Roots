using TMPro;
using UnityEngine;

public class Keyboard : MonoBehaviour
{
    private static KeyboardHandler _keyboard;

    private void Start()
    {
        _keyboard = GetComponent<KeyboardHandler>();
    }

    public static void Show(TMP_InputField targetInputField, bool actionAfterHide = false) => _keyboard.Show(targetInputField, actionAfterHide);
}
