using System;
using Cinemachine.Utility;
using Guiyuu.PlayerAction;
using Guiyuu.VisionModule;
using Modules.Tween.Scripts;
using UnityEngine;

namespace Guiyuu.GridModule {
    public class PlayerObject : MonoBehaviour {
        
        public static PlayerObject player { private set; get; }
        
        // #################### 常数变量 ####################
        // Constant variables that could be assign from inspector
        public float speed = 7f;

        // #################### 私有的属性 ####################
        // Private variables that is only meaningful within the class
        private TweenBuilder tweenBackTweener;
        public Rigidbody2D r2d { private set; get; }

        public Vector3 positionCopy;
        
        private void Start() {
            r2d = GetComponent<Rigidbody2D>();
            player = this;
        }

        private bool moved = false;
        public float accumulatedSlowness { get; private set; } = 0;

        private Vector3 lastPosition;
        private void Update() {
            Vector2 moveDirection = new Vector2(0, 0);
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) 
                moveDirection += new Vector2(0, 1);
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) 
                moveDirection += new Vector2(0, -1);
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) 
                moveDirection += new Vector2( -1, 0);
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) 
                moveDirection += new Vector2(1, 0);
            Vector2 shiftDirectionCopy = new Vector2(0, 0);
            if (moveDirection != new Vector2(0, 0)) {
                if (moveDirection.x == 0) {
                    float xPos = transform.position.x;
                    float roundedUp = Mathf.Round(xPos);
                    if (roundedUp > xPos) 
                        moveDirection.x = 0.05f;
                    else if (roundedUp < xPos)
                        moveDirection.x = -0.05f;
                    shiftDirectionCopy = new Vector2(moveDirection.x, 0);
                }
                else if (moveDirection.y == 0) {
                    float yPos = transform.position.y;
                    float roundedUp = Mathf.Round(yPos);
                    if (roundedUp > yPos) 
                        moveDirection.y = 0.05f;
                    else if (roundedUp < yPos)
                        moveDirection.y = -0.05f;
                    shiftDirectionCopy = new Vector2(0, moveDirection.y);
                }
            }
            moveDirection = moveDirection.normalized;

            if (moveDirection != new Vector2(0, 0)) {

                float factor = 1;
                int holdingCount = PlayerGrid.inst.getSize();
                if (holdingCount is <= 2 and > 0) {
                    factor = 1f;
                    accumulatedSlowness = 0;
                }
                else if (holdingCount is >= 3 and <= 5) {
                    factor = 0.9f;
                    accumulatedSlowness = 0;
                }
                else if (holdingCount == 6) {
                    factor = 0.8f;
                    accumulatedSlowness = 0;
                }
                else if (holdingCount >= 7) {
                    float m(int x) {
                        x -= 6;
                        float output = Mathf.Exp(-x/2f) / 2f + 0.2f;
                        return output;
                    }

                    float defaultFactor = m(holdingCount);
                    factor = Mathf.Lerp(defaultFactor, 0, accumulatedSlowness);
                    accumulatedSlowness += Mathf.Pow(Time.deltaTime, 0.5f) / 40f;
                    accumulatedSlowness = Mathf.Min(accumulatedSlowness, 1);
                }
                
                moved = true;
                FarCollider.refresh = true;
                r2d.AddForce(moveDirection * (speed * factor * Time.deltaTime));

                Vector3 posDiff = (lastPosition - this.transform.position).Abs();
                if (posDiff.x < 0.001f || posDiff.y < 0.001f) {
                    Vector2 displacement = shiftDirectionCopy * (10 * factor * Time.deltaTime);
                    this.transform.position += new Vector3(displacement.x, displacement.y, 0);
                }

                lastPosition = this.transform.position;
                if (tweenBackTweener is null) return;
                tweenBackTweener.addProperty("is-cancelled", "t");
                tweenBackTweener = null;
                GridObject.setPlayerReturnAnimInterrupt(true);
            }
            else {
                if (tweenBackTweener is not null) return;
                if (!moved) return;
                moved = false;
                
                var position = transform.position;

                Vector2Int closestBlockCoord = GridModule.getBlockCoord(position);
                Vector2 snapPos = GridModule.getWorldCoord(closestBlockCoord);

                TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {position.x},{position.y},0;
                        end-value: {snapPos.x},{snapPos.y},0;
                        duration: 0.15;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
                tb.setSetterX(x => {
                        if (tb.getProperty("is-cancelled") == "t") return;
                        r2d.MovePosition(new Vector2(x, position.y));
                        tb.addProperty("last-x", x);
                    })
                    .setSetterY(x => {
                        if (tb.getProperty("is-cancelled") == "t") return;
                        r2d.MovePosition(new Vector2(float.Parse(tb.getProperty("last-x")), x));
                    })
                    .setSetterZ(_ => { })
                    .setOnCompleteX(() => {
                        if (tb.getProperty("is-cancelled") == "f") {
                            tweenBackTweener = null;
                            GridObject.setPlayerReturnAnimInterrupt(false);
                            FarCollider.refresh = true;
                        }
                    });
                tweenBackTweener = tb;
                tweenBackTweener.register<Vector3>();
            }
            
            positionCopy = transform.position;
        }
    }
}