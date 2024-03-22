using System;
using UnityEngine;

/// <summary>
/// A configurable component used to swap out materials with various options set in the inspector
/// </summary>
public class ConfigurableMaterial : ConfigurableComponent
{
    //the renderer you wish to change the material on
    [Tooltip("Renderer containing the material you wish to modify")]
    [SerializeField] 
    Renderer objectRenderer;
    
    //Group ID can be assigned 
    //This is used when 'apply all' button is pressed
    //All objects with a matching group ID will have the change applied
    [Tooltip("The group this object belongs to. Objects with the same group ID will be changed when 'apply to all' is selected")]
    public int GroupID = 0;
    
    //What material in the material list to change (check your renderer for this)
    [Tooltip("0 based index of what material to modify (Check your renderer to find this)")]
    [SerializeField]
    protected int materialToChange = 0;
    
    //All material options for this object, UI buttons will be generated based on this
    [Tooltip("All material options you wish to have for this object")]
    [SerializeField]
    protected MaterialData[] materialOptions;

    //The original material, used when we 'reset'
    protected Material originalMaterial;
    
    //Event for when the material has been changed, useful for triggering UI changes
    public delegate void MaterialChangedEvent(Material mat);
    public event MaterialChangedEvent OnMaterialChanged;
    
    protected virtual void Start()
    {
        if (objectRenderer == default)
        {
            objectRenderer = GetComponent<Renderer>();
        }

        originalMaterial = objectRenderer.materials[materialToChange];
    }
    
    //Swap the renderer material with one of the material options
    public virtual void ChangeMaterial(Material mat)
    {
        Material[] mats = objectRenderer.materials;
        mats[materialToChange] = mat;
        objectRenderer.materials = mats;
        RaiseMaterialChangedEvent(mat);
    }

    //Wrapping this in a protected function allows child classes to raise this class's event
    protected void RaiseMaterialChangedEvent(Material mat)
    {
        OnMaterialChanged?.Invoke(mat);
    }

    //Get all material options
    //Used for generating UI buttons
    public virtual MaterialData[] GetMaterialOptions()
    {
        return materialOptions;
    }

    //Reset material back to the original
    public override void Reset()
    {
        Material[] mats = objectRenderer.materials;
        mats[materialToChange] = originalMaterial;
        objectRenderer.materials = mats;
    }
}

//Class that describes a material
//Here we have options for material name and thumbnail
//This is visible in the inspector
//You can always add more detail to this if you need more information about your materials
[Tooltip("Data collection for material options. Contains material, thumbnail, and a name for use in UI")]
[Serializable]
public class MaterialData
{
    public Material mat;
    public Sprite thumbnail;
    public string name;
}
