using System;
using System.Collections.Generic;
using Guiyuu.GridModule;
using Unity.VisualScripting;
using UnityEngine;

namespace Guiyuu.VisionModule {
    public class CloseCollider : MonoBehaviour {
        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.gameObject.CompareTag("MovableBlock")) return;
            GridObject go = GridObject.getGridObject(other.gameObject);
            go.isClose = true;
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!other.gameObject.CompareTag("MovableBlock")) return;
            if (other.gameObject == null) return;
            GridObject go = GridObject.getGridObject(other.gameObject);
            if (go == null) return;
            go.isClose = false;
        }
    }
}