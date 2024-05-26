using System;
using System.Collections.Generic;
using DefaultNamespace;
using Guiyuu.Gameloop;
using Guiyuu.GridModule;
using ModuleManager;
using UnityEngine;
using UnityEngine.Windows;
using Input = UnityEngine.Input;

namespace Guiyuu.PlayerAction {
    public class PlayerActionModule : Module {

        public GameObject barrierPrefab;
        public GameObject placeHolderPrefab; // 用于保持方块和玩家的相对位置不变的同步器的预设
        
        public override string getName() {
            return "PlayerActionModule";
        }

        public override List<string> getDependencies() {
            return new List<string>();
        }

        public override void construct() {
            
        }

        private void Update() {
            left();
            right();
            wheel();
            qne();
        }

        private void qne() {
            if (Input.GetKeyDown(KeyCode.Q)) {
                wheelTimer.run(() => {
                    rotate(false);
                });
            }
            else if (Input.GetKeyDown(KeyCode.E)) {
                wheelTimer.run(() => {
                    rotate(true);
                });
            }
        }
        
        private Timer wheelTimer = new Timer(200);
        private void wheel() {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0) {
                wheelTimer.run(() => {
                    rotate(false);
                });
            }
            else if (scroll < 0) {
                wheelTimer.run(() => {
                    rotate(true);
                });
            }
        }

        public Transform barriersRoot;
        private void rotate(bool isClockwise) {
            bool success = true;
            foreach (PlayerGridPlaceHolder placeHolder in PlayerGrid.inst.getAllPlaceHolders()) {
                if (!placeHolder.rotateTest(isClockwise)) {
                    success = false;
                    break;
                }
            }

            if (!success) {
                foreach (PlayerGridPlaceHolder placeHolder in PlayerGrid.inst.getAllPlaceHolders()) {
                    Vector2Int pos = placeHolder.getRotatedPos(isClockwise);
                    GameObject go = Instantiate(barrierPrefab, barriersRoot);
                    go.transform.localPosition = new Vector3(pos.x * GridModule.GridModule.inst.cellSize,
                        pos.y * GridModule.GridModule.inst.cellSize, go.transform.position.z);
                }
            }

            else {
                List<PlayerGridPlaceHolder> placeHolders = PlayerGrid.inst.getAllPlaceHolders();
                foreach (PlayerGridPlaceHolder placeHolder in placeHolders) {
                    placeHolder.wipePositionInPlayerGrid();
                }
                
                foreach (PlayerGridPlaceHolder placeHolder in placeHolders) {
                    placeHolder.rotate(isClockwise);
                }

                foreach (PlayerGridPlaceHolder placeHolder in placeHolders) {
                    foreach (PlayerGridPlaceHolder placeHolder1 in placeHolders) 
                        placeHolder1.isChecked = false;
                    placeHolder.connectToPlayer();
                }
                
                if (placeHolders.Count != 0)
                    SfxController.inst.play("Cursor Rotate");
            }
        }

        private bool select = false;
        private void left() {
            // 左键按下时
            if (Input.GetMouseButtonDown(0)) {
                GridObject go = MouseTrigger.inst.getSelection();
                if (go is null) return;
                // 当该物体没有被捡起，也就是，当GameObject下没有PlaceHolder
                Transform t = null;
                if (go.isPickedUp) t = go.transform.GetChild(0);
                if (t is null) {
                    leftSelect(go);
                    select = true;
                }
                else {
                    leftDrop(t);
                    select = false;
                }
            } else if (Input.GetMouseButton(0)) {
                GridObject go = MouseTrigger.inst.getSelection();
                if (go is null) return;
                // 当该物体没有被捡起，也就是，当GameObject下没有PlaceHolder
                Transform t = null;
                if (go.isPickedUp) t = go.transform.GetChild(0);
                if (select) {
                    if (t is null) leftSelect(go);
                }
                else if (t is not null) {
                    leftDrop(t);
                }
            }
        }

        private void right() {
            // 右键按下时
            if (!Input.GetMouseButtonDown(1)) return;
            if (PlayerGridPlaceHolder.isRotating) return;
            bool anything = false;
            foreach (PlayerGridPlaceHolder placeHolder in PlayerGrid.inst.getAllPlaceHolders()) {
                placeHolder.drop();
                anything = true;
            }
            if (anything) SfxController.inst.play("Block Drop");
        }

        private void leftSelect(GridObject go) {
            if (!DayBeginModule.DayBeginModule.inst.isFinished) return;
            if (go.isBeenPushing) return;
            PrefabParameters.initPrefab(placeHolderPrefab, go.transform, go);
        }
        
        private void leftDrop(Transform t) {
            if (PlayerGridPlaceHolder.isRotating) return;
            t.GetComponent<PlayerGridPlaceHolder>().drop();
            SfxController.inst.play("Block Drop");
        }
    }
}