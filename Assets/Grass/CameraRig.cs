using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour {

    public GameObject horse;
    public float radius;
    public float height;
    public Vector3 targetOffset;
    public float forwardOffset;

    Vector3 center;
    float speed;
	// Use this for initialization
	void Start () {
        HorseRunner script = horse.GetComponent<HorseRunner>();
        center = script.center;
        speed = script.speed;
	}

    float t;
	// Update is called once per frame
	void Update () {
        t += Time.deltaTime;
        transform.position = new Vector3(center.x + radius * Mathf.Cos(speed * (forwardOffset + t)), height, center.z + radius * Mathf.Sin(speed * (forwardOffset+t)));
        transform.LookAt(center);
    }
}
