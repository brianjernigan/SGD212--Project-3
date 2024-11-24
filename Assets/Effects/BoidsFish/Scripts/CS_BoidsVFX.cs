using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

namespace SDQQ1234
{
    public class CS_BoidsVFX : MonoBehaviour
    {
        [System.Serializable] 
        struct BoidData
        {
            public Vector3 velocity;
            public Vector3 position;
        }
        public enum FishShape
        {
            Box = 0,
            Target = 1
        }

        public FishShape shape = FishShape.Box;

        public VisualEffect boidsVFX;

        public float bornRadius = 5;
        private GraphicsBuffer BoidDataGraphicsPositionBuffer;
        private GraphicsBuffer BoidDataGraphicsVelocityBuffer;

        public ComputeShader boidsComputeShader;
        [Range(16, 10000)]
        [Tooltip("FishCount")]
        [SerializeField] private int boidsCount = 1000;

        [Header("CAS Radius")]
        [SerializeField] private float cohesionRadius = 5.0f; // The radius at which cohesion is applied to other individuals
        [SerializeField] private float alignmentRadius = 3.0f; // Aligns the radius of other individuals
        [SerializeField] private float separationRadius = 2f; // The radius of the separation

        [Header("CAS Forces")]
        [SerializeField] private float cohesionWeight = 1f;
        [SerializeField] private float alignmentWeight = 1f; 
        [SerializeField] private float separationWeight = 2.0f;
        
        [Header("Boid")]
        [SerializeField] private float boidMaximumSpeed = 20.0f;
        [SerializeField] private float boidMaxSteeringForce = 10.0f;
        
        
        [Header("Simulation")]
        [SerializeField] private Vector3 simulationCenter = Vector3.zero;
        [SerializeField] private Vector3 simulationDimensions = new Vector3(32.0f, 32.0f, 32.0f);
        [SerializeField] private float simulationBoundsAvoidWeight = 10.0f;

        [Header("FollowTarget")] 
        [SerializeField] private Transform targetPos;
        [Header("SphereCollider")] 
        [SerializeField] private SphereCollider playerCollider;

        private ComputeBuffer _boidsSteeringForcesBuffer; // A buffer used to store the steering force value of the boid
        private ComputeBuffer _boidsDataBuffer; // Buffers to store Boid's basic data (speed, position, Transform, etc.)

        private uint _storedThreadGroupSize;
        private int _dispatchedThreadGroupSize;

        private int _steeringForcesKernelId; // The kernel used to handle the boids steering force calculations
        private int _boidsDataKernelId; // The Kernel for handling boids, position, speed, etc

        // Start is called before the first frame update
        void Start()
        {
            InitBuffers();
            InitKernels();
        }

        void UpdateVFXProp()
        {
            if (boidsVFX)
            {
                
                boidsVFX.SetInt("ParticleCount",boidsCount);
                boidsVFX.SetVector3("BoundsCenter",simulationCenter);
                boidsVFX.SetVector3("BoundsSize",simulationDimensions);
                boidsVFX.SetGraphicsBuffer("PositionBuffer",BoidDataGraphicsPositionBuffer);
                boidsVFX.SetGraphicsBuffer("VelocityBuffer",BoidDataGraphicsVelocityBuffer);
            }
        }
        private void InitBuffers()
        {
            boidsVFX.Reinit();
            //float bornRadius = Mathf.Min(simulationDimensions.x,simulationDimensions.y) * 0.5f;
            _boidsDataBuffer = new ComputeBuffer(boidsCount, sizeof(float) * 6); // 6 for two Vector3
            _boidsSteeringForcesBuffer = new ComputeBuffer(boidsCount, sizeof(float) * 3); // 3 for one Vector3

            BoidDataGraphicsPositionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, boidsCount, Marshal.SizeOf(typeof(Vector3)));
            BoidDataGraphicsVelocityBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, boidsCount, Marshal.SizeOf(typeof(Vector3)));
            
            // Prepare data arrays
            Vector3[] forceArr = new Vector3[boidsCount];
            BoidData[] boidDataArr = new BoidData[boidsCount];
            Vector3[] positionArr = new Vector3[boidsCount];
            Vector3[] velocityArr = new Vector3[boidsCount];
            
            for (var i = 0; i < boidsCount; i++)
            {
                forceArr[i] = Vector3.zero;
                switch (shape)
                {
                    case FishShape.Box:
                        //初始化在方块里的位置和速度
                        positionArr[i] = boidDataArr[i].position = Random.insideUnitSphere * bornRadius - simulationDimensions.z*0.5f*Vector3.forward;
                        velocityArr[i] = boidDataArr[i].velocity = Vector3.forward;
                        break;
                    case FishShape.Target:
                         positionArr[i] = boidDataArr[i].position = Random.insideUnitSphere * bornRadius;
                         velocityArr[i] = boidDataArr[i].velocity = targetPos.position - boidDataArr[i].position;
                         break;
                }

            }

            // Set data to buffers
            _boidsSteeringForcesBuffer.SetData(forceArr);
            _boidsDataBuffer.SetData(boidDataArr); 
            BoidDataGraphicsPositionBuffer.SetData(positionArr);
            BoidDataGraphicsVelocityBuffer.SetData(velocityArr);

        }
        
        private void InitKernels()
        {
            _steeringForcesKernelId = boidsComputeShader.FindKernel("SteeringForcesCS");
            _boidsDataKernelId = boidsComputeShader.FindKernel("BoidsDataCS");

            boidsComputeShader.GetKernelThreadGroupSizes(_steeringForcesKernelId, out _storedThreadGroupSize, out _, out _);
            var dispatchedThreadGroupSize = boidsCount / (int)_storedThreadGroupSize;
            if (dispatchedThreadGroupSize % _storedThreadGroupSize == 0)
            {
                _dispatchedThreadGroupSize = dispatchedThreadGroupSize;
                return;
            }
            while (dispatchedThreadGroupSize % _storedThreadGroupSize != 0)
            {
                dispatchedThreadGroupSize += 1;
                if (dispatchedThreadGroupSize % _storedThreadGroupSize != 0) continue;
           
                _dispatchedThreadGroupSize = dispatchedThreadGroupSize;
           
                Debug.LogFormat("Initial threads: {0}", _storedThreadGroupSize);
                Debug.LogFormat("Threads X used: {0}", _dispatchedThreadGroupSize);
                break;
            }
        }
        
        private void Simulation(int steeringForcesKernelId, int boidsDataKernelId)
        {
            if(boidsComputeShader == null || _boidsDataBuffer == null) return;

            boidsComputeShader.SetInt("_BoidsCount", boidsCount);
            boidsComputeShader.SetInt("_Shape",(int)shape);

            boidsComputeShader.SetBuffer(steeringForcesKernelId, "_BoidsDataBuffer", _boidsDataBuffer);
            boidsComputeShader.SetBuffer(steeringForcesKernelId, "_BoidsSteeringForcesBufferRw", _boidsSteeringForcesBuffer);
            boidsComputeShader.SetBuffer(boidsDataKernelId, "_BoidsSteeringForcesBuffer", _boidsSteeringForcesBuffer);
            boidsComputeShader.SetBuffer(boidsDataKernelId, "_BoidsDataBufferRw", _boidsDataBuffer);
            
            boidsComputeShader.SetBuffer(boidsDataKernelId,"_PositionBufferRW",BoidDataGraphicsPositionBuffer);
            boidsComputeShader.SetBuffer(boidsDataKernelId,"_VelocityBufferRW",BoidDataGraphicsVelocityBuffer);

            boidsComputeShader.SetFloat("_CohesionRadius", cohesionRadius);
            boidsComputeShader.SetFloat("_AlignmentRadius", alignmentRadius);
            boidsComputeShader.SetFloat("_SeparationRadius", separationRadius);
            boidsComputeShader.SetFloat("_BoidMaximumSpeed", boidMaximumSpeed);
            boidsComputeShader.SetFloat("_BoidMaximumSteeringForce", boidMaxSteeringForce);
            boidsComputeShader.SetFloat("_SeparationWeight", separationWeight);
            boidsComputeShader.SetFloat("_CohesionWeight", cohesionWeight);
            boidsComputeShader.SetFloat("_AlignmentWeight", alignmentWeight);
            boidsComputeShader.SetFloat("_SimulationBoundsAvoidWeight", simulationBoundsAvoidWeight);

            boidsComputeShader.SetVector("_SimulationCenter", simulationCenter);
            boidsComputeShader.SetVector("_SimulationDimensions", simulationDimensions);
       
            boidsComputeShader.SetFloat("_DeltaTime", Time.deltaTime);

            if (targetPos)
            {
                boidsComputeShader.SetVector("_targetPos", targetPos.position - this.transform.position);
            }
            
            
            if (playerCollider)
            {
                Vector4 p_pos = playerCollider.transform.position-this.transform.position; //Calculate the player's position
                boidsComputeShader.SetVector("_PlayerPosition",p_pos);
                float radiusScale = Mathf.Max(playerCollider.transform.localScale.x,
                    Mathf.Max(playerCollider.transform.localScale.y, playerCollider.transform.localScale.z));
                boidsComputeShader.SetFloat("_PlayerRadius",playerCollider.radius*radiusScale);
            }
            
            boidsComputeShader.Dispatch(steeringForcesKernelId, _dispatchedThreadGroupSize, 1, 1);
            boidsComputeShader.Dispatch(boidsDataKernelId, _dispatchedThreadGroupSize, 1, 1);
           
            
        }
        
        // Update is called once per frame
        void Update()
        {
             Simulation(_steeringForcesKernelId, _boidsDataKernelId);
             UpdateVFXProp();
        }

        private void SafeReleaseBuffer(ref ComputeBuffer buffer)
        {
            if (buffer == null) return;
            buffer.Release();
            buffer = null;
        }
        private void ReleaseBuffer()
        {
            SafeReleaseBuffer(ref _boidsDataBuffer);
            SafeReleaseBuffer(ref _boidsSteeringForcesBuffer);
            if (BoidDataGraphicsPositionBuffer != null) { BoidDataGraphicsPositionBuffer.Release(); BoidDataGraphicsPositionBuffer = null; }
            if (BoidDataGraphicsVelocityBuffer != null) { BoidDataGraphicsVelocityBuffer.Release(); BoidDataGraphicsVelocityBuffer = null; }
        }
        private void OnDestroy()
        {
            ReleaseBuffer();
        }

        private void OnDrawGizmos()
        {
            switch (shape)
            {
                case FishShape.Box:
                    DrawBoxShape();
                    break;
                case FishShape.Target:
                     if (targetPos)
                     {
                         Gizmos.color = Color.yellow;
                         Gizmos.DrawSphere(targetPos.position,targetPos.localScale.magnitude);
                     }

                     Gizmos.color = Color.green;
                     Gizmos.DrawWireSphere(this.transform.position,bornRadius);
                     break;
            }

            if (playerCollider)
            {
                Gizmos.color = Color.red;
                float radiusScale = Mathf.Max(playerCollider.transform.localScale.x,
                    Mathf.Max(playerCollider.transform.localScale.y, playerCollider.transform.localScale.z));
                Gizmos.DrawWireSphere(playerCollider.transform.position,playerCollider.radius * radiusScale);
            }
            
            
        }

        void DrawBoxShape()
        {
            Vector3 centerPos = simulationCenter+transform.position;
            Vector3 startPos = centerPos - new Vector3(0, 0, simulationDimensions.z * 0.5f);
            Vector3 endPos = centerPos + new Vector3(0, 0, simulationDimensions.z * 0.5f);
            //float bornRadius = Mathf.Min(simulationDimensions.x,simulationDimensions.y) * 0.5f;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPos,bornRadius);//DrawStartPoint
            Gizmos.DrawLine(startPos,endPos);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(centerPos,simulationDimensions);
        }

    }

}
