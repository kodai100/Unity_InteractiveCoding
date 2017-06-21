using UnityEngine;

public static class Utils4D {

    #region Readonly
    public readonly static Vector4[] tesseractPoints = new Vector4[]{
        new Vector4(1,1,1,1), new Vector4(1,1,1,-1),
        new Vector4(1,1,-1,1), new Vector4(1,1,-1,-1),
        new Vector4(1,-1,1,1), new Vector4(1,-1,1,-1),
        new Vector4(1,-1,-1,1), new Vector4(1,-1,-1,-1),
        new Vector4(-1,1,1,1), new Vector4(-1,1,1,-1),
        new Vector4(-1,1,-1,1), new Vector4(-1,1,-1,-1),
        new Vector4(-1,-1,1,1), new Vector4(-1,-1,1,-1),
        new Vector4(-1,-1,-1,1), new Vector4(-1,-1,-1,-1)
    };

    public readonly static int[,] tesseractPlanes = new int[23, 4]{
        {0,1,5,4}, {0,2,6,4}, {0,8,12,4}, {0,2,3,1},
        {0,1,9,8}, {0,2,10,8}, {1,3,7,5}, {1,9,13,5},
        {1,9,11,3}, {2,3,7,6}, {11,10,2,3}, {2,10,14,6},
        {3,11,15,7}, {4,12,13,5}, {4,6,14,12}, {4,6,7,5},
        {5,7,15,13}, {7,6,14,15}, {8,10,14,12}, {8,9,13,12},
        {9,8,10,11}, {9,11,15,13}, {10,11,15,14}
    };
    #endregion ReadOnly

    /// <summary>
    /// 与えられた4次元頂点群を3次元へ透視投影します
    /// </summary>
    /// <param name="points">投影するオブジェクトのローカル頂点座標</param>
    /// <param name="transformMatrix">座標変換行列</param>
    /// <param name="results">投影した座標値を返す</param>
    /// <param name="fov">4次元のFOV</param>
    /// <param name="from">カメラの4次元位置</param>
    /// <param name="to">オブジェクトの4次元位置</param>
    /// <param name="up">カメラの4次元アップベクトル</param>
    /// <param name="right">カメラの4次元ライトベクトル</param>
    public static void ProjectTo3DPerspective(Vector4[] points, Matrix4x4 transformMatrix, ref Vector3[] results, float fov, Vector4 from, Vector4 to, Vector4 up, Vector4 right) {
        Vector4 wa, wb, wc, wd;
        CalculateMatrix(out wa, out wb, out wc, out wd, from, to, up, right);

        float t = 1 / Mathf.Tan((fov * Mathf.Deg2Rad) / 2);

        for (int i = 0; i < points.Length; ++i) {

            Vector4 vp = transformMatrix * points[i];
            Vector4 v = vp - from;                      // 点までの距離
            float s = t / Vector4.Dot(v, wd);

            results[i] = new Vector3(s * Vector4.Dot(v, wa), s * Vector4.Dot(v, wb), s * Vector4.Dot(v, wc));
        }
    }

    /// <summary>
    /// 4次元->3次元プロジェクション変換行列を算出します。
    /// </summary>
    /// <param name="Wa"></param>
    /// <param name="Wb"></param>
    /// <param name="Wc"></param>
    /// <param name="Wd"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="up"></param>
    /// <param name="right"></param>
    public static void CalculateMatrix(out Vector4 Wa, out Vector4 Wb, out Vector4 Wc, out Vector4 Wd, Vector4 from, Vector4 to, Vector4 up, Vector4 right) {
        // 正規化された4次元視線ベクトルを算出
        Wd = to - from;
        float norm = Length4(Wd);
        if (norm == 0) Debug.LogError("To point and From point are the same.");
        Wd *= 1 / norm;

        // Calculate the normalized Wa column-vector.
        Wa = Cross4(up, right, Wd);
        norm = Length4(Wa);
        if (norm == 0) Debug.LogError("Invalid Up vector.");
        Wa *= 1 / norm;

        // Calculate the normalized Wb column-vector.
        Wb = Cross4(right, Wd, Wa);
        norm = Length4(Wb);
        if (norm == 0) Debug.LogError("Invalid Over vector.");
        Wb *= 1 / norm;

        // Calculate the Wc column-vector.
        Wc = Cross4(Wd, Wa, Wb);
    }

    /// <summary>
    /// 4次元ベクトルのクロス積を計算します。
    /// </summary>
    /// <param name="U"></param>
    /// <param name="V"></param>
    /// <param name="W"></param>
    /// <returns></returns>
	public static Vector4 Cross4(Vector4 U, Vector4 V, Vector4 W) {
        float A, B, C, D, E, F;

        A = (V.x * W.y) - (V.y * W.x);
        B = (V.x * W.z) - (V.z * W.x);
        C = (V.x * W.w) - (V.w * W.x);
        D = (V.y * W.z) - (V.z * W.y);
        E = (V.y * W.w) - (V.w * W.y);
        F = (V.z * W.w) - (V.w * W.z);

        Vector4 result = Vector4.zero;
        result.x = (U.y * F) - (U.z * E) + (U.w * D);
        result.y = -(U.x * F) + (U.z * C) - (U.w * B);
        result.z = (U.x * E) - (U.y * C) + (U.z * A);
        result.w = -(U.x * D) + (U.y * B) - (U.z * A);

        return result;
    }

    public static float Length4(Vector4 V) {
        return Mathf.Sqrt(Vector4.Dot(V, V));
    }

    #region RotationMatrix
    /// <summary>
    /// XY平面に対しての回転行列を作成します。
    /// </summary>
    /// <param name="radians">回転の大きさ</param>
    /// <returns></returns>
    public static Matrix4x4 CreateRotationMatrixXY(float radians) {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        Matrix4x4 mat = new Matrix4x4();
        mat[0, 0] = cos;
        mat[0, 1] = sin;
        mat[0, 2] = 0;
        mat[0, 3] = 0;

        mat[1, 0] = -sin;
        mat[1, 1] = cos;
        mat[1, 2] = 0;
        mat[1, 3] = 0;

        mat[2, 0] = 0;
        mat[2, 1] = 0;
        mat[2, 2] = 1;
        mat[2, 3] = 0;

        mat[3, 0] = 0;
        mat[3, 1] = 0;
        mat[3, 2] = 0;
        mat[3, 3] = 1;

        return mat;
    }

    public static Matrix4x4 CreateRotationMatrixYZ(float radians) {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        Matrix4x4 mat = new Matrix4x4();
        mat[0, 0] = 1;
        mat[0, 1] = 0;
        mat[0, 2] = 0;
        mat[0, 3] = 0;

        mat[1, 0] = 0;
        mat[1, 1] = cos;
        mat[1, 2] = sin;
        mat[1, 3] = 0;

        mat[2, 0] = 0;
        mat[2, 1] = -sin;
        mat[2, 2] = cos;
        mat[2, 3] = 0;

        mat[3, 0] = 0;
        mat[3, 1] = 0;
        mat[3, 2] = 0;
        mat[3, 3] = 1;

        return mat;
    }

    public static Matrix4x4 CreateRotationMatrixZX(float radians) {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        Matrix4x4 mat = new Matrix4x4();
        mat[0, 0] = cos;
        mat[0, 1] = 0;
        mat[0, 2] = -sin;
        mat[0, 3] = 0;

        mat[1, 0] = 0;
        mat[1, 1] = 1;
        mat[1, 2] = 0;
        mat[1, 3] = 0;

        mat[2, 0] = sin;
        mat[2, 1] = 0;
        mat[2, 2] = cos;
        mat[2, 3] = 0;

        mat[3, 0] = 0;
        mat[3, 1] = 0;
        mat[3, 2] = 0;
        mat[3, 3] = 1;

        return mat;
    }

    public static Matrix4x4 CreateRotationMatrixXW(float radians) {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        Matrix4x4 mat = new Matrix4x4();
        mat[0, 0] = cos;
        mat[0, 1] = 0;
        mat[0, 2] = 0;
        mat[0, 3] = sin;

        mat[1, 0] = 0;
        mat[1, 1] = 1;
        mat[1, 2] = 0;
        mat[1, 3] = 0;

        mat[2, 0] = 0;
        mat[2, 1] = 0;
        mat[2, 2] = 1;
        mat[2, 3] = 0;

        mat[3, 0] = -sin;
        mat[3, 1] = 0;
        mat[3, 2] = 0;
        mat[3, 3] = cos;

        return mat;
    }

    public static Matrix4x4 CreateRotationMatrixYW(float radians) {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        Matrix4x4 mat = new Matrix4x4();
        mat[0, 0] = 1;
        mat[0, 1] = 0;
        mat[0, 2] = 0;
        mat[0, 3] = 0;

        mat[1, 0] = 0;
        mat[1, 1] = cos;
        mat[1, 2] = 0;
        mat[1, 3] = -sin;

        mat[2, 0] = 0;
        mat[2, 1] = 0;
        mat[2, 2] = 1;
        mat[2, 3] = 0;

        mat[3, 0] = 0;
        mat[3, 1] = sin;
        mat[3, 2] = 0;
        mat[3, 3] = cos;

        return mat;
    }

	public static Matrix4x4 CreateRotationMatrixZW(float radians) {
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        Matrix4x4 mat = new Matrix4x4();
        mat[0, 0] = 1;
        mat[0, 1] = 0;
        mat[0, 2] = 0;
        mat[0, 3] = 0;

        mat[1, 0] = 0;
        mat[1, 1] = 1;
        mat[1, 2] = 0;
        mat[1, 3] = 0;

        mat[2, 0] = 0;
        mat[2, 1] = 0;
        mat[2, 2] = cos;
        mat[2, 3] = -sin;

        mat[3, 0] = 0;
        mat[3, 1] = 0;
        mat[3, 2] = sin;
        mat[3, 3] = cos;

        return mat;
    }

    #endregion RotationMatrix
}

