using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Class for coloring planes based on their classification
/// </summary>
public class PlaneColorClassification : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SetColorByClassification();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void SetColorByClassification()
    {
        ARPlane plane = GetComponent<ARPlane>();
        Color color = Color.white;
        //For this app we only care that the ceiling is colored blue
        switch (plane.classification)
        {
            case PlaneClassification.Ceiling:
                color = Color.blue;
                break;
        }
        Material mat = GetComponent<MeshRenderer>().material;
        mat.SetColor("_BaseColor", color);
        GetComponent<MeshRenderer>().material = mat;
    }
}
