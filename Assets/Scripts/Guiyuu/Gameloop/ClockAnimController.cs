using UnityEngine;

namespace Guiyuu.Gameloop {
    public class ClockAnimController : MonoBehaviour {
        private static ClockAnimController cac;
        public static ClockAnimController inst => cac;

        public Sprite clock0;
        public Sprite clock1;
        public Sprite clock2;
        private SpriteRenderer spriteRenderer;

        private void Start() {
            cac = this;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void openAndClose() {
            Invoke(nameof(clock_0), 0.5f);
            Invoke(nameof(clock_1), 1f);
            Invoke(nameof(clock_2), 1.5f);
            Invoke(nameof(clock_1), 2f);
            Invoke(nameof(clock_0), 2.5f);
        }

        public void open() {
            Invoke(nameof(clock_0), 0.25f);
            Invoke(nameof(clock_1), 0.5f);
            Invoke(nameof(clock_2), 0.75f);
        }

        public void close() {
            Invoke(nameof(clock_2), 0.25f);
            Invoke(nameof(clock_1), 0.5f);
            Invoke(nameof(clock_0), 0.75f);
        }

        private void clock_0() {
            spriteRenderer.sprite = clock0;
        }

        private void clock_1() {
            spriteRenderer.sprite = clock1;
        }

        private void clock_2() {
            spriteRenderer.sprite = clock2;
        }
    }
}