using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MaterialSwapEffect : IEffect
{
    [SerializeField]
    [HideInInspector]
#pragma warning disable CS0414 // Inspector uses this as a helpful label in lists.
    private string name = "Material Swap";
#pragma warning restore CS0414 // Inspector uses this as a helpful label in lists.

    [SerializeField]
    [Tooltip("Threshold value to activate this effect. When the state value is above this number, the effect will activate.")]
    private float activationThreshold = 0.001f;

    [SerializeField]
    [Tooltip("The Image to switch sprites for.")]
    private RawImage target;

    [SerializeField]
    [Tooltip("The texture to set when the state is active.")]
    private Material activeMateiral;

    [SerializeField]
    [Tooltip("The texture to set when the state is inactive.")]
    private Material inactiveMaterial;

    /// <summary>
    /// Initializes a new instance of the <see cref="Microsoft.MixedReality.Toolkit.UX.SpriteSwapEffect"/> class.
    /// </summary>
    public MaterialSwapEffect() { }

    /// <inheritdoc />
    public void Setup(PlayableGraph graph, GameObject owner) { }

    /// <inheritdoc />
    public bool Evaluate(float parameter)
    {
        if (target == null)
        {
            return false;
        }

        Material correctMaterial = parameter > activationThreshold ? activeMateiral : inactiveMaterial;

        if (target.material != correctMaterial)
        {
            target.material = correctMaterial;
        }

        return true; // We are always immediately done.
    }
}
