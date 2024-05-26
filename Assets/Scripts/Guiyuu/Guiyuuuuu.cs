using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Guiyuu {
    public class Guiyuuuuu : MonoBehaviour {
        public static Guiyuuuuu inst { get; private set; }
        
        // Start is called before the first frame update
        void Start() {
            inst = this;
        }

        private readonly ConcurrentDictionary<Action, bool> noParamTasksToRun = new();
        public Action scheduleNoParamTaskToMain(Action noParamTask) {
            noParamTasksToRun.TryAdd(noParamTask, true);

            void cancelAction() {
                if (noParamTasksToRun.ContainsKey(noParamTask)) {
                    noParamTasksToRun.Remove(noParamTask, out bool _);
                }
            }

            return cancelAction;
        }
        
        // Update is called once per frame
        void Update() {
            foreach (Action task in new List<Action>(noParamTasksToRun.Keys)) {
                task();
            }
            noParamTasksToRun.Clear();
        }
    }

}