using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {

    private Material mat;

    private float time;

	// Use this for initialization
	void Start () {
        mat = GetComponent<MeshRenderer>().material;
        time = 0;
	}
	
	// Update is called once per frame
	void Update () {
		//time += 
	}
}
