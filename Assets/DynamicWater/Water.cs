using UnityEngine;
using System.Collections;

public class Water : MonoBehaviour {
    
    Vector2[] positions;
    float[] velocities;
    float[] accelerations;
    
    GameObject[] meshobjects;
    GameObject[] colliders;
    Mesh[] meshes;
    
    public GameObject watermesh;
    
    const float springconstant = 0.02f;
    const float damping = 0.04f;
    const float spread = 0.05f;
    const float z = -1f;
    
    float baseheight;
    public float left = -10f;   // 左端の位置
    public float bottom = -3;   // 下端の位置

    public float width = 20;    // 幅
    public float top = 0;       // 高さ

    void Start() {
        SpawnWater(-left,width,top,bottom);
    }

    public void Splash(float xpos, float velocity) {
        if (xpos >= positions[0].x && xpos <= positions[positions.Length-1].x) {
            //Offset the x position to be the distance from the left side
            xpos -= positions[0].x;

            //Find which spring we're touching
            int index = Mathf.RoundToInt((positions.Length-1)*(xpos / (positions[positions.Length-1].x - positions[0].x)));

            //Add the velocity of the falling object to the spring
            velocities[index] += velocity;
        }
    }

    public void SpawnWater(float Left, float Width, float Top, float Bottom) {

        // 全体にコライダを付加
        gameObject.AddComponent<BoxCollider2D>();
        GetComponent<BoxCollider2D>().offset = new Vector2(Left + Width / 2, (Top + Bottom) / 2);
        GetComponent<BoxCollider2D>().size = new Vector2(Width, Top - Bottom);
        GetComponent<BoxCollider2D>().isTrigger = true;
        
        //Calculating the number of edges and nodes we have
        int edgecount = Mathf.RoundToInt(Width) * 5;
        int nodecount = edgecount + 1;

        //Declare our physics arrays
        positions = new Vector2[nodecount];
        velocities = new float[nodecount];
        accelerations = new float[nodecount];
        
        //Declare our mesh arrays
        meshobjects = new GameObject[edgecount];
        meshes = new Mesh[edgecount];
        colliders = new GameObject[edgecount];

        //Set our variables
        baseheight = Top;
        bottom = Bottom;
        left = Left;

        //For each node, set the our physics arrays
        for (int i = 0; i < nodecount; i++) {
            positions[i].y = Top;
            positions[i].x = Left + Width * i / edgecount;
            accelerations[i] = 0;
            velocities[i] = 0;
        }

        //Setting the meshes now:
        for (int i = 0; i < edgecount; i++) {
            meshes[i] = new Mesh();

            Vector3[] Vertices = new Vector3[4];
            Vertices[0] = new Vector3(positions[i].x, positions[i].y, z);
            Vertices[1] = new Vector3(positions[i + 1].x, positions[i + 1].y, z);
            Vertices[2] = new Vector3(positions[i].x, bottom, z);
            Vertices[3] = new Vector3(positions[i+1].x, bottom, z);

            Vector2[] UVs = new Vector2[4];
            UVs[0] = new Vector2(0, 1);
            UVs[1] = new Vector2(1, 1);
            UVs[2] = new Vector2(0, 0);
            UVs[3] = new Vector2(1, 0);
            
            int[] tris = new int[6] { 0, 1, 3, 3, 2, 0};
            
            meshes[i].vertices = Vertices;
            meshes[i].uv = UVs;
            meshes[i].triangles = tris;
            
            meshobjects[i] = Instantiate(watermesh,Vector3.zero,Quaternion.identity) as GameObject;
            meshobjects[i].GetComponent<MeshFilter>().mesh = meshes[i];
            meshobjects[i].transform.parent = transform;

            //Create our colliders, set them be our child
            colliders[i] = new GameObject();
            colliders[i].name = "Trigger";
            colliders[i].AddComponent<BoxCollider2D>();
            colliders[i].transform.parent = transform;

            //Set the position and scale to the correct dimensions
            colliders[i].transform.position = new Vector3(Left + Width * (i + 0.5f) / edgecount, Top - 0.5f, 0);
            colliders[i].transform.localScale = new Vector3(Width / edgecount, 1, 1);

            //Add a WaterDetector and make sure they're triggers
            colliders[i].GetComponent<BoxCollider2D>().isTrigger = true;
            colliders[i].AddComponent<WaterDetector>();

        }

        
        
        
    }

    //Same as the code from in the meshes before, set the new mesh positions
    void UpdateMeshes()
    {
        for (int i = 0; i < meshes.Length; i++)
        {

            Vector3[] Vertices = new Vector3[4];
            Vertices[0] = new Vector3(positions[i].x, positions[i].y, z);
            Vertices[1] = new Vector3(positions[i+1].x, positions[i+1].y, z);
            Vertices[2] = new Vector3(positions[i].x, bottom, z);
            Vertices[3] = new Vector3(positions[i+1].x, bottom, z);

            meshes[i].vertices = Vertices;
        }
    }
    
    void FixedUpdate()
    {
        //Here we use the Euler method to handle all the physics of our springs:
        for (int i = 0; i < positions.Length ; i++)
        {
            float force = springconstant * (positions[i].y - baseheight) + velocities[i]*damping ;
            accelerations[i] = -force;
            positions[i].y += velocities[i];
            velocities[i] += accelerations[i];
        }

        //Now we store the difference in heights:
        float[] leftDeltas = new float[positions.Length];
        float[] rightDeltas = new float[positions.Length];

        //We make 8 small passes for fluidity:
        for (int j = 0; j < 8; j++)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                //We check the heights of the nearby nodes, adjust velocities accordingly, record the height differences
                if (i > 0)
                {
                    leftDeltas[i] = spread * (positions[i].y - positions[i-1].y);
                    velocities[i - 1] += leftDeltas[i];
                }
                if (i < positions.Length - 1)
                {
                    rightDeltas[i] = spread * (positions[i].y - positions[i + 1].y);
                    velocities[i + 1] += rightDeltas[i];
                }
            }

            //Now we apply a difference in position
            for (int i = 0; i < positions.Length; i++)
            {
                if (i > 0)
                    positions[i-1].y += leftDeltas[i];
                if (i < positions.Length - 1)
                    positions[i + 1].y += rightDeltas[i];
            }
        }
        
        UpdateMeshes();
	}

}
