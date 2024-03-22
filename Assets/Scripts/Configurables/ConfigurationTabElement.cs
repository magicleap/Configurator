using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.UI;

//Class to describe a configuration tab
//These are UI elements that behave like tabs
//When selected they show/hide their 'content'
public class ConfigurationTabElement : MonoBehaviour
{
    //Parent object containing all content to show when tab is selected
    [Tooltip("Parent object that acts as a container of all your content that will be displayed when this tab is selected")]
    public GameObject contentContainer;
    
    //parent object containing 'UI elements', usually a layout of some sort
    public GameObject buttonParent;

    //All content has a vertical layout group to help layout UI elements
    [Tooltip("All tabs are laid out vertically, please reference that here")]
    public VerticalLayoutGroup verticalLayoutGroup;

    //If there is an apply all button, reference it here
    [Tooltip("If this tab should have an apply all button with it's content, please reference it here")]
    public GameObject applyAllButton;
    
    //The label to show when the content is 'empty'
    [Tooltip("Label that will show when the content is 'empty' or not being displayed")]
    public GameObject emptyContentLabel;

    //When this is toggle on or off, notify the tab manager
    //This allows for Group Tab behavior as only 1 tab can be active at a time
    //Used for highlight typically
    public void NotifyTabManagerOfToggle()
    {
        TabElementManager tabManager = transform.parent.GetComponent<TabElementManager>();
        if (tabManager != null)
        {
            tabManager.DetoggleOtherTabs(this);
        }
    }

    private bool shouldShowWhenEnabled = false;
    private void OnDisable()
    {
        if (contentContainer.activeSelf)
        {
            shouldShowWhenEnabled = true;
        }
    }

    private void OnEnable()
    {
        if (shouldShowWhenEnabled)
        {
            shouldShowWhenEnabled = false;
            GetComponent<PressableButton>().ForceSetToggled(true);
        }
    }
}
