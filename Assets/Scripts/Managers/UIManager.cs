using System;
using System.Collections;
using System.Linq;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;
using Slider = Microsoft.MixedReality.Toolkit.UX.Slider;



/// <summary>
/// Class for controlling UI functionality and flow across the scene
/// </summary>
public class UIManager : Singleton<UIManager>
{
    [Tooltip("Object to use as the users head position. This is used for head tracking")]
    public GameObject userHead;
    [Tooltip("Home Menu reference (UI object)")]
    public GameObject HomeMenuObject;
    public HomeMenu homeMenuComponent;

    [Tooltip("Configuration Menu reference (UI Object)")]
    public GameObject ConfigurationMenuObject;
    public ConfigurationMenu configMenuComponent;

    [Tooltip("Input Action Asset reference for obtaining magic leap controller events")]
    public InputActionAsset IAA_ControllerInput;
    private InputAction IA_MenuButton;
    private InputAction IA_Trigger;

    [Tooltip("Reference to the tab manager (UI Object)")]
    [SerializeField]
    private TabElementManager TabManager;


    public Configurable CurrentConfigurable => configMenuComponent != null && configMenuComponent.loadedObject != null
        ? configMenuComponent.loadedObject.GetComponent<Configurable>()
        : null;
    
    [Tooltip("Reference to the haptics object to trigger/update magic leap controller haptics")]
    [SerializeField]
    ControllerHaptics haptics;
    [SerializeField]
    MRTKRayInteractor farRayInteractor;
    
    private GameObject layoutUIObject = null;


    // Start is called before the first frame update
    void Start()
    {
        IA_MenuButton = IAA_ControllerInput.FindAction("Menu");
        IA_Trigger = IAA_ControllerInput.FindAction("TriggerButton");
        IA_MenuButton.performed += OnMenuButtonPressed;
        IA_Trigger.performed += DeselectIfEmpty;
        HomeMenuObject.GetComponent<HomeMenu>().ToggleEditAndResetButtons(false);
        configMenuComponent = ConfigurationMenuObject.GetComponent<ConfigurationMenu>();
        homeMenuComponent = HomeMenuObject.GetComponent<HomeMenu>();
        configMenuComponent.OnModelLoaded += OnModelLoaded;
        ToggleConfigurationMenu(false);
    }


    #region Settings Functions
    //referenced in scene by settings menu toggle
    public void ToggleSegmentedDimming()
    {
        GlobalManager.Instance.ToggleSegmentedDimming();
    }
    
    public void ToggleFloorSnapping()
    {
        GlobalManager.Instance.ToggleFloorSnapping();
    }

    public void ToggleWallSnapping()
    {
        GlobalManager.Instance.ToggleWallSnapping();
    }

    public void ToggleAnimationHotspots()
    {
        GlobalManager.Instance.ToggleWorldSpaceAnimationHotspots();
    }

    [SerializeField] private Slider globalDimmingSlider;
    
    public void OnGlobalDimmingSliderValueChanged()
    {
        //Gets called on first frame (slider component issue)
        //In this case, the global manager instance can be null, so we check for that
        GlobalManager.Instance?.SetGlobalDimmingValue(globalDimmingSlider.Value);
    }
    
    #endregion

    
    //Hide all UI
    //Show only the home menu
    //The home menu should always be presented to the user (follow HMD)
    void OnMenuButtonPressed(InputAction.CallbackContext context)
    {
        //Hide any open menus
        HideAllMenus(MenuObjects.None);
        //Set the menu in front of the user's face
        Transform userHeadTransform = Camera.main.transform;
        HomeMenuObject.transform.position = userHeadTransform.position + (userHeadTransform.forward * 0.8f);
        HomeMenuObject.transform.rotation =
            Quaternion.LookRotation(HomeMenuObject.transform.position - userHeadTransform.position);
        
        //if a configurable exists, show the config menu and the first tab
        if (CurrentConfigurable != null)
        {
            ToggleConfigurationMenu(true);
            ShowFirstTab();
        }
    }

    
    //Deselect an object if the ray is not pointed at anything
    void DeselectIfEmpty(InputAction.CallbackContext context)
    {
        if (CurrentConfigurable != null)
        {
            if (CurrentConfigurable.CurrentSelectedObject != null)
            {
                //if the ray interactor is not hovering over anything, deselect the selected object
                if (!farRayInteractor.interactablesHovered.Any())
                {
                    DeselectObject();
                }
            }
        }
    }

    //Visually toggle tabs and their content
    public void ToggleConfigurationMenu(bool show)
    {
        configMenuComponent.SetMenuVisuals(show);
    }

    //When a model is loaded, we want to reset much of the UI and load a few things
    public void OnModelLoaded()
    {
        homeMenuComponent.ToggleEditButton(true);
        homeMenuComponent.ToggleResetButton(true);
        homeMenuComponent.ToggleAddModelMenu(false);
        configMenuComponent.ResetConfigOptions();
        ToggleConfigurationMenu(true);
        configMenuComponent.ToggleColorTab(true);
        configMenuComponent.ToggleMaterialTab(true);
        UpdateHaptics();
        TabManager.ResetAllTabs();
    }

    public void UpdateHaptics()
    {
        haptics.UpdateHapticEvents();
    }

    public void SelectObject(GameObject obj)
    {
        if(CurrentConfigurable != null)
        {
            //if user is selecting the object that has already been selected, deselect it
            if (obj == CurrentConfigurable.CurrentSelectedObject)
            {
                DeselectObject();
                return;
            }
            //otherwise, deselect the current selection if one exists
            if (CurrentConfigurable.CurrentSelectedObject != null)
            {
                DeselectObject();
            }
            //update selection
            CurrentConfigurable.SetSelectedObject(obj);
            //reset drag timer
            obj.GetComponent<DraggableObjectManipulator>()?.ResetDragTimer(true);
            //load config options for newly selected object
            LoadConfigOptionsForSelected();
            //update haptic events for any new buttons created from loading new config options
            haptics.UpdateHapticEvents();
        }
    }

    void LoadConfigOptionsForSelected()
    {
        ConfigurableComponent[] components = CurrentConfigurable.GetAllConfigurableForSelected();
        
        //load color options
        foreach (ConfigurableColor colorConfig in components.Where(comp=> comp is ConfigurableColor))
        {
            Color[] colors = colorConfig.GetColorOptions();
            //generate buttons for each color option
            configMenuComponent.SetColorOptions(colors);
        }
        //load material options
        foreach (ConfigurableMaterial matConfig in components.Where(comp=> comp is ConfigurableMaterial))
        {
            MaterialData[] mats = matConfig.GetMaterialOptions();
            //generate buttons for each material option
            configMenuComponent.SetMaterialOptions(mats);
        }
    }

    private bool colorButtonActive = true;
    public void ToggleColorButton()
    {
        colorButtonActive = !colorButtonActive;
        configMenuComponent.ToggleColorTab(colorButtonActive);
    }
    
    public void ToggleColorButton(bool show)
    {
        colorButtonActive = show;
        configMenuComponent.ToggleColorTab(show);
    }

    private bool materialButtonActive = true;
    public void ToggleMaterialButton()
    {
        materialButtonActive = !materialButtonActive;
        configMenuComponent.ToggleMaterialTab(materialButtonActive);
    }
    
    public void ToggleMaterialButton(bool show)
    {
        materialButtonActive = show;
        configMenuComponent.ToggleMaterialTab(show);
    }

    public void HideMaterialButton()
    {
        materialButtonActive = false;
        configMenuComponent.ToggleMaterialTab(materialButtonActive);
    }

    void ResetConfigOptions()
    {
        configMenuComponent.SetColorOptions(null);
        configMenuComponent.SetMaterialOptions(null);
    }
    
    public void CreateConfigUI(GameObject UIPrefab)
    {
        if (layoutUIObject != null)
        {
            configMenuComponent.RemoveAllConfigTabs();
            Destroy(layoutUIObject);
        }

        layoutUIObject = Instantiate(UIPrefab, transform, false);
    }

    public GameObject GetLayoutUIObject()
    {
        return layoutUIObject;
    }

    public void CreateConfigTab(ConfigurationTabElement tabElement)
    {
        configMenuComponent.AddTab(tabElement);
    }

    public void ShowFirstTab()
    {
        GameObject tabObject = configMenuComponent.addedTabs.First(tab =>
            tab.name.Contains("layout", StringComparison.CurrentCultureIgnoreCase));
        if (tabObject != null)
        {
            PressableButton buttonComponent = tabObject.GetComponent<PressableButton>();
            //Set toggled after 1 frame or else ui visuals will not update
            StartCoroutine(SetToggledAfterOneFrame(buttonComponent));
            buttonComponent.OnClicked.Invoke();
        }
    }

    IEnumerator SetToggledAfterOneFrame(PressableButton buttonComponent)
    {
        yield return null;
        buttonComponent.ForceSetToggled(true);
    }

    public void DeselectObject()
    {
        if(CurrentConfigurable != null)
        {
            CurrentConfigurable.CurrentSelectedObject?.GetComponent<DraggableObjectManipulator>()?.ResetDragTimer();
            CurrentConfigurable.SetSelectedObject(null);
            ResetConfigOptions();
        }
    }

    public void ResetModel()
    {
        ConfigurationMenu configMenu = ConfigurationMenuObject.GetComponent<ConfigurationMenu>();
        GameObject obj = configMenu.loadedObject;
        DeselectObject();
        configMenuComponent.ToggleColorTab(true);
        configMenuComponent.ToggleMaterialTab(true);
        HideAllMenus(MenuObjects.None);
        if (obj != null)
        {
             obj.GetComponent<Configurable>().ResetAll();
             obj.transform.position = configMenu.GetSpawnPosition();
             obj.transform.rotation = configMenu.GetSpawnRotation(obj.transform.position);
             obj.transform.localScale = Vector3.one;
        }
        ToggleConfigurationMenu(true);
        ShowFirstTab();
    }

    public void RefreshVerticalLayoutComponent(VerticalLayoutGroup layoutGroup)
    {
        StartCoroutine(WaitFrameForLayout(layoutGroup));
    }

    public enum MenuObjects
    {
        Settings,
        AddModel,
        ConfigurationTabs,
        None
    }
    public void HideAllMenus(MenuObjects toExclude)
    {
        homeMenuComponent.HideAllMenus(toExclude);
        configMenuComponent.HideAllTabContent();
        if (toExclude != MenuObjects.ConfigurationTabs)
        {
            configMenuComponent.SetMenuVisuals(false);
        }
    }

    public void HideAllNonConfigMenus()
    {
        homeMenuComponent.HideAllMenus(MenuObjects.None);
    }

    //This is a workaround for the layout not updating when toggling various objects with their own layout components
    IEnumerator WaitFrameForLayout(LayoutGroup layoutGroup)
    {
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false;
            //wait for 1 frame
            yield return 0;
            layoutGroup.enabled = true; 
        }
    }
}
