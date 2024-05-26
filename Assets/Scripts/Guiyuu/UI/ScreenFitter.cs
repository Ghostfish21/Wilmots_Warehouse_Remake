using UnityEngine;

namespace Guiyuu.UI {
    public class ScreenFitter : MonoBehaviour {
        private int screenWidth;
        private int screenHeight;
        public int spriteWidth = 2555;
        public int spriteHeight = 1445;
        private RectTransform rt;

        private void Start() {
            rt = GetComponent<RectTransform>();

            screenWidth = Screen.width;
            screenHeight = Screen.height;

            float spriteRate = spriteWidth / (float)spriteHeight;
            float screenRate = screenWidth / (float)screenHeight;

            float rate = 0;
            if (screenRate > spriteRate) rate = screenHeight / (float)spriteHeight;
            else rate = screenWidth / (float)spriteWidth;
            Vector2 sizeDelta = new Vector2(rate * spriteWidth, rate * spriteHeight);
            Vector2 scale = new Vector2(spriteWidth, spriteHeight) / sizeDelta;
            rt.localScale = new Vector2(1, 1) / scale;
        }
    }
}