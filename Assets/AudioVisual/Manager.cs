using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kodai.Audio.Ribbon {

    public class Manager : MonoBehaviour {

        public static AudioSource audio;
        public static int ribbonNum = 50;
        public static int band = 1024;

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

                o.GetComponent<MeshRenderer>().material.SetColor("_Color", new Color(0, 0.8f - (float)(i+1)/ribbonNum, 1 - (float)(i + 1) / ribbonNum));
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
        void Update() {

            audio.GetOutputData(audioData[0], 1);   // バンド数変えると変更が必要

            // 手前のデータをコピー
            // 昇順じゃダメ
            for (int i = count; i > 0; i--) {
                audioData[i - 1].CopyTo(audioData[i], 0);   // ミソ
            }

            frame++;

            count = frame / 20;
            if (count > ribbonNum) count = ribbonNum;
        }
    }
}