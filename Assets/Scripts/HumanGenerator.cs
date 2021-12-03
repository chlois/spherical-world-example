using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HumanGenerator : MonoBehaviour {
    int count = 0;
    Rigidbody m_Rigidbody;
    public GameObject prototype;

    void Start() {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    void Update() {
        GenerateHuman();
    }

    void GenerateHuman() {
        if (Input.GetMouseButtonDown(0)) {
            var myMousePosition = Input.mousePosition;
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(myMousePosition).origin,
                Camera.main.ScreenPointToRay(myMousePosition).direction, out hit, 100,
                Physics.DefaultRaycastLayers)) {
                if (hit.rigidbody == m_Rigidbody) {
                    GameObject human = GameObject.Instantiate(prototype);
                    human.transform.position = new Vector3(0, 5.1f, -0.172f);
                    human.name = "human"+count.ToString();
                    int skinToneIndex = Random.Range(0, 3);
                    int long_shirt = Random.Range(0, 2); // 0: short, 1: long
                    int long_pants = Random.Range(0, 2); // 0: short, 1: long
                    int shirtColorIndex = Random.Range(0, 3);
                    int pantsColorIndex = Random.Range(0, 3);
                    Material skinTone = Resources.Load("Materials/skin"+skinToneIndex.ToString(), typeof(Material)) as Material;
                    Material shirtColor = Resources.Load("Materials/shirt"+shirtColorIndex.ToString(), typeof(Material)) as Material;
                    Material pantsColor = Resources.Load("Materials/pants"+pantsColorIndex.ToString(), typeof(Material)) as Material;
                    foreach (var child in human.GetComponentsInChildren<Renderer>()){
                        if (child.gameObject.name == "head"){
                            child.material = skinTone;
                        }
                        else if (child.gameObject.name == "rightforearm"){
                            if (long_shirt == 0)
                                child.material = skinTone;
                            else
                                child.material = shirtColor;
                        }
                        else if (child.gameObject.name == "leftforearm"){
                            if (long_shirt == 0)
                                child.material = skinTone;
                            else
                                child.material = shirtColor;
                        }
                        else if (child.gameObject.name == "leftlowleg"){
                            if (long_pants == 0)
                                child.material = skinTone;
                            else
                                child.material = pantsColor;
                        }
                        else if (child.gameObject.name == "rightlowleg"){
                            if (long_pants == 0)
                                child.material = skinTone;
                            else
                                child.material = pantsColor;
                        }
                        else if (child.gameObject.name == "lefthand"){
                            child.material = skinTone;
                        }
                        else if (child.gameObject.name == "righthand"){
                            child.material = skinTone;
                        }
                        else if (child.gameObject.name == "leftfoot"){
                            child.material = skinTone;
                        }
                        else if (child.gameObject.name == "rightfoot"){
                            child.material = skinTone;
                        }
                        else if (child.gameObject.name == "body"){
                            child.material = shirtColor;
                        }
                        else if (child.gameObject.name == "rightarm"){
                            child.material = shirtColor;
                        }
                        else if (child.gameObject.name == "leftarm"){
                            child.material = shirtColor;
                        }
                        else if (child.gameObject.name == "rightupleg"){
                            child.material = pantsColor;
                        }
                        else if (child.gameObject.name == "leftupleg"){
                            child.material = pantsColor;
                        }
                    }
                    count += 1;
                }
            }
        }
    }
}
