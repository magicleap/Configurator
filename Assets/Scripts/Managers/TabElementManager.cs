using System.Linq;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;

/// <summary>
/// Class for managing UI tabs
/// Allows for us to have 'tab group' behavior where only 1 tab can be active/highlighted at a time
/// </summary>
public class TabElementManager : MonoBehaviour
{
    public void ResetAllTabs()
    {
        ConfigurationTabElement[] tabs = GetComponentsInChildren<ConfigurationTabElement>();
        foreach (ConfigurationTabElement tab in tabs)
        {
            tab.GetComponent<PressableButton>().ForceSetToggled(false, true);
        }
    }

    public void DetoggleOtherTabs(ConfigurationTabElement tabElementToKeep)
    {
        ConfigurationTabElement[] tabs = GetComponentsInChildren<ConfigurationTabElement>();
        foreach (ConfigurationTabElement tab in tabs.Where(tab => tab != tabElementToKeep))
        {
            tab.GetComponent<PressableButton>().ForceSetToggled(false, true);
        }
    }

    private void OnDisable()
    {
        ResetAllTabs();
    }
}
