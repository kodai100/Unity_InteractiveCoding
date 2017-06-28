using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kodai.Audio.Ribbon {

    public class Manager : MonoBehaviour {

        public static AudioSource audio;
        public int ribbonNum = 50;
        [Range(10, 1024)] public int band = 1024;
        public Vector2 beginEndPosX = new Vector2(0, 10);

        public float decay = 0.8f;

        public GameObject ribbonPrefab;

        Ribbon[] ribbons;

        public static float[][] audioData;

        void Start() {
            audio = GetComponent<AudioSource>();

            ribbons = new Ribbon[ribbonNum];
            for (int i = 0; i < ribbons.Length; i++) {
                GameObject o = Instantiate(ribbonPrefab, new Vector3(0, 0, i*2), Quaternion.identity, transform);
                ribbons[i] = o.GetComponent<Ribbon>();
                ribbons[i].id = i;
                ribbons[i].manager = this;

                o.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(0, 0.8f - (float)(i+1)/ribbonNum, 1 - (float)(i + 1) / ribbonNum));
                if(i==0) o.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.white);
            }

            // バンド幅を動的変更できるように1024分の配列を確保しておく
            audioData = new float[ribbonNum][];
            for (int i = 0; i < ribbonNum; i++) {
                audioData[i] = new float[1024]; // 1024確保
                for (int j = 0; j < 1024; j++) {
                    audioData[i][j] = 0;
                }
            }

        }

        int frame = 0;
        int count = 0;
        float[] fftData = new float[1024];
        void Update() {

            audio.GetOutputData(fftData, 1);

            // 正規化
            if (band != 1024) {
                for (int i = 0; i < band; i++) {
                    audioData[0][i] = fftData[i];
                }
            } else {
                if(band > 1024) {
                    Debug.Log("Invalid Band Length");
                    band = 1024;
                } else {
                    fftData.CopyTo(audioData[0], 0);
                }
            }

            // 手前のデータをコピー
            // 昇順じゃダメ
            for (int i = count; i > 0; i--) {
                audioData[i - 1].CopyTo(audioData[i], 0);   // ミソ
            }

            frame++;

            count = frame / 20;
            if (count > ribbonNum - 1) count = ribbonNum - 1;
        }
    }
}