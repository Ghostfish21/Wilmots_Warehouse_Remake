using Guiyuu.DayCirculation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Guiyuu.Gameloop {
    public class Tag : MonoBehaviour {
        public Employee employee;
        public SpriteRenderer spriteRenderer { get; private set; }
        public ItemSlot slot0;
        public ItemSlot slot1;
        public ItemSlot slot2;

        public UiTag uiTag;

        private void Start() {
            Employee.addTag(this);
            this.spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void start() {
            int dayCount = DayCirculationModule.dayCount;
            if (dayCount <= 2)
                slot0.pickRandomItem();
            else if (dayCount is > 2 and <= 5) {
                int r = Random.Range(0, 10);
                if (r < 4) slot0.pickRandomItem();
                else {
                    slot0.pickRandomItem();
                    slot1.pickRandomItem();
                }
            }
            else {
                int r = Random.Range(0, 20);
                if (r < 6) slot0.pickRandomItem();
                else if (r < 16) {
                    slot0.pickRandomItem();
                    slot1.pickRandomItem();
                }
                else {
                    slot0.pickRandomItem();
                    slot1.pickRandomItem();
                    slot2.pickRandomItem();
                }
            }

            uiTag.setTag(this);
        }

        public void stop() {
            slot0.clear();
            slot1.clear();
            slot2.clear();
            uiTag.setTag(this);
        }

        public void removeDeliverable(string type, int amount) {
            if (slot0.type == type)
                slot0.removeDeliverable(amount, slot1, slot2);
            else if (slot1.type == type)
                slot1.removeDeliverable(amount, slot2);
            else if (slot2.type == type)
                slot2.removeDeliverable(amount);
            uiTag.setTag(this);
        }

        public bool isCleared() {
            if (slot0.blockCount <= 0 && slot1.blockCount <= 0 && slot2.blockCount <= 0) return true;
            return false;
        }
    }
}