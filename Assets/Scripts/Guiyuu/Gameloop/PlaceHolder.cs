using UnityEngine;

namespace Guiyuu.Gameloop {
    public class PlaceHolder : MonoBehaviour {
        public ImportantNotice importantNotice;

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.gameObject.CompareTag("Player")) {
                importantNotice.isPlayerInZone = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.gameObject.CompareTag("Player")) {
                importantNotice.isPlayerInZone = false;
            }
        }
    }
}