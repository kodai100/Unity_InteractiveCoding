using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Kodai.Primitives {

    [RequireComponent(typeof(MeshFilter))]
    public class OctahedronSphereTester : MonoBehaviour {

        public int subdivisions = 0;
        public float radius = 1f;

        void Awake() {
            //GetComponent<MeshFilter>().mesh = OctahedronSphereCreator.Create(subdivisions, radius);
            AssetDatabase.CreateAsset(OctahedronSphereCreator.Create(subdivisions, radius), "Assets/Universe/Octahedron.asset");
        }

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }
    }
}