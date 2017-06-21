using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseRunner : MonoBehaviour {

    public float radius;
    public float speed;
    public Vector3 center;

	// Use this for initialization
	void Start () {
		
	}

    float t;
	// Update is called once per frame
	void Update () {
        t += Time.deltaTime;
        transform.position = new Vector3(center.x + radius * Mathf.Cos(speed * t), center.y, center.z + radius * Mathf.Sin(speed * t));
        transform.LookAt(new Vector3(center.x + radius * Mathf.Cos(speed * (t+0.1f)), center.y, center.z + radius * Mathf.Sin(speed * (t+0.1f))));
	}
}
