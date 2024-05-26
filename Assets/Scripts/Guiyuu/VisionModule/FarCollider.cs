using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Guiyuu.GridModule;
using Unity.VisualScripting;
using UnityEngine;

namespace Guiyuu.VisionModule {
    public class FarCollider : MonoBehaviour {
        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.gameObject.CompareTag("MovableBlock")) return;
            GridObject go = GridObject.getGridObject(other.gameObject);
            go.isFar = true;
            farGoList.Add(go);
            go.recalculateVisibility();
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!other.gameObject.CompareTag("MovableBlock")) return;
            if (other.gameObject == null) return;
            GridObject go = GridObject.getGridObject(other.gameObject);
            if (go == null) return;
            go.isFar = false;
            farGoList.Remove(go);
            go.setInvisible();
        }
        
        public static bool refresh = false;
        private readonly List<GridObject> farGoList = new();
        private void FixedUpdate() {
            foreach (GridObject go in farGoList)
                go.recalculateVisibility();
        }
    }
}