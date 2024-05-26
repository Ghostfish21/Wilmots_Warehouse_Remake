using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modules.Tween.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Guiyuu.Gameloop {
    public class StarCollectionManager : MonoBehaviour {
        private static readonly SortedDictionary<float, List<Action>> tasksToExecute = new();

        public static void clear() {
            tasksToExecute.Clear();
        }
        public static void checkAndExecuteTasks(float currentTime) {
            var largerValues = tasksToExecute
                .Where(pair => pair.Key > currentTime)
                .Select(pair => pair);
            
            foreach (KeyValuePair<float,List<Action>> value in new List<KeyValuePair<float, List<Action>>>(largerValues)) {
                float timestamp = value.Key;
                tasksToExecute.Remove(timestamp);
                foreach (Action action in value.Value) {
                    action();
                }
            }
        }
        
        public List<Star> stars;

        private float totalTime;

        private void Start() {
            object[] param = PrefabParameters.getParameters(gameObject);
            float startTime = 0;
            if (param == null) totalTime = Random.Range(50, 80);
            else {
                totalTime = 30;
                startTime = 10;
            }

            float interval = totalTime / stars.Count;

            for (int i = 0; i < stars.Count; i++) {
                float timestamp = interval * i + startTime;
                GameObject star = stars[i].gameObject;
                registerAction(timestamp, () => {
                    if (star == null) return;
                    starFlink(star);
                    stars.Remove(star.GetComponent<Star>());
                });
            }
        }

        private int getRemainStarCount() {
            return stars.Count;
        }

        public void settleStars() {
            int starCount = getRemainStarCount();
            SkipCounterModule.inst.addStar(starCount);
        }

        private void registerAction(float timeToRun, Action actionToRun) {
            if (!tasksToExecute.ContainsKey(timeToRun))
                tasksToExecute[timeToRun] = new List<Action>();
            tasksToExecute[timeToRun].Add(actionToRun);
        }

        public void hideAll() {
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: 1;
                        end-value: 0;
                        duration: 0.375;
                        ease: InOutSine;
                    ");
            tb.setSetter(x => {
                foreach (Star star in stars) {
                    if (star != null)
                        star.spriteRenderer.color = new Color(star.spriteRenderer.color.r, star.spriteRenderer.color.g, star.spriteRenderer.color.b, x);
                }
            }).setOnComplete(() => {
                if (this != null && this.gameObject != null)
                    Destroy(this.gameObject);
            });
            tb.register<float>();
        }

        private void starFlink(GameObject star) {
            StartCoroutine(delayedAction(star));
        }
        
        private IEnumerator delayedAction(GameObject star) {
            yield return new WaitForSeconds(0.35f); 
            star.SetActive(false);
            yield return new WaitForSeconds(0.35f); 
            star.SetActive(true);
            yield return new WaitForSeconds(0.35f); 
            star.SetActive(false);
            yield return new WaitForSeconds(0.35f); 
            star.SetActive(true);
            yield return new WaitForSeconds(0.35f); 
            star.SetActive(false);
        }
    }
}