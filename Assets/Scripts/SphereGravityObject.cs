using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereGravityObject : MonoBehaviour {
    public SphereGravityAttractor attractor;
    
    void Start() {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        GetComponent<Rigidbody>().useGravity = false;
    }

    void Update() {
        // get correct gravity force and up direction every frame
        attractor.Attract(transform);
    }
}
