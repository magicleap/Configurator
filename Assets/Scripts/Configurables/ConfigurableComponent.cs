using UnityEngine;

/// <summary>
/// Base class for configurable components
/// A Configurable Component can have any functionality you want to create
/// For instance, a <see cref="ConfigurableColor"/> or <see cref="ConfigurableMaterial"/>. 
/// </summary>
public abstract class ConfigurableComponent : MonoBehaviour
{
    protected Configurable configurableRoot;

    //This can be overriden but should always be called with base.Awake();
    protected virtual void Awake()
    {
        Init();
    }

    void Init()
    {
        //find configurable, should be at the root of the asset
        if (configurableRoot == null)
        {
            Transform currentObject = gameObject.transform;
            while (configurableRoot == null)
            {
                if (currentObject == null)
                {
                    //nothing found, error
                    Debug.LogError("No configurable component found in hierarchy.");
                }
                configurableRoot = currentObject.GetComponent<Configurable>();
                currentObject = currentObject.transform.parent;
            }
        }
        
        //add this component to configurable
        configurableRoot.AddComponent(this);
    }

    //Rest to default values
    public abstract void Reset();
}
