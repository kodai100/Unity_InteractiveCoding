using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kodai.Audio.Ribbon {

    public class Manager : MonoBehaviour {

        public static AudioSource audio;
        public int ribbonNum = 50;
        public int band = 1024;

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

            audioData = new float[100][];
            for (int i = 0; i < 100; i++) {
                audioData[i] = new float[band];
                for (int j = 0; j < band; j++) {
                    audioData[i][j] = 0;
                }
            }

        }

        int frame = 0;
        int count = 0;
        float[] fftData = new float[1024];
        void Update() {

            audio.GetOutputData(audioData[0], 1);   // バンド数変えると変更が必要

            // 正規化
            //if(band != 1024) {
            //    for (int i = 0; i < band; i++) {
            //        audioData[0][i] = fftData[i];
            //    }
            //}

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