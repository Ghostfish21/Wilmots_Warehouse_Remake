using UnityEngine;

namespace Guiyuu.Gameloop {
    public class ImportantNotice : MonoBehaviour {
        public bool isPlayerInZone;
        public Canvas importantNoticeCanvas;

        private void Update() {
            if (!isPlayerInZone) return;

            if (Input.GetKeyDown(KeyCode.Space)) {
                importantNoticeCanvas.gameObject.SetActive(true);
            }
        }
    }
}