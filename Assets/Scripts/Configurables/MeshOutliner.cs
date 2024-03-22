using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

/// <summary>
/// Class for outlining a mesh when hovered or selected
/// </summary>
[RequireComponent(typeof(DraggableObjectManipulator))]
public class MeshOutliner : ConfigurableComponent
{
    protected bool IsHovered = false;

    protected bool IsSelected = false;

    //components needed for hooking into selection/hover events
    protected DraggableObjectManipulator manipulator;
    protected ConfigurableColor colorConfigComponent;
    protected ConfigurableMaterial materialConfigComponent;
    
    //used to track this objects original layer
    protected int layerBeforeSelection = -1;

    protected int outlineLayer;


    protected override void Awake()
    {
        base.Awake();
        manipulator = GetComponent<DraggableObjectManipulator>();
        colorConfigComponent = GetComponent<ConfigurableColor>();
        materialConfigComponent = GetComponent<ConfigurableMaterial>();
        outlineLayer = LayerMask.NameToLayer("Outline");
    }

    protected virtual void InitEventListeners()
    {
        manipulator.hoverEntered.AddListener(delegate { OnHoverEnter(); });
        manipulator.hoverExited.AddListener(delegate { OnHoverExit(); });

        configurableRoot.OnSelectionChanged += OnSelectionChanged;
        if (colorConfigComponent != null)
        {
            colorConfigComponent.OnColorChanged += OnColorChanged;
        }

        if (materialConfigComponent != null)
        {
            materialConfigComponent.OnMaterialChanged += OnMaterialChanged;
        }
    }

    protected virtual void RemoveEventListeners()
    {
        manipulator.hoverEntered.RemoveListener(delegate { OnHoverEnter(); });
        manipulator.hoverExited.RemoveListener(delegate { OnHoverExit(); });

        configurableRoot.OnSelectionChanged -= OnSelectionChanged;
        if (colorConfigComponent != null)
        {
            colorConfigComponent.OnColorChanged -= OnColorChanged;
        }
        
        if (materialConfigComponent != null)
        {
            materialConfigComponent.OnMaterialChanged -= OnMaterialChanged;
        }
    }

    //subscribe to events
    protected virtual void OnEnable()
    {
        InitEventListeners();
    }

    //unsubscribe from events
    protected virtual void OnDisable()
    {
        RemoveEventListeners();
    }

    //when this event is fired, select or deselect this object
    //ensures only 1 object is outlined when selected at a time
    protected virtual void OnSelectionChanged(GameObject obj)
    {
        if (obj != this.gameObject)
        {
            OnDeselect();
        }
        else
        {
            OnSelect();
        }
    }

    //When hovered, outline this object
    protected virtual void OnHoverEnter()
    {
        IsHovered = true;
        if (layerBeforeSelection == -1)
        {
            layerBeforeSelection = gameObject.layer;
        }
        gameObject.layer = outlineLayer;
    }

    //When not hovered and NOT selected, stop outlining by going back to original layer
    protected virtual void OnHoverExit()
    {
        IsHovered = false;
        if (!IsSelected && layerBeforeSelection != -1)
        {
            gameObject.layer = layerBeforeSelection;
        }
    }

    //When selected, outline
    //If selected while already selected, toggle the outline by deselecting
    protected virtual void OnSelect()
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
        gameObject.layer = outlineLayer;
    }

    //stop outlining this object
    protected virtual void OnDeselect()
    {
        IsSelected = false;
        if (gameObject.layer == outlineLayer)
        {
            gameObject.layer = layerBeforeSelection;
        }
    }

    //force deselect by external class
    public virtual void Deselect()
    {
        OnDeselect();
        UIManager.Instance.DeselectObject();
    }
    
    //When the color is changed, stop outlining
    protected virtual void OnColorChanged(Color col)
    {
        Reset();
    }

    //When the material is changes, stop outlining
    protected virtual void OnMaterialChanged(Material mat)
    {
        Reset();
    }

    //reset object layer to original layer
    public override void Reset()
    {
        //RESET
        IsSelected = false;
        IsHovered = false;
        if (gameObject.layer == outlineLayer)
        {
            gameObject.layer = layerBeforeSelection;
        }
    }
}
