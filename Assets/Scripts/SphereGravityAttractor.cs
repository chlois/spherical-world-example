using System.Collections;
using UnityEngine;

public class SphereGravityAttractor : MonoBehaviour {
    public float gravity = -10;
    
    // add gravity force to object and rotate object to correct up direction
    public void Attract(Transform objectTransform) {
        Vector3 objectUp = objectTransform.up; // from direction
        Vector3 gravityUp = GetUp(objectTransform); // to direction
        
        objectTransform.GetComponent<Rigidbody>().AddForce(gravity * gravityUp);
        
        Quaternion targetRotation = Quaternion.FromToRotation(objectUp, gravityUp) * objectTransform.rotation;
        objectTransform.rotation = Quaternion.Slerp(objectTransform.rotation, targetRotation, 50*Time.deltaTime);
    }
    
    // gravity up direction
    public Vector3 GetUp(Transform objectTransform) {
        return (objectTransform.position - transform.position).normalized;
    }
    
    // gravity force
    public Vector3 GetForce(Transform objectTransform) {
        return GetUp(objectTransform) * gravity;
    }
}
