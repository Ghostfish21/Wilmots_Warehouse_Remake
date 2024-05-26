using System;
using System.Collections.Generic;
using Guiyuu.Gameloop;
using ModuleManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guiyuu {
    public class TimerModule : Module {
        private static TimerModule tm;
        public static TimerModule inst => tm;

        public Sprite blackBar;
        public Sprite whiteBar;
        public string format;
        private string format2 = "You served everyone with {x} seconds leftHighly commendable performance.";
        public TMP_Text text;
        public TMP_Text text2;
        public Image progressBarBackground;
        public Image progressBar;

        public DateBar dateBar;

        private RectTransform progressBarRectTransform;
        private Image selfImage;

        public override void construct() {
            tm = this;
            selfImage = GetComponent<Image>();
            progressBarRectTransform = progressBar.GetComponent<RectTransform>();

            switchFormat(" ");
            switchPalette("b");
        }

        public override List<string> getDependencies() {
            return new List<string>();
        }

        public override string getName() {
            return "TimerModule";
        }

        private float countDown;
        private float counting;
        private Action toRun;

        public bool isPaused { get; private set; } = false;

        public void pause() {
            isPaused = true;
        }

        public void resume() {
            isPaused = false;
        }

        public void createTimer(float countDown, Action toRun) {
            if (countDown == 0) return;
            if (this.toRun != null) return;
            this.countDown = countDown;
            this.counting = countDown;
            this.toRun = toRun;
        }

        public void switchPalette(string palette) {
            if (palette == "w") {
                selfImage.sprite = whiteBar;
                selfImage.color = new Color(1, 1, 1, 1);
                text.color = new Color(0, 0, 0, 1);
                progressBarBackground.color = new Color(181f / 255f, 175f / 255f, 153f / 255f, 1);
                progressBar.color = new Color(0, 0, 0, 1);
                dateBar.changePalette("w");
            }
            else {
                selfImage.sprite = blackBar;
                selfImage.color = new Color(1, 1, 1, 0.851f);
                text.color = new Color(1, 1, 1, 1);
                progressBarBackground.color = new Color(77f / 255f, 76f / 255f, 71f / 255f, 1);
                progressBar.color = new Color(1, 1, 1, 1);
                dateBar.changePalette("b");
            }
        }

        public void switchFormat(string format) {
            if (format == "d")
                this.format = "DELIVERY PHASE: {m}m {s}s LEFT";
            else if (format == "s")
                this.format = "SERVICE PHASE: {m}m {s}s LEFT";
            else if (format == "a")
                this.format = "STOCK TAKE";
            else
                this.format = "";
            this.text.text = this.format;
        }

        public void killTimer() {
            Action temp = toRun;
            toRun = null;
            temp();
        }

        public bool isToRunEmpty() {
            return toRun == null;
        }

        // Update is called once per frame
        void Update() {
            if (toRun == null) return;

            float time = Time.deltaTime;
            if (!isPaused)
                counting -= time;

            int minute = (int)(counting / 60);
            int second = (int)(counting % 60);
            text.text = this.format.Replace("{m}", minute + "").Replace("{s}", second + "");
            text2.text = format2.Replace("{x}", second + minute * 60 + "");

            float timeIn1 = counting / countDown;
            progressBarRectTransform.sizeDelta = new Vector2(timeIn1 * 100f, 100);

            if (countDown == 90f) {
                StarCollectionManager.checkAndExecuteTasks(counting);
            }

            if (counting < 0)
                killTimer();
        }
    }
}