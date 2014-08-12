using UnityEngine;
using System.Collections;

public enum HotkeyType{
    BIO_OFFENSIVE_KEY,
    ELECTRIC_OFFENSIVE_KEY,
    RADIATION_OFFENSIVE_KEY,
    BIO_DEFENSIVE_KEY,
    ELECTRIC_DEFENSIVE_KEY,
    RADIATION_DEFENSIVE_KEY
}

public static class HotkeyTypeExtensions{
#if UNITY_STANDALONE_WIN
    private static KeyCode[] _codes = {KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.A, KeyCode.S, KeyCode.D};
#else
    private static KeyCode[] _codes = {KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.A, KeyCode.S, KeyCode.D};
#endif

    public static KeyCode getKeyCode(this HotkeyType hotkeyType) {
        return _codes[(int)hotkeyType];
    }
}


public class HotkeyLabel : MonoBehaviour {
    public HotkeyType hotkeyType;

    public void Awake() {
        UILabel label = GetComponent<UILabel>();
        label.text = hotkeyType.getKeyCode().ToString();
    }
}
