using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kodai.NURBS.Surface {

    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class Surface : MonoBehaviour {

        float[] u_knots = { 0.0f, 0.143f, 0.286f, 0.429f, 0.571f, 0.714f, 0.857f, 1.0f };
        float[] v_knots = { 0.0f, 0.143f, 0.286f, 0.429f, 0.571f, 0.714f, 0.857f, 1.0f };

        GameObject[,] cps;
        int u_res = 5;
        int v_res = 5;

        float u_space;
        float v_space;

        float width = 10;
        float height = 10;

        public GameObject cpgizmo;
        public int resolution = 50;
        public bool animation = false;

        float time;

        void Start() {
            time = 0;
            cps = new GameObject[u_res, v_res];

            u_space = width / u_res;
            v_space = width / v_res;

            for(int i = 0; i < u_res; i++) {
                for(int j = 0; j < v_res; j++) {
                    cps[i,j] = Instantiate(cpgizmo, new Vector3(u_space * i, Random.Range(-4, 4), -v_space * j), Quaternion.identity, transform);
                }
            }

            
            // For Mesh Rendering
            mf = GetComponent<MeshFilter>();
        }

        void OnDrawGizmos() {

            if (!Application.isPlaying) return;

            for (int i = 0; i < u_res; i++) {
                for (int j = 0; j < v_res; j++) {
                    Gizmos.DrawWireSphere(cps[i, j].transform.position, 0.1f);
                }
            }

            Gizmos.color = Color.gray;

            for (int i = 0; i < u_res; i++) {
                // 横
                Gizmos.DrawLine(cps[i, 0].transform.position, cps[i, 1].transform.position);
                Gizmos.DrawLine(cps[i, 1].transform.position, cps[i, 2].transform.position);
                Gizmos.DrawLine(cps[i, 2].transform.position, cps[i, 3].transform.position);
                Gizmos.DrawLine(cps[i, 3].transform.position, cps[i, 4].transform.position);

                //縦
                Gizmos.DrawLine(cps[0, i].transform.position, cps[1, i].transform.position);
                Gizmos.DrawLine(cps[1, i].transform.position, cps[2, i].transform.position);
                Gizmos.DrawLine(cps[2, i].transform.position, cps[3, i].transform.position);
                Gizmos.DrawLine(cps[3, i].transform.position, cps[4, i].transform.position);
            }
        }


        Mesh mesh;
        MeshFilter mf;

        void Update() {
            
            if (animation) {
                time += Time.deltaTime;
                AnimateCP();
            } else {
                if (Input.GetKeyUp(KeyCode.I)) {
                    mouseClicked();
                }
            }
            
            // Degree
            int u_deg = u_knots.Length - u_res - 1; // 7 - 5 - 1 = 1
            int v_deg = v_knots.Length - v_res - 1; // 7 - 5 - 1 = 1
            

            mesh = new Mesh();

            List<Vector3> vertex = new List<Vector3>();
            // int[] triangles = new int[];
            // draw surface


            if (resolution < 0) {
                Debug.Log("Illegal Resolution");
                return;
            }

            //float s = (u_knots[u_knots.Length - u_deg - 1] - u_knots[u_deg]) / (float)resolution;
            float s = 0.01f;

            // ノット順
            // 0.143 -> 0.857 の間を0.01刻みで
            for (float u = u_knots[u_deg]; u <= u_knots[u_knots.Length - u_deg - 1] - s; u += s) {
                for (float v = v_knots[v_deg]; v <= v_knots[v_knots.Length - v_deg - 1]; v += s) {
                    Vector3 pt_uv = new Vector3();
                    Vector3 pt_u1v = new Vector3(); // u plus 0.01

                    for (int i = 0; i < u_res; i++) {
                        for (int j = 0; j < v_res; j++) {
                            float basisv = basisn(v, j, v_deg, v_knots);
                            float basisu = basisn(u, i, u_deg, u_knots);
                            float basisu1 = basisn(u + 0.01f, i, u_deg, u_knots);
                            Vector3 pk = cps[i,j].transform.position * (basisu * basisv);
                            Vector3 pk1 = cps[i,j].transform.position * (basisu1 * basisv);

                            pt_uv += pk;
                            pt_u1v += pk1;
                        }
                    }

                    vertex.Add(new Vector3(pt_uv.x, pt_uv.y, pt_uv.z));
                    vertex.Add(new Vector3(pt_u1v.x, pt_u1v.y, pt_u1v.z));
                }
            }

            mesh.vertices = vertex.ToArray();

            int[] index = new int[vertex.Count];
            for(int i = 0; i<index.Length; i++) {
                index[i] = i;
            }
            mesh.SetIndices(index, MeshTopology.Points, 0);

            mf.sharedMesh = mesh;
        }

        float basisn(float u, int k, int d, float[] knots) {
            if (d == 0) {
                return basis0(u, k, knots);
            } else {
                float b1 = basisn(u, k, d - 1, knots) * (u - knots[k]) / (knots[k + d] - knots[k]);
                float b2 = basisn(u, k + 1, d - 1, knots) * (knots[k + d + 1] - u) / (knots[k + d + 1] - knots[k + 1]);
                return b1 + b2;
            }
        }

        float basis0(float u, int k, float[] knots) {
            if (u >= knots[k] && u < knots[k + 1]) {
                return 1;
            } else {
                return 0;
            }
        }


        void mouseClicked() {
            // randomise the control points
            for (int i = 0; i < u_res; i++) {
                for (int j = 0; j < v_res; j++) {
                    cps[i, j].transform.SetPositionAndRotation(new Vector3(u_space * i, Random.Range(-4, 4), -v_space * j), Quaternion.identity); 
                }
            }
        }

        void AnimateCP() {
            for (int i = 0; i < u_res; i++) {
                for (int j = 0; j < v_res; j++) {
                    cps[i, j].transform.SetPositionAndRotation(new Vector3(u_space * i, (Mathf.PerlinNoise((i+1) * time/4, (j+1) * time/4) * 2 - 1) * 4, -v_space * j), Quaternion.identity);
                }
            }
        }


    }
}