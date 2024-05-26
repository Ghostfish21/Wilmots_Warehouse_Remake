using UnityEngine;

namespace Guiyuu.Gameloop {
    public class SkipCounterPlaceHolder : MonoBehaviour {
        public SkipCounterModule skipCounterModule;

        private bool isFirstEnter = false;

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.gameObject.CompareTag("Player")) {
                skipCounterModule.isPlayerInZone = true;
                if (!isFirstEnter) {
                    isFirstEnter = true;
                    Windows.inst.openWindow("theClock");
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.gameObject.CompareTag("Player")) {
                skipCounterModule.isPlayerInZone = false;
            }
        }
    }
}