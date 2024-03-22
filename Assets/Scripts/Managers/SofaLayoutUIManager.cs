using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Slider = Microsoft.MixedReality.Toolkit.UX.Slider;

/// <summary>
/// Class for managing the custom UI for the sofa asset
/// </summary>
public class SofaLayoutUIManager : MonoBehaviour
{
    //the configurable component of the sofa
    Configurable sofaConfigurable;

    //track the current layout that is applied
    string currentLayoutName = string.Empty;
    string currentOttomanLayoutName = string.Empty;


    //reference to the ottoman object
    GameObject ottomanObject;
    GameObject centerPillowObject;

    //track current pillow count
    int currentPillowNum = 10;

    //configurable components of the sofa
    ConfigurableLayout configLayout;
    ConfigurableVisualToggle configVisualToggle;

    //Keep a list of toggle buttons so we can toggle/detoggle as a group
    [SerializeField]
    List<PressableButton> OttomanToggleButtons;

    [SerializeField] private List<GameObject> twoSeaterToggleButtons;
    [SerializeField] private List<GameObject> threeSeaterToggleButtons;
    [SerializeField] private GridLayoutGroup toggleButtonsLayout;
    
    [SerializeField]
    Slider pillowSlider;

    [SerializeField] 
    TMP_Text pillowCountLabel;

    [SerializeField] private ConfigurationTabElement LayoutTab;
    [SerializeField] private ConfigurationTabElement AddOnTab;

    public List<GameObject> ConfigTabs => new() { LayoutTab.gameObject, AddOnTab.gameObject };

    
    // Start is called before the first frame update
    void Start()
    {
        //This object only exists when the current configurable is the sofa
        TryGetReferences();
        currentLayoutName = string.Empty;
        ApplyOttomanLayout("OttomanCloseRight");
    }

    void TryGetReferences()
    {
        sofaConfigurable ??= UIManager.Instance.CurrentConfigurable;
        configVisualToggle ??= sofaConfigurable.gameObject.GetComponent<ConfigurableVisualToggle>();
        configLayout ??= sofaConfigurable.gameObject.GetComponent<ConfigurableLayout>();
    }
    
    //The layout group needs to be refreshed when toggling parts of the UI
    public void RefreshLayoutGroup()
    {
        UIManager.Instance.RefreshVerticalLayoutComponent(AddOnTab.verticalLayoutGroup);
    }
    
    public void SetOttomanAndPillowReference(GameObject ottomanObj, GameObject pillowObj)
    {
        ottomanObject = ottomanObj;
        centerPillowObject = pillowObj;
        TryGetReferences();
        currentLayoutName = string.Empty;
        ApplyOttomanLayout("OttomanCloseRight");
    }

    //Apply the selected layout by name
    public void ApplyLayout(string layoutName)
    {
        bool shouldShowOttoman = ottomanObject.activeInHierarchy;
        configLayout.ApplyLayout(layoutName);
        currentLayoutName = layoutName;
        //Apply Layout will activate all objects.
        //Here, we just the ottoman status and make sure to apply it after the full layout is applied
        configLayout.ToggleObject(ottomanObject, shouldShowOttoman);
        //Pillow toggle might get reset when applying a general layout, update pillows again
        UpdateNumberOfPillows(currentPillowNum);
    }

    //Apply selected layout by name
    //Specific to Ottoman object
    public void ApplyOttomanLayout(string layoutName)
    {
        //If the same toggle button was selected, show/hide the ottoman
        if (currentOttomanLayoutName == layoutName)
        {
            //toggle ottoman object on and off
            configLayout.ToggleObject(ottomanObject, !ottomanObject.activeInHierarchy);
            return;
        }
        //otherwise, apply only the position of the ottoman
        ottomanObject.SetActive(true);
        configLayout.ApplyLayoutPositionOnly(layoutName);
        currentOttomanLayoutName = layoutName;
    }

    //Helper method to detoggle all 'other' toggle buttons
    public void DetoggleOtherButtons(PressableButton btn)
    {
        foreach (PressableButton pressableBtn in OttomanToggleButtons.Where(toggle => toggle != btn))
        {
            pressableBtn.ForceSetToggled(false);
        }
    }

    [SerializeField] private GameObject threeSeaterHighlight;
    [SerializeField] private GameObject twoSeaterHighlight;

    public void OnSelectTwoSeater()
    {
        foreach (GameObject go in threeSeaterToggleButtons)
        {
            go.SetActive(false);
        }

        foreach (GameObject go in twoSeaterToggleButtons)
        {
            go.SetActive(true);
        }

        twoSeaterHighlight.SetActive(true);
        threeSeaterHighlight.SetActive(false);
        toggleButtonsLayout.constraintCount = 2;
    }

    public void OnSelectThreeSeater()
    {
        foreach (GameObject go in twoSeaterToggleButtons)
        {
            go.SetActive(false);
        }

        foreach (GameObject go in threeSeaterToggleButtons)
        {
            go.SetActive(true);
        }
        twoSeaterHighlight.SetActive(false);
        threeSeaterHighlight.SetActive(true);
        toggleButtonsLayout.constraintCount = 3;
    }
    
    public void PillowSliderValueChanged()
    {
        float currentSliderValue = pillowSlider.Value;
        //this is a workaround as there is a bug in the MRTK slider component
        //We are only working with a value of 0-1, so multiply by 10 to get whole numbers
        currentSliderValue *= 10;
        UpdateNumberOfPillows((int)currentSliderValue);
        //update slider label value
        pillowCountLabel.text = Convert.ToString((int)currentSliderValue);
    }

    //using the visual toggle config component, toggle the pillows by slider value
    void UpdateNumberOfPillows(int num)
    {
        //show/hide pillows
        configVisualToggle.ShowNumberOfObjects(num);
        currentPillowNum = num;

        if (currentLayoutName.Contains("two seat", StringComparison.CurrentCultureIgnoreCase))
        {
            StartCoroutine(SetInactiveAfterOneFrame(centerPillowObject));
        }
    }

    IEnumerator SetInactiveAfterOneFrame(GameObject obj)
    {
        yield return null;
        obj.SetActive(false);
    }
}
