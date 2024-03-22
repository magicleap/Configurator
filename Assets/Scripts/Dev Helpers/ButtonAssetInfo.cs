using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class containing data for UI buttons for the configurator app
/// Many of our UI buttons have a label and an image, so we can contain all that data in one class
/// </summary>
public class ButtonAssetInfo : MonoBehaviour
{
    //Reference to the button label
    public TMP_Text buttonLabel;

    //reference to the button script
    public PressableButton button;

    //reference to the desired button image
    public Image buttonImage;

    //If you'd like, you can set an image via script
    public void SetImage(Sprite sprite)
    {
        if (sprite != null)
        {
            buttonImage.gameObject.SetActive(true);
            buttonImage.sprite = sprite;
        }
    }
}
