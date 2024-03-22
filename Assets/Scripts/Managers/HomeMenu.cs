using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;

/// <summary>
/// Class that allows management of the core 'home' UI
/// This is the menu that contains add model, edit, reset, settings, and close app buttons and behavior
/// </summary>
public class HomeMenu : MonoBehaviour
{
    //references to UI objects
    [SerializeField]
    private PressableButton editButton;

    [SerializeField]
    private PressableButton resetButton;

    [SerializeField] private GameObject addModelMenu;
    [SerializeField] private GameObject settingsMenu;

    [SerializeField]
    private ConfigurationMenu configurationMenuComponent;

    [SerializeField] private GameObject ConfigurationTabContainer;

    public void OnClickAddModel()
    {
        //show configuration menu with Model Select menu open
        if (settingsMenu.activeInHierarchy)
        {
            ToggleSettingsMenu(false);
        }

        if (configurationMenuComponent.menuVisualsActive)
        {
            configurationMenuComponent.SetMenuVisuals(false);
        }
        ToggleAddModelMenu(!addModelMenu.activeInHierarchy);
        configurationMenuComponent.OnClickAddModel();
    }

    private bool editMenuActive => configurationMenuComponent.menuVisualsActive;
    public void OnClickEdit()
    {
        //show configuration menu with Edit Config menu open
        UIManager.Instance.HideAllNonConfigMenus();
        configurationMenuComponent.SetMenuVisuals(!editMenuActive);
    }

    public void OnClickResetModel()
    {
        UIManager.Instance.ResetModel();
    }

    public void ToggleAddModelMenu(bool show)
    {
        addModelMenu.SetActive(show);
    }
    
    public void HideAllMenus(UIManager.MenuObjects toExclude)
    {
        switch (toExclude)
        {
            case UIManager.MenuObjects.Settings:
                ToggleAddModelMenu(false);
                break;
            case UIManager.MenuObjects.AddModel:
                ToggleSettingsMenu(false);
                break;
            default:
                ToggleSettingsMenu(false);
                ToggleAddModelMenu(false);
                break;
        }
        
    }

    public void ToggleSettingsMenu(bool show)
    {
        settingsMenu.SetActive(show);
    }
    
    public void OnClickSettings()
    {
        if (addModelMenu.activeInHierarchy)
        {
            ToggleAddModelMenu(false);
        }
        if (configurationMenuComponent.menuVisualsActive)
        {
            configurationMenuComponent.SetMenuVisuals(false);
        }
        ToggleSettingsMenu(!settingsMenu.activeInHierarchy);
    }

    public void ToggleEditAndResetButtons(bool show)
    {
        editButton.enabled = show;
        resetButton.enabled = show;
    }

    public void ToggleEditButton(bool show)
    {
        editButton.enabled = show;
    }

    public void ToggleResetButton(bool show)
    {
        resetButton.enabled = show;
    }
    
    //TODO: Add confirmation prompt?
    public void OnClickExitApplication()
    {
        Application.Quit();
    }
}
