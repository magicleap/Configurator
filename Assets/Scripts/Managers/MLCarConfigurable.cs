using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLCarConfigurable : Configurable
{
    public override void Start()
    {
        base.Start();
        //car does not have material options, hide the button in the UI
        UIManager.Instance.ToggleMaterialButton(false);
        UIManager.Instance.ToggleColorButton(true);
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
        foreach(GameObject tab in layoutUIManager.ConfigTabs)
        {
            UIManager.Instance.CreateConfigTab(tab.GetComponent<ConfigurationTabElement>());
        }
        UIManager.Instance.ShowFirstTab();
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
        if (UIManager.Instance.CurrentConfigurable == this)
        {
            UIManager.Instance.ToggleMaterialButton(true);
        }
    }

    public override void Reset()
    {
        base.Reset();
        CarLayoutUIManager uiManager = FindObjectOfType<CarLayoutUIManager>();
        if (uiManager.doorAnimPlayed)
        {
            TryPlayDoorAnim();
        }
        UIManager.Instance.ToggleMaterialButton(false);
    }
}
