using System;
using System.Collections;
using UnityEngine;

namespace DefaultNamespace {
    public static class Delay {
        // public static void run(MonoBehaviour mono, float delayTime, Action action) {
        //     mono.StartCoroutine(DelayCoroutine(action, delayTime));
        // }
        
        public static void delay(this MonoBehaviour mono, float delayTime, Action action) {
            mono.StartCoroutine(DelayCoroutine(action, delayTime));
        }

        private static IEnumerator DelayCoroutine(Action action, float delayTime) {
            yield return new WaitForSeconds(delayTime);
            action.Invoke();
        }
    }
}