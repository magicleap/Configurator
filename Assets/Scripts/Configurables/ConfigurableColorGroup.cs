using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

/// <summary>
/// Class for when an objects mesh has separate game objects that you wish to treat as one object
/// We can take multiple renderers and loop through them to modify the color of each
/// </summary>
public class ConfigurableColorGroup : ConfigurableColor
{
    //List of renderers we will be modifying 
    [Tooltip("List of object renderers and index of the materials you wish to modify colors of")]
    public List<RendererAndMaterial> objectRenderers = new();

    //Dictionary of original colors and their respective renderers/material index
    private Dictionary<RendererAndMaterial, Color> originalColors = new();
    
    //Dictionary of original colors (Light and Dark colors) and their respective renderers/material index
    private Dictionary<RendererAndMaterial, Color[]> originalMultiColors = new();


    //Same logic as base class, but we use the first object renderer in our list to retrieve original colors
    protected override void Start()
    {
        foreach (RendererAndMaterial rendAndMat in objectRenderers)
        {
            if (IsShaderGraphShader)
            {
                originalColors.Add(rendAndMat, rendAndMat.rend.materials[rendAndMat.materialIndex].GetColor("_Color"));
            }
            else if (IsMultiColor)
            {
                originalMultiColors.Add(rendAndMat,
                    new[]
                    {
                        rendAndMat.rend.materials[rendAndMat.materialIndex].GetColor("_LightColor"),
                        rendAndMat.rend.materials[rendAndMat.materialIndex].GetColor("_DarkColor")
                    });
            }
            else
            {
                originalColors.Add(rendAndMat, rendAndMat.rend.materials[rendAndMat.materialIndex].color);
            }
        }
        

        //there should always be a DraggableObjectManipulator on this object
        if (manipulator == default)
        {
            manipulator = GetComponent<DraggableObjectManipulator>();
            manipulator?.selectEntered.AddListener(delegate { Select(); });
        }
    }

    //Similar to base class, but instead of changing the color of 1 renderer material, we change all renderer's materials
    public override void ChangeColor(Color col)
    {
        if (IsShaderGraphShader)
        {
            foreach (RendererAndMaterial rendAndMat in objectRenderers)
            {
                rendAndMat.rend.materials[rendAndMat.materialIndex].SetColor("_Color", col);
            }
        }
        else if (IsMultiColor)
        {
            //set light color and darkcolor
            foreach (RendererAndMaterial rendAndMat in objectRenderers)
            {
                rendAndMat.rend.materials[rendAndMat.materialIndex].SetColor("_LightColor", col);
                rendAndMat.rend.materials[rendAndMat.materialIndex].SetColor("_DarkColor", col);
            }
            
        }
        else
        {
            foreach (RendererAndMaterial rendAndMat in objectRenderers)
            {
                rendAndMat.rend.materials[rendAndMat.materialIndex].color = col;
            }
        }
        RaiseColorChangedEvent(col);
    }

    //Same as base class, but we reset all renderer materials instead of just one
    public override void Reset()
    {
        if (IsMultiColor)
        {
            foreach (KeyValuePair<RendererAndMaterial, Color[]> pair in originalMultiColors)
            {
                pair.Key.rend.materials[pair.Key.materialIndex].SetColor("_LightColor", pair.Value[0]);
                pair.Key.rend.materials[pair.Key.materialIndex].SetColor("_DarkColor", pair.Value[1]);
            }

            return;
        }
        
        foreach (KeyValuePair<RendererAndMaterial, Color> pair in originalColors)
        {
            if (IsShaderGraphShader)
            {
                pair.Key.rend.materials[pair.Key.materialIndex].SetColor("_Color", pair.Value);
            }
            else
            {
                pair.Key.rend.materials[pair.Key.materialIndex].color = pair.Value;
            }
        }
    }
}

[Serializable]
public class RendererAndMaterial
{
    public Renderer rend;
    public int materialIndex;
}
