using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugLoggerUI : MonoBehaviour
{
    public static DebugLoggerUI Instance;
    public TextMeshProUGUI debugText;

    void Awake()
    {
        Instance = this; // Make it globally accessible
    }

    public void Log(string message)
    {
        if (debugText != null)
        {
            debugText.text += message + "\n"; // Append debug messages
        }
    }
}
