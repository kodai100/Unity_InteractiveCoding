using UnityEngine;

[System.Serializable]
public class Rotations {
    public float rotationXY;
    public float rotationYZ;
    public float rotationZX;
    public float rotationXW;
    public float rotationYW;
    public float rotationZW;
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Tesseract : MonoBehaviour {

    public Transform viewPoint;
    public bool animate = true;
    public float fov = 25;
    public float rotationSpeed = 14;
    public Rotations rotations = new Rotations();
    public Color32[] vertexColors = new Color32[16];

    private Vector3[] vertices;
    private Mesh mesh;
    private MeshFilter filter;

    protected void Awake() {
        vertices = new Vector3[Utils4D.tesseractPoints.Length];

        mesh = new Mesh();
        mesh.subMeshCount = 2;              // ワイヤーフレーム描画用とサーフェス描画用
        mesh.vertices = vertices;
        mesh.colors32 = vertexColors;
        mesh.MarkDynamic();

        GeneratePlaneMesh(mesh, 0);
        GenerateLineMesh(mesh, 1);

        filter = GetComponent<MeshFilter>();
        filter.mesh = mesh;
    }

    public void Update() {

        if (animate) {
            float amount = rotationSpeed * Time.deltaTime;
            rotations.rotationXY += amount;
            rotations.rotationZX += amount;
            rotations.rotationYW += amount;
        }

        GenerateVertices(vertices);

        mesh.vertices = vertices;
    }

    /// <summary>
    /// 4次元のハイパーキューブを3次元に射影した結果を返す
    /// </summary>
    /// <param name="vertices">プロジェクションする頂点</param>
    public void GenerateVertices(Vector3[] vertices) {

        // それぞれの平面での回転行列を作成
        Matrix4x4 matrixXY = Utils4D.CreateRotationMatrixXY(rotations.rotationXY * Mathf.Deg2Rad);
        Matrix4x4 matrixYZ = Utils4D.CreateRotationMatrixYZ(rotations.rotationYZ * Mathf.Deg2Rad);
        Matrix4x4 matrixZX = Utils4D.CreateRotationMatrixZX(rotations.rotationZX * Mathf.Deg2Rad);
        Matrix4x4 matrixXW = Utils4D.CreateRotationMatrixXW(rotations.rotationXW * Mathf.Deg2Rad);
        Matrix4x4 matrixYW = Utils4D.CreateRotationMatrixYW(rotations.rotationYW * Mathf.Deg2Rad);
        Matrix4x4 matrixZW = Utils4D.CreateRotationMatrixZW(rotations.rotationZW * Mathf.Deg2Rad);

        Matrix4x4 matrix = matrixXY * matrixYZ * matrixZX * matrixXW * matrixYW * matrixZW; // 最終的な4次元回転行列

        // 視点ベクトルの生成
        Vector3 tp = transform.position;    // オブジェクトの位置
        Vector3 cp = viewPoint.position;    // カメラの位置
        Vector3 cu = viewPoint.up;          // カメラの上方向(ワールド)
        Vector3 co = viewPoint.right;

        Vector4 toDir = new Vector4(tp.x, tp.y, tp.z, 0);
        Vector4 fromDir = new Vector4(cp.x, cp.y, cp.z, 0);
        Vector4 upDir = new Vector4(cu.x, cu.y, cu.z, 0);
        Vector4 overDir = new Vector4(co.x, co.y, co.z, 0);

        Utils4D.ProjectTo3DPerspective(Utils4D.tesseractPoints, matrix, ref vertices, fov, fromDir, toDir, upDir, overDir);
    }

    /// <summary>
    /// ワイヤーフレームのサブメッシュを作成
    /// </summary>
    /// <param name="targetMesh">サブメッシュを作成するメッシュインスタンス</param>
    /// <param name="submesh">サブメッシュの番号</param>
    public void GenerateLineMesh(Mesh targetMesh, int submesh = 0) {
        int ti = 0, i = 0, l = Utils4D.tesseractPlanes.GetLength(0);
        int[] indices = new int[8 * l];
        for (; i < l; ++i) {

            indices[ti] = Utils4D.tesseractPlanes[i, 0];
            indices[ti + 1] = Utils4D.tesseractPlanes[i, 1];

            indices[ti + 2] = Utils4D.tesseractPlanes[i, 1];
            indices[ti + 3] = Utils4D.tesseractPlanes[i, 2];

            indices[ti + 4] = Utils4D.tesseractPlanes[i, 2];
            indices[ti + 5] = Utils4D.tesseractPlanes[i, 3];

            indices[ti + 6] = Utils4D.tesseractPlanes[i, 3];
            indices[ti + 7] = Utils4D.tesseractPlanes[i, 0];

            ti += 8;
        }

        targetMesh.SetIndices(indices, MeshTopology.Lines, submesh);
    }

    /// <summary>
    /// サーフェスのサブメッシュを作成
    /// </summary>
    /// <param name="targetMesh">サブメッシュを作成するメッシュインスタンス</param>
    /// <param name="submesh">サブメッシュの番号</param>
    public void GeneratePlaneMesh(Mesh targetMesh, int submesh = 0) {
        int ti = 0, i = 0, l = Utils4D.tesseractPlanes.GetLength(0);
        int[] indices = new int[6 * l];
        for (; i < l; ++i) {

            indices[ti] = Utils4D.tesseractPlanes[i, 0];
            indices[ti + 1] = Utils4D.tesseractPlanes[i, 1];
            indices[ti + 2] = Utils4D.tesseractPlanes[i, 3];

            indices[ti + 3] = Utils4D.tesseractPlanes[i, 1];
            indices[ti + 4] = Utils4D.tesseractPlanes[i, 2];
            indices[ti + 5] = Utils4D.tesseractPlanes[i, 3];

            ti += 6;
        }

        targetMesh.SetIndices(indices, MeshTopology.Triangles, submesh);
    }

}

