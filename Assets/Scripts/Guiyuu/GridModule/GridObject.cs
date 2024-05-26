using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Guiyuu.BlockDocument;
using Guiyuu.Gameloop;
using Guiyuu.PlayerAction;
using Modules.Tween.Scripts;
using Unity.VisualScripting;
using UnityEngine;

namespace Guiyuu.GridModule {
    public class GridObject : MonoBehaviour {

        
        private static readonly Dictionary<GameObject, GridObject> gridObjects = new();
        public static GridObject getGridObject(GameObject go) {
            if (gridObjects.ContainsKey(go)) return gridObjects[go];
            return null;
        }
        private static Sprite getSprite(string blockName) {
            return BlockDocuments.getBlock(blockName);
        }

        // #################### 公开属性 ####################
        // Public variables that could be assign from outside
        public Vector2Int blockCoord; // 这个 GridObject 的网格坐标
        #region 暴露的网格坐标方法

        public void setPosAndRecord(Vector2Int newPos) {
            if (GridModule.inst.getBlockAt(newPos) is not null) return;
            GridModule.inst.setBlockAt(newPos, this);
            blockCoord = newPos;
            updateWorldCoord();
        }
        public void updateWorldCoord() {
            r2d.MovePosition(GridModule.getWorldCoord(blockCoord));
        }
        #endregion
        public string blockName; // 这个 GridObject 的方块种类
        
        // #################### 私有变量 ####################
        // Private variables that is only meaningful within the class
        private SpriteRenderer spriteRenderer;

        public void setAlpha(float a) {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g
                , spriteRenderer.color.b, a);
        }
        public Collider2D c2d { get; private set; }
        public Rigidbody2D r2d { get; private set; }
        public Transform topBar;
        public Transform botBar;
        public Transform leftBar;
        public Transform rightBar;
        
        // #################### 私有属性 ####################
        // Private variables that hold status of the game object
        public bool isPickedUp { get; private set; } = false;
        #region 捡起和放下暴露的方法

        public void remove() {
            GridModule.inst.removeBlockAt(blockCoord);
            checkAdjacentAir();
            gridObjects.Remove(gameObject);
            if (blockName != "Wall") BlockDocuments.minusBlockCount(blockName, 1);
            Destroy(gameObject);
        }
        public GridObject pickUp() {
            if (isPickedUp) return this;
            isPickedUp = true;
            
            GridModule.inst.removeBlockAt(blockCoord);
            checkAdjacentAir();
            
            transform.SetParent(PlayerObject.player.transform);
            GetComponent<BoxCollider2D>().isTrigger = true;

            gameObject.tag = "HeldItem";
            
            return this;
        }

        public bool canPutDown() {
            if (GridModule.inst.getBlockAt(blockCoord) is not null) return false;
            return true;
        }

        public bool canPushForward(Vector2Int direction) {
            if (blockName == "Wall") return false;
            
            Vector2Int newPos = direction + blockCoord;
            if (newPos.y <= -15) return false;
            GridObject block = GridModule.inst.getBlockAt(newPos);
            if (block is null) return true;
            return block.canPushForward(direction);
        }

        public bool isBeenPushing { get; private set; }
        public void pushForward(Vector2Int direction, Action onComplete = null) {
            if (!canPushForward(direction)) return;
            
            Vector2Int newPos = direction + blockCoord;
            Vector2Int oldPos = blockCoord;
            GridObject block = GridModule.inst.getBlockAt(newPos);
            if (block is not null) block.pushForward(direction);
            
            Vector2 worldCoord = GridModule.getWorldCoord(newPos);
            
            TweenBuilder posTween = new TweenBuilder().setProperties($@"
                        start-value: {transform.position.x},{transform.position.y},0;
                        end-value: {worldCoord.x},{worldCoord.y},0;
                        duration: 0.25;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
            posTween.setSetterX(x => {
                    if (posTween.getProperty("is-cancelled") == "t") return;
                    r2d.MovePosition(new Vector2(x, transform.position.y));
                    posTween.addProperty("last-x", x);
                    isBeenPushing = true;
                })
                .setSetterY(x => {
                    if (posTween.getProperty("is-cancelled") == "t") return;
                    r2d.MovePosition(new Vector2(float.Parse(posTween.getProperty("last-x")), x));
                })
                .setSetterZ(_ => { })
                .setOnCompleteX(() => {
                    isBeenPushing = false;
                    GridModule.inst.removeBlockAt(oldPos);
                    this.blockCoord = GridModule.getBlockCoord(transform.position);
                    GridModule.inst.setBlockAt(this.blockCoord, this);
                    checkAdjacentAir(oldPos);
                    checkAdjacentAir();
                    checkAir();
                    
                    onComplete?.Invoke();
                });
            posTween.register<Vector3>();
        }

        public void putDown() {
            this.blockCoord = GridModule.getBlockCoord(transform.position);
            GridModule.inst.setBlockAt(this.blockCoord, this);
            checkAir();
            checkAdjacentAir();
            
            transform.SetParent(GridModule.inst.transform);
            GetComponent<BoxCollider2D>().isTrigger = false;
            
            gameObject.tag = "MovableBlock";
            
            isPickedUp = false;
        }
        
        #endregion
        private bool isTopAir = false;
        private bool isBottomAir = false;
        private bool isLeftAir = false;
        private bool isRightAir = false;
        private bool isTopLeftAir = false;
        private bool isTopRightAir = false;
        private bool isBotLeftAir = false;
        private bool isBotRightAir = false;
        #region 检查周围是否有空气的方法
        public void checkAir() {
            isTopAir = GridModule.inst.getBlockAt(blockCoord + new Vector2Int(0, -1)) is null;
            isBottomAir = GridModule.inst.getBlockAt(blockCoord + new Vector2Int(0, 1)) is null;
            isLeftAir = GridModule.inst.getBlockAt(blockCoord + new Vector2Int(-1, 0)) is null;
            isRightAir = GridModule.inst.getBlockAt(blockCoord + new Vector2Int(1, 0)) is null;
            isTopLeftAir = GridModule.inst.getBlockAt(blockCoord + new Vector2Int(-1, -1)) is null;
            isTopRightAir = GridModule.inst.getBlockAt(blockCoord + new Vector2Int(1, -1)) is null;
            isBotLeftAir = GridModule.inst.getBlockAt(blockCoord + new Vector2Int(-1, 1)) is null;
            isBotRightAir = GridModule.inst.getBlockAt(blockCoord + new Vector2Int(1, 1)) is null;
        }
        public void checkAdjacentAir(Vector2Int? blockCoord1 = null) {
            Vector2Int blockCoord2 = this.blockCoord;
            if (blockCoord1 is not null) blockCoord2 = blockCoord1.Value;
            
            GridObject topBlock = GridModule.inst.getBlockAt(blockCoord2 + new Vector2Int(0, 1));
            if (topBlock is not null) topBlock.checkAir();
            GridObject bottomBlock = GridModule.inst.getBlockAt(blockCoord2 + new Vector2Int(0, -1));
            if (bottomBlock is not null) bottomBlock.checkAir();
            GridObject leftBlock = GridModule.inst.getBlockAt(blockCoord2 + new Vector2Int(-1, 0));
            if (leftBlock is not null) leftBlock.checkAir();
            GridObject rightBlock = GridModule.inst.getBlockAt(blockCoord2 + new Vector2Int(1, 0));
            if (rightBlock is not null) rightBlock.checkAir();
            
            GridObject trBlock = GridModule.inst.getBlockAt(blockCoord2 + new Vector2Int(1, 1));
            if (trBlock is not null) trBlock.checkAir();
            GridObject brBlock = GridModule.inst.getBlockAt(blockCoord2 + new Vector2Int(1, -1));
            if (brBlock is not null) brBlock.checkAir();
            GridObject blBlock = GridModule.inst.getBlockAt(blockCoord2 + new Vector2Int(-1, -1));
            if (blBlock is not null) blBlock.checkAir();
            GridObject tlBlock = GridModule.inst.getBlockAt(blockCoord2 + new Vector2Int(-1, 1));
            if (tlBlock is not null) tlBlock.checkAir();
        }
        private float outputBool(bool status) {
            if (status) return 1f;
            return 0f;
        }
        #endregion
        
        // #################### 视野范围属性 ####################
        // Vision range variables
        public float angleUnit = 3.0f;
        public bool isClose = false; // 该方块是否在近视野 Collider 内
        public bool isFar = false; // 该方块是否在远视野 Collider 内
        public bool isMiddle = false;
        private Bounds spriteBound;
        private Sprite color;
        private Sprite outline;
        private float visibility = 2; // 0为清晰可见，1为轮廓可见，2为网格可见
        private TweenBuilder visibilityTween = null;
        private Action cancelTweenRegister = null;
        private float distance2Player = 10000;
        private Vector3 positionCopy;

        private static readonly ConcurrentDictionary<int, GridObject> angle2Go = new();
        public static void recalculateVisibilityPrep() {
            angle2Go.Clear();
        }

        public void setInvisible() {
            tweenVisibility(2);
        }
        public void setVisible() {
            tweenVisibility(0);
        }

        public static void recalculateVisibilityFinal() {
            foreach (GridObject go in angle2Go.Values) {
                go.tweenVisibility(0);
            }
        }
        
        #region 视野范围方法

        private float dist2Player() {
            if (Math.Abs(distance2Player - 10000) > 0.1f) return distance2Player;
            distance2Player = (positionCopy - PlayerObject.player.positionCopy).magnitude;
            return distance2Player;
        }

        private Vector2 lowerLeftCorner => spriteBound.min;
        private Vector2 upperRightCorner => spriteBound.max;
        private Vector2 lowerRightCorner => new Vector2(spriteBound.max.x, spriteBound.min.y);
        private Vector2 upperLeftCorner => new Vector2(spriteBound.min.x, spriteBound.max.y);

        private float previousTarget = -1;
        private void tweenVisibility(float target) {
            if (cancelTweenRegister is not null) cancelTweenRegister();

            void a() {
                TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {visibility};
                        end-value: {target};
                        duration: 0.3;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
                tb.setSetter(x => {
                        if (tb.getProperty("is-cancelled") == "t") return;
                        visibility = x;
                    })
                    .setOnComplete(() => {
                        if (visibilityTween == tb) visibilityTween = null;
                    });
                if (visibilityTween is not null) visibilityTween.addProperty("is-cancelled", "t");
                visibilityTween = tb;
                tb.register<float>();
            }

            if (target != previousTarget)
                a();
            previousTarget = target;
        }
        
        public void recalculateVisibility() {
            if (isClose) {
                tweenVisibility(0);
                return;
            }
            if (isMiddle) {
                Vector2Int playerPos = GridModule.getBlockCoord(PlayerObject.player.positionCopy);
                Vector2Int blockPos = blockCoord;

                Vector2 rayOrigin = PlayerObject.player.positionCopy;

                float hDistance = playerPos.x - blockPos.x; // 水平距离
                float vDistance = playerPos.y - blockPos.y; // 垂直距离

                if (Math.Abs(hDistance) > Math.Abs(vDistance))
                {
                    if (hDistance < 0) rayOrigin.x += 0.5f; // 玩家在方块左方
                    else rayOrigin.x -= 0.5f; // 玩家在方块右方
                }
                else
                {
                    if (vDistance < 0) rayOrigin.y += 0.5f; // 玩家在方块下方
                    else rayOrigin.y -= 0.5f; // 玩家在方块上方
                }

                Ray2D ray1 = new Ray2D(rayOrigin, new Vector2(blockPos.x + 0.5f, blockPos.y) - rayOrigin);
                Ray2D ray2 = new Ray2D(rayOrigin, new Vector2(blockPos.x - 0.5f, blockPos.y) - rayOrigin);
                Ray2D ray3 = new Ray2D(rayOrigin, new Vector2(blockPos.x, blockPos.y + 0.5f) - rayOrigin);
                Ray2D ray4 = new Ray2D(rayOrigin, new Vector2(blockPos.x, blockPos.y - 0.5f) - rayOrigin);

                RaycastHit2D hit1 = Physics2D.Raycast(ray1.origin, ray1.direction, 16, LayerMask.GetMask("MovableBlock"));
                RaycastHit2D hit2 = Physics2D.Raycast(ray2.origin, ray2.direction, 16, LayerMask.GetMask("MovableBlock"));
                RaycastHit2D hit3 = Physics2D.Raycast(ray3.origin, ray3.direction, 16, LayerMask.GetMask("MovableBlock"));
                RaycastHit2D hit4 = Physics2D.Raycast(ray4.origin, ray4.direction, 16, LayerMask.GetMask("MovableBlock"));

                GameObject[] hitObjects = new GameObject[4];
                hitObjects[0] = hit1.collider is null ? null : hit1.collider.gameObject;
                hitObjects[1] = hit2.collider is null ? null : hit2.collider.gameObject;
                hitObjects[2] = hit3.collider is null ? null : hit3.collider.gameObject;
                hitObjects[3] = hit4.collider is null ? null : hit4.collider.gameObject;

                bool isVisible = false;
                try
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (hitObjects[i] is null) continue;
                        if (hitObjects[i] == gameObject) { isVisible = true; break; }
                    }
                }
                catch (System.Exception) { isVisible = false; }

                if (isVisible) { tweenVisibility(0); return; }
                else { tweenVisibility(1); return; }
            }
            
            if (isFar) {
                tweenVisibility(1);
                return;
            }

            tweenVisibility(2);
        }

        #endregion
        
        
        // #################### 着色器属性ID ####################
        // Shader property ID
        private static readonly int topId = Shader.PropertyToID("_Top");
        private static readonly int bottomId = Shader.PropertyToID("_Bottom");
        private static readonly int leftId = Shader.PropertyToID("_Left");
        private static readonly int rightId = Shader.PropertyToID("_Right");
        private static readonly int topLeftId = Shader.PropertyToID("_TopLeft");
        private static readonly int bottomLeftId = Shader.PropertyToID("_BotLeft");
        private static readonly int topRightId = Shader.PropertyToID("_TopRight");
        private static readonly int bottomRightId = Shader.PropertyToID("_BotRight");
        private static readonly int mainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int outlineTexId = Shader.PropertyToID("_OutlineTex");
        private static readonly int showEdgeId = Shader.PropertyToID("_ShowEdge");
        private static readonly int visibilityId = Shader.PropertyToID("_Visibility");
        private static readonly int pickUpId = Shader.PropertyToID("_IsPickedUp");

        // #################### 边缘动画属性 ####################
        // Edge animation properties
        private bool isTopHoldAir = false;
        private bool isBottomHoldAir = false;
        private bool isRightHoldAir = false;
        private bool isLeftHoldAir = false;
        private TweenBuilder edgeTween;
        private float edgeFactor = 0.08f;
        #region 边缘动画方法
        public void updateHoldAir(bool t, bool b, bool l, bool r) {
            isTopHoldAir = t;
            isBottomHoldAir = b;
            isLeftHoldAir = l;
            isRightHoldAir = r;
        }
        private void setBarsToClose() {
            topBar.gameObject.SetActive(false);
            botBar.gameObject.SetActive(false); 
            leftBar.gameObject.SetActive(false);
            rightBar.gameObject.SetActive(false);
        }
        public void playBarsStartTween(string type) {
            if (edgeTween is not null) {
                if (type == "cancel") return;
                if (type == "override") 
                    edgeTween.addProperty("is-cancelled", "t");
            }
            
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {edgeFactor};
                        end-value: 1.09;
                        duration: 0.2;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
            tb.setSetter(x => {
                if (tb.getProperty("is-cancelled") == "t") return;
                edgeFactor = x;
                float lengthScale = x;
                float height = (1.09f-x) / 3;
                if (isTopHoldAir) {
                    topBar.localScale = new Vector3(lengthScale, topBar.localScale.y, topBar.localScale.z);
                    topBar.localPosition = new Vector3(0, 0.5f + height, 0);
                }
                else topBar.gameObject.SetActive(false);
                if (isBottomHoldAir) {
                    botBar.localScale = new Vector3(lengthScale, botBar.localScale.y, topBar.localScale.z);
                    botBar.localPosition = new Vector3(0, -0.5f - height, 0);
                }
                else botBar.gameObject.SetActive(false);
                if (isLeftHoldAir) {
                    leftBar.localScale = new Vector3(leftBar.localScale.x, lengthScale, topBar.localScale.z);
                    leftBar.localPosition = new Vector3(-0.5f - height, 0, 0);
                }
                else leftBar.gameObject.SetActive(false);
                if (isRightHoldAir) {
                    rightBar.localScale = new Vector3(rightBar.localScale.x, lengthScale, topBar.localScale.z);
                    rightBar.localPosition = new Vector3(0.5f + height, 0, 0);
                }
                else rightBar.gameObject.SetActive(false);
            }).setOnComplete(() => {
                if (edgeTween == tb) edgeTween = null;
            });
            edgeTween = tb;
            
            if (isTopHoldAir) topBar.gameObject.SetActive(true);
            if (isBottomHoldAir) botBar.gameObject.SetActive(true);
            if (isLeftHoldAir) leftBar.gameObject.SetActive(true);
            if (isRightHoldAir) rightBar.gameObject.SetActive(true);
            tb.register<float>();
        }
        public void playBarsEndTween(string type) {
            if (edgeTween is not null) {
                if (type == "cancel") return;
                if (type == "override") 
                    edgeTween.addProperty("is-cancelled", "t");
            }
            
            TweenBuilder tb = new TweenBuilder().setProperties($@"
                        start-value: {edgeFactor};
                        end-value: 0.08;
                        duration: 0.2;
                        ease: InOutSine;
                        is-cancelled: f;
                    ");
            tb.setSetter(x => {
                if (tb.getProperty("is-cancelled") == "t") return;
                edgeFactor = x;
                float lengthScale = x;
                float height = (1.09f-x) / 3;
                if (isTopHoldAir) {
                    topBar.localScale = new Vector3(lengthScale, topBar.localScale.y, topBar.localScale.z);
                    topBar.localPosition = new Vector3(0, 0.5f + height, 0);
                }
                if (isBottomHoldAir) {
                    botBar.localScale = new Vector3(lengthScale, botBar.localScale.y, topBar.localScale.z);
                    botBar.localPosition = new Vector3(0, -0.5f - height, 0);
                }
                if (isLeftHoldAir) {
                    leftBar.localScale = new Vector3(leftBar.localScale.x, lengthScale, topBar.localScale.z);
                    leftBar.localPosition = new Vector3(-0.5f - height, 0, 0);
                }
                if (isRightHoldAir) {
                    rightBar.localScale = new Vector3(rightBar.localScale.x, lengthScale, topBar.localScale.z);
                    rightBar.localPosition = new Vector3(0.5f + height, 0, 0);
                }
            }).setOnComplete(() => {
                if (edgeTween == tb) edgeTween = null;
                setBarsToClose();
            });
            edgeTween = tb;
            tb.register<float>();
        }
        #endregion
        
        #region Unity 事件
        private void Start() {
            object[] param = PrefabParameters.getParameters(gameObject);
            if (param.Length >= 2) {
                blockName = (string)param[0];
                blockCoord = (Vector2Int)param[1];
            }
            else if (blockName is null) return;
            
            spriteRenderer = GetComponent<SpriteRenderer>();
            r2d = GetComponent<Rigidbody2D>();
            c2d = GetComponent<Collider2D>();

            color = getSprite(blockName);
            outline = BlockDocuments.getOutline(blockName);

            spriteRenderer.sprite = outline;
            spriteRenderer.sprite = color;
            spriteBound = spriteRenderer.bounds;
            spriteBound.min -= new Vector3(0, 0.5f, 0);
            spriteBound.max -= new Vector3(0, 0.5f, 0);
            
            transform.position = GridModule.getWorldCoord(blockCoord);
            gameObject.name = $"{blockName} @{blockCoord.x},{blockCoord.y}";

            if (blockName == "Wall") gameObject.tag = "Wall";

            gridObjects[gameObject] = this;
            
            checkAir();
            
            if (blockName != "Wall") BlockDocuments.addBlockCount(blockName, 1);
        }

        private void Update() {
            spriteBound = spriteRenderer.bounds;
            spriteBound.min -= new Vector3(0, 0.5f, 0);
            spriteBound.max -= new Vector3(0, 0.5f, 0);
            
            distance2Player = 10000;
            
            void debugDraw() {
                if (gameObject.layer == 31) return;
                Transform debug = transform.Find("Debug");
                debug.gameObject.SetActive(true);
                if (visibility == 0) debug.localScale = new Vector3(0, 0, 0);
                if (visibility == 1) debug.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                if (visibility == 2) debug.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }
            // debugDraw();
            
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ||
                Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
                if (currentPushedBlock.Count != 0) {
                    currentPushedBlock.Clear();
                    satisfiedBlock = 0;
                    foreach (Collider2D c2d1 in PlayerGrid.inst.getAllColliders(true)) {
                        c2d1.enabled = false;
                        c2d1.enabled = true;
                    }
                }
            }
            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.UpArrow) &&
               !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.DownArrow) &&
               !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.LeftArrow) &&
               !Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.RightArrow)) {
                if (currentPushedBlock.Count != 0) {
                    currentPushedBlock.Clear();
                    satisfiedBlock = 0;
                }
            }
            
            float tlAir = 0f, trAir = 0f, blAir = 0f, brAir = 0f;
            if (tebug) Debug.Log(1);
            if (!isTopAir && !isLeftAir && isTopLeftAir) tlAir = 1f;
            if (!isTopAir && !isRightAir && isTopRightAir) trAir = 1f;
            if (!isBottomAir && !isLeftAir && isBotLeftAir) blAir = 1f;
            if (!isBottomAir && !isRightAir && isBotRightAir) brAir = 1f;
            
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            Texture mainTexture = color.texture;
            if (blockName != "Wall") {
                Texture outlineTexture = outline.texture;
                props.SetTexture(outlineTexId, outlineTexture);
            }
            props.SetTexture(mainTexId, mainTexture);
            props.SetFloat(topId, outputBool(isTopAir));
            props.SetFloat(bottomId, outputBool(isBottomAir));
            props.SetFloat(leftId, outputBool(isLeftAir));
            props.SetFloat(rightId, outputBool(isRightAir));
            props.SetFloat(topLeftId, tlAir);
            props.SetFloat(topRightId, trAir);
            props.SetFloat(bottomLeftId, blAir);
            props.SetFloat(bottomRightId, brAir);
            props.SetFloat(visibilityId, visibility);
            props.SetFloat(pickUpId, outputBool(isPickedUp));
            if (blockName == "Wall") 
                props.SetFloat(showEdgeId, outputBool(true));
            spriteRenderer.SetPropertyBlock(props);

            positionCopy = transform.position;
        }
        #endregion

        // #################### 推箱子属性 ####################
        // Push box variables
        private static Dictionary<GridObject, Vector2Int> currentPushedBlock = new(); // 存储所有当前正在被推动的第一级方块
        private static int satisfiedBlock = 0; // 存储当前满足推动时间的第一级方块的数量
        private static bool playerReturnAnimInterrupt = false;
        private long lastEnterTime = 0;
        private long pushedTime = 0;
        private long neededTime = 0;
        private Vector2Int pushDirection = new(0, 0);
        #region 推箱子方法
        public static void setPlayerReturnAnimInterrupt(bool to) {
            playerReturnAnimInterrupt = to;
            if (playerReturnAnimInterrupt) {
                currentPushedBlock.Clear();
                satisfiedBlock = 0;
            }
        }
        
        private void finishAllPush() {
            if (currentPushedBlock.Count != 0) {
                if (Utility.currentTimeMillis() - playedPushSound > 1000) {
                    playedPushSound = Utility.currentTimeMillis();
                    SfxController.inst.play("Cursor Slide Blocks");
                }
            }

            foreach (GridObject go in new List<GridObject>(currentPushedBlock.Keys)) {
                go.pushForward(go.pushDirection);
                go.pushedTime = 0;
            }
            satisfiedBlock = 0;
        }
        
        private void startAPush(Vector2Int pushingObjPos) {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) 
                pushDirection = new Vector2Int(0, 1);
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) 
                pushDirection = new Vector2Int(0, -1);
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) 
                pushDirection = new Vector2Int( -1, 0);
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) 
                pushDirection = new Vector2Int(1, 0);

            if (pushingObjPos != blockCoord && pushingObjPos + pushDirection != blockCoord) 
                return;

            currentPushedBlock[this] = pushingObjPos;
            pushedTime = 0;
            
            int getBlockCount(GridObject block, Vector2Int direction) {
                GridObject go = block;
                Vector2Int pos = go.blockCoord;
                int count = 0;
                while (go is not null) {
                    count += 1;
                    if (count >= 100) break;
                    pos += direction;
                    go = GridModule.inst.getBlockAt(pos);
                }

                return count;
            }
            int blockCount = getBlockCount(this, pushDirection);

            long needTimeFunc(int x) {
                long output = (long)((Mathf.Exp(x / 2f - 2) / 2 + 0.5) * 1000);
                return output;
            }
            neededTime = needTimeFunc(blockCount);
        }

        private static long playedPushSound = 0;
        public bool tebug = false;

        private void PushCheck(Collision2D other)
        {
            if (Guiyuu.DayBeginModule.DayBeginModule.inst.isFinished == false) return;

            if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("HeldItem")) return;

            Vector2 colliderPos = new Vector2(other.gameObject.transform.position.x, other.gameObject.transform.position.y)
                                  + other.collider.offset;
            colliderPos = new Vector2(Mathf.Round(colliderPos.x), Mathf.Round(colliderPos.y));

            startAPush(GridModule.getBlockCoord(colliderPos));

            lastEnterTime = Utility.currentTimeMillis();
        }

        private void OnCollisionEnter2D(Collision2D other) {
            PushCheck(other);
        }

        private void OnCollisionStay2D(Collision2D other) {
            // if (neededTime == 0) { PushCheck(other); return; }

            if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("HeldItem")) return;
            
            long currentEnterTime = Utility.currentTimeMillis();

            long deltaTime = currentEnterTime - lastEnterTime;
            deltaTime = (long)(deltaTime * (1 - PlayerObject.player.accumulatedSlowness));
            pushedTime += deltaTime;
            if (pushedTime >= neededTime) {
                satisfiedBlock += 1;
                if (satisfiedBlock >= currentPushedBlock.Count) 
                    finishAllPush();
            }
                
            lastEnterTime = currentEnterTime;
        }
        #endregion
    }
}