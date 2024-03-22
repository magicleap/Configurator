using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A configurable component used to swap out multiple renderer's materials with various options set in the inspector
/// </summary>
public class ConfigurableMaterialGroup : ConfigurableMaterial
{
    //List of all renderers we wish to modify the materials of
    [Tooltip("List of object renderers containing the material you wish to modify")]
    [SerializeField] 
    List<Renderer> objectRenderers = new();

    //List of all original materials for each renderer
    private List<Material> originalMaterials = new();
    

    // Start is called before the first frame update
    protected override void Start()
    {
        foreach (Renderer rend in objectRenderers)
        {
            originalMaterials.Add(rend.materials[materialToChange]);
        }
    }

    //Similar to base class, but we loop through multiple renderers instead of 1
    public override void ChangeMaterial(Material mat)
    {
        //check for empty list to avoid raising changed event if nothing actually changed
        if (objectRenderers.Count == 0)
        {
            return;
        }
        //loop through each renderer and change it's material
        foreach (Renderer rend in objectRenderers)
        {
            Material[] mats = rend.materials;
            mats[materialToChange] = mat;
            rend.materials = mats;
        }
        
        RaiseMaterialChangedEvent(mat);
    }

    //reset all renderer materials by looping through them
    public override void Reset()
    {
        for (int i = 0; i < originalMaterials.Count; i++)
        {
            Material[] mats = objectRenderers[i].materials;
            mats[materialToChange] = originalMaterials[i];
            objectRenderers[i].materials = mats;
        }
    }
}
