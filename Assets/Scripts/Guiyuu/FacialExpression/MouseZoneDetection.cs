using System;
using UnityEngine;

namespace Guiyuu.FacialExpression {
    public class MouseZoneDetection : MonoBehaviour {
        public Collider2D upZone;
        public Collider2D downZone;

        public string mouseZone { get; private set; } = "null";
        private Action<string, string> onZoneChange;

        public void registerOnZoneChange(Action<string, string> action) {
            this.onZoneChange += action;
        }

        // Update is called once per frame
        void Update() {
            Vector2 mousePos = getMousePosition();
            string zone = getZone(mousePos);

            if (mouseZone != zone) {
                onZoneChange?.Invoke(mouseZone, zone);
                mouseZone = zone;
            }
        }

        public string getZone(Vector2 pos) {
            string localMouseZone = "null";
            bool isInZone = false;
            if (upZone.OverlapPoint(pos)) {
                localMouseZone = "up";
                isInZone = true;
            }

            if (downZone.OverlapPoint(pos)) {
                localMouseZone = "down";
                isInZone = true;
            }

            if (!isInZone) {
                if (pos.x < transform.position.x)
                    localMouseZone = "left";
                else localMouseZone = "right";
            }

            return localMouseZone;
        }

        protected Vector2 getMousePosition() {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = Camera.main.nearClipPlane;
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            return mouseWorldPosition;
        }
    }
}