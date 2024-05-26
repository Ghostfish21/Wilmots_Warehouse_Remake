using System;
using Modules.Tween.Scripts;
using TMPro;
using UnityEngine;

namespace Guiyuu.Gameloop {
    public class HintBar : MonoBehaviour {

        private static HintBar hb;
        public static HintBar inst => hb;
        
        public TMP_Text tmpText;
        private RectTransform rt;

        private void Start() {
            hb = this;
        }

        public void setText(string text) {
            tmpText.text = text;
            rt = GetComponent<RectTransform>();
        }

        public void show() {
            if (isOut) return;
            isOut = true;
            
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {this.rt.anchoredPosition.y};
                        end-value: 0;
                        duration: 0.5;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
            tb.setSetter(x => {
                if (tb.getProperty("is-cancelled") == "t") return;
                Vector2 pos = rt.anchoredPosition;
                rt.anchoredPosition = new Vector2(pos.x, x);
            }).setOnComplete(() => {
                heightTween = null;
            });

            if (heightTween != null) heightTween.addProperty("is-cancelled", "t");
            heightTween = tb;
            tb.register<float>();
        }

        private bool isOut;
        private TweenBuilder heightTween = null;

        public void hide() {
            if (!isOut) return;
            isOut = false;
            
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {this.rt.anchoredPosition.y};
                        end-value: {-100 * rt.localScale.y};
                        duration: 0.5;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
            tb.setSetter(x => {
                if (tb.getProperty("is-cancelled") == "t") return;
                Vector2 pos = rt.anchoredPosition;
                rt.anchoredPosition = new Vector2(pos.x, x);
            }).setOnComplete(() => {
                heightTween = null;
            });

            if (heightTween != null) heightTween.addProperty("is-cancelled", "t");
            heightTween = tb;
            tb.register<float>();
        }
    }
}