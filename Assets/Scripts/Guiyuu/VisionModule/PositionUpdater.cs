using System;
using Guiyuu.GridModule;
using UnityEngine;

namespace Guiyuu.VisionModule {
    public class PositionUpdater : MonoBehaviour {
        public static Vector2 positionCopy;
        private void Update() {
            transform.position = PlayerObject.player.transform.position;
            positionCopy = transform.position;
        }
    }
}