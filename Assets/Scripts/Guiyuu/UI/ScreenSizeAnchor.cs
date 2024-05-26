using UnityEngine;

namespace Guiyuu.UI {
    public class ScreenSizeAnchor : MonoBehaviour {
        public float widthRatio;
        public float spriteWidth;

        private void Start() {
            float screenWidth = Screen.width;
            float spriteWidthAssume = screenWidth / widthRatio;
            this.GetComponent<RectTransform>().localScale /= spriteWidth / spriteWidthAssume;
        }
    }
}