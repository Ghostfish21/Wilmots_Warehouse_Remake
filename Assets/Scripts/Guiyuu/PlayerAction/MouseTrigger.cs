using System;
using System.Collections.Generic;
using Guiyuu.GridModule;
using UnityEngine;

namespace Guiyuu.PlayerAction {
    public class MouseTrigger : MonoBehaviour {

        private static MouseTrigger mouseTrigger;
        public static MouseTrigger inst => mouseTrigger;

        [SerializeField] private List<GridObject> selection;

        private void Start() {
            mouseTrigger = this;
        }

        // Update is called once per frame
        void Update() {
            if (selection.Count != 0 && selection[0].CompareTag("Wall")) selection = null;
            
            Vector2 mousePos = getMousePosition();
            transform.position = mousePos;
        }
    
        private Vector2 getMousePosition() {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = Camera.main.nearClipPlane;
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            return mouseWorldPosition;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.CompareTag("MovableBlock") || other.gameObject.CompareTag("HeldItem")) 
                selection.Add(other.gameObject.GetComponent<GridObject>());
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.CompareTag("MovableBlock") || other.gameObject.CompareTag("HeldItem"))
                selection.Remove(other.gameObject.GetComponent<GridObject>());
        }

        public GridObject getSelection() {
            if (selection.Count == 0) return null;
            return selection[0];
        }
    }
}
