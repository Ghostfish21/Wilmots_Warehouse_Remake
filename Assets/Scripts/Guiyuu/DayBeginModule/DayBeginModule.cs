using System;
using System.Collections.Generic;
using Cinemachine;
using DefaultNamespace;
using Guiyuu.BlockDocument;
using Guiyuu.DayCirculation;
using Guiyuu.Gameloop;
using Guiyuu.GridModule;
using ModuleManager;
using Modules.Tween.Scripts;
using UnityEngine;

namespace Guiyuu.DayBeginModule {
    public class DayBeginModule : Module {
        private static DayBeginModule dbm;
        public static DayBeginModule inst => dbm;
        
        // #################### 组件和游戏对象 ####################
        // Components and GameObjects
        public Transform truck; // 货车的 Transform 组件, Truck's Transform component
        
        // #################### 检视板可调整变量 ####################
        // Inspector adjustable variables
        public float truckDuration = 1f;  // 货车从起点到终点的时间, Time for the truck to move from start to end
        public float truckStartY = -0.5f; // 货车起点的 y 坐标, Truck's start y coordinate
        public float truckEndY = 0.5f; // 货车终点的 y 坐标, Truck's end y coordinate

        public bool isFinished { get; private set; } = false;
        public Transform anchor;
        public CinemachineBrain cb;
        
        public override string getName() {
            return "DayBeginModule";
        }

        public override List<string> getDependencies() {
            return new List<string> {
                "DayCirculationModule"
            };
        }

        public override void construct() {
            dbm = this;
            
            DayCirculationModule.normal.subscribeEventBlock(true, "deliverIn", () => {
                isFinished = false;
                HintBar.inst.setText("Picking up and pushing is disabled during the delivery.");
                HintBar.inst.show();
                unlockNewBlock();
                var blocks = createNewBlocks();
                truckDriveIn(blocks);
            });
        }

        private void unlockNewBlock() {
            BlockDocuments.addAvailableBlock();
            BlockDocuments.addAvailableBlock();
            BlockDocuments.addAvailableBlock();
            BlockDocuments.addAvailableBlock();
        }

        private RandomBlockCollection createNewBlocks() {
            RandomBlockCollection rbc = new RandomBlockCollection(5, 4);
            return rbc;
        }

        private void truckDriveIn(RandomBlockCollection rbc) {
            camTweenToTruck();
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                    start-value: {truck.localPosition.y};
                    end-value: {truckEndY};
                    duration: {truckDuration};
                    ease: InOutSine;"
                ).setSetter(x => {
                    truck.localPosition = new Vector3(truck.localPosition.x, x, truck.localPosition.z);
                })
                .setOnComplete(() => {
                    Invoke(nameof(camTweenToPlayer), 1.5f);
                    pushBlocks1Row(rbc, 0);
                });
            tb.register<float>();
        }

        private void camTweenToTruck() {
            cb.enabled = false;
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                    start-value: 0;
                    end-value: 1;
                    duration: 0.6;
                    ease: InOutSine;"
            ).setSetter(x => {
                Vector3 pos1 = PlayerObject.player.transform.position;
                Vector3 pos2 = anchor.transform.position;
                Vector3 curPos = new Vector3(Mathf.Lerp(pos1.x, pos2.x, x), Mathf.Lerp(pos1.y, pos2.y, x), cb.transform.position.z);
                cb.gameObject.transform.position = curPos;
            });
            tb.register<float>();
        }

        private void camTweenToPlayer() {
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                    start-value: 1;
                    end-value: 0;
                    duration: 0.6;
                    ease: InOutSine;"
            ).setSetter(x => {
                Vector3 pos1 = PlayerObject.player.transform.position;
                Vector3 pos2 = anchor.position;
                Vector3 curPos = new Vector3(Mathf.Lerp(pos1.x, pos2.x, x), Mathf.Lerp(pos1.y, pos2.y, x), cb.transform.position.z);
                cb.gameObject.transform.position = curPos;
            }).setOnComplete(() => {
                cb.enabled = true;
            });
            tb.register<float>();
        }
        
        private void truckDriveAway() {
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                    start-value: {truck.localPosition.y};
                    end-value: {truckStartY};
                    duration: {truckDuration};
                    ease: InOutSine;"
                ).setSetter(x => {
                    truck.localPosition = new Vector3(truck.localPosition.x, x, truck.localPosition.z);
                })
                .setOnComplete(() => {
                    DayCirculationModule.inst.nextEventBlock();
                    isFinished = true;
                    HintBar.inst.hide();
                });
            tb.register<float>();
        }

        private void pushBlocks1Row(RandomBlockCollection rbc, int row) {
            int[] blockIndices = {-3, -2, -1, 0, 1};
            for (int i = 0; i < blockIndices.Length; i++) {
                Vector2Int pos = new Vector2Int(blockIndices[i], -14);
                string blockName = rbc.getBlockAt(i, row);
                this.delay(0.12f * i, () => {
                    push1ColBlocks1Row(pos, new Vector2Int(0, 1), blockName, () => {
                        if (row < 4) pushBlocks1Row(rbc, row + 1);
                        else Invoke(nameof(truckDriveAway), 0.1f);
                    });
                });
            }
        }

        private int succeedTime = 0;
        private void push1ColBlocks1Row(Vector2Int pos, Vector2Int direction, string newBlockName, Action pushNextRow) {
            GridObject go = GridModule.GridModule.inst.getBlockAt(pos);

            void onComplete() {
                succeedTime++;
                GameObject newGo = PrefabParameters.initPrefab(BlockDocuments.blockPrefab, GridModule.GridModule.inst.transform, newBlockName, pos);
                GridModule.GridModule.inst.setBlockAt(pos, newGo.GetComponent<GridObject>());
                
                if (succeedTime != 5) return;
                succeedTime = 0;
                pushNextRow();
            }

            if (go is not null) 
                go.pushForward(direction, onComplete);
            else 
                onComplete();
        }
    }
}