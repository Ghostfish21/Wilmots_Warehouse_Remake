using System;
using Modules.Tween.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guiyuu.Gameloop {
    public class DateBar : MonoBehaviour {
        private static DateBar dateBar;
        public static DateBar inst => dateBar;

        private void Start() {
            dateBar = this;
            float day = getProgress(monthCount);
            backgroundBar.material.SetFloat("_Progress", day);
        }

        public Image backgroundBar;
        public Image foregroundBar;
        public TMP_Text text;
        public Image star;
        public TMP_Text starCountText;

        public void changePalette(string palette) {
            if (palette == "w") {
                foregroundBar.color = new Color(1, 1, 1, 1);
                Color backgroundColor = new Color(181f / 255f, 175f / 255f, 153f / 255f, 1);
                backgroundBar.material.SetColor("_Color", backgroundColor);
                Color progressColor = new Color(0, 0, 0, 1);
                backgroundBar.material.SetColor("_ProgressColor", progressColor);
                text.color = new Color(0, 0, 0, 1);
                star.color = new Color(0, 0, 0, 1);
                starCountText.color = new Color(0, 0, 0, 1);
            }
            else {
                foregroundBar.color = new Color(0, 0, 0, 0.8f);
                Color backgroundColor = new Color(77f / 255f, 76f / 255f, 71f / 255f, 1);
                backgroundBar.material.SetColor("_Color", backgroundColor);
                Color progressColor = new Color(1, 1, 1, 1);
                backgroundBar.material.SetColor("_ProgressColor", progressColor);
                text.color = new Color(1, 1, 1, 1);
                star.color = new Color(1, 1, 1, 1);
                starCountText.color = new Color(1, 1, 1, 1);
            }
        }

        public int monthCount { get; private set; } = 0;
        private int quarter = 1;
        private int year = 1996;
        private int starCount = 0;

        public void addStar(int star) {
            this.starCount += star;
            starCountText.text = $"{starCount}";
        }

        private TweenBuilder scaleTween = null;
        public void playStarAnim() {
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: 0;
                        end-value: 0.6;
                        duration: 0.2;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
            tb.setSetter(x => {
                if (tb.getProperty("is-cancelled") == "t") return;
                float var1 = x - 0.3f;
                float var2 = -Mathf.Abs(var1);
                float var3 = var2 + 0.3f;
                float var4 = var3 + 0.5f;
                star.transform.localScale = new Vector3(var4, var4, var4);
            }).setOnComplete(() => {
                scaleTween = null;
            });
            if (scaleTween is not null) 
                scaleTween.addProperty("is-cancelled", "t");
            scaleTween = tb;
            tb.register<float>();
        }
        public Vector3 getStarPos() {
            return star.transform.position;
        }
        public void addDay() {
            monthCount++;
            if (monthCount >= 12) {
                year++;
                monthCount = 0;
            }
            if (monthCount is >= 0 and <= 2) quarter = 1;
            else if (monthCount is >= 3 and <= 5) quarter = 2;
            else if (monthCount is >= 6 and <= 8) quarter = 3;
            else quarter = 4;
            
            float day = getProgress(monthCount);
            backgroundBar.material.SetFloat("_Progress", day);

            text.text = $"{getMonth(monthCount)} {year} - Q{quarter}";
        }

        private string getMonth(int month) {
            month %= 12;
            switch (month) {
                case 0: return "January";
                case 1: return "February";
                case 2: return "March";
                case 3: return "April";
                case 4: return "May";
                case 5: return "June";
                case 6: return "July";
                case 7: return "August";
                case 8: return "September";
                case 9: return "October";
                case 10: return "November";
                case 11: return "December";
            }

            return null;
        }

        private float getProgress(int month) {
            month %= 12;
            switch (month) {
                case 0: return 0.08f;
                case 1: return 0.12f;
                case 2: return 0.16f;
                case 3: return 0.215f;
                case 4: return 0.245f;
                case 5: return 0.28f;
                case 6: return 0.35f;
                case 7: return 0.38f;
                case 8: return 0.42f;
                case 9: return 0.485f;
                case 10: return 0.515f;
                case 11: return 0.56f;
            }

            return -1f;
        }
    }
}