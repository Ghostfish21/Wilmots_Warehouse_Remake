using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Guiyuu.Gameloop {
    public class UiTag : MonoBehaviour {
        public Image itemSlot1;
        public Image itemSlot2;
        public Image itemSlot3;
        public TMP_Text tmp1;
        public TMP_Text tmp2;
        public TMP_Text tmp3;

        public void setTag(Tag tag) {
            itemSlot1.sprite = tag.slot0.spriteRenderer.sprite;
            itemSlot2.sprite = tag.slot1.spriteRenderer.sprite;
            itemSlot3.sprite = tag.slot2.spriteRenderer.sprite;
            tmp1.text = tag.slot0.text.text;
            tmp2.text = tag.slot1.text.text;
            tmp3.text = tag.slot2.text.text;
        }
    }
}