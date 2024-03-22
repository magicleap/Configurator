using System;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

/// <summary>
/// A configurable component used to allow changing the color of a material via mesh renderer
/// </summary>

[RequireComponent(typeof(DraggableObjectManipulator))]
public class ConfigurableColor : ConfigurableComponent
{
    //The renderer containing the material you will be changing the color of
    [Tooltip("Renderer containing the material you wish to modify the color of")]
    [SerializeField] 
    private Renderer objectRenderer;

    //Group ID can be assigned 
    //This is used when 'apply all' button is pressed
    //All objects with a matching group ID will have the change applied
    [Tooltip("The group this object belongs to. Objects with the same group ID will be changed when 'apply to all' is selected")]
    public int GroupID = 0;
    
    //What material in the material list to change (check your renderer for this)
    [Tooltip("0 based index of what material to modify the color of (Check your renderer to find this)")]
    [SerializeField]
    private int materialToChange = 0;

    //All color options for this object, UI buttons will be generated based on this
    [Tooltip("All color options you wish to have for this object")]
    [SerializeField] 
    [ColorUsage(true, true)]
    protected Color[] colorOptions;

    //The original color of the material being modified, used for 'reset'
    [ColorUsage(true, true)]
    protected Color originalColor;
    
    //This is a special color for when you want to use a 'gradient' as your color
    //Typically gradients are composed of a light and dark color
    [ColorUsage(true, true)]
    protected Color originalColorDark;

    //There should be a manipulator attached to this object so we can hook into any 
    //selection or hover events
    protected DraggableObjectManipulator manipulator;

    //This should be set to true if your material has a light and dark color
    //Unity does not allow for gradients to be exposed to the editor or scripts, 
    //so our work around is to have a light and dark color we lerp between in our shaders
    [Tooltip("Flag to allow us to modify two colors of a shader since Unity does not allow Gradients to be exposed to scripts yet")]
    public bool IsMultiColor = false;
    
    //This is specific to our metallic car shader created via shader graph
    //We use this to get the color (_Color) from the shader being used
    [Tooltip("Flag to allow us to modify the color of a shader whose color identifier is '_Color', typically the result of a shader made via shadergraph")]
    public bool IsShaderGraphShader = false;
    
    //Event for color changing, useful for UI
    public delegate void ColorChangedAction(Color col);
    public event ColorChangedAction OnColorChanged;

    protected override void Awake()
    {
        base.Awake();
        if (objectRenderer == default)
        {
            objectRenderer = GetComponent<Renderer>();
        }
    }

    protected virtual void Start()
    {
        //If this asset is using the shadergraph, grab the color via name "_Color"
        //If this asset is using a shader that has 2 colors in place of a gradient, grab them with their names "_LightColor" and "_DarkColor"
        //otherwise, just grab the color directly from the material
        if (IsShaderGraphShader)
        {
            originalColor = objectRenderer.materials[materialToChange].GetColor("_Color");
        }
        else if (IsMultiColor)
        {
            originalColor = objectRenderer.materials[materialToChange].GetColor("_LightColor");
            originalColorDark = objectRenderer.materials[materialToChange].GetColor("_DarkColor");
        }
        else
        {
            originalColor = objectRenderer.materials[materialToChange].color;
        }

        //there should always be a DraggableObjectManipulator on this object
        if (manipulator == default)
        {
            manipulator = GetComponent<DraggableObjectManipulator>();
            manipulator?.selectEntered.AddListener(delegate { Select(); });
        }
    }


    protected virtual void  Select()
    {
        //notify global UI manager of currently selected object
        UIManager.Instance.SelectObject(gameObject);
    }

    public virtual Color[] GetColorOptions()
    {
        return colorOptions;
    }
    
    //Change the color of this object
    public virtual void ChangeColor(Color col)
    {
        if (IsShaderGraphShader)
        {
            objectRenderer.materials[materialToChange].SetColor("_Color", col);
        }
        else if (IsMultiColor)
        {
            //set light color and darkcolor
            objectRenderer.materials[materialToChange].SetColor("_LightColor", col);
            objectRenderer.materials[materialToChange].SetColor("_DarkColor", col);
        }
        else
        {
            objectRenderer.materials[materialToChange].color = col;
        }
        //Fire changed color event
        RaiseColorChangedEvent(col);
    }

    //Wrapping this in a protected function allows child classes to raise this class's event
    protected void RaiseColorChangedEvent(Color col)
    {
        OnColorChanged?.Invoke(col);
    }

    //Reset this object to its original color
    public override void Reset()
    {
        if (IsShaderGraphShader)
        {
            objectRenderer.materials[materialToChange].SetColor("_Color", originalColor);
        }
        else if (IsMultiColor)
        {
            objectRenderer.materials[materialToChange].SetColor("_LightColor", originalColor);
            objectRenderer.materials[materialToChange].SetColor("_DarkColor", originalColorDark);
        }
        else
        {
            objectRenderer.materials[materialToChange].color = originalColor;

        }
    }
}
