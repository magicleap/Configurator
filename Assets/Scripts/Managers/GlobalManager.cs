using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR.MagicLeap;

public class GlobalManager : Singleton<GlobalManager>
{
    Configurable CurrentConfigurable => UIManager.Instance.CurrentConfigurable;

    //Material used for plane mesh visualization
    [SerializeField] 
    Material ARPlaneMaterial;
    //scene reflection probe
    [SerializeField] 
    ReflectionProbe reflProbe;
    //Outline material used for outlining objects when hovered/selected
    [SerializeField] 
    Material OutlineMaterial;
    //scene lights used for certain assets
    [SerializeField] 
    Light carLight;
    [SerializeField] 
    Light seatAndSofaLight;


    
    private void Start()
    {
        //SegDimming needs to be enabled manually at app startup
        //In Settings.cs, it is disabled by default. 
        ToggleSegmentedDimming();
        //Initialize the global vector for the ShowWhenClose shader
        Shader.SetGlobalVector("_ConfigPosition", new Vector3(1000,1000,1000));
    }

    #region Settings
    
    public void ToggleSegmentedDimming()
    {
        if (Settings.SegmentedDimmingEnabled)
        {
            MLSegmentedDimmer.Deactivate();
        }
        else
        {
            MLSegmentedDimmer.Activate();
        }

        Settings.ToggleSegmentedDimming();
    }
    
    public void ToggleFloorSnapping()
    {
        Settings.ToggleFloorSnapping();
        PlanesManager.Instance.UpdateActiveFloorPlanes();
    }
    
    public void ToggleWallSnapping()
    {
        Settings.ToggleWallSnapping();
        PlanesManager.Instance.UpdateActiveWallPlanes();
    }
    
    public void ToggleWorldSpaceAnimationHotspots()
    {
        Settings.ToggleShowAnimationHotspots();
        if (CurrentConfigurable != null)
        {
            CurrentConfigurable.ShowWorldSpaceAnimations(Settings.ShowAnimationHotspots);
        }
    }
    
    public void SetGlobalDimmingValue(float val)
    {
        Settings.SetGlobalDimmingValue(val);
        MLGlobalDimmer.SetValue(val, true);
    }
    #endregion
    
    
    //Assets are different sizes, so the radius of the shader is altered per asset
    public void SetARPlaneClipDistance(float dist)
    {
        ARPlaneMaterial.SetFloat("_ClipDistance", dist);
    }

    public void EnableCarLightAndReflections()
    {
        carLight.gameObject.SetActive(true);
        seatAndSofaLight.gameObject.SetActive(false);
        reflProbe.clearFlags = ReflectionProbeClearFlags.Skybox;
        reflProbe.RenderProbe();
    }

    public void EnableSeatAndSofaLightAndReflections()
    {
        carLight.gameObject.SetActive(false);
        seatAndSofaLight.gameObject.SetActive(true);
        reflProbe.clearFlags = ReflectionProbeClearFlags.SolidColor;
        reflProbe.backgroundColor = Color.black;
        reflProbe.RenderProbe();
    }

    //Assets have varying outline thickness that looks pleasant on them, so we can alter this per asset
    public void SetOutlineWidth(float width)
    {
        OutlineMaterial.SetFloat("_Outline_Thickness", width);
    }
    
}
