using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Slider = Microsoft.MixedReality.Toolkit.UX.Slider;

public class MLSofaUIManager : MonoBehaviour
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
    int currentPillowNum = 6;

    //configurable components of the sofa
    ConfigurableLayout configLayout;
    ConfigurableVisualToggle configVisualToggle;

    //Keep a list of toggle buttons so we can toggle/detoggle as a group
    [SerializeField]
    List<PressableButton> OttomanToggleButtons;

    [SerializeField] private List<GameObject> twoSeaterToggleButtons;
    [SerializeField] private List<GameObject> threeSeaterToggleButtons;
    [SerializeField] private GridLayoutGroup toggleButtonsLayout;
    [SerializeField] private PressableButton ottomanCloseMiddleButton;
    [SerializeField] private PressableButton ottomanFarMiddleButton;
    
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
        OnSelectThreeSeater();
        //default layout is Three Seater and OttomanCloseRight
        threeSeaterToggleButtons.Find(x => x.name == "Ottoman Close Right Button").GetComponent<PressableButton>().ForceSetToggled(true);
        currentOttomanLayoutName = "OttomanCloseRight";
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
    }

    //Apply the selected layout by name
    public void ApplyLayout(string layoutName)
    {
        configLayout.ApplyLayout(layoutName);
        currentLayoutName = layoutName;
        //Pillow toggle might get reset when applying a general layout, update pillows again
        UpdateNumberOfPillows(currentPillowNum);
        UpdateOttomanToggleButtons();
    }

    //Apply selected layout by name
    //Specific to Ottoman object
    public void ApplyOttomanLayout(string layoutName, bool shouldToggleOttoman = true)
    {
        //If the same toggle button was selected, show/hide the ottoman
        if (currentOttomanLayoutName == layoutName && shouldToggleOttoman)
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

    void UpdateOttomanToggleButtons()
    {
        if (!ottomanObject.activeInHierarchy)
        {
            DetoggleOtherButtons(null);
            return;
        }
        if (currentLayoutName == "Two Seater")
        {
            if (currentOttomanLayoutName.Contains("FarRight"))
            {
                GameObject twoSeaterFarRightObject =
                    twoSeaterToggleButtons.Find(x => x.name.Contains("Two Seater Far Right"));
                if (twoSeaterFarRightObject)
                {
                    PressableButton btn = twoSeaterFarRightObject.GetComponent<PressableButton>();
                    btn.ForceSetToggled(true);
                    DetoggleOtherButtons(btn);
                }
            }
            else if (currentOttomanLayoutName.Contains("CloseRight"))
            {
                GameObject twoSeaterCloseRightObject =
                    twoSeaterToggleButtons.Find(x => x.name.Contains("Two Seater Close Right"));
                if (twoSeaterCloseRightObject)
                {
                    PressableButton btn = twoSeaterCloseRightObject.GetComponent<PressableButton>();
                    btn.ForceSetToggled(true);
                    DetoggleOtherButtons(btn);
                }
            }
            else if (currentOttomanLayoutName.Contains("FarLeft") || currentOttomanLayoutName.Contains("FarMiddle"))
            {
                ApplyOttomanLayout("OttomanFarMiddle", false);
                ottomanFarMiddleButton.ForceSetToggled(true);
                DetoggleOtherButtons(ottomanFarMiddleButton);
            }
            else if (currentOttomanLayoutName.Contains("CloseLeft") || currentOttomanLayoutName.Contains("CloseMiddle"))
            {
                ApplyOttomanLayout("OttomanCloseMiddle", false);
                ottomanCloseMiddleButton.ForceSetToggled(true);
                DetoggleOtherButtons(ottomanCloseMiddleButton);
            }
        }
        else if (currentLayoutName == "Three Seater" || currentLayoutName == "Default")
        {
            if (currentOttomanLayoutName.Contains("FarRight"))
            {
                GameObject threeSeaterFarRightObject =
                    threeSeaterToggleButtons.Find(x => x.name == ("Ottoman Far Right Button"));
                if (threeSeaterFarRightObject)
                {
                    PressableButton btn = threeSeaterFarRightObject.GetComponent<PressableButton>();
                    btn.ForceSetToggled(true);
                    DetoggleOtherButtons(btn);
                }
            }
            else if (currentOttomanLayoutName.Contains("CloseRight"))
            {
                GameObject threeSeaterCloseRightObject =
                    threeSeaterToggleButtons.Find(x => x.name == ("Ottoman Close Right Button"));
                if (threeSeaterCloseRightObject)
                {
                    PressableButton btn = threeSeaterCloseRightObject.GetComponent<PressableButton>();
                    btn.ForceSetToggled(true);
                    DetoggleOtherButtons(btn);
                }
            }
            else if (currentOttomanLayoutName.Contains("FarMiddle"))
            {
                GameObject threeSeaterFarMiddleObject =
                    threeSeaterToggleButtons.Find(x => x.name == ("Ottoman Far Middle Button"));
                if (threeSeaterFarMiddleObject)
                {
                    PressableButton btn = threeSeaterFarMiddleObject.GetComponent<PressableButton>();
                    btn.ForceSetToggled(true);
                    DetoggleOtherButtons(btn);
                }
            }
            else if (currentOttomanLayoutName.Contains("CloseMiddle"))
            {
                GameObject threeSeaterFarMiddleObject =
                    threeSeaterToggleButtons.Find(x => x.name == ("Ottoman Close Middle Button"));
                if (threeSeaterFarMiddleObject)
                {
                    PressableButton btn = threeSeaterFarMiddleObject.GetComponent<PressableButton>();
                    btn.ForceSetToggled(true);
                    DetoggleOtherButtons(btn);
                }
            }
        }
    }

    public void PillowSliderValueChanged()
    {
        float currentSliderValue = pillowSlider.Value;
        float mappedValue = map(currentSliderValue, 0, 1, 0, 6);
        UpdateNumberOfPillows((int)mappedValue);
        //update slider label value
        pillowCountLabel.text = Convert.ToString((int)mappedValue);
    }

    float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
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

    public void OnReset()
    {
        GameObject toggleButton = threeSeaterToggleButtons.Find(x => x.name.Contains("Ottoman Close Right"));
        if (toggleButton)
        {
            PressableButton btn = toggleButton.GetComponent<PressableButton>();
            btn.ForceSetToggled(true);
            DetoggleOtherButtons(btn);
        }

        currentLayoutName = "Default";
        currentOttomanLayoutName = string.Empty;
        pillowSlider.Value = 1;
    }
}
