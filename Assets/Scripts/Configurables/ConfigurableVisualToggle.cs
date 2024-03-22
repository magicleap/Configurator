using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A configurable component used to visually toggle objects on and off
/// </summary>
public class ConfigurableVisualToggle : ConfigurableComponent
{
    //List of all objects we can toggle
    [Tooltip("List of all objects you wish to toggle on and off visually")]
    public List<GameObject> objectsToToggle;

    //Show or hide objects by number
    public void ShowNumberOfObjects(int num)
    {
        for (int i = 0; i < num; i++)
        {
            objectsToToggle[i].SetActive(true);
        }

        for (int i = num; i < objectsToToggle.Count; i++)
        {
            objectsToToggle[i].SetActive(false);
        }
    }

    //Turn all objects back on
    public override void Reset()
    {
        foreach (GameObject go in objectsToToggle)
        {
            go.SetActive(true);
        }
    }
}
