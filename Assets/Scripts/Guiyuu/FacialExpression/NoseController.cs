using DefaultNamespace;
using Modules.Tween.Scripts;
using UnityEngine;

namespace Guiyuu.FacialExpression {
    public class NoseController : FacialController {
        // #################### ������ʱ���� ####################
        // Local Temp Variable, Only meaningful within certain methods
        private static readonly Vector2 orbitCenter = new Vector2(0.0025f, 0.05f); // �������Χ����ת��Բ��
        private static readonly float orbitRadius = 0.07f;
        private static readonly float orbitScaleFactor = 2; // �������Χ�Ƶ�Բ�ι����x�����������

        private new void Start() {
            base.Start();
            mouseZoneDetection.registerOnZoneChange(tweenNoseFromAction);
            this.registerOnMovingDirectionChange(checkLookMoveDirection);
        }

        // Update is called once per frame
        private new void Update() {
            base.Update();
            checkLookMousePosition();
            checkLookRandom();
        }

        #region Tweener ����

        private TweenBuilder noseTween = null;

        private void tweenNoseFromAction(string fromZone, string toZone) {
            if (isMoving()) return;
            tweenNose(fromZone, toZone);
        }

        private void tweenNose(string fromZone, string toZone) {
            if (isZoneOpposite(fromZone, toZone)) {
                TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {transform.localScale.x};
                        end-value: -1;
                        duration: 0.075;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
                tb.setSetter(x => {
                    if (tb.getProperty("is-cancelled") == "t") return;
                    transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
                }).setOnComplete(() => {
                    transform.localScale = new Vector3(1, transform.localScale.y, transform.localScale.z);
                    transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y,
                        transform.eulerAngles.z + 180f));
                });

                if (noseTween != null) noseTween.addProperty("is-cancelled", "t");
                noseTween = tb;
                noseTween.register<float>();
            }
            else {
                float toAngle = 0;
                if (toZone == "down") toAngle = 90f;
                if (toZone == "right") toAngle = 180f;
                if (toZone == "up") toAngle = 270f;

                toAngle = findShortestToAngle(transform.eulerAngles.z, toAngle);

                TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {transform.eulerAngles.z},{transform.localScale.x},0;
                        end-value: {toAngle},1,0;
                        duration: 0.1;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
                tb.setSetterX(x => {
                    if (tb.getProperty("is-cancelled") == "t") return;
                    transform.rotation =
                        Quaternion.Euler(new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, x));
                }).setSetterY(x => {
                    if (tb.getProperty("is-cancelled") == "t") return;
                    transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
                }).setSetterZ(x => { });

                if (noseTween != null) noseTween.addProperty("is-cancelled", "t");
                noseTween = tb;
                noseTween.register<Vector3>();
            }
        }

        private TweenBuilder nosePosTween = null;

        private void tweenNosePos(float toDegreeAngle) {
            Vector2 posIncrement = CalculateUnitVector(orbitRadius, toDegreeAngle);
            posIncrement.x /= orbitScaleFactor;
            Vector2 newPos = orbitCenter + posIncrement * orbitRadius;

            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {transform.localPosition.x},{transform.localPosition.y},0;
                        end-value: {newPos.x},{newPos.y},0;
                        duration: 0.15;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
            tb.setSetterX(x => {
                if (tb.getProperty("is-cancelled") == "t") return;
                transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
            }).setSetterY(x => {
                if (tb.getProperty("is-cancelled") == "t") return;
                transform.localPosition = new Vector3(transform.localPosition.x, x, transform.localPosition.z);
            }).setSetterZ(_ => { });

            if (nosePosTween != null) nosePosTween.addProperty("is-cancelled", "t");
            nosePosTween = tb;
            nosePosTween.register<Vector3>();
        }

        private float findShortestToAngle(float fromAngle, float toAngle) {
            float curDist = Mathf.Abs(toAngle - fromAngle);
            float antiToAngle = toAngle - 360f;
            if (toAngle < fromAngle) antiToAngle += 720f;
            float antiDist = Mathf.Abs(antiToAngle - fromAngle);
            if (antiDist < curDist) return antiToAngle;
            return toAngle;
        }

        private bool isZoneOpposite(string fromZone, string toZone) {
            if (fromZone == "up" && toZone == "down") return true;
            if (fromZone == "down" && toZone == "up") return true;
            if (fromZone == "left" && toZone == "right") return true;
            if (fromZone == "right" && toZone == "left") return true;
            return false;
        }

        private string getZoneByAngle(float degreeAngle) {
            Vector2 unitVector = CalculateUnitVector(7f, degreeAngle);
            Vector2 pos = unitVector + new Vector2(transform.position.x, transform.position.y);
            string zone = mouseZoneDetection.getZone(pos);
            return zone;
        }

        #endregion

        #region �����ƶ�Senerios

        private void checkLookBlock() {
        }

        private float i = 1;

        private void checkLookMoveDirection(string lastDirection, string currentDirection) {
            // �� Scenario ͨ�� Facial Controller �ṩ�� �ƶ�������� �¼���ʵ��
            if (!isMoving()) return;

            if (currentDirection == "w") {
                tweenNose(mouseZoneDetection.mouseZone, "up");
                tweenNosePos(90f);
            }

            if (currentDirection == "s") {
                tweenNose(mouseZoneDetection.mouseZone, "down");
                tweenNosePos(270f);
            }

            if (currentDirection == "a") {
                tweenNose(mouseZoneDetection.mouseZone, "left");
                tweenNosePos(180f);
            }

            if (currentDirection == "d") {
                tweenNose(mouseZoneDetection.mouseZone, "right");
                tweenNosePos(0f);
            }
        }

        private void checkLookMousePosition() {
            if (isMoving()) return;
            if (!isMouseActive()) return;

            if (nosePosTween != null) {
                nosePosTween.addProperty("is-cancelled", "t");
                tweenNose(mouseZoneDetection.mouseZone, mouseZoneDetection.mouseZone);
                nosePosTween = null;
            }

            Vector2 mousePosInWorld = getMousePosition();
            Vector2 unitVector = (mousePosInWorld - orbitCenter - getPlayerPos()).normalized;
            Vector2 posIncrement =
                CalculateUnitVector(orbitRadius, CalculateAngleFromUnitVector(unitVector.x, unitVector.y));
            posIncrement.x /= orbitScaleFactor;
            Vector2 newPos = getPlayerPos() + orbitCenter + posIncrement * orbitRadius;

            Vector2 posDiff = newPos - new Vector2(transform.position.x, transform.position.y);
            Vector2 moveDiff = posDiff / 5f;
            transform.position += new Vector3(moveDiff.x, moveDiff.y, 0);
        }

        private Timer lookTimer = new Timer(3000);

        private void checkLookRandom() {
            if (isMoving()) return;
            if (isMouseActive()) return;

            lookTimer.run(() => {
                float randomAngle = (float)(new System.Random((int)(Utility.currentTimeMillis() / 1000)).NextDouble()) *
                                    360;
                string zone = getZoneByAngle(randomAngle);
                tweenNose(mouseZoneDetection.mouseZone, zone);
                tweenNosePos(randomAngle);
            });
        }

        #endregion

        #region �Ƕ�ת������ѧ����

        private static float DegreeToRadian(float angleInDegrees) {
            return Mathf.PI * angleInDegrees / 180.0f;
        }

        public static Vector2 CalculateUnitVector(float radius, float angleInDegrees) {
            // ���Ƕ�ת��Ϊ����
            float angleInRadians = DegreeToRadian(angleInDegrees);

            // ���㵥λ������x��y����
            float x = Mathf.Cos(angleInRadians);
            float y = Mathf.Sin(angleInRadians);

            // ���ص�λ����
            return new Vector2(x, y);
        }

        private static float RadianToDegree(float angleInRadians) {
            return angleInRadians * (180.0f / Mathf.PI);
        }

        public static float CalculateAngleFromUnitVector(float x, float y) {
            // ʹ��Math.Atan2���㻡��
            float angleInRadians = Mathf.Atan2(y, x);

            // ������ת��Ϊ��
            float angleInDegrees = RadianToDegree(angleInRadians);

            // ȷ�����صĽǶ�������
            return angleInDegrees >= 0 ? angleInDegrees : 360 + angleInDegrees;
        }

        #endregion

        private Vector2 getPlayerPos() {
            return facialComponets.player.position;
        }
    }
}