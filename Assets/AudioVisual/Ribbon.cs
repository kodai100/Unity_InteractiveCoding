using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Kodai.Audio.Ribbon {

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Ribbon : MonoBehaviour {

        public Manager manager;
        public int id;

        public Material mat;

        Mesh mesh;

        void Start() {
            MeshFilter mf = GetComponent<MeshFilter>();
            mesh = new Mesh();
            mf.mesh = mesh;
        }

        
        void Update() {

            Vector3[] v = new Vector3[manager.band * 2];
            for(int i = 0; i<manager.band; i+=2) {
                v[i] = new Vector3(0.2f * i, Manager.audioData[id][i] * 30 * Mathf.Pow(manager.decay, id) + 0.3f, transform.position.z);
                v[i + 1] = new Vector3(0.2f * i, Manager.audioData[id][i] * 30 * Mathf.Pow(manager.decay, id) - 0.3f, transform.position.z);
            }
            mesh.vertices = v;

            int[] idx = new int[manager.band * 2 * 3];
            int count = 0;
            for(int i = 0; i<manager.band - 1; i++) {
                idx[count] = 2 * i;
                idx[count + 1] = 2 * i + 3;
                idx[count + 2] = 2 * i + 1;
                idx[count + 3] = 2 * i;
                idx[count + 4] = 2 * i + 2;
                idx[count + 5] = 2 * i + 3;
                count += 6;
            }
            mesh.SetIndices(idx, MeshTopology.Triangles, 0);
            mesh.RecalculateNormals();

        }
        
    }
}