using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Taylor : MonoBehaviour {

    int num = 0;

    RenderTexture texture;

    public int width = 512;
    public int height = 512;

    public float range = Mathf.PI * 2;

    public ComputeShader cs;

	void Start () {

        texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        texture.enableRandomWrite = true;
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Create();

        GetComponent<Renderer>().material.mainTexture = this.texture;

    }
    

    void Update () {
        if (Input.GetMouseButtonUp(0)) {
            num += 1;
        }

        cs.SetInt("_Num", num);
        cs.SetInt("_Width", width);
        cs.SetInt("_Height", height);
        cs.SetFloat("_Range", range);

        int kernel = cs.FindKernel("Sin");

        cs.SetTexture(0, "_Result", texture);
        cs.Dispatch(kernel, width / 32, height / 32, 1);
    }

    private void OnGUI() {
        GUILayout.Label("Num Terms : " + num);
    }
}