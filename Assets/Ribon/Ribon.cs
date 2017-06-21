using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class Ribon : MonoBehaviour
{
	[SerializeField] int particleCount;
	[SerializeField] Shader shader;
	[SerializeField] ComputeShader particleupdater;
	[SerializeField] Color col;

	[SerializeField] Mesh[] particleMesh;
	[SerializeField] List<int> particleContaner;
	[SerializeField] Color[] colors;
	[SerializeField] float length = 20f;

	Material[] mat;
	ComputeBuffer[] particleBuffer;

	struct Particle
	{
		public Vector3 position;
		public Vector3 velocity;
		public Color color;
	}

	// Use this for initialization
	void Start ()
	{
		var v = Mathf.FloorToInt (particleCount / 64000f);
		if (v == 0) {
			particleContaner.Add (particleCount);
		} else {
			for (int i = 0; i < v; i++) {
				particleContaner.Add (64000);
			}
			particleContaner.Add (particleCount % 64000);
		}

		initParticleBuffer ();
		initMesh ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		updateParticleBuffer ();
		particleRender ();
	}

	void particleRender ()
	{
		if (mat == null) {
			mat = new Material[particleContaner.Count];
			for (int i = 0; i < particleContaner.Count; i++) {
				mat [i] = new Material (shader);
			}
		}

		for (int i = 0; i < particleContaner.Count; i++) {
			mat [i].SetPass (0);
			Shader.SetGlobalFloat ("_Length", length);
			mat [i].SetBuffer ("_Particles", particleBuffer [i]);
			Graphics.DrawMesh (particleMesh [i], Matrix4x4.identity, mat [i], 0);
		}
	}

	void initParticleBuffer () {
		particleBuffer = new ComputeBuffer[particleContaner.Count];

		for (int k = 0; k < particleContaner.Count; k++) {
			particleBuffer [k] = new ComputeBuffer (particleContaner [k], Marshal.SizeOf (typeof(Particle)));

			Particle[] particles = new Particle[particleContaner [k]];
			for (int i = 0; i < particleContaner [k]; i++) {
				Particle _particle = new Particle ();
				var _pos = Random.insideUnitSphere;
				_particle.color = colors [Random.Range (0, colors.Length)];
				_particle.position = _pos;
				particles [i] = _particle;
			}

			particleBuffer [k].SetData (particles);
		}
	}

	void initMesh ()
	{
		particleMesh = new Mesh[particleContaner.Count];
		for (int k = 0; k < particleContaner.Count; k++) {
			int vertexCount = particleContaner [k];
			Vector3[] vertices = new Vector3[vertexCount];

			int[] indices = new int[vertexCount];

			for (int i = 0; i < vertexCount; i++) {
				vertices [i] = Random.insideUnitSphere;
				indices [i] = i;
			}

			particleMesh [k] = new Mesh ();
			particleMesh [k].vertices = vertices;
			particleMesh [k].SetIndices (indices, MeshTopology.Points, 0);
			particleMesh [k].RecalculateBounds ();
		}
	}

	void updateParticleBuffer ()
	{
		for (int i = 0; i < particleContaner.Count; i++) {
			particleupdater.SetBuffer (0, "_Particles", particleBuffer [i]);
			particleupdater.Dispatch (0, particleContaner [i] / 8 + 1, 1, 1);
		}
	}

	//render object
	void OnDisable ()
	{
		for (int i = 0; i < particleContaner.Count; i++) {
			particleBuffer [i].Release ();
		}
	}
}
