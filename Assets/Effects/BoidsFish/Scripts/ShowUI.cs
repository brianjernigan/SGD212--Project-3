using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SDQQ1234
{
    public class ShowUI : MonoBehaviour
    {
        public Text fpsUI;

        float deltaTime = 0.0f;
        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            fpsUI.text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        }

        // Start is called before the first frame update
        void Start()
        {
            QualitySettings.vSyncCount = 0;  // Disable VSync
            Application.targetFrameRate = 240;
        }
    }
}

