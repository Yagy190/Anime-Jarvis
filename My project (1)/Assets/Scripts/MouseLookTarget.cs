using UnityEngine;

public class MouseLookTarget : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        if (mainCam != null)
        {
            // Get the current mouse position on the screen
            Vector3 mousePos = Input.mousePosition;
            
            // Set the distance in front of the camera where the target should hover
            mousePos.z = 3.0f; 
            
            // Smoothly move this object to where the mouse is pointing
            Vector3 worldPos = mainCam.ScreenToWorldPoint(mousePos);
            transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * 6f);
        }
    }
}
