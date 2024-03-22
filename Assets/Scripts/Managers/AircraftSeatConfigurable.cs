using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


/// <summary>
/// Class specifically for the AircraftSeat asset
/// You can create any special configurable you wish by inheriting from <see cref="Configurable"/>
/// This allows you to create any behavior you would like with a configurable asset
/// </summary>
public class AircraftSeatConfigurable : Configurable
{
    //References that we will pass to the UI manager
    public GameObject CabinReference;
    public GameObject ScaleableWoodBlock;
    public GameObject ScaleableSidePanel;

    //List of all seats created
    public List<GameObject> allSeats = new();

    //Individual light for the seat, just allows us to light the seat better
    public GameObject seatLight;

    //current seat distance during cabin mode
    private float seatDist = 1.2f;
    //Since we create copies of this seat when we turn the cabin on
    //we do NOT want those seats to fire off any logic, so we skip initialization 
    public bool SkipInitialization = false;

    //References for video 'easter egg'
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoRenderImage;
    [SerializeField] Canvas videoCanvas;
    [SerializeField] private RenderTexture videoRenderTexturePrefab;

    public bool IsVideoPlaying => videoPlayer.isPlaying;

    public override void Start()
    {
        if (SkipInitialization)
        {
            return;
        }
        
        base.Start();
        //check settings to see if we should show animation hotspots or not
        ShowWorldSpaceAnimations(Settings.ShowAnimationHotspots);
        //turn on the light in the scene for the seat
        GlobalManager.Instance.EnableSeatAndSofaLightAndReflections();
    }

    //Create this assets specific configurable UI
    public override void CreateUI()
    {
        UIManager.Instance.CreateConfigUI(customUIPrefab);
        GameObject layoutUIObject = UIManager.Instance.GetLayoutUIObject();
        AircraftLayoutUIManager layoutUIManager = layoutUIObject.GetComponent<AircraftLayoutUIManager>();
        layoutUIManager.SetConfigurable(this);
        foreach(GameObject tab in layoutUIManager.ConfigTabs)
        {
            UIManager.Instance.CreateConfigTab(tab.GetComponent<ConfigurationTabElement>());
        }
        UIManager.Instance.ShowFirstTab();
    }

    //Here, we populate the cabin with seats when the cabin is toggled on
    //By default, there is 10 seats that are 1.2m apart
    public void PopulateCabinWithCurrentConfig()
    {
        //deselect all objects
        foreach (MeshOutliner highlighter in GetComponentsInChildren<MeshOutliner>())
        {
            highlighter.Deselect();
        }

        //disable seat interaction
        ToggleAssetInteraction(gameObject, false);

        //if there are any seats, destroy them 
        if (allSeats.Any())
        {
            foreach (GameObject go in allSeats)
            {
                Destroy(go);
            }
            allSeats.Clear();
        }
        
        //Create seats There is a total of 10, INCLUDING this one, so we will only be creating 9!
        //5 in front of this one
        for (int i = 1; i < 6; i++)
        {
            //distance from this seat that the new one will spawn at
            float spawnDistance = seatDist * i;
            Vector3 curPos = transform.position;
            Vector3 spawnDirection = transform.forward;
            Vector3 spawnPos = curPos + spawnDirection * spawnDistance;
            GameObject go = Instantiate(gameObject, spawnPos, gameObject.transform.rotation);
            //Destroy the bounding box on the new seat
            Destroy(go.GetComponent<BoundsControl>());
            //Have to search for bounding box component since we cannot access SqueezableBoxVisuals (internal MRTK class)
            //If that class was not internal, we would not need this workaround
            //We could also use a tag or check the name of the object, but I find this to be the easiest/most performant
            BoundingBox boundsObject = go.GetComponentInChildren<BoundingBox>();
            if (boundsObject != null)
            {
                Destroy(boundsObject.gameObject);
            }
            allSeats.Add(go);
        }
        //Create the remaining 4 seats behind this one
        for (int i = 1; i < 5; i++)
        {
            float spawnDistance = -seatDist * i;
            Vector3 curPos = transform.position;
            Vector3 spawnDirection = transform.forward;
            Vector3 spawnPos = curPos + spawnDirection * spawnDistance;
            GameObject go = Instantiate(gameObject, spawnPos, gameObject.transform.rotation);
            Destroy(go.GetComponent<BoundsControl>());
            
            //Have to search for bounding box component since we cannot access SqueezableBoxVisuals (internal MRTK class)
            //If that class was not internal, we would not need this workaround
            //We could also use a tag or check the name of the object, but I find this to be the easiest/most performant
            BoundingBox boundsObject = go.GetComponentInChildren<BoundingBox>();
            if (boundsObject != null)
            {
                Destroy(boundsObject.gameObject);
            }
            allSeats.Add(go);
        }

        
        foreach (GameObject go in allSeats)
        {
            //Disable interaction of this seat
            ToggleAssetInteraction(go, false);
            
            //turn off cabin for all seats created
            AircraftSeatConfigurable aircraftSeatConfigurable = go.GetComponent<AircraftSeatConfigurable>();
            aircraftSeatConfigurable.CabinReference.SetActive(false);
            //turn on the seat light
            aircraftSeatConfigurable.seatLight.SetActive(true);
            //apply current theme if there is one as these seats are supposed to be copies of 'this' one
            aircraftSeatConfigurable.themeApplied = themeApplied;
            aircraftSeatConfigurable.currentTheme = currentTheme;
            //get original materials for reset purposes
            aircraftSeatConfigurable.TryGetOriginalObjects(originalMaterialsAndObjects);
            //all seats we created will NOT have their AnimationHotspots shown, disable them
            aircraftSeatConfigurable.ShowWorldSpaceAnimations(false);
            //skip initialization on the copies
            aircraftSeatConfigurable.SkipInitialization = true;
            //enable video for all copies
            aircraftSeatConfigurable.EnableVideoCanvas();
        }
        seatLight.SetActive(true);
        ToggleBounds();
    }

    //Update the distance between seats
    public void UpdateSeatDistance(float dist)
    {
        seatDist = (float)Math.Round(dist * (gameObject.transform.lossyScale.sqrMagnitude / 3), 2);
        UpdateSeatPosition();
    }

    //Update seat positions based on current desired seat distance
    void UpdateSeatPosition()
    {
        if (!allSeats.Any())
        {
            return;
        }
        
        int seatIndex = 0;
        Vector3 blockScale = Vector3.one;
        Vector3 panelScale = Vector3.one;
        for (int i = 1; i < 6; i++)
        {
            if (i == 5)
            {
                //if the seat distance is > 1.2, we do not want to show the most forward seat as it will
                //clip through the cabin, so we turn it off
                if (seatDist > 1.2f)
                {
                    allSeats[seatIndex].SetActive(false);
                }
                else
                {
                    allSeats[seatIndex].SetActive(true);
                }
            }
            //move the seat to desired distance
            float spawnDistance = seatDist * i;
            Vector3 curPos = transform.position;
            Vector3 spawnDirection = transform.forward;
            Vector3 spawnPos = curPos + spawnDirection * spawnDistance;
            allSeats[seatIndex].transform.position = spawnPos;
            //when the distance between seats changes, we also want to scale the wood block and the side panel to 
            //hide any gaps in the geometry
            GameObject woodBlock = allSeats[seatIndex].GetComponent<AircraftSeatConfigurable>().ScaleableWoodBlock;
            GameObject sidePanel = allSeats[seatIndex].GetComponent<AircraftSeatConfigurable>().ScaleableSidePanel;
            blockScale = woodBlock.transform.localScale;
            panelScale = sidePanel.transform.localScale;
            float scaleFactor = (gameObject.transform.lossyScale.sqrMagnitude / 3);
            //These values were found via editor and hardcoded here as they gave us the desired result
            switch (seatDist)
            {
                case 1.2f:
                    blockScale.z = 1.73f * scaleFactor;
                    panelScale.z = 1.28f * scaleFactor;
                    break;
                case 1.3f:
                    blockScale.z = 2.39f * scaleFactor;
                    panelScale.z = 1.53f * scaleFactor;
                    break;
                case 1.4f:
                    blockScale.z = 3.06f * scaleFactor;
                    panelScale.z = 1.78f * scaleFactor;
                    break;
                case 1.5f:
                    blockScale.z = 3.72f * scaleFactor;
                    panelScale.z = 2.03f * scaleFactor;
                    break;
            }

            //apply new scale
            sidePanel.transform.localScale = panelScale;
            woodBlock.transform.localScale = blockScale;
            seatIndex++;
        }
        //4 behind
        for (int i = 1; i < 5; i++)
        {
            if (i == 4)
            {
                //if the seat distance is > 1.2, we do not want to show the seat furthest behind this seat as it will
                //clip through the cabin, so we turn it off
                if (seatDist > 1.2f)
                {
                    allSeats[seatIndex].SetActive(false);
                }
                else
                {
                    allSeats[seatIndex].SetActive(true);
                }
            }
            //move seat to desired distance
            float spawnDistance = -seatDist * i;
            Vector3 curPos = transform.position;
            Vector3 spawnDirection = transform.forward;
            Vector3 spawnPos = curPos + spawnDirection * spawnDistance;
            allSeats[seatIndex].transform.position = spawnPos;
            //when the distance between seats changes, we also want to scale the wood block and the side panel to 
            //hide any gaps in the geometry
            GameObject woodBlock = allSeats[seatIndex].GetComponent<AircraftSeatConfigurable>().ScaleableWoodBlock;
            GameObject sidePanel = allSeats[seatIndex].GetComponent<AircraftSeatConfigurable>().ScaleableSidePanel;
            blockScale = woodBlock.transform.localScale;
            panelScale = sidePanel.transform.localScale;
            float scaleFactor = (gameObject.transform.lossyScale.sqrMagnitude / 3);
            switch (seatDist)
            {
                case 1.2f:
                    blockScale.z = 1.73f * scaleFactor;
                    panelScale.z = 1.28f * scaleFactor;
                    break;
                case 1.3f:
                    blockScale.z = 2.39f * scaleFactor;
                    panelScale.z = 1.53f * scaleFactor;
                    break;
                case 1.4f:
                    blockScale.z = 3.06f * scaleFactor;
                    panelScale.z = 1.78f * scaleFactor;
                    break;
                case 1.5f:
                    blockScale.z = 3.72f * scaleFactor;
                    panelScale.z = 2.03f * scaleFactor;
                    break;
            }

            //apply the scale
            sidePanel.transform.localScale = panelScale;
            woodBlock.transform.localScale = blockScale;
            seatIndex++;
        }

        //be sure to apply the scale to THIS seat as well, not just the copies
        ScaleableWoodBlock.transform.localScale = blockScale;
        ScaleableSidePanel.transform.localScale = panelScale;
    }

    //resets wood block to original scale
    public void ResetWoodBlockScale()
    {
        Vector3 blockScale = ScaleableWoodBlock.transform.localScale;
        blockScale.z = 1.73f;
        ScaleableWoodBlock.transform.localScale = blockScale;
    }

    //resets side panel to original scale
    public void ResetSidePanelScale()
    {
        Vector3 panelScale = ScaleableSidePanel.transform.localScale;
        panelScale.z = 1.28f;
        ScaleableSidePanel.transform.localScale = panelScale;
    }

    //turns seat copies on and off
    public void ToggleSeats(bool show)
    {
        if (!show)
        {
            ToggleAssetInteraction(gameObject, true);

            //check if table collider should be enabled
            if (!tableAnimPlayed)
            {
                tableCollider.enabled = false;
            }

        }
        foreach (GameObject go in allSeats)
        {
            if (!show)
            {
                Destroy(go);
            }
        }
        allSeats.Clear();
        seatLight.SetActive(show);
    }

    //when this asset is destroyed, destroy all copies of seats if there are any
    private void OnDestroy()
    {
        foreach (GameObject go in allSeats)
        {
            Destroy(go);
        }

        AircraftLayoutUIManager uiManager = GameObject.FindObjectOfType<AircraftLayoutUIManager>();
        if(uiManager != null && uiManager.cabinActive && UIManager.Instance.CurrentConfigurable == this)
        {
            UIManager.Instance.ToggleColorButton(false);
            UIManager.Instance.ToggleMaterialButton(false);
        }
    }

    //allows us to copy over any materials and their objects so that we always have the original materials
    //for resetting this asset
    public void TryGetOriginalObjects(Dictionary<Renderer, Material[]> referenceDictionary)
    {
        foreach (KeyValuePair<Renderer, Material[]> pair in referenceDictionary)
        {
            Transform objectTransform = GetComponentsInChildren<ConfigurableComponent>()
                .FirstOrDefault(obj => obj.name == pair.Key.gameObject.name)?.transform;
            if (objectTransform != null)
            {
                originalMaterialsAndObjects.Add(objectTransform.gameObject.GetComponent<Renderer>(), pair.Value);
            }
        }
    }

    void ToggleAssetInteraction(GameObject obj, bool enabled)
    {
        foreach (ConfigurableComponent component in obj.GetComponentsInChildren<ConfigurableComponent>())
        {
            component.enabled = enabled;
        }

        foreach (DraggableObjectManipulator manipulator in obj.GetComponentsInChildren<DraggableObjectManipulator>())
        {
            manipulator.enabled = enabled;
        }
    }
    
    //AIRLINE THEMES
    public AirlineTheme AA_Theme;
    public AirlineTheme JA_Theme;
    public AirlineTheme UA_Theme;
    public AirlineTheme DA_Theme;
    public AirlineTheme LA_Theme;

    public Material MLHeadrestCover;

    //all renderers needed to apply themes
    public MeshRenderer seatBaseRenderer;
    public MeshRenderer seatBackRenderer;
    public MeshRenderer headrestRenderer;
    public MeshRenderer legRestUpperRenderer;
    public MeshRenderer legRestLowerRenderer;
    public MeshRenderer seatShellRenderer;
    public MeshRenderer headRestCoverRenderer;
    public MeshRenderer seatBackCenterRenderer;
    public MeshRenderer seatBaseCenterRenderer;
    public MeshRenderer armRestRenderer;
    //dictionary of renderers and their materials for easy resetting
    private Dictionary<Renderer, Material[]> originalMaterialsAndObjects = new();
    private bool themeApplied = false;
    public Theme currentTheme = Theme.NONE;
    
    //Themes can be applied here
    //If the same theme gets applied twice in a row, we toggle it
    public void ToggleAirlineTheme(Theme theme)
    {
        //If a theme was applied, clear it first
        if (themeApplied)
        {
            Theme prevTheme = currentTheme;
            //toggle theme
            ClearTheme();
            
            //if the theme we want to apply is the theme that was already applied, return here
            //this allows for 'toggle' behavior
            if (theme == prevTheme)
            {
                return;
            }
        }

        AirlineTheme airlineTheme = null;
        //Get the theme
        switch (theme)
        {
            case Theme.AmericanAirlines:
                airlineTheme = AA_Theme;
                break;
            case Theme.JapanAirlines:
                airlineTheme = JA_Theme;
                break;
            case Theme.UnitedAirlines:
                airlineTheme = UA_Theme;
                break;
            case Theme.DeltaAirlines:
                airlineTheme = DA_Theme;
                break;
            case Theme.LufthansaAirlines:
                airlineTheme = LA_Theme;
                break;
        }

        //if no theme found, return as there is nothing to apply
        if (airlineTheme == null)
        {
            return;
        }
        
        //Deselect any objects that are currently highlighted
        //This prevents the highlight color from being saved as an 'original' color
        foreach (MeshOutliner highlighter in GetComponentsInChildren<MeshOutliner>())
        {
            highlighter.Deselect();
        }
        
        currentTheme = theme;
        themeApplied = true;
        
        //THEME APPLICATION
        
        //apply materials to SeatBase
        originalMaterialsAndObjects.Add(seatBaseRenderer, seatBaseRenderer.materials);
        Material[] mats = seatBaseRenderer.materials;
        mats[0] = airlineTheme.matA;
        seatBaseRenderer.materials = mats;

        //apply materials to SeatBack
        originalMaterialsAndObjects.Add(seatBackRenderer, seatBackRenderer.materials);
        mats = seatBackRenderer.materials;
        mats[0] = airlineTheme.matA;
        seatBackRenderer.materials = mats;
        
        //apply materials to SeatBackCenter
        originalMaterialsAndObjects.Add(seatBackCenterRenderer, seatBackCenterRenderer.materials);
        mats = seatBackCenterRenderer.materials;
        mats[0] = airlineTheme.matB;
        seatBackCenterRenderer.materials = mats;
        
        //apply materials to SeatBaseCenter
        originalMaterialsAndObjects.Add(seatBaseCenterRenderer, seatBaseCenterRenderer.materials);
        mats = seatBaseCenterRenderer.materials;
        mats[0] = airlineTheme.matB;
        seatBaseCenterRenderer.materials = mats;
        
        //apply materials to HeadRestCover
        originalMaterialsAndObjects.Add(headRestCoverRenderer, headRestCoverRenderer.materials);
        mats = headRestCoverRenderer.materials;
        mats[0] = airlineTheme.headrestCoverMat;
        headRestCoverRenderer.materials = mats;

        //apply materials to Headrest
        originalMaterialsAndObjects.Add(headrestRenderer, headrestRenderer.materials);
        mats = headrestRenderer.materials;
        mats[0] = airlineTheme.matB;
        headrestRenderer.materials = mats;

        //apply materials to UpperLegRest
        originalMaterialsAndObjects.Add(legRestUpperRenderer, legRestUpperRenderer.materials);
        mats = legRestUpperRenderer.materials;
        mats[0] = airlineTheme.matB;
        legRestUpperRenderer.materials = mats;

        //apply materials to LowerLegRest
        originalMaterialsAndObjects.Add(legRestLowerRenderer, legRestLowerRenderer.materials);
        mats = legRestLowerRenderer.materials;
        mats[0] = airlineTheme.matB;
        legRestLowerRenderer.materials = mats;

        //apply materials to SeatShell
        originalMaterialsAndObjects.Add(seatShellRenderer, seatShellRenderer.materials);
        mats = seatShellRenderer.materials;
        mats[9] = airlineTheme.matB;
        seatShellRenderer.materials = mats;
        
        //apply materials to armrest
        originalMaterialsAndObjects.Add(armRestRenderer, armRestRenderer.materials);
        mats = armRestRenderer.materials;
        mats[0] = airlineTheme.matB;
        armRestRenderer.materials = mats;
    }
    
    //Apply Magic Leap headrest cover
    public void ApplyMagicLeapHeadrest()
    {
        Material[] mats = headRestCoverRenderer.materials;
        mats[0] = MLHeadrestCover;
        headRestCoverRenderer.materials = mats;
    }

    //resets the seat to default theme
    void ClearTheme()
    {
        foreach (KeyValuePair<Renderer, Material[]> pair in originalMaterialsAndObjects)
        {
            pair.Key.materials = pair.Value;
        }
        originalMaterialsAndObjects.Clear();
        themeApplied = false;
        currentTheme = Theme.NONE;
    }

    //The themes you can have on this asset
    public enum Theme
    {
        AmericanAirlines,
        JapanAirlines,
        UnitedAirlines,
        DeltaAirlines,
        LufthansaAirlines,
        NONE
    }

    //Reset for the entire aircraft seat, no matter the current state (cabin toggled on/off, animations played, etc)
    public override void Reset()
    {
        base.Reset();
        AircraftLayoutUIManager uiManager = FindObjectOfType<AircraftLayoutUIManager>();
        if (uiManager != null)
        {
            uiManager.ResetAnimationsToDefaultState();
        }
        ToggleSeats(false);
        if (CabinReference.activeSelf)
        {
            uiManager.ToggleCabin();
        }
        ClearTheme();
        ResetSidePanelScale();
        ResetWoodBlockScale();
        if (videoPlayer.isPlaying)
        {
            ToggleVideo();
        }
    }

    //Animation function for animation hotspots attached to this configurable
    public bool tableAnimPlayed = false;
    [SerializeField] private BoxCollider tableCollider;
    public void TryPlayTableAnim()
    {
        AircraftLayoutUIManager uiManager = FindObjectOfType<AircraftLayoutUIManager>();
        if (uiManager)
        {
            uiManager.PlayTableAnim(true);
            if (!CabinReference.activeInHierarchy && tableAnimPlayed)
            {
                tableCollider.enabled = true;
            }
            else if (!CabinReference.activeInHierarchy && !tableAnimPlayed)
            {
                tableCollider.enabled = false;
            }
        }
    }

    public void ToggleTableCollider(bool enable)
    {
        if (CabinReference.activeInHierarchy)
        {
            return;
        }
        
        tableCollider.enabled = enable;
    }
    
    //Animation function for animation hotspots attached to this configurable
    public void TryPlayShelfAnim()
    {
        AircraftLayoutUIManager uiManager = FindObjectOfType<AircraftLayoutUIManager>();
        if (uiManager)
        {
            uiManager.PlayDeviceShelfAnim(true);
        }
    }
    
    //Animation function for animation hotspots attached to this configurable
    public void TryPlaySeatBaseAnim()
    {
        AircraftLayoutUIManager uiManager = FindObjectOfType<AircraftLayoutUIManager>();
        if (uiManager)
        {
            uiManager.PlaySeatBaseAnim(true);
        }
    }
    
    //Animation function for animation hotspots attached to this configurable
    public void TryPlayLegRestAnim()
    {
        AircraftLayoutUIManager uiManager = FindObjectOfType<AircraftLayoutUIManager>();
        if (uiManager)
        {
            uiManager.PlayLegRestAnim(true);
        }
    }

    private bool videoInitialized = false;
    public void ToggleVideo()
    {
        if (IsVideoPlaying)
        {
            videoPlayer.Stop();
            videoRenderImage.gameObject.SetActive(false);
        }
        else
        {
            videoPlayer.Play();
            //stop playing all other videos as we only allow for 1 to be played at a time
            AircraftSeatConfigurable[] configurables = FindObjectsOfType<AircraftSeatConfigurable>();
            foreach (AircraftSeatConfigurable config in configurables)
            {
                if (config == this)
                {
                    continue;
                }

                if (config.IsVideoPlaying)
                {
                    config.ToggleVideo();
                }
            }
            
            if (!videoInitialized)
            {
                //workaround, the video render texture gets filled with 'artifacts' for the first few seconds while the 
                //video begins playing for the first time
                //Here, we just wait to 'show' the render texture in order to hide those artifacts
                StartCoroutine(EnableRenderTextureAfterFrames());
            }
            else
            {
                videoRenderImage.gameObject.SetActive(true);
            }
        }
    }
    
    IEnumerator EnableRenderTextureAfterFrames()
    {
        yield return new WaitForSecondsRealtime(2);
        videoRenderImage.gameObject.SetActive(true);
        videoInitialized = true;
    }

    private RenderTexture videoRenderTexture;
    public void EnableVideoCanvas()
    {
        videoCanvas.gameObject.SetActive(true);
        videoRenderTexture = Instantiate(videoRenderTexturePrefab);
        videoRenderImage.texture = videoRenderTexture;
        videoPlayer.targetTexture = videoRenderTexture;
        videoRenderImage.gameObject.SetActive(false);
    }
}

/// <summary>
/// A class to contain an Airline Theme
/// These themes contain a material A and material B that cover all bases for this asset
/// Themes also can have a different headrest, so that is included as well
/// </summary>
[Serializable]
public class AirlineTheme
{
    public Material matA;
    public Material matB;
    public Material headrestCoverMat;
}
