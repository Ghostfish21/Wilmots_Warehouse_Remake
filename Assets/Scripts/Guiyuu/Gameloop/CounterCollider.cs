using System;
using Modules.Tween.Scripts;
using UnityEngine;

namespace Guiyuu.Gameloop {
    public class CounterCollider : MonoBehaviour {
        private static CounterCollider cc;
        public static CounterCollider inst => cc;

        private void Start() {
            cc = this;
            uiRt = uiTagRoot.GetComponent<RectTransform>();
        }

        public GameObject uiTagRoot;
        private RectTransform uiRt;

        private int startPos = 200;
        private int endPos = 0;

        private TweenBuilder heightTween = null;
        private bool isOut = false;
        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.gameObject.CompareTag("Employees")) return;

            hide();
        }

        public void hide() {
            if (!isOut) return;
            isOut = false;
            
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {uiRt.anchoredPosition.y};
                        end-value: {startPos};
                        duration: 0.4;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
            tb.setSetter(x => {
                if (tb.getProperty("is-cancelled") == "t") return;
                Vector2 pos = uiRt.anchoredPosition;
                uiRt.anchoredPosition = new Vector2(pos.x, x);
            }).setOnComplete(() => {
                heightTween = null;
            });

            if (heightTween != null) heightTween.addProperty("is-cancelled", "t");
            heightTween = tb;
            tb.register<float>();
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!other.gameObject.CompareTag("Employees")) return;

            if (isOut) return;
            isOut = true;
            if (!DeliverOutPhaseModule.inst.isInPhase) return;
            if (Employee.isAllSatisfied) return;
            
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {uiRt.anchoredPosition.y};
                        end-value: {endPos};
                        duration: 0.4;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
            tb.setSetter(x => {
                if (tb.getProperty("is-cancelled") == "t") return;
                Vector2 pos = uiRt.anchoredPosition;
                uiRt.anchoredPosition = new Vector2(pos.x, x);
            }).setOnComplete(() => {
                heightTween = null;
            });

            if (heightTween != null) heightTween.addProperty("is-cancelled", "t");
            heightTween = tb;
            tb.register<float>();
        }
    }
}