using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Guiyuu.GridModule;
using Unity.VisualScripting;
using UnityEngine;

namespace Guiyuu.VisionModule {
    public class MiddleCollider : MonoBehaviour {

        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.gameObject.CompareTag("MovableBlock")) return;
            GridObject go = GridObject.getGridObject(other.gameObject);
            go.isMiddle = true;
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!other.gameObject.CompareTag("MovableBlock")) return;
            if (other.gameObject == null) return;
            GridObject go = GridObject.getGridObject(other.gameObject);
            if (go == null) return;
            go.isMiddle = false;
        }
    }
}