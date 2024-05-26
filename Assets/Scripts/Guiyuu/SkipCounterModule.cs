using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Guiyuu.Gameloop;
using ModuleManager;
using Modules.Tween.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Guiyuu {
    public class SkipCounterModule : Module {
        private static SkipCounterModule skipCounterModule;
        public static SkipCounterModule inst => skipCounterModule;

        public List<Star> upStars = new();
        private StarCollectionManager downScm = null;

        public Canvas starCanvas;

        public GameObject threeBPrefab;
        public GameObject starPrefab;

        public bool isSkipAllowed = false;
        public bool isPlayerInZone = false;

        public TimerModule timerModule;

        public override void construct() {
            skipCounterModule = this;
        }

        private float worldStarWidth = 0.48f * 0.2524f;
        private float screenWidthDefault = 752;
        private List<Star> earnedStars = new();
        private int currentCur = 0;

        public void addStar(int amount) {
            for (int i = currentCur; i < currentCur + amount; i++) {
                if (upStars.Count <= i) return;
                Star star = upStars[i];
                earnedStars.Add(star);
                star.gameObject.SetActive(true);
            }

            currentCur += amount;
        }

        public void settleAllStars() {
            foreach (Star star in new List<Star>(downScm.stars))
                earnedStars.Add(star);

            float screenWidthRate = Screen.width / screenWidthDefault;

            List<GameObject> canvasStars = new();
            foreach (Star star in earnedStars) {
                if (Employee.isAllSatisfied) {
                    GameObject canvasStar = Instantiate(starPrefab, starCanvas.transform);
                    Vector3 screenPoint = Camera.main.WorldToScreenPoint(star.transform.position);
                    canvasStar.transform.position = screenPoint;
                    canvasStar.transform.localScale *= screenWidthRate;
                    canvasStars.Add(canvasStar);
                }
                star.gameObject.SetActive(false);
            }

            float xCenter = 0;
            float yCenter = 0;
            if (canvasStars.Count != 0) {
                xCenter = canvasStars.Select(c => c.transform.position.x).Average();
                yCenter = canvasStars.Select(c => c.transform.position.y).Average();
            }

            int i = 0;
            if (canvasStars.Count != 0)
                SfxController.inst.play("Final Customer Leaves");

            foreach (GameObject star in canvasStars) {
                i++;
                Vector2 endPos =
                    (new Vector2(star.gameObject.transform.position.x, star.gameObject.transform.position.y)
                     - new Vector2(xCenter, yCenter)) * 2f + new Vector2(xCenter, yCenter);
                float endScale = 3f * star.transform.localScale.x;

                TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {star.transform.position.x},{star.transform.position.y},{star.transform.localScale.x};
                        end-value: {endPos.x},{endPos.y},{endScale};
                        duration: 0.7;
                        ease: InOutSine;
                    ");
                var i1 = i;
                tb.setSetterX(x => {
                    star.transform.position = new Vector3(x, star.transform.position.y, star.transform.position.z);
                }).setSetterY(x => {
                    star.transform.position = new Vector3(star.transform.position.x, x, star.transform.position.z);
                }).setSetterZ(x => { star.transform.localScale = new Vector3(x, x, x); }).setOnCompleteX(() => {
                    StartCoroutine(playPositionAnim(star, i1 * 0.4f));
                });

                tb.register<Vector3>();
            }
        }

        private IEnumerator playPositionAnim(GameObject canvasStar, float waitTime) {
            yield return new WaitForSeconds(waitTime);

            Vector3 endPos = DateBar.inst.getStarPos();

            int random = Random.Range(0, 10);
            TweenBuilder tb1 = new TweenBuilder().setProperties($@"
                        start-value: {canvasStar.transform.position.x},{canvasStar.transform.position.y},0;
                        end-value: {endPos.x},{endPos.y},0;
                        duration: 0.35;
                    ");
            tb1.setSetterX(x => {
                if (canvasStar != null)
                    canvasStar.transform.position = new Vector3(x, canvasStar.transform.position.y,
                        canvasStar.transform.position.z);
            }).setSetterY(x => {
                if (canvasStar != null)
                    canvasStar.transform.position = new Vector3(canvasStar.transform.position.x, x,
                        canvasStar.transform.position.z);
            }).setSetterZ(_ => { }).setEaseZ(new InOutSine()).setOnCompleteX(() => {
                Destroy(canvasStar);
                Guiyuuuuu.inst.scheduleNoParamTaskToMain(() => { DateBar.inst.playStarAnim(); });
                DateBar.inst.addStar(1);
            });

            if (random < 5) tb1.setEaseX(new InQuad()).setEaseY(new OutQuad());
            else tb1.setEaseX(new OutQuad()).setEaseY(new InQuad());

            tb1.register<Vector3>();
        }

        public void makeDownStars() {
            if (downScm is not null) {
                Destroy(downScm.gameObject);
                earnedStars.Clear();
                currentCur = 0;
                foreach (Star star in upStars)
                    star.gameObject.SetActive(false);
            }

            GameObject scm = PrefabParameters.initPrefab(threeBPrefab, transform);
            scm.transform.localPosition =
                new Vector3(scm.transform.localPosition.x, -0.756f, scm.transform.localPosition.z);
            downScm = scm.GetComponent<StarCollectionManager>();
        }

        public override List<string> getDependencies() {
            return new List<string>();
        }

        public override string getName() {
            return "SkipCounterModule";
        }

        // Update is called once per frame
        void Update() {
            if (!isSkipAllowed) return;
            if (!isPlayerInZone) return;
            if (timerModule.isToRunEmpty()) return;

            if (Input.GetKeyDown(KeyCode.Space)) {
                timerModule.killTimer();
            }
        }

        private void OnTriggerStay2D(Collider2D other) {
            if (!isSkipAllowed) return;
            if (!isPlayerInZone) return;
            if (timerModule.isToRunEmpty()) return;
            if (timerModule.isPaused) return;

            if (Input.GetMouseButtonDown(0)) {
                timerModule.killTimer();
            }
        }
    }
}