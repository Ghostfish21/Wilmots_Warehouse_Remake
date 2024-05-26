
using Guiyuu.BlockDocument;
using Guiyuu.DayCirculation;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Guiyuu.Gameloop {
    public class ItemSlot : MonoBehaviour {
        public SpriteRenderer spriteRenderer { get; private set; }
        public Tag tag;
        public TMP_Text text;
        public string type { get; private set; }
        public int blockCount { get; private set; } = -1;

        private void Start() {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void pickRandomItem() {
            if (Employee.secret1.Count >= BlockDocuments.getExistingBlockTypeCount()) return;
            (string name, int count) var = BlockDocuments.getRandomBlock(Employee.secret1);
            int dayCount = DayCirculationModule.dayCount;
            if (dayCount == 1) blockCount = 1;
            else if (dayCount is >= 2 and < 5) {
                int r = Random.Range(0, 10);
                if (r < 3) blockCount = 1;
                else blockCount = 2;
                if (var.count < blockCount) blockCount = var.count;
            }
            else {
                int count = Random.Range(1, var.count) / 3;
                if (count == 0) count = 1;
                blockCount = count;
            }

            Sprite block = BlockDocuments.getBlock(var.name);
            spriteRenderer.sprite = block;
            type = var.name;
            text.text = blockCount + "";

            Employee.addDeliverable(var.name, (blockCount, tag.employee));
        }

        public void clear() {
            blockCount = -1;
            text.text = "";
            spriteRenderer.sprite = null;
        }

        public void removeDeliverable(int amount, params ItemSlot[] slots) {
            if (slots != null && slots.Length == 0) slots = null;

            blockCount -= amount;
            if (blockCount > 0) text.text = blockCount + "";
            else {
                text.text = "";
                spriteRenderer.sprite = null;
                if (slots != null) replaceWith(slots[0], slots, 1);
            }
        }

        private void replaceWith(ItemSlot itemSlot, ItemSlot[] slots, int i) {
            if (i >= slots.Length) return;
            if (slots[i] == null) return;

            type = itemSlot.type;
            blockCount = itemSlot.blockCount;

            if (type == null || blockCount <= 0) {
                spriteRenderer.sprite = null;
                text.text = "";
            }
            else {
                Sprite block = BlockDocuments.getBlock(type);
                spriteRenderer.sprite = block;
                text.text = blockCount + "";
                if (blockCount <= 0) text.text = "";

                itemSlot.type = null;
                itemSlot.spriteRenderer.sprite = null;
                itemSlot.blockCount = 0;
                itemSlot.text.text = "";
            }

            itemSlot.replaceWith(slots[i], slots, i + 1);
        }
    }
}