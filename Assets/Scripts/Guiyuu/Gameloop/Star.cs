using System;
using UnityEngine;

namespace Guiyuu.Gameloop {
    public class Star : MonoBehaviour {
        public SpriteRenderer spriteRenderer { get; private set; }

        private void Start() {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}