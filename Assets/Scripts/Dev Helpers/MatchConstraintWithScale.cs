using UnityEngine;
using UnityEngine.Animations;

/// <summary>
/// Helper class to make it so the ParentConstraint component takes scale of an object into account (does not be default)
/// </summary>

[RequireComponent(typeof(ParentConstraint))]
public class MatchConstraintWithScale : MonoBehaviour
{
    private ParentConstraint constraint;
    private Vector3 initialConstriantPos;
    private Vector3 newConstraintPos = Vector3.zero;
    
    // Start is called before the first frame update
    void Start()
    {
        constraint = GetComponent<ParentConstraint>();
        initialConstriantPos = constraint.GetTranslationOffset(0);
    }

    // Update is called once per frame
    void Update()
    {
        newConstraintPos = initialConstriantPos * transform.parent.localScale.x;
        constraint.SetTranslationOffset(0, newConstraintPos);
    }
}
