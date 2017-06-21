using UnityEngine;
using System.Collections;
using System.Collections.Generic;

struct CP {
    public float x;
    public float y;
    public float weight;
}

public class NURBSSpline : MonoBehaviour {

    Vector2 RationalBSplinePoint(List<CP> cps, int degree, float[] KnotVector, float t) {

        float x = 0, y = 0;
        float rationalWeight = 0;

        for (int i = 0; i < cps.Count; i++) {
            float temp = Nip(i, degree, KnotVector, t) * cps[i].weight;
            rationalWeight += temp;
        }

        for (int i = 0; i < cps.Count; i++) {
            float temp = Nip(i, degree, KnotVector, t);
            x += cps[i].x * cps[i].weight * temp / rationalWeight;
            y += cps[i].y * cps[i].weight * temp / rationalWeight;
        }
        return new Vector2(x, y);
    }

    private float Nip(int i, int p, float[] U, float u) {
        float[] N = new float[p + 1];
        float saved, temp;

        int m = U.Length - 1;
        if ((i == 0 && u == U[0]) || (i == (m - p - 1) && u == U[m]))
            return 1;

        if (u < U[i] || u >= U[i + p + 1])
            return 0;

        for (int j = 0; j <= p; j++) {
            if (u >= U[i + j] && u < U[i + j + 1])
                N[j] = 1;
            else
                N[j] = 0;
        }

        for (int k = 1; k <= p; k++) {
            if (N[0] == 0)
                saved = 0;
            else
                saved = ((u - U[i]) * N[0]) / (U[i + k] - U[i]);

            for (int j = 0; j < p - k + 1; j++) {
                float Uleft = U[i + j + 1];
                float Uright = U[i + j + k + 1];

                if (N[j + 1] == 0) {
                    N[j] = saved;
                    saved = 0;
                } else {
                    temp = N[j + 1] / (Uright - Uleft);
                    N[j] = saved + (Uright - u) * temp;
                    saved = (u - Uleft) * temp;
                }
            }
        }
        return N[0];
    }
}