using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

/// <summary>
/// Class allowing us to outline multiple objects that we want to treat as one object
/// </summary>
[RequireComponent((typeof(DraggableObjectManipulator)))]
public class MeshOutlinerGroup : MeshOutliner
{
    public List<Renderer> renderers = new();
    
    //same as base class, just over multiple renderers
    public override void Deselect()
    {
        IsSelected = false;
        foreach (Renderer rend in renderers)
        {
            if (rend.gameObject.layer == LayerMask.NameToLayer("Outline"))
            {
                rend.gameObject.layer = layerBeforeSelection;
            }
        }
    }

    //same as base class, just over multiple renderers
    protected override void OnSelect()
    {
        if (IsSelected)
        {
            OnDeselect();
            return;
        }
        if (layerBeforeSelection == -1)
        {
            layerBeforeSelection = gameObject.layer;
        }
        IsSelected = true;

        foreach (Renderer rend in renderers)
        {
            rend.gameObject.layer = outlineLayer;
        }
    }
    
    //same as base class, just over multiple renderers
    protected override void OnHoverEnter()
    {
        IsHovered = true;
        if (layerBeforeSelection == -1)
        {
            layerBeforeSelection = gameObject.layer;
        }
        foreach (Renderer rend in renderers)
        {
            rend.gameObject.layer = outlineLayer;
        }
    }

    //same as base class, just over multiple renderers
    protected override void OnHoverExit()
    {
        IsHovered = false;
        if (!IsSelected && layerBeforeSelection != -1)
        {
            foreach (Renderer rend in renderers)
            {
                rend.gameObject.layer = layerBeforeSelection;
            }
        }
    }
}
