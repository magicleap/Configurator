using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using Slider = Microsoft.MixedReality.Toolkit.UX.Slider;

/// <summary>
/// Class for managing the custom UI and functions for the aircraft seat
/// </summary>
public class AircraftLayoutUIManager : MonoBehaviour
{
    //reference to the aircraft cabin, this is toggled on and off
    private GameObject cabinObject;
    //reference to the aircraft configurable class
    private AircraftSeatConfigurable config;

    //references to UI objects
    public Slider seatDistanceSlider;
    public TMP_Text distanceLabel;
    public GameObject seatDistanceLabel;
    
    [SerializeField] private ConfigurationTabElement AnimationsTab;
    [SerializeField] private ConfigurationTabElement AirlineThemeTab;

    [SerializeField]
    private PressableButton tableAnimToggle;
    [SerializeField]
    private PressableButton seatBaseAnimToggle;
    [SerializeField]
    private PressableButton deviceShelfAnimToggle;
    [SerializeField]
    private PressableButton legRestAnimToggle;
    [SerializeField] 
    private PressableButton cabinToggle;

    public bool cabinActive { get; private set; } = false;
    //List of all tabs that will be included in the configurable menu when this asset is loaded
    public List<GameObject> ConfigTabs => new()
        { AnimationsTab.gameObject, AirlineThemeTab.gameObject };
    
    void SetCabinReference(GameObject obj)
    {
        cabinObject = obj;
    }

    public void SetConfigurable(AircraftSeatConfigurable configurable)
    {
        config = configurable;
        SetCabinReference(config.CabinReference);
    }

    //When the seat distance slider is updated, update seat distance to match
    public void OnSeatDistanceValueChanged()
    {
        //need to remap the values because the MRTK slider is bugged in the version this project uses
        //The slider only works with values 0-1
        float val = map(seatDistanceSlider.Value, 0, 1, 1.2f, 1.5f);
        distanceLabel.text = $"{val:F1}" + "m";
        config.UpdateSeatDistance(val);
    }

    //map utility function
    float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    //Play animation from UI
    public bool TableAnimPlayed = false;
    public void PlayTableAnim(bool worldSpaceTrigger)
    {
        //if this animation was triggered by an AnimationHotspot (World space UI in the scene attached to the asset)
        //we need to also set the UI toggle so that the states are in sync
        if (worldSpaceTrigger)
        {
            tableAnimToggle.ForceSetToggled(!tableAnimToggle.IsToggled, true);
        }
        //Allows for us to play animations on ALL aircraft seats currently active in the scene
        var anims = FindObjectsOfType<Animator>().Where(obj => obj.gameObject.name.Contains("Table"));

        foreach (Animator anim in anims)
        {
            if (TableAnimPlayed)
            {
                anim.Play("Reverse");
            }
            else
            {
                anim.Play("Forward");
            }
        }

        TableAnimPlayed = !TableAnimPlayed;
        config.tableAnimPlayed = TableAnimPlayed;
        //The table has a collider that is disabled when the table is put away and enabled when the table is
        //brought out of it's case via animation
        config.ToggleTableCollider(TableAnimPlayed);
    }

    //Play animation from UI
    public bool ShelfAnimPlayed = false;
    public void PlayDeviceShelfAnim(bool worldSpaceTrigger = false)
    {
        //if this animation was triggered by an AnimationHotspot (World space UI in the scene attached to the asset)
        //we need to also set the UI toggle so that the states are in sync
        if (worldSpaceTrigger)
        {
            deviceShelfAnimToggle.ForceSetToggled(!deviceShelfAnimToggle.IsToggled, true);
        }
        //Allows for us to play animations on ALL aircraft seats currently active in the scene
        var anims = FindObjectsOfType<Animator>().Where(obj => obj.gameObject.name.Contains("DEVICE_Shelf"));

        foreach (Animator anim in anims)
        {
            if (ShelfAnimPlayed)
            {
                anim.Play("Reverse");
            }
            else
            {
                anim.Play("Forward");
            }
        }

        ShelfAnimPlayed = !ShelfAnimPlayed;
    }

    //Play animation from UI
    public bool SeatBaseAnimPlayed = false;
    public void PlaySeatBaseAnim(bool worldSpaceTrigger)
    {
        //if this animation was triggered by an AnimationHotspot (World space UI in the scene attached to the asset)
        //we need to also set the UI toggle so that the states are in sync
        if (worldSpaceTrigger)
        {
            seatBaseAnimToggle.ForceSetToggled(!seatBaseAnimToggle.IsToggled, true);
        }
        //Allows for us to play animations on ALL aircraft seats currently active in the scene
        var anims = FindObjectsOfType<Animator>().Where(obj => obj.gameObject.name.Contains("Seat_Base"));

        foreach (Animator anim in anims)
        {
            if (SeatBaseAnimPlayed)
            {
                anim.Play("Reverse");
            }
            else
            {
                anim.Play("Forward");
            }
        }

        SeatBaseAnimPlayed = !SeatBaseAnimPlayed;
    }

    //Play animation from UI
    public bool LegRestAnimPlayed = false;
    public void PlayLegRestAnim(bool worldSpaceTrigger)
    {
        //if this animation was triggered by an AnimationHotspot (World space UI in the scene attached to the asset)
        //we need to also set the UI toggle so that the states are in sync
        if (worldSpaceTrigger)
        {
            legRestAnimToggle.ForceSetToggled(!legRestAnimToggle.IsToggled, true);
        }
        //Allows for us to play animations on ALL aircraft seats currently active in the scene
        var anims = FindObjectsOfType<Animator>().Where(obj => obj.gameObject.name.Contains("Leg_Rest"));

        foreach (Animator anim in anims)
        {
            if (LegRestAnimPlayed)
            {
                anim.Play("Reverse");
            }
            else
            {
                anim.Play("Forward");
            }
        }

        LegRestAnimPlayed = !LegRestAnimPlayed;
    }

    //Apply american airlines theme
    public void ApplyAATheme()
    {
        foreach (GameObject go in config.allSeats)
        {
            go.GetComponent<AircraftSeatConfigurable>()
                .ToggleAirlineTheme(AircraftSeatConfigurable.Theme.AmericanAirlines);
        }
        config.ToggleAirlineTheme(AircraftSeatConfigurable.Theme.AmericanAirlines);

    }

    //Apply Japan Airlines theme
    public void ApplyJATheme()
    {
        foreach (GameObject go in config.allSeats)
        {
            go.GetComponent<AircraftSeatConfigurable>()
                .ToggleAirlineTheme(AircraftSeatConfigurable.Theme.JapanAirlines);
        }
        config.ToggleAirlineTheme(AircraftSeatConfigurable.Theme.JapanAirlines);
    }

    //Apply United Airlines theme
    public void ApplyUATheme()
    {
        foreach (GameObject go in config.allSeats)
        {
            go.GetComponent<AircraftSeatConfigurable>()
                .ToggleAirlineTheme(AircraftSeatConfigurable.Theme.UnitedAirlines);
        }
        config.ToggleAirlineTheme(AircraftSeatConfigurable.Theme.UnitedAirlines);
    }
    
    //Apply Delta Airlines theme
    public void ApplyDATheme()
    {
        foreach (GameObject go in config.allSeats)
        {
            go.GetComponent<AircraftSeatConfigurable>()
                .ToggleAirlineTheme(AircraftSeatConfigurable.Theme.DeltaAirlines);
        }
        config.ToggleAirlineTheme(AircraftSeatConfigurable.Theme.DeltaAirlines);
    }
    
    //Apply Lufthansa Airlines theme
    public void ApplyLATheme()
    {
        foreach (GameObject go in config.allSeats)
        {
            go.GetComponent<AircraftSeatConfigurable>()
                .ToggleAirlineTheme(AircraftSeatConfigurable.Theme.LufthansaAirlines);
        }
        config.ToggleAirlineTheme(AircraftSeatConfigurable.Theme.LufthansaAirlines);
    }

    //Apply magic leap headrest and default theme
    public void ApplyMagicLeapHeadrest()
    {
        foreach (GameObject go in config.allSeats)
        {
            go.GetComponent<AircraftSeatConfigurable>().ApplyMagicLeapHeadrest();
            if (config.currentTheme != AircraftSeatConfigurable.Theme.NONE)
            {
                go.GetComponent<AircraftSeatConfigurable>().ToggleAirlineTheme(config.currentTheme);
            }
        }
        config.ApplyMagicLeapHeadrest();
        if (config.currentTheme != AircraftSeatConfigurable.Theme.NONE)
        {
            config.ToggleAirlineTheme(config.currentTheme);
        }
    }

    //Reset all animations to their default state
    public void ResetAnimationsToDefaultState()
    {
        foreach (Animator anim in FindObjectsOfType<Animator>())
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Forward"))
            {
                anim.Play("Reverse");
            }
        }

        List<PressableButton> toggles = new()
            { tableAnimToggle, deviceShelfAnimToggle, legRestAnimToggle, seatBaseAnimToggle, cabinToggle };
        foreach (PressableButton toggle in toggles)
        {
            if (toggle.IsToggled)
            {
                toggle.ForceSetToggled(false);
            }
        }
    }

    //Toggle the cabin on/off
    public void ToggleCabin()
    {
        //When cabin is toggled on, we do not allow the user to edit color or materials, so toggle the buttons along with this
        cabinObject.SetActive(!cabinObject.activeInHierarchy);
        cabinActive = !cabinActive;
        //For now, when we toggle the cabin on, lets also populate the cabin
        //TODO: Optimization: Make use of object pooling so we don't create and destroy objects so often
        if (cabinObject.activeInHierarchy)
        {
            config.PopulateCabinWithCurrentConfig();
            //workaround, we wait one frame to toggle the slider on and off
            //this is because as soon as the slider is enabled, it fires OnValueChanged events
            //that were causing undesired behavior (MRTK issue?)
            StartCoroutine(ToggleSliderAfterOneFrame(true));
            //if the bounding box is visible, toggle it as we do not want the user to be able to manipulate the 
            //bounding box when the cabin is on
            if(config.boundsVisible)
            {
                config.ToggleBounds();
            }
            UIManager.Instance.ToggleColorButton(false);
            UIManager.Instance.ToggleMaterialButton(false);
        }
        else
        {
            //cabin is turned off, turn of all other seats, reset values of cabin configuration
            config.ToggleSeats(false);
            seatDistanceSlider.Value = seatDistanceSlider.MinValue;
            StartCoroutine(ToggleSliderAfterOneFrame(false));
            config.ResetWoodBlockScale();
            config.ResetSidePanelScale();
            UIManager.Instance.ToggleColorButton(true);
            UIManager.Instance.ToggleMaterialButton(true);
        }
    }

    //We wait one frame so that the slider can update it's value 
    //This also causes the scaling of the wooden block object to go
    //back to it's original size rather than possibly staying extended
    IEnumerator ToggleSliderAfterOneFrame(bool show)
    {
        yield return null;
        seatDistanceSlider.enabled = show;
        seatDistanceSlider.gameObject.SetActive(show);
        foreach (Transform t in seatDistanceSlider.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.SetActive(show);
        }
        seatDistanceLabel.SetActive(show);
    }
}
