using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLSofaConfigurable : Configurable
{
    //references to pass to UI object
    public GameObject OttomanReference;
    public GameObject centerPillowReference;
    
    public override void CreateUI()
    {
        UIManager.Instance.CreateConfigUI(customUIPrefab);
        GameObject layoutUIObject = UIManager.Instance.GetLayoutUIObject();
        MLSofaUIManager layoutUIManager = layoutUIObject.GetComponent<MLSofaUIManager>();
        layoutUIManager.SetOttomanAndPillowReference(OttomanReference, centerPillowReference);
        foreach (GameObject tab in layoutUIManager.ConfigTabs)
        {
            UIManager.Instance.CreateConfigTab(tab.GetComponent<ConfigurationTabElement>());
        }
        UIManager.Instance.ShowFirstTab();
        GlobalManager.Instance.EnableSeatAndSofaLightAndReflections();
    }

    public override void Reset()
    {
        base.Reset();
        GameObject layoutUIObject = UIManager.Instance.GetLayoutUIObject();
        MLSofaUIManager layoutUIManager = layoutUIObject.GetComponent<MLSofaUIManager>();
        layoutUIManager.OnSelectThreeSeater();
        layoutUIManager.OnReset();
    }
}
