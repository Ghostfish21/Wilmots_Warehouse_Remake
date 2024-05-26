using System;
using UnityEngine;

namespace Guiyuu.PlayerAction {
    public class Barrier : MonoBehaviour {

        private void Start() {
            Invoke(nameof(destroy), 0.35f);
        }

        private void destroy() {
            Destroy(this.gameObject);
        }

    }
}