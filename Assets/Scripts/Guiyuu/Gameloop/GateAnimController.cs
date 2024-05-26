using UnityEngine;

namespace Guiyuu.Gameloop {
    public class GateAnimController : MonoBehaviour {
        private static GateAnimController gac;
        public static GateAnimController inst => gac;

        private SpriteRenderer spriteRenderer;

        public Sprite gate0;
        public Sprite gate1;
        public Sprite gate2;
        public Sprite gate3;

        public bool status { get; private set; } = false;

        private void Start() {
            gac = this;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void open() {
            Invoke(nameof(gate_1), 0.1f);
            Invoke(nameof(gate_2), 0.2f);
            Invoke(nameof(gate_3), 0.3f);
            Invoke(nameof(gate_4), 0.4f);
            status = true;
            SfxController.inst.play("Customer Hatch Open");
        }

        public void close() {
            Invoke(nameof(gate_3), 0.1f);
            Invoke(nameof(gate_2), 0.2f);
            Invoke(nameof(gate_1), 0.3f);
            Invoke(nameof(gate_0), 0.4f);
            status = false;
            SfxController.inst.play("Customer Hatch Close");
        }

        private void gate_0() {
            spriteRenderer.sprite = gate0;
        }

        private void gate_1() {
            spriteRenderer.sprite = gate1;
        }

        private void gate_2() {
            spriteRenderer.sprite = gate2;
        }

        private void gate_3() {
            spriteRenderer.sprite = gate3;
        }

        private void gate_4() {
            spriteRenderer.sprite = null;
        }
    }
}