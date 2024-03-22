using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

/// <summary>
/// Class describing a layout
/// A layout contains a list of names and their positions
/// You can also name a layout if you desire
/// </summary>
[Tooltip("A Layout is a collection of object names and their position/rotations. You can also give a layout a name")]
[Serializable]
public class Layout
{
    public List<string> objectNames = new();
    public List<PositionData> objectPositionData = new();
    public string Name;
    public Layout(Dictionary<string, PositionData> objects, string layoutName)
    {
        foreach (KeyValuePair<string, PositionData> valuePair in objects)
        {
            objectNames.Add(valuePair.Key);
            objectPositionData.Add(valuePair.Value);
        }
        Name = layoutName;
    }
}

/// <summary>
/// Class making it easier to access and store an objects transform information
/// PositionData consists of an objects position and rotation
/// </summary>
[Tooltip("Data collection of position and rotation of a given game object")]
[Serializable]
public class PositionData
{
    public Vector3 position;
    public Quaternion rotation;
    public bool objectEnabled;
    
    public PositionData(Vector3 pos, Quaternion rot, bool isEnabled)
    {
        position = pos;
        rotation = rot;
        objectEnabled = isEnabled;
    }
}

/// <summary>
/// A configurable layout allows for defining preset transform configurations of an asset.
/// For example, if you have a sofa that you would like to have a 2 and 3 seater variant of
/// you can create a layout for each one and swap between them with a UI button
/// </summary>
public class ConfigurableLayout : ConfigurableComponent
{
    //We use scriptable objects for saving layouts as assets.
    //This is a reference to that layout object
    //You can create a layout scriptable object on your own
    //It comes with a custom inspector to allow you to easily save a layout in the editor and apply a name to it
    [Tooltip("The Scriptable Object created for this layout (Create one if you have not done so and reference it here!)")]
    [SerializeField]
    public LayoutScriptableObject layoutSO;

    //List of objects that have been set active or inactive so we can easily access them when needed
    [Tooltip("List of objects that have been set active or inactive by this layout")]
    public List<GameObject> toggledObjects = new();

    //List of GameObjects to ignore for all layouts on this object
    [Tooltip("List of GameObjects to ignore for all layouts on this object")]
    public List<GameObject> ignoreObjects = new();

    //This is the bulk of the layout class
    //Here, we match the name of a layout with one in the scriptable object refernce
    //if one is found, we loop through each object and apply it's relevant position, rotation,
    //and set it active or inactive in the scene
    public void ApplyLayout(string layoutName)
    {
        //If there is no scriptable object, do nothing
        if (layoutSO == null)
        {
            return;
        }
        
        ////First, enable ALL objects as they will be toggled off later if not found in the layout being applied
        //foreach (Transform t in transform.GetComponentsInChildren<Transform>(true))
        //{
        //    t.gameObject.SetActive(true);
        //}
        
        //Search for matching layout via layout name
        Layout toApply = layoutSO.layouts.FirstOrDefault(layout => layout.Name == layoutName);
        
        //Some assets may have a bounding box attached
        //Make sure to ignore this object and any of it's children
        BoundsControl boundsControl = GetComponent<BoundsControl>();
        GameObject boundsVisual = null;
        List<string> boundsObjectsToIgnore = new();
        //Our assets have bounding boxes, this is how we find them and ignore them as they are generated at runtime
        if (boundsControl != null)
        {
            boundsVisual = boundsControl.BoundsVisualsPrefab;
            //ignore any children the bounding box may have (usually the handles)
            foreach (Transform t in boundsVisual.transform.GetComponentsInChildren<Transform>(true))
            {
                boundsObjectsToIgnore.Add(t.gameObject.name);
            }
            //The MRTK bounds control uses 'clone' at the end of the name, so include that
            boundsObjectsToIgnore.Add(boundsVisual.name + "(Clone)");
        }

        //Loop through all child objects, but ignore any that are part of the bounding box
        foreach (Transform t in transform.GetComponentsInChildren<Transform>(true)
                     .Where(obj =>
                         obj.name != gameObject.name && !boundsObjectsToIgnore.Contains(obj.gameObject.name) &&
                         !ignoreObjects.Contains(obj.gameObject)))
        {
            //If the current object is part of the layout being applied, apply position and rotation
            //Otherwise, deactivate the object as it was NOT included in the layout
            if (toApply.objectNames.Exists(n => n == t.name))
            {
                int index = toApply.objectNames.FindIndex(n => n == t.name);
                //found in layout, apply transform
                PositionData foundObject = toApply.objectPositionData[index];
                t.localPosition = foundObject.position;
                t.localRotation = foundObject.rotation;
                t.gameObject.SetActive(foundObject.objectEnabled);
            }
            else
            {
                //Unity doesn't like when you enable and disable a game object
                //in the same frame, so a coroutine can help with this by delaying
                //the action by 1 frame.
                //StartCoroutine(DisableObject(t.gameObject));
            }
        }

        //The asset may have gotten smaller by deactivating objects, so recompute the bounding box bounds
        boundsControl?.RecomputeBounds();
    }

    //Coroutine for waiting 1 frame and then deactivating object
    //Unity does not like it when you enable and disable an object in the same frame (bug?)
    IEnumerator DisableObject(GameObject obj)
    {
        //wait one frame
        yield return null;
        obj.SetActive(false);
    }

    //If we do NOT want to enable all objects when applying a layout, we can use this
    public void ApplyLayoutPositionOnly(string layoutName)
    {
        //this method will NOT enable all other objects and will only apply positions
        foreach (Transform t in transform.GetComponentsInChildren<Transform>(true)
                     .Where(obj => obj.name != gameObject.name && !ignoreObjects.Contains(obj.gameObject)))
        {
            Layout toApply = layoutSO.layouts.FirstOrDefault(layout => layout.Name == layoutName);
            if (toApply.objectNames.Exists(n => n == t.name))
            {
                int index = toApply.objectNames.FindIndex(n => n == t.name);
                //found in layout, apply transform
                PositionData foundObject = toApply.objectPositionData[index];
                t.localPosition = foundObject.position;
            }
        }
    }

    //Used to toggle an object on or off
    public void ToggleObject(GameObject obj, bool show)
    {
        if (!toggledObjects.Contains(obj))
        {
            toggledObjects.Add(obj);
        }

        toggledObjects.Find(toggledObj => toggledObj == obj).SetActive(show);
    }
    
    //The default layout of an asset should be named "Default"
    public override void Reset()
    {
        ApplyLayout("Default");
    }
}