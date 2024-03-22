using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ConfigurationMenu : MonoBehaviour
{
    //Event for loading a new model, other components might want to do something
    //when a new model is loaded
    public delegate void ModelLoadedEvent();
    public event ModelLoadedEvent OnModelLoaded;
    
    //Parent object where all 'Model buttons' live
    [SerializeField] 
    private GameObject AddModelButtonParent;

    //how far away from the user to spawn the asset
    [SerializeField]
    private float modelSpawnDistance = 2.5f;
    
    //Button prefabs, one for models, colors, and materials
    [SerializeField] private GameObject ModelButtonPrefab;
    [SerializeField] private GameObject ColorButtonPrefab;
    [SerializeField] private GameObject MaterialButtonPrefab;

    //All current configurable asset. Add more to this list and more buttons will be generated
    [SerializeField]
    List<GameObject> allModelPrefabs;

    //Currently loaded model
    public GameObject loadedObject { get; private set; } = null;
    
    //Parent object of all Configuration Tabs
    [SerializeField]
    private GameObject TabContainer;
    //Color and material tab elements exist between all configurable assets
    //These are their references
    [SerializeField]
    private ConfigurationTabElement colorTabElement;
    [SerializeField]
    private ConfigurationTabElement materialTabElement;
    //Parent object of any configuration tab content
    //We have a parent for many of these UI objects for layout purposes
    [SerializeField]
    private GameObject ContentParentObject;

    //Container of the entire configuration menu, used to toggle visuals on and off when needed
    [SerializeField] private GameObject menuContainer;
    
    //current status of this menu being visible to the user
    public bool menuVisualsActive { get; private set; } = false;
    
    //All current configuration tabs for the loaded asset
    public List<GameObject> addedTabs { get; private set; } = new();
    
    //button generation bools
    //buttons get created and destroyed for every asset that is loaded
    //this is how we keep track of what buttons are currently generated
    private bool modelButtonsGenerated = false;
    private bool colorOptionsGenerated = false;
    private bool materialOptionsGenerated = false;

    
    public void SetMenuVisuals(bool shouldShow)
    {
        menuContainer.SetActive(shouldShow);
        TabContainer.SetActive(shouldShow);
        menuVisualsActive = shouldShow;
    }

    //When the add model button is clicked, generate model buttons if they haven't been generated yet
    //update haptics as we just created new buttons
    public void OnClickAddModel()
    {
        if (!modelButtonsGenerated)
        {
            //Generate buttons
            CreateModelButtons();
            UIManager.Instance.UpdateHaptics();
        }
    }

    //For every model in the all models list, create a button for it with information obtained from the configurble
    //All assets in allmodelprefabs should have Configurable component at the root
    void CreateModelButtons()
    {
        foreach (GameObject obj in allModelPrefabs)
        {
            Configurable config = obj.GetComponent<Configurable>();
            GameObject button = Instantiate(ModelButtonPrefab, AddModelButtonParent.transform, false);
            ButtonAssetInfo info = button.GetComponent<ButtonAssetInfo>();
            info.buttonLabel.text = config.Name;
            info.SetImage(config.thumbnail);
            info.button.OnClicked.AddListener(delegate { LoadModel(obj); });
        }
        modelButtonsGenerated = true;
    }

    //Add a configuration tab to the UI
    public void AddTab(ConfigurationTabElement tabElement)
    {
        //Set the parent to the tabcontainer
        tabElement.transform.SetParent(TabContainer.transform, false);
        //Add onClick event
        tabElement.GetComponent<PressableButton>().OnClicked.AddListener(delegate { OnClickGenericTab(tabElement); });
        addedTabs.Add(tabElement.gameObject);
        //This ensures that any 'layout' tab is at the top of the tab list in the UI
        SetLayoutTabAsLast();
        //Refresh UI layout since we added a new element to it
        //Unity doesn't refresh this automatically (it should, but it doesn't, so we force it)
        UIManager.Instance.RefreshVerticalLayoutComponent(TabContainer.GetComponent<VerticalLayoutGroup>());
    }

    //For this project we want the layout tab to always be at the top of the UI
    //so we 'sort' the tabs everytime we add one
    void SetLayoutTabAsLast()
    {
        foreach (GameObject tab in addedTabs)
        {
            if (tab.name.Contains("layout", StringComparison.CurrentCultureIgnoreCase))
            {
                tab.transform.SetAsLastSibling();
                break;
            }
        }
    }

    public void RemoveTab(ConfigurationTabElement tabElement)
    {
        GameObject obj = addedTabs.Find(x => x == tabElement.gameObject);
        if (obj)
        {
            addedTabs.Remove(obj);
            Destroy(obj);
        }
        UIManager.Instance.RefreshVerticalLayoutComponent(TabContainer.GetComponent<VerticalLayoutGroup>());
    }

    //When an asset is loaded, we remove any pre-existing configuration tabs
    public void RemoveAllConfigTabs()
    {
        foreach (GameObject go in addedTabs)
        {
            ConfigurationTabElement tabElement = go.GetComponent<ConfigurationTabElement>();
            Destroy(tabElement.contentContainer);
            Destroy(go);
        }
        addedTabs.Clear();
        UIManager.Instance.RefreshVerticalLayoutComponent(TabContainer.GetComponent<VerticalLayoutGroup>());
    }
    
    //Set color tab active/inactive
    public void ToggleColorTab(bool shouldShow)
    {
        colorTabElement.gameObject.SetActive(shouldShow);
    }
    
    //Set material tab active/inactive
    public void ToggleMaterialTab(bool shouldShow)
    {
        materialTabElement.gameObject.SetActive(shouldShow);
    }
    
    public void OnClickChangeColor()
    {
        //if color tab is open, close it (toggle behavior)
        if (colorTabElement.contentContainer.activeInHierarchy)
        {
            HideAllTabContent();
            return;
        }
        //Hide all menues, exclude configuration menu (this menu)
        UIManager.Instance.HideAllMenus(UIManager.MenuObjects.ConfigurationTabs);
        //Ensure tab content parent is enabled
        ContentParentObject.SetActive(true);
        //show color tab content
        colorTabElement.contentContainer.SetActive(true);
        colorTabElement.contentContainer.transform.SetParent(ContentParentObject.transform, false);
        UIManager.Instance.RefreshVerticalLayoutComponent(colorTabElement.verticalLayoutGroup);
        //disable apply all button for color tab
        ToggleApplyAllButton(colorTabElement, false);
    }

    //On click event for all configuration tabs
    void OnClickGenericTab(ConfigurationTabElement tabElement)
    {
        //toggle behavior, if the tab content is currently active, deactivate it 
        if (tabElement.contentContainer.activeInHierarchy)
        {
            HideAllTabContent();
            tabElement.contentContainer.SetActive(false);
            return;
        }
        UIManager.Instance.HideAllMenus(UIManager.MenuObjects.ConfigurationTabs);
        ContentParentObject.SetActive(true);
        //show tab content
        tabElement.contentContainer.SetActive(true);
        tabElement.contentContainer.transform.SetParent(ContentParentObject.transform, false);
        UIManager.Instance.RefreshVerticalLayoutComponent(tabElement.verticalLayoutGroup);
    }

    public void OnClickChangeMaterial()
    {
        if (materialTabElement.contentContainer.activeInHierarchy)
        {
            HideAllTabContent();
            return;
        }
        UIManager.Instance.HideAllMenus(UIManager.MenuObjects.ConfigurationTabs);
        ContentParentObject.SetActive(true);
        materialTabElement.contentContainer.SetActive(true);
        materialTabElement.contentContainer.transform.SetParent(ContentParentObject.transform, false);
        UIManager.Instance.RefreshVerticalLayoutComponent(materialTabElement.verticalLayoutGroup);
        ToggleApplyAllButton(materialTabElement, false);
    }
    
    //We can use apply all on materials and colors
    public void OnClickApplyToAll()
    {
        //if context is color change (color tab content container is currently active)
        //otherwise, apply all to materials since it is the only other option
        if (colorTabElement.contentContainer.activeInHierarchy)
        {
            //apply color to all
            Configurable configurable = loadedObject.GetComponent<Configurable>();
            configurable.ApplyColorToAll();
        }
        else if (materialTabElement.contentContainer.activeInHierarchy)
        {
            //apply material to all
            Configurable configurable = loadedObject.GetComponent<Configurable>();
            configurable.ApplyMaterialToAll();
        }
    }
    //Hide all tab content, but NOT the tabs themselves
    public void HideAllTabContent()
    {
        foreach (GameObject obj in addedTabs)
        {
            obj.GetComponent<ConfigurationTabElement>().contentContainer.SetActive(false);
        }
        colorTabElement.contentContainer.SetActive(false);
        materialTabElement.contentContainer.SetActive(false);
    }

    //Load the selected configurable prefab
    public void LoadModel(GameObject prefab)
    {
        Vector3 spawnPos = default;
        Quaternion spawnRotation = default;
        bool wasDestroyed = false;
        
        //if there is an already loaded model, use it's position as the new spawn position
        if (loadedObject)
        {
            spawnPos = loadedObject.transform.position;
            wasDestroyed = true;
            Destroy(loadedObject);
        }
        
        loadedObject = Instantiate(prefab);

        //If there wasn't a previously loaded asset, get a default spawn position
        if (!wasDestroyed)
        {
            spawnPos = GetSpawnPosition();
        }

        //Get rotation (spawn position is needed for this calculation)
        spawnRotation = GetSpawnRotation(spawnPos);
        loadedObject.transform.position = spawnPos;
        loadedObject.transform.rotation = spawnRotation;
        Configurable config = loadedObject.GetComponent<Configurable>();
        //Update the PlaneClipDistance for plane visualization as this changes per asset (assets are different sizes)
        config.SetARPlaneClipDist();
        //Assets also have different outline widths that they look pleasant with, so update it
        config.SetOutlineWidth();
        OnModelLoaded?.Invoke();
    }

    public Vector3 GetSpawnPosition()
    {
        Transform headTransform = UIManager.Instance.userHead.transform;
        //Spawn the object in front of the user at a specific distance
        Vector3 spawnPos = headTransform.position + headTransform.forward * modelSpawnDistance;
        //Do not take into account the y axis in position and rotation calculations to avoid up/down variance
        //We don't want to spawn the object up in the air, rather, on the 'floor'
        ARPlane floorPlane = PlanesManager.Instance.GetNearestPlane(spawnPos, PlaneClassification.Floor);
        if (floorPlane != null)
        {
            spawnPos.y = floorPlane.transform.position.y;
        }
        else
        {
            spawnPos.y -= 0.8f;
        }
        return spawnPos;
    }

    public Quaternion GetSpawnRotation(Vector3 spawnPos)
    {
        Vector3 headPos = UIManager.Instance.userHead.transform.position;
        headPos.y = spawnPos.y;
        //Create a rotation that is the object looking in the direction of the users head
        Quaternion rot = Quaternion.LookRotation(headPos - spawnPos, Vector3.up);
        //if the configurable has a spawn rotation (offset), apply it here
        rot *= Quaternion.Euler(loadedObject.GetComponent<Configurable>().spawnRotation);
        return rot;
    }

    //Configurable assets can have different color options per configurable object
    //Handle creating buttons for each color option
    public void SetColorOptions(Color[] colors)
    {
        if (colorOptionsGenerated)
        {
            ResetColorOptions();
        }

        if (colors == null)
        {
            colorOptionsGenerated = false;
            return;
        }
        
        //generate buttons for each color
        CreateColorButtons(colors);
        colorTabElement.emptyContentLabel.SetActive(false);
        UIManager.Instance.RefreshVerticalLayoutComponent(colorTabElement.verticalLayoutGroup);
        ToggleApplyAllButton(colorTabElement, false);
    }

    void CreateColorButtons(Color[] colors)
    {
        foreach (Color col in colors)
        {
            GameObject button = Instantiate(ColorButtonPrefab, colorTabElement.buttonParent.transform, false);
            ButtonAssetInfo info = button.GetComponent<ButtonAssetInfo>();
            info.buttonLabel.text = "";
            info.buttonImage.color = col;
            info.button.OnClicked.AddListener(delegate { ChangeColor(col); });
        }
        colorOptionsGenerated = true;
    }

    //Material and color tabs have an apply all button, here we can activate/deactivate it
    void ToggleApplyAllButton(ConfigurationTabElement tabElement, bool shouldEnable)
    {
        tabElement.applyAllButton.GetComponent<PressableButton>().enabled = shouldEnable;
    }

    //Configurable assets can have different material options per configurable object
    //Handle creating buttons for each material option
    public void SetMaterialOptions(MaterialData[] mats)
    {
        if (materialOptionsGenerated)
        {
            ResetMaterialOptions();
        }

        if (mats == null)
        {
            materialOptionsGenerated = false;
            return;
        }
        
        CreateMaterialButtons(mats);
        materialTabElement.emptyContentLabel.SetActive(false);
        UIManager.Instance.RefreshVerticalLayoutComponent(materialTabElement.verticalLayoutGroup);
        ToggleApplyAllButton(materialTabElement, false);
    }

    void CreateMaterialButtons(MaterialData[] mats)
    {
        foreach (MaterialData matData in mats)
        {
            GameObject button = Instantiate(MaterialButtonPrefab, materialTabElement.buttonParent.transform, false);
            ButtonAssetInfo info = button.GetComponent<ButtonAssetInfo>();
            info.buttonLabel.text = matData.name;
            info.SetImage(matData.thumbnail);
            info.button.OnClicked.AddListener(delegate { ChangeMaterial(matData.mat); });
        }
        materialOptionsGenerated = true;
    }

    //clear color and material options (destroy generated buttons)
    public void ResetConfigOptions()
    {
        ResetColorOptions();
        ResetMaterialOptions();
    }

    void ResetColorOptions()
    {
        foreach (ButtonAssetInfo objButtonInfo in
                 colorTabElement.buttonParent.GetComponentsInChildren<ButtonAssetInfo>())
        {
            Destroy(objButtonInfo.gameObject);
        }

        colorOptionsGenerated = false;
        colorTabElement.emptyContentLabel.SetActive(true);
        UIManager.Instance.RefreshVerticalLayoutComponent(colorTabElement.verticalLayoutGroup);
        ToggleApplyAllButton(colorTabElement, false);
    }
    
    void ResetMaterialOptions()
    {
        foreach (ButtonAssetInfo objButtonInfo in materialTabElement.buttonParent
                     .GetComponentsInChildren<ButtonAssetInfo>())
        {
            Destroy(objButtonInfo.gameObject);
        }

        materialOptionsGenerated = false;
        materialTabElement.emptyContentLabel.SetActive(true);
        UIManager.Instance.RefreshVerticalLayoutComponent(materialTabElement.verticalLayoutGroup);
        ToggleApplyAllButton(materialTabElement, false);

    }

    void ChangeColor(Color col)
    {
        UIManager.Instance.CurrentConfigurable.ChangeColor(col);
        ToggleApplyAllButton(colorTabElement, true);
    }

    void ChangeMaterial(Material mat)
    {
        UIManager.Instance.CurrentConfigurable.ChangeMaterial(mat);
        ToggleApplyAllButton(materialTabElement, true);
    }
}
