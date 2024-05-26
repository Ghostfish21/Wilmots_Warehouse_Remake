using System;
using UnityEngine;

namespace Guiyuu.Gameloop {
    public class Windows : MonoBehaviour {
        private static Windows w;
        public static Windows inst => w;

        private void Start() {
            w = this;
        }

        public GameObject theClock;
        public GameObject cjDisappointed;
        public GameObject serviceHatchOpen;
        public GameObject teaBreak;

        public void exitGame() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void openWindow(string windowName) {
            switch (windowName) {
                case "theClock":
                    theClock.SetActive(true);
                    TimerModule.inst.pause();
                    break;
                case "cjDisappointed":
                    cjDisappointed.SetActive(true);
                    TimerModule.inst.pause();
                    break;
                case "serviceHatchOpen":
                    serviceHatchOpen.SetActive(true);
                    TimerModule.inst.pause();
                    break;
                case "teaBreak":
                    teaBreak.SetActive(true);
                    TimerModule.inst.pause();
                    break;
            }
        }

        public void closeWindows() {
            if (theClock.activeSelf) theClock.SetActive(false);
            if (cjDisappointed.activeSelf) cjDisappointed.SetActive(false);
            if (serviceHatchOpen.activeSelf) serviceHatchOpen.SetActive(false);
            if (teaBreak.activeSelf) teaBreak.SetActive(false);
            TimerModule.inst.resume();
        }

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) openWindow("teaBreak");
        }
    }
}