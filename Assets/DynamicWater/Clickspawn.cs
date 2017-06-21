using UnityEngine;
using System.Collections;

public class Clickspawn : MonoBehaviour {

    public GameObject ball;
	
	void Update () {
        if (Input.GetMouseButtonDown(0)) {
            GameObject brick = Instantiate(ball, Camera.main.ScreenToWorldPoint(Input.mousePosition + new Vector3(0,0,10)), ball.transform.rotation) as GameObject;
            brick.transform.Rotate(0, 0, UnityEngine.Random.Range(0, 0));
            float randomScale = Random.Range(4f, 6f);
            brick.transform.localScale = new Vector3(randomScale, randomScale,1);
            brick.GetComponent<Rigidbody2D>().mass = brick.transform.localScale.x * brick.transform.localScale.y / 20f;
            Destroy(brick, 1.4f);
        }
	}
}
