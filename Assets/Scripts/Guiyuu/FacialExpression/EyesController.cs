using DefaultNamespace;
using Modules.Tween.Scripts;
using UnityEngine;

namespace Guiyuu.FacialExpression {
    public class EyesController : FacialController {
        // #################### ������ʱ���� ####################
        // Local Temp Variable, Only meaningful within certain methods
        private static readonly Vector2 orbitCenter = new Vector2(0f, 0.075f); // �������Χ����ת��Բ��
        private static readonly float orbitRadius = 0.075f;
        private static readonly float orbitScaleFactor = 1.2f; // �������Χ�Ƶ�Բ�ι����x�����������

        private new void Start() {
            base.Start();
            this.registerOnMovingDirectionChange(checkLookMoveDirection);
        }

        // Update is called once per frame
        private new void Update() {
            base.Update();
            checkLookMousePosition();
            checkLookRandom();
        }

        #region Tweener ����

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

            if (currentDirection == "w")
                tweenNosePos(90f);

            if (currentDirection == "s")
                tweenNosePos(270f);

            if (currentDirection == "a")
                tweenNosePos(180f);

            if (currentDirection == "d")
                tweenNosePos(0f);
        }

        private void checkLookMousePosition() {
            if (isMoving()) return;
            if (!isMouseActive()) return;

            if (nosePosTween != null)
                nosePosTween.addProperty("is-cancelled", "t");

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