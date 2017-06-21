using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ProceduralMesh : MonoBehaviour {

    Material[] mat;
    public Shader shader;
    public Vector2 dimension = new Vector2(100, 100);
    public Vector2 range = new Vector2(10, 10);

    public float length;
    public float width;
    public float distribution;
    public Color color;
    public Vector3 gravity;
    public float debugFloat;

    ComputeBuffer[] particleBuffer;
    Mesh[] particleMesh;

    [SerializeField] List<int> particleContainer = new List<int>();

    int particleNum;

	void Start () {

        particleNum = (int)(dimension.x * dimension.y);

        var v = Mathf.FloorToInt(particleNum / 64000f);
        Debug.Log(v);
        if (v == 0) {
            particleContainer.Add(particleNum);
        } else {
            for (int i = 0; i < v; i++) {
                particleContainer.Add(64000);
            }
            particleContainer.Add(particleNum % 64000);
        }


        // buffer
        particleBuffer = new ComputeBuffer[particleContainer.Count];

        for (int k = 0; k < particleContainer.Count; k++) {
            particleBuffer[k] = new ComputeBuffer(particleContainer[k], Marshal.SizeOf(typeof(Vector3)));

            Vector3[] p = new Vector3[particleContainer[k]];
            for (int i = 0; i < particleContainer[k]; i++) {
                p[i] = new Vector3(Random.Range(0, range.x), 0, Random.Range(0, range.y));
            }

            particleBuffer[k].SetData(p);
        }

        //mesh
        particleMesh = new Mesh[particleContainer.Count];
        for (int k = 0; k < particleContainer.Count; k++) {
            int vertexCount = particleContainer[k];
            Vector3[] vertices = new Vector3[vertexCount];
            int[] indices = new int[vertexCount];

            for (int i = 0; i < vertexCount; i++) {
                vertices[i] = new Vector3(Random.Range(0, range.x), 0, Random.Range(0, range.y));
                indices[i] = i;
            }

            particleMesh[k] = new Mesh();
            particleMesh[k].vertices = vertices;
            particleMesh[k].SetIndices(indices, MeshTopology.Points, 0);
            particleMesh[k].RecalculateBounds();
        }


        
	}
	
	void Update () {

        mat = new Material[particleContainer.Count];
        for (int i = 0; i < particleContainer.Count; i++) {
            mat[i] = new Material(shader);
        }
        

        for (int i = 0; i < particleContainer.Count; i++) {
            mat[i].SetPass(0);
            mat[i].SetColor("_Color", color);
            mat[i].SetFloat("_Length", length);
            mat[i].SetFloat("_Width", width);
            mat[i].SetFloat("_DebugFloat", debugFloat);
            mat[i].SetFloat("_Distribution", distribution);
            mat[i].SetVector("_Gravity", gravity);
            mat[i].SetBuffer("_Particles", particleBuffer[i]);
            Graphics.DrawMesh(particleMesh[i], Matrix4x4.identity, mat[i], 0);
        }
    }


    void OnDestroy() {
        for (int i = 0; i < particleContainer.Count; i++) {
            if (particleBuffer[i] != null) {
                particleBuffer[i].Release();
            }
        }
        
    }
}
