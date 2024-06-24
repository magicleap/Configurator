using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

/// <summary>
/// Class that is attached to the root of each configurable asset
/// This is where all configuration behavior kicks off
///
/// A configurable contains a list of ConfigurableComponents
/// Each component has it's own behavior and allows for different things to be
/// configured by the user (Ex: <see cref="ConfigurableColor"/>)
/// </summary>
public class Configurable : MonoBehaviour
{
    //Each configurable asset has a name and a thumbnail
    //These can be set in the inspector
    [Tooltip("Name for the asset, can be used to display in UI")]
    public string Name;
    [Tooltip("Thumbnail or icon for the asset, can be used to display in UI")]
    public Sprite thumbnail;
    
    //List of all configurable components that make up this configurable asset
    [Tooltip("List of all configurable components for this asset, populated automatically at runtime")]
    [SerializeField]
    protected List<ConfigurableComponent> components;

    //Current object selected by the user
    //This can be individual objects that make up an asset and contain their own configurable components
    [Tooltip("The currently selected gameobject, usually is a child of the configurable object (this)")]
    public GameObject CurrentSelectedObject { get; private set; }

    //offest rotation, can be set in inspector
    //Currently used for the car asset to spawn it sideways
    [Tooltip("Offset spawn rotation for this asset")]
    public Vector3 spawnRotation = Vector3.zero;

    //Current material and color selections
    [Tooltip("The most recent selected material resulting from a material change")]
    public Material currentMaterialSelection;
    
    [Tooltip("The most recent selected color resulting from a color change")]
    public Color currentColorSelection;

    //ClipDistance for each configurable asset, used by ShowWhenClose shader as a visualization 
    //of an ARPlane mesh
    [Tooltip("Custom clipping distance for ShowWhenClose.shader, this shader is used to visualize ARPlane mesh's")]
    [SerializeField]
    protected float clipDist = 1f;

    //Width of the outline when selected or hovered for this asset
    //Different assets look better with different widths
    [Tooltip("Width of the selection outline for this asset")]
    [SerializeField] private float outlineWidth = 0.008f;
    
    //a reference to any custom UI you have made for this layout
    [Tooltip("Reference to a custom UI prefab you have created for this asset")]
    public GameObject customUIPrefab;
    
    //Selection changed event, useful for UI
    public delegate void SelectionChangedEvent(GameObject obj);
    public event SelectionChangedEvent OnSelectionChanged;

    //Set by DraggableObjectManipulator when an asset is being 'dragged'
    //Used to update ARPlane query and ARPlane visualization
    [Tooltip("Flag to identify when the user is dragging or rotating this asset")]
    public bool IsBeingManipulated = false;

    public virtual void Start()
    {
        CreateUI();
    }

    private void Update()
    {
        //If the configurable is being manipulated, update the global shader position
        //otherwise, set it far away
        //This is used to achieve the visual effect in the ShowWhenClose shader
        if (IsBeingManipulated)
        {
            Shader.SetGlobalVector("_ConfigPosition", transform.position);
        }
        else
        {
            Shader.SetGlobalVector("_ConfigPosition", new Vector3(1000,1000,1000));
        }
    }

    //Add a configurable component to this configurable
    public void AddComponent(ConfigurableComponent comp)
    {
        components.Add(comp);
    }

    //Remove a configurable component to this configurable
    public void RemoveComponent(ConfigurableComponent comp)
    {
        components.Remove(comp);
    }

    //Used to set selection
    public void SetSelectedObject(GameObject obj)
    {
        CurrentSelectedObject = obj;
        OnSelectionChanged?.Invoke(obj);
    }

    //Children should override this to create any custom UI for the configuration menu
    public virtual void CreateUI()
    {
        
    }

    //Reset all configurable components
    public void ResetAll()
    {
        //Call in case any child has overridden this
        Reset();
        
        //Reset materials, and then colors
        //if colors are done first, the original material might still have a modified color
        foreach (ConfigurableMaterial config in components.Where(comp => comp is ConfigurableMaterial))
        {
            config.Reset();
        }

        foreach (ConfigurableColor config in components.Where(comp=> comp is ConfigurableColor))
        {
            config.Reset();
        }

        //reset the rest of the components
        foreach (ConfigurableComponent config in components.Where(comp =>
                     comp is not ConfigurableColor && comp is not ConfigurableMaterial))
        {
            config.Reset();
        }

        currentColorSelection = default;
        currentMaterialSelection = null;
    }

    //Change the color of a selected object
    public void ChangeColor(Color col)
    {
        ConfigurableComponent[] components = GetAllConfigurableForSelected();

        foreach (ConfigurableColor configColor in components
                     .Where(comp => comp is ConfigurableColor))
        {
            configColor.ChangeColor(col);
        }

        currentColorSelection = col;
    }

    //Apply color to all ConfigurableColor components that have a matching GroupID
    public void ApplyColorToAll()
    {
        if (currentColorSelection == default)
        {
            return;
        }

        ConfigurableColor currentConfigColor =
            (ConfigurableColor)GetAllConfigurableForSelected().FirstOrDefault(comp => comp is ConfigurableColor);
        int groupID = currentConfigColor.GroupID;
        foreach (ConfigurableColor configColor in components.Where(comp => comp is ConfigurableColor))
        {
            if (configColor.GroupID == groupID)
            {
                configColor.ChangeColor(currentColorSelection);
            }
        }
    }

    //Apply color to all ConfigurableMaterial components that have a matching GroupID
    public void ApplyMaterialToAll()
    {
        if (currentMaterialSelection == null)
        {
            return;
        }
        ConfigurableMaterial currentConfigMaterial =
            (ConfigurableMaterial)GetAllConfigurableForSelected().FirstOrDefault(comp => comp is ConfigurableMaterial);
        int groupID = currentConfigMaterial.GroupID;
        foreach (ConfigurableMaterial configMaterial in components.Where(comp => comp is ConfigurableMaterial))
        {
            if (configMaterial.GroupID == groupID)
            {
                configMaterial.ChangeMaterial(currentMaterialSelection);
            }
        }
    }

    //Change material of a selected object
    public void ChangeMaterial(Material mat)
    {
        ConfigurableComponent[] components = GetAllConfigurableForSelected();
        foreach (ConfigurableMaterial configMat in components
                     .Where(comp => comp is ConfigurableMaterial))
        {
            configMat.ChangeMaterial(mat);
        }

        currentMaterialSelection = mat;
    }

    //Get all configurable components of selected object
    //An object can have an unlimited number of configurable components
    //Ex: could have ConfigurableMaterial and ConfigurableColor on the same object
    //to allow for changing the material AND the color of said object
    public ConfigurableComponent[] GetAllConfigurableForSelected()
    {
        if (CurrentSelectedObject != null)
        {
            return CurrentSelectedObject.GetComponents<ConfigurableComponent>();
        }
        return null;
    }

    //Get ALL configurableComponents if needed
    public ConfigurableComponent[] GetAllConfigurables()
    {
        return components.ToArray();
    }

    public bool boundsVisible { get; private set; } = true;
    //Used to toggle the bounding box visuals of an asset
    public void ToggleBounds()
    {
        boundsVisible = !boundsVisible;
        BoundsControl boundsControl = GetComponent<BoundsControl>();
        if (boundsControl != null)
        {
            boundsControl.HandlesActive = boundsVisible;
        }
    }
    
    public void ToggleBounds(bool shouldShow)
    {
        boundsVisible = shouldShow;
        BoundsControl boundsControl = GetComponent<BoundsControl>();
        if (boundsControl != null)
        {
            boundsControl.HandlesActive = boundsVisible;
        }
    }

    //Children can override this to add custom reset functionality
    public virtual void Reset()
    {
        
    }

    //Set clipping distance for ShowWhenClose shader (ARPlane visual)
    public void SetARPlaneClipDist()
    {
        GlobalManager.Instance.SetARPlaneClipDistance(clipDist);
    }

    //Set outline width for selection/hover outline
    public void SetOutlineWidth()
    {
        GlobalManager.Instance.SetOutlineWidth(outlineWidth);
    }
    
    //Toggles any AnimationHotspots this configurable may have
    public void ShowWorldSpaceAnimations(bool show)
    {
        Canvas[] allCanvas = GetComponentsInChildren<Canvas>(true);
        foreach (Canvas canvas in allCanvas)
        {
            canvas.gameObject.SetActive(show);
        }
    }
}
