using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable object that has a list of layouts
/// Allows us to create layout lists for various assets and store them
/// </summary>
[CreateAssetMenu ( fileName = "Layout", menuName = "ConfigurableLayout/Create Layout SO", order = 1 )]
public class LayoutScriptableObject : ScriptableObject
{
    public List<Layout> layouts = new();
}
