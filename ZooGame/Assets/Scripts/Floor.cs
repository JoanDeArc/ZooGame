using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit, 100))
        {
            GetComponent<Renderer>().material.SetVector("_ObjPos", new Vector4(hit.point.x, hit.point.y, hit.point.z, 0));
        }
    }
}
