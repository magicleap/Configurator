using UnityEngine;

/// <summary>
/// Class specifically for the Sofa asset
/// You can create any special configurable you wish by inheriting from <see cref="Configurable"/>
/// This allows you to create any behavior you would like with a configurable asset
/// </summary>
public class SofaConfigurable : Configurable
{
    //references to pass to UI object
    public GameObject OttomanReference;
    public GameObject centerPillowReference;
    
    [SerializeField]
    [ColorUsage(true, true)]
    Color defaultColor;

    [SerializeField] private GameObject leftSection, middleSection, rightSection;

    public override void Start()
    {
        base.Start();
        //We want to offset the bright white color of the sofa when it spawns to something less 'eye piercing'
        leftSection.GetComponent<ConfigurableColor>().ChangeColor(defaultColor);
        middleSection.GetComponent<ConfigurableColor>().ChangeColor(defaultColor);
        rightSection.GetComponent<ConfigurableColor>().ChangeColor(defaultColor);
        OttomanReference.GetComponent<ConfigurableColor>().ChangeColor(defaultColor);
    }

    public override void CreateUI()
    {
        UIManager.Instance.CreateConfigUI(customUIPrefab);
        GameObject layoutUIObject = UIManager.Instance.GetLayoutUIObject();
        SofaLayoutUIManager layoutUIManager = layoutUIObject.GetComponent<SofaLayoutUIManager>();
        layoutUIManager.SetOttomanAndPillowReference(OttomanReference, centerPillowReference);
        foreach (GameObject tab in layoutUIManager.ConfigTabs)
        {
            UIManager.Instance.CreateConfigTab(tab.GetComponent<ConfigurationTabElement>());
        }
        UIManager.Instance.ShowFirstTab();
        GlobalManager.Instance.EnableSeatAndSofaLightAndReflections();
    }
}
