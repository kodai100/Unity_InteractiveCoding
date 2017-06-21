
using System.Runtime.InteropServices;
using UnityEngine;

namespace Kodai.NBody2D {

    public class NBodySimulation : MonoBehaviour {

        #region Public
        public LatticeOptimizeUtil.Num num = LatticeOptimizeUtil.Num.NUM_8K;

        public float G = 6.673e-11f;
        public float solarMass = 1.98892e30f;
        public float dt = 1f / 60;

        public Vector2 range = new Vector2(100, 100);
        public Vector2 gridDim = new Vector2(10,10);
        #endregion

        #region Private
        [SerializeField] private int numParticles;
        Particle[] particles;

        #endregion

        #region GPU
        public ComputeShader cs;
        private readonly int SIMULATION_BLOCK_SIZE = 32;
        private int threadGroupSize;
        private ComputeBuffer particlesBufferRead;
        private ComputeBuffer particlesBufferWrite;
        private ComputeBuffer tmpBuffer;
        #endregion

        void Start() {
            // numParticles = (int)num;
            Debug.Log(numParticles);

            InitializeParticles();
            InitializeComputeBuffers();
        }

        void Update() {
            cs.SetFloat("_G", G);
            cs.SetFloat("_SolarMass", solarMass);
            cs.SetFloat("_DT", dt);
            cs.SetInt("_NumParticles", numParticles);

            int kernel = 0;

            kernel = cs.FindKernel("Update");
            cs.SetBuffer(kernel, "_ParticlesBufferRead", particlesBufferRead);
            cs.SetBuffer(kernel, "_ParticlesBufferWrite", particlesBufferWrite);
            cs.Dispatch(kernel, threadGroupSize, 1, 1);

            SwapBuffer();
        }

        void OnDestroy() {
            if (particlesBufferRead != null) {
                particlesBufferRead.Release();
            }
            if (particlesBufferWrite != null) {
                particlesBufferWrite.Release();
            }
            if (tmpBuffer != null) {
                tmpBuffer.Release();
            }
        }


        void InitializeParticles() {
            particles = new Particle[numParticles];
            for(int i = 0; i< particles.Length - 1; i++) {
                Vector2 pos = Random.insideUnitCircle * 100; // new Vector2(1e18f * Mathf.Exp(-1.8f) * (0.5f - Random.value), 1e18f * Mathf.Exp(-1.8f) * (0.5f - Random.value));

                particles[i] = new Particle(pos, Vector2.zero, 1000);
            }
            particles[particles.Length - 1] = new Particle(Vector2.zero, Vector2.zero, 1e10f);
        }

        void InitializeComputeBuffers() {
            particlesBufferRead = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(Particle)));
            particlesBufferWrite = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(Particle)));
            tmpBuffer = new ComputeBuffer(numParticles, Marshal.SizeOf(typeof(Particle)));
            particlesBufferRead.SetData(particles);
            particlesBufferWrite.SetData(particles);
            tmpBuffer.SetData(particles);
            threadGroupSize = numParticles / SIMULATION_BLOCK_SIZE + 1;
        }

        void SwapBuffer() {
            ComputeBuffer tmp = particlesBufferRead;
            particlesBufferRead = particlesBufferWrite;
            particlesBufferWrite = tmp;
        }

        public ComputeBuffer GetBuffer() {
            return particlesBufferRead;
        }

        public int GetNumParticles() {
            return numParticles;
        }
    }

    struct Particle {
        public Vector2 pos;
        public Vector2 vel;
        public Vector2 force;
        public float mass;

        public Particle(Vector2 pos, Vector2 vel, float mass) {
            this.pos = pos;
            this.vel = vel;
            this.force = Vector2.zero;
            this.mass = mass;
        }
    }
}