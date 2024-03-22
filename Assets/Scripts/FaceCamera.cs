using UnityEngine;

/// <summary>
/// Class used for making UI objects face the camera
/// </summary>
public class FaceCamera : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
    }
}
