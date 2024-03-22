using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class specifically for the Car asset
/// You can create any special configurable you wish by inheriting from <see cref="Configurable"/>
/// This allows you to create any behavior you would like with a configurable asset
/// </summary>
public class CarConfigurable : Configurable
{
    //The car has 1 theme, this is a list containing all renderers the theme will be applied to
    [SerializeField]
    private List<MeshRenderer> ThemeRenderers = new();

    //The material to apply when theme is applied
    [SerializeField]
    private Material stripeMaterial;

    //the original material of the car, used for resetting
    [SerializeField] private Material originalMaterial;
    
    public override void Start()
    {
        base.Start();
        //car does not have material options, hide the button in the UI
        UIManager.Instance.HideMaterialButton();
        //check settings to see if we should show animation hotspots
        ShowWorldSpaceAnimations(Settings.ShowAnimationHotspots);
        //enable the light and reflections for the car in the scene
        GlobalManager.Instance.EnableCarLightAndReflections();
    }

    //creat the car specific configurable UI
    public override void CreateUI()
    {
        UIManager.Instance.CreateConfigUI(customUIPrefab);
        GameObject layoutUIObject = UIManager.Instance.GetLayoutUIObject();
        CarLayoutUIManager layoutUIManager = layoutUIObject.GetComponent<CarLayoutUIManager>();
        //layoutUIManager.SetCarConfig(this);
        foreach(GameObject tab in layoutUIManager.ConfigTabs)
        {
            UIManager.Instance.CreateConfigTab(tab.GetComponent<ConfigurationTabElement>());
        }
        UIManager.Instance.ShowFirstTab();
    }


    //Toggle the stripe theme for the car
    private bool themeApplied = false;
    public void ToggleStripeTheme()
    {
        foreach (MeshRenderer renderer in ThemeRenderers)
        {
            if (!themeApplied)
            {
                renderer.material = stripeMaterial;
            }
            else
            {
                renderer.material = originalMaterial;
                renderer.material.color = Color.white;
            }
        }

        themeApplied = !themeApplied;

        //alter configurable color logic when theme is applied
        //for the car, the pinstripe theme materials have a shader that uses _Color as it's color, so it is a shadergraph shader
        //the metallic material uses a light and dark color for it's color, so it is multiColor
        if (themeApplied)
        {
            foreach (ConfigurableColor configColor in components.Where(comp => comp is ConfigurableColor))
            {
                configColor.IsShaderGraphShader = true;
                configColor.IsMultiColor = false;
            }
        }
        else
        {
            foreach (ConfigurableColor configColor in components.Where(comp => comp is ConfigurableColor))
            {
                configColor.IsShaderGraphShader = false;
                configColor.IsMultiColor = true;
            }
        }
    }

    //used for animation hotspots to trigger door open/close animations
    public void TryPlayDoorAnim()
    {
        CarLayoutUIManager uiManager = FindObjectOfType<CarLayoutUIManager>();
        if (uiManager)
        {
            uiManager.PlayDoorAnim(true);
        }
    }

    //When the car is destroyed, turn the material button back on
    private void OnDestroy()
    {
        UIManager.Instance.ToggleMaterialButton();
    }

    public override void Reset()
    {
        base.Reset();
        UIManager.Instance.HideMaterialButton();
        if (themeApplied)
        {
            ToggleStripeTheme();
        }
    }
}
