using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<InputDevice> inputDevices = new();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        InputDevices.GetDevices(inputDevices);

        var devicesLog = inputDevices.Count == 0 ? "" : inputDevices.Select(x => x.name).Aggregate((a, b) => $"{a}, {b}");
        Debug.Log($"[F:{Time.frameCount}] {inputDevices.Count} input devices found. ( {devicesLog} )");
    }
}
