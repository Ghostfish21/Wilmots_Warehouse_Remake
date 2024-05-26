using System.Collections.Generic;
using System.Numerics;
using Guiyuu.BlockDocument;
using ModuleManager;
using UnityEngine;
using UnityEngine.Rendering;
using Vector2 = UnityEngine.Vector2;

namespace Guiyuu.GridModule {
    public class GridModule : Module {
        
        // #################### 静态单例变量 ####################
        // Static singleton variables
        private static GridModule module;
        public static GridModule inst => module;
        
        // #################### 常量参数 ####################
        // Constant parameters
        public float cellSize = 1f; // Length of each cell
        #region 常量参数暴露的公共方法
        public static Vector2Int getBlockCoord(Vector2 pos) {
            pos /= module.cellSize;
            return new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        }
        public static Vector2 getWorldCoord(Vector2Int blockCoord) {
            return new Vector2(blockCoord.x * module.cellSize, blockCoord.y * module.cellSize);
        }
        #endregion
        
        // #################### 私有合集 ####################
        // Private collections that shouldn't be changed from outside
        private readonly Dictionary<Vector2Int, GridObject> gridSheet = new(); // 坐标 对 方块名，Coordinate to block name
        #region 网格暴露出来的公共方法

        // 该方法只负责抄录方块到列表里，不负责更改动画等等
        // This method is only responsible to copy a backup data to the grid, isn't responsible for anything else like change animation
        public void setBlockAt(Vector2Int pos, GridObject block) {
            if (getBlockAt(pos) is not null) return;
            block.gameObject.name = $"{block.blockName} @{block.blockCoord.x},{block.blockCoord.y}";
            gridSheet[pos] = block;
        }
        // 该方法返回指定坐标上的方块名，如果没有方块则返回空
        // This method will return the block name on the given coordinate, return null if there is nothing on it
        public GridObject getBlockAt(Vector2Int pos) {
            if (gridSheet.ContainsKey(pos)) return gridSheet[pos];
            return null;
        }

        public void removeBlockAt(Vector2Int pos) {
            gridSheet.Remove(pos);
        }

        #endregion
        #region 修改地图上的方块的公共方法

        public void createBlockAt(Vector2Int pos, string blockName) {
            GridObject go = getBlockAt(pos);
            if (go is not null) {
                removeBlockAt(pos);
                Destroy(go.gameObject);
            }

            GameObject prefab = BlockDocuments.wallPrefab;
            if (blockName != "Wall") prefab = BlockDocuments.blockPrefab;
            
            GameObject block = PrefabParameters.initPrefab(prefab, transform, blockName, pos);
            GridObject go2 = block.GetComponent<GridObject>();
            setBlockAt(pos, go2);

            go2.checkAdjacentAir();
        }

        public void createCuboidAt(Vector2Int min, Vector2Int max, string blockName) {
            int minX = Mathf.Min(min.x, max.x);
            int minY = Mathf.Min(min.y, max.y);
            int maxX = Mathf.Max(min.x, max.x);
            int maxY = Mathf.Max(min.y, max.y);
            for (int x = minX; x <= maxX; x++) {
                for (int y = minY; y <= maxY; y++) {
                    createBlockAt(new Vector2Int(x, y), blockName);
                }
            }
        }

        #endregion
        
        public override string getName() {
            return "GridModule";
        }

        public override List<string> getDependencies() {
            return new List<string> {"BlockDocumentsModule"};
        }

        public override void construct() {
            module = this;
            generateMap();
        }

        private void generateMap() {
            // 中间的四根柱子
            createCuboidAt(new Vector2Int(10, 9), new Vector2Int(16, 3), "Wall");
            createCuboidAt(new Vector2Int(10, -4), new Vector2Int(16, -10), "Wall");
            createCuboidAt(new Vector2Int(-10, 9), new Vector2Int(-16, 3), "Wall");
            createCuboidAt(new Vector2Int(-10, -4), new Vector2Int(-16, -10), "Wall");
            
            // 地图边缘
            createCuboidAt(new Vector2Int(-8, -15), new Vector2Int(-42, -21), "Wall");
            createCuboidAt(new Vector2Int(8, -15), new Vector2Int(42, -21), "Wall");
            createCuboidAt(new Vector2Int(-31, -15), new Vector2Int(-42, 24), "Wall");
            createCuboidAt(new Vector2Int(31, -15), new Vector2Int(42, 24), "Wall");
            createCuboidAt(new Vector2Int(-8, 24), new Vector2Int(-42, 15), "Wall");
            createCuboidAt(new Vector2Int(8, 24), new Vector2Int(42, 15), "Wall");
            
            // 柜台
            createCuboidAt(new Vector2Int(7, 24), new Vector2Int(-7, 17), "Wall");
        }
    }
}