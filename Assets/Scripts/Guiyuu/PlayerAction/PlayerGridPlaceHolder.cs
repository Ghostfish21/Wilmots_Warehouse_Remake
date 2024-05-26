using System;
using System.Collections.Generic;
using Guiyuu.Gameloop;
using Guiyuu.GridModule;
using Guiyuu.VisionModule;
using Modules.Tween.Scripts;
using UnityEngine;

namespace Guiyuu.PlayerAction {
    public class PlayerGridPlaceHolder : MonoBehaviour {
        private static Dictionary<PlayerGridPlaceHolder, TweenBuilder> scaleTweens = new();
        
        public GridObject gridObjectInHold;
        private Vector2Int playerGridPosInt;
        private PlayerObject playerObject;
        private PlayerGridPlaceHolder parentPlaceHolder = null;
        private List<PlayerGridPlaceHolder> childrenPlaceHolder = new();

        private TweenBuilder posTween;
        private void Start() {
            object[] param = PrefabParameters.getParameters(gameObject);
            if (param.Length != 1) return;

            gridObjectInHold = (GridObject)param[0];
            transform.SetSiblingIndex(0);

            // 运行前检查
            if (gridObjectInHold.isPickedUp) {
                Destroy(this.gameObject);
                return;
            }
            if (gridObjectInHold.CompareTag("Wall")) {
                Destroy(this.gameObject);
                return;
            }
            
            this.playerObject = PlayerObject.player;

            // 计算相对玩家的网格位置
            Vector2 distance = transform.position - this.playerObject.transform.position;
            Vector2 playerGridPos = distance / GridModule.GridModule.inst.cellSize;
            // playerGridPosInt = new Vector2Int(Mathf.RoundToInt(playerGridPos.x), Mathf.RoundToInt(playerGridPos.y));
            playerGridPosInt = GridModule.GridModule.getBlockCoord(playerGridPos);
            
            if (!isConnectedToPlayer(playerGridPosInt)) {
                Destroy(this.gameObject);
                return;
            }
            
            Vector2 currentRelativeDist = gridObjectInHold.transform.position - playerObject.transform.position;
            Vector2 relativeDist = new Vector2(playerGridPosInt.x, playerGridPosInt.y) * GridModule.GridModule.inst.cellSize;
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {currentRelativeDist.x},{currentRelativeDist.y},0;
                        end-value: {relativeDist.x},{relativeDist.y},0;
                        duration: 0.15;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
            tb.setSetterX(x => {
                    if (tb.getProperty("is-cancelled") == "t") return;
                    gridObjectInHold.transform.position = new Vector3(x + playerObject.transform.position.x, gridObjectInHold.transform.position.y,
                        gridObjectInHold.transform.position.z);
                })
                .setSetterY(x => {
                    if (tb.getProperty("is-cancelled") == "t") return;
                    gridObjectInHold.transform.position = new Vector3(gridObjectInHold.transform.position.x, x + playerObject.transform.position.y,
                        gridObjectInHold.transform.position.z);
                })
                .setSetterZ(_ => { });
            posTween = tb;
            tb.register<Vector3>();
            
            TweenBuilder tb1 = new TweenBuilder().setProperties($@"
                        start-value: 0;
                        end-value: 1;
                        duration: 1;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
            tb1.setSetter(x => {
                if (tb1.getProperty("is-cancelled") == "t") return;
                if (gridObjectInHold == null) return;
                float var1 = x - 0.5f;
                float var2 = Mathf.Abs(var1);
                float var3 = var2 - 0.5f;
                float var4 = 1 + var3;
                gridObjectInHold.transform.localScale = new Vector3(var4, var4, 1);
            }).setOnComplete(() => { if (scaleTweens.ContainsKey(this)) scaleTweens.Remove(this); });
            
            if (scaleTweens.ContainsKey(this)) 
                scaleTweens[this].addProperty("is-cancelled", "t");
            scaleTweens[this] = tb1;
            tb1.register<float>();
            
            // 执行捡起操作
            gridObjectInHold.pickUp();
            connectToPlayer();
            PlayerGrid.inst.placeGridObject(playerGridPosInt, this);

            foreach (PlayerGridPlaceHolder var in PlayerGrid.inst.getAllPlaceHolders()) {
                var.gridObjectInHold.playBarsStartTween("cancel");
            }
            
            gridObjectInHold.setVisible();
            FarCollider.refresh = true;
            
            SfxController.inst.play("Block Pickup");
        }
        
        // 判断某个坐标上的方块是否相邻与玩家，或者相邻与被玩家拿起的其他方块
        private bool isConnectedToPlayer(Vector2Int position) {
            // 如果方块距离玩家小于等于1单位长度，即，方块在玩家的正东南西北的一格处
            if (Math.Abs(position.magnitude - 1.0f) < 0.05f) return true;
            
            Vector2Int yp = position + new Vector2Int(0, 1);
            Vector2Int ym = position + new Vector2Int(0, -1);
            Vector2Int xp = position + new Vector2Int(1, 0);
            Vector2Int xm = position + new Vector2Int(-1, 0);

            // 如果该方块的上下左右任何格子上存在已经被玩家拿起的方块
            if (PlayerGrid.inst.hasCollider(yp) || PlayerGrid.inst.hasCollider(ym) || PlayerGrid.inst.hasCollider(xp) ||
                PlayerGrid.inst.hasCollider(xm)) return true;

            return false;
        }

        public bool isChecked = false;
        private PlayerGridPlaceHolder findPathToPlayer() {
            if (isChecked) return null;
            isChecked = true;
            
            // 如果方块距离玩家小于等于1单位长度，即，方块在玩家的正东南西北的一格处
            if (Math.Abs(playerGridPosInt.magnitude - 1.0f) < 0.05f) return this;
            
            List<Vector2Int> directions = new() {
                playerGridPosInt + new Vector2Int(0, 1),
                playerGridPosInt + new Vector2Int(0, -1),
                playerGridPosInt + new Vector2Int(1, 0),
                playerGridPosInt + new Vector2Int(-1, 0)
            };

            foreach (Vector2Int direction in directions) {
                if (PlayerGrid.inst.hasCollider(direction)) {
                    PlayerGridPlaceHolder temp = PlayerGrid.inst.getPlaceHolder(direction).findPathToPlayer();
                    if (temp is not null) {
                        if (parentPlaceHolder is not null)
                            parentPlaceHolder.childrenPlaceHolder.Remove(this);
                        parentPlaceHolder = temp;
                        parentPlaceHolder.childrenPlaceHolder.Add(this);
                        return this;
                    }
                }
            }
            
            return null;
        }

        public bool debug;
        public bool connectToPlayer() {
            if (debug) Debug.Log("1");
            // 如果方块距离玩家小于等于1单位长度，即，方块在玩家的正东南西北的一格处
            if (Math.Abs(playerGridPosInt.magnitude - 1.0f) < 0.05f) return true;

            // 如果该方块的上下左右任何格子上存在已经被玩家拿起的方块
            PlayerGridPlaceHolder placeHolder = findPathToPlayer();
            if (placeHolder is not null) return true;
            return false;
        }

        public void drop() {
            if (parentPlaceHolder is not null) 
                parentPlaceHolder.childrenPlaceHolder.Remove(this);
            
            gridObjectInHold.putDown();
            PlayerGrid.inst.removeGridObject(playerGridPosInt);
            
            foreach (PlayerGridPlaceHolder child in new List<PlayerGridPlaceHolder>(childrenPlaceHolder)) {
                child.parentPlaceHolder = null;
                if (!child.connectToPlayer()) child.drop();
            }
            
            Vector2 endPos = new Vector2(gridObjectInHold.blockCoord.x * GridModule.GridModule.inst.cellSize, 
                gridObjectInHold.blockCoord.y * GridModule.GridModule.inst.cellSize);
            Vector3 position = gridObjectInHold.transform.position;
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {position.x},{position.y},0;
                        end-value: {endPos.x},{endPos.y},0;
                        duration: 0.15;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
            tb.setSetterX(x => {
                    if (tb.getProperty("is-cancelled") == "t") return;
                    gridObjectInHold.transform.position = new Vector3(x, gridObjectInHold.transform.position.y,
                        gridObjectInHold.transform.position.z);
                })
                .setSetterY(x => {
                    if (tb.getProperty("is-cancelled") == "t") return;
                    gridObjectInHold.transform.position = new Vector3(gridObjectInHold.transform.position.x, x,
                        gridObjectInHold.transform.position.z);
                })
                .setSetterZ(_ => { });
            
            if (posTween is not null) posTween.addProperty("is-cancelled", "t");
            posTween = tb;
            
            tb.register<Vector3>();
            
            foreach (PlayerGridPlaceHolder var in PlayerGrid.inst.getAllPlaceHolders()) {
                var.gridObjectInHold.playBarsStartTween("override");
            }
            gridObjectInHold.playBarsEndTween("override");
            FarCollider.refresh = true;
            Destroy(this.gameObject);
        }

        public void cancelTween() {
            if (posTween is not null) posTween.addProperty("is-cancelled", "t");
            posTween = null;
        }

        public bool rotateTest(bool isClockwise) {
            Vector2Int newPos = getRotatedPos(isClockwise) + GridModule.GridModule.getBlockCoord(playerObject.transform.position);
            Vector3 pos1 = GridModule.GridModule.getWorldCoord(newPos) +
                           new Vector2(-0.4f, -0.4f) * GridModule.GridModule.inst.cellSize;
            Vector3 pos2 = GridModule.GridModule.getWorldCoord(newPos) +
                           new Vector2(0.4f, 0.4f) * GridModule.GridModule.inst.cellSize;
            Collider2D[] result = Physics2D.OverlapAreaAll(pos1, pos2);

            void debugDraw() {
                Transform debug = gridObjectInHold.transform.Find("Debug");
                if (debug is null) debug = GameObject.Find(gridObjectInHold.gameObject.GetInstanceID() + "").transform;
                debug.name = gridObjectInHold.gameObject.GetInstanceID() + "";
                debug.gameObject.SetActive(true);
                debug.SetParent(null);
                debug.position = (pos2 - pos1) / 2 + pos1;
                debug.localScale = pos2 - pos1;
            }
            // debugDraw();
            
            bool result1 = true;
            foreach (Collider2D c in result) {
                if (c is null) continue;
                if (c.isTrigger) continue;
                if (c.gameObject.CompareTag("Player") || c.gameObject.CompareTag("HeldItem")) continue;
                result1 = false;
                break;
            }

            return result1;
        }

        public Vector2Int getRotatedPos(bool isClockwise) {
            Vector2Int newPos;
            if (isClockwise) newPos = new Vector2Int(playerGridPosInt.y, -playerGridPosInt.x);
            else newPos = new Vector2Int(-playerGridPosInt.y, playerGridPosInt.x);
            return newPos;
        }

        private void Update() {
            isChecked = false;
        }

        public static bool isRotating = false;
        private TweenBuilder rotateTween = null;
        public void rotate(bool isClockwise) {
            Vector2Int newPos = getRotatedPos(isClockwise);
            playerGridPosInt = newPos;
            
            Vector2 relativeDist = new Vector2(playerGridPosInt.x, playerGridPosInt.y) * GridModule.GridModule.inst.cellSize;
            Vector2 currentRelativeDist = gridObjectInHold.transform.position - playerObject.transform.position;
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {currentRelativeDist.x},{currentRelativeDist.y},0;
                        end-value: {relativeDist.x},{relativeDist.y},0;
                        duration: 0.07;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
            tb.setSetterX(x => {
                    if (tb.getProperty("is-cancelled") == "t") return;
                    gridObjectInHold.transform.position = new Vector3(x + playerObject.transform.position.x, gridObjectInHold.transform.position.y,
                        gridObjectInHold.transform.position.z);
                    isRotating = true;
                })
                .setSetterY(x => {
                    if (tb.getProperty("is-cancelled") == "t") return;
                    gridObjectInHold.transform.position = new Vector3(gridObjectInHold.transform.position.x, x + playerObject.transform.position.y,
                        gridObjectInHold.transform.position.z);
                })
                .setSetterZ(_ => { })
                .setOnCompleteX(() => {
                    isRotating = false;
                });
            if (rotateTween is not null) rotateTween.addProperty("is-cancelled", "t");
            rotateTween = tb;
            tb.register<Vector3>();
            
            parentPlaceHolder = null;
            childrenPlaceHolder.Clear();
            PlayerGrid.inst.placeGridObject(playerGridPosInt, this);
            
            foreach (PlayerGridPlaceHolder var in PlayerGrid.inst.getAllPlaceHolders()) {
                var.gridObjectInHold.playBarsStartTween("override");
            }
        }
        
        public void wipePositionInPlayerGrid() {
            PlayerGrid.inst.removeGridObject(playerGridPosInt);
        }
    }
}