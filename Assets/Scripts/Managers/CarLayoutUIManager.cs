using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;

/// <summary>
/// Class for managing the custom UI and functions for the car
/// </summary>
public class CarLayoutUIManager : MonoBehaviour
{
    //UI tabs
    [SerializeField] private ConfigurationTabElement AnimationTab;
    
    //door animation UI toggle reference
    [SerializeField]
    private PressableButton doorAnimToggle;
    
    //list of all tabs unique to this asset that can be added to the configurable UI
    public List<GameObject> ConfigTabs => new()
        { AnimationTab.gameObject };
    

    public bool doorAnimPlayed { get; private set;} = false;
    public void PlayDoorAnim(bool worldSpaceTrigger)
    {
        if (worldSpaceTrigger)
        {
            doorAnimToggle.ForceSetToggled(!doorAnimToggle.IsToggled, true);
        }
        
        var anims = FindObjectsOfType<Animator>().Where(obj => obj.gameObject.name.Contains("Car"));

        foreach (Animator anim in anims)
        {
            if (doorAnimPlayed)
            {
                anim.Play("Reverse");
            }
            else
            {
                anim.Play("Forward");
            }
        }

        doorAnimPlayed = !doorAnimPlayed;
    }
}
