using System;
using UnityEngine;

namespace Guiyuu.FacialExpression {
    [RequireComponent(typeof(SpriteRenderer))]
    public class FacialController : MonoBehaviour {
        // #################### 其他 Components 的引用 ####################
        // Referece to other Components
        public FacialComponets facialComponets; // Facial Component 主系统的引用
        public MouseZoneDetection mouseZoneDetection; // 检测鼠标的碰撞箱
        private SpriteRenderer renderer; // 显示Sprite的Component

        // #################### 本地临时变量 ####################
        // Local Temp Vacriable, Only meaningful within certain methods
        private Vector2 lastMousePosition = new Vector2(0, 0); // 鼠标上一帧的位置
        private float totalMouseFreezeTime; // 鼠标累计不动的时间
        private const float maxMouseFreezeTime = 1; // 鼠标最多不动的时间
        private bool isMouseAFK; // 鼠标是否挂机

        #region 更新鼠标有无动的状态的私有方法

        private void checkMouseMove() {
            Vector2 curMousePos = getMousePosition();
            if (curMousePos == lastMousePosition)
                totalMouseFreezeTime += Time.deltaTime;
            else totalMouseFreezeTime = 0;

            if (totalMouseFreezeTime > maxMouseFreezeTime)
                isMouseAFK = true;
            else isMouseAFK = false;

            lastMousePosition = curMousePos;
        }

        #endregion

        #region 鼠标有无动的状态的暴露方法

        protected bool isMouseActive() {
            return !isMouseAFK;
        }

        #endregion

        public string lastMovingDirection { get; private set; } = "null"; // 玩家当前的移动朝向
        private bool isCurrentMoving = false;
        private Action<string, string> onMovingDirectionChange; // 玩家移动朝向变化时调用的函数

        #region 更新玩家移动朝向的私有方法

        private void checkPlayerInput() {
            string tempKey = "null";
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) tempKey = "w";
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) tempKey = "s";
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) tempKey = "a";
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) tempKey = "d";
            if (tempKey == "null") {
                isCurrentMoving = false;
                return;
            }

            isCurrentMoving = true;

            if (lastMovingDirection != tempKey)
                onMovingDirectionChange?.Invoke(lastMovingDirection, tempKey);
            lastMovingDirection = tempKey;
        }

        #endregion

        #region 玩家移动朝向的暴露方法

        protected void registerOnMovingDirectionChange(Action<string, string> action) {
            this.onMovingDirectionChange += action;
        }

        #endregion

        public void changeSprite(Sprite sprite) {
            renderer.sprite = sprite;
        }

        protected Vector2 getMousePosition() {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = Camera.main.nearClipPlane;
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            return mouseWorldPosition;
        }

        protected bool isMoving() {
            return isCurrentMoving;
        }

        // Start is called before the first frame update
        protected void Start() {
            renderer = GetComponent<SpriteRenderer>();
        }

        protected void Update() {
            checkMouseMove();
            checkPlayerInput();
        }
    }
}