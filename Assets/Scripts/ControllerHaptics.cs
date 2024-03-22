using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.XR.MagicLeap;

/// <summary>
/// Class for managing controller haptics on the ML device
/// </summary>
public class ControllerHaptics : MonoBehaviour
{
    //All pressable buttons
    private List<PressableButton> pressableButtons = new();

    // Start is called before the first frame update
    void Start()
    {
        UpdateHapticEvents();
    }

    public void UpdateHapticEvents()
    {
        //if we already have buttons, only add listeners to ones that do not have them already
        if (pressableButtons != null || pressableButtons.Count != 0)
        {
            PressableButton[] buttons = FindObjectsOfType<PressableButton>(true);
            foreach (PressableButton button in buttons)
            {
                if (!pressableButtons.Contains(button))
                {
                    pressableButtons.Add(button);
                    button.hoverEntered.AddListener(delegate { HandleOnEnter(); });
                }
            }

            return;
        }
        
        pressableButtons = FindObjectsOfType<PressableButton>(true).ToList();
        foreach (var eventTrigger in pressableButtons)
        {
            eventTrigger.hoverEntered.AddListener(delegate { HandleOnEnter(); });
        }
    }

    void HandleOnEnter()
    {
        var preDefined = InputSubsystem.Extensions.Haptics.PreDefined.Create(InputSubsystem.Extensions.Haptics.PreDefined.Type.C);
        preDefined.StartHaptics();
    }
    
}
