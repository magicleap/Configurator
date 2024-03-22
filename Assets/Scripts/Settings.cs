using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for referencing and storing all settings for this app
/// </summary>
public static class Settings
{
    public static float GlobalDimmingValue { get; private set; } = 0;
    public static bool SegmentedDimmingEnabled { get; private set; } = false;
    public static bool ShowAnimationHotspots { get; private set; } = true;
    public static bool FloorSnappingEnabled { get; private set; } = true;
    public static bool WallSnappingEnabled { get; private set; } = false;

    public static void ToggleSegmentedDimming()
    {
        SegmentedDimmingEnabled = !SegmentedDimmingEnabled;
    }

    public static void SetSegmentedDimming(bool enabled)
    {
        SegmentedDimmingEnabled = enabled;
    }

    public static void ToggleFloorSnapping()
    {
        FloorSnappingEnabled = !FloorSnappingEnabled;
    }

    public static void SetFloorSnapping(bool enabled)
    {
        FloorSnappingEnabled = enabled;
    }
    
    public static void ToggleWallSnapping()
    {
        WallSnappingEnabled = !WallSnappingEnabled;
    }

    public static void SetWallSnapping(bool enabled)
    {
        WallSnappingEnabled = enabled;
    }
    
    public static void ToggleShowAnimationHotspots()
    {
        ShowAnimationHotspots = !ShowAnimationHotspots;
    }

    public static void SetShowAnimationHotspots(bool enabled)
    {
        ShowAnimationHotspots = enabled;
    }

    public static void SetGlobalDimmingValue(float val)
    {
        GlobalDimmingValue = val;
    }
    
}
