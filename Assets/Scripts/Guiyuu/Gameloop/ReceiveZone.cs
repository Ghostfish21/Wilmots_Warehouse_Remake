using System.Collections.Generic;
using Guiyuu.GridModule;
using Guiyuu.PlayerAction;
using Modules.Tween.Scripts;
using UnityEngine;

namespace Guiyuu.Gameloop {
    public class ReceiveZone : MonoBehaviour {
        private Dictionary<GridObject, bool> removedGO = new();
        private bool isReceive = false;

        private void OnTriggerEnter2D(Collider2D other) {
            Receive(other);
        }

        private void OnTriggerStay2D(Collider2D other) {
            Receive(other);
        }

        private void Receive(Collider2D other) {
            bool isRemoved = false;

            if (!GateAnimController.inst.status) return;
            if (!other.gameObject.CompareTag("Player")) return;
            if (other is not CircleCollider2D) return;

            Dictionary<string, (int amount, List<PlayerGridPlaceHolder> list)> amountList = new();
            foreach (PlayerGridPlaceHolder pgph in PlayerGrid.inst.getAllPlaceHolders()) {
                string type = pgph.gridObjectInHold.blockName;
                if (!amountList.ContainsKey(type)) amountList[type] = (0, new List<PlayerGridPlaceHolder>());

                int needAmount = Employee.getRequestedAmount(type);
                (int amount, List<PlayerGridPlaceHolder> list) var = amountList[type];

                if (var.amount < needAmount) {
                    var.amount += 1;
                    var.list.Add(pgph);
                    amountList[type] = var;
                }
            }

            foreach ((int amountList, List<PlayerGridPlaceHolder> list) var in amountList.Values) {
                foreach (PlayerGridPlaceHolder pgph in var.list) {
                    GridObject go = pgph.gridObjectInHold;
                    string type = go.blockName;
                    int needAmount = Employee.getRequestedAmount(type);
                    if (needAmount != 0) {
                        (int amount, Employee employee) info = Employee.getRequestedInfo(type);
                        if (pgph != null) {
                            pgph.drop();
                            pgph.cancelTween();
                        }

                        Vector3 endPos = info.employee.gameObject.transform.position;
                        Vector3 startPos = go.transform.position;

                        TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {startPos.x},{startPos.y},1;
                        end-value: {endPos.x},{endPos.y},0;
                        duration: 0.8;
                        ease: InOutSine;
                    ");
                        tb.setSetterX(x => { tb.addProperty("x", x); })
                            .setSetterY(x => {
                                if (go == null) return;
                                if (tb.getProperty("x") == null) return;
                                go.transform.position = new Vector3(float.Parse(tb.getProperty("x")), x);
                            })
                            .setSetterZ(x => {
                                if (go == null) return;
                                go.setAlpha(x);
                            })
                            .setOnCompleteX(() => {
                                go.remove();
                                removedGO.Remove(go);
                            });
                        go.c2d.isTrigger = true;
                        if (removedGO.ContainsKey(go)) continue;
                        tb.register<Vector3>();
                        info.employee.removeDeliverable(type, 1);
                        removedGO[go] = true;
                        isRemoved = true;
                    }
                }
            }

            if (isRemoved)
                SfxController.inst.play("Block Delivered");
        }
    }
}