using UnityEngine;

/// <summary>
/// Class describing highlight and selection colors
/// This is unused now, but was used in early versions before <see cref="MeshOutliner"/> was created
/// </summary>
[CreateAssetMenu ( fileName = "SelectionColor", menuName = "ConfigurableHighlighter/Create SelectionColor SO", order = 1 )]
public class HighlightColorsScriptableObject : ScriptableObject
{
    public Color highlightColor;
    public Color selectionColor;
}
