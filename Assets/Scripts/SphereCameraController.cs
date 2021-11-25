using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

// Spin and view the sphere with right drag. Zoom in or out with wheel.

public class SphereCameraController : MonoBehaviour {
    public float dragSensitivity = 1.0f; 
    public float scrollSensitivity = 1.0f; 
    public float zoomSize = 2.0f;
    
    private Vector3 cameraPosition;
    
    void Update() {
        HandleDrag();        
        HandleScroll();
    }
    
    // Rotate sphere by dragging right mousebutton
    private void HandleDrag() {
        if (Input.GetMouseButtonDown(1)) {
            cameraPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(1)) {
            Vector3 moveDir = Input.mousePosition - cameraPosition;
            Quaternion rotationX = Quaternion.AngleAxis(moveDir.x * dragSensitivity, transform.up);
            Quaternion rotationY = Quaternion.AngleAxis(moveDir.y * dragSensitivity, -transform.right);
            transform.rotation = rotationX * rotationY * transform.rotation;
            cameraPosition = Input.mousePosition;
        }
    }
    
    // Zoom in/out by mouse scrollwheel
    private void HandleScroll() {
        zoomSize -= scrollSensitivity * CrossPlatformInputManager.GetAxis("Mouse ScrollWheel");
        transform.position = transform.forward * -zoomSize;
    }
}
