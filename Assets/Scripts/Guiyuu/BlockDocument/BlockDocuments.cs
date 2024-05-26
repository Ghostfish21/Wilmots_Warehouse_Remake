using System;
using System.Collections.Generic;
using System.Linq;
using ModuleManager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Guiyuu.BlockDocument {
    public class BlockDocuments : Module {
        public override string getName() {
            return "BlockDocumentsModule";
        }

        public override List<string> getDependencies() {
            return new List<string>();
        }

        public override void construct() {
            blockPrefab = Resources.Load<GameObject>("Arts/Blocks/A Block Prefab");
            wallPrefab = Resources.Load<GameObject>("Arts/Blocks/A Wall Prefab");
            
            Sprite[] blockSprites = Resources.LoadAll<Sprite>("Arts/Grid Objects");
            Sprite[] outlinePics = Resources.LoadAll<Sprite>("Arts/Outlines");
            Sprite[] employeePics = Resources.LoadAll<Sprite>("Arts/Employees");
            foreach (Sprite block in blockSprites) {
                string id = block.name.Split('_')[0];
                blocks.Add(id, block);
                if (id != "Wall") interactiveBlocks.Add(id);
            }
            foreach (Sprite block in outlinePics) {
                string id = block.name.Split('_')[0];
                outlines.Add(id, block);
            }
            foreach (Sprite block in employeePics) {
                string id = block.name.Split('_')[0];
                employees.Add(int.Parse(id), block);
            }
        }

        public static GameObject blockPrefab { get; private set; } = null;
        public static GameObject wallPrefab { get; private set; } = null;
        private static readonly Dictionary<string, Sprite> blocks = new();
        private static readonly Dictionary<string, Sprite> outlines = new();
        private static readonly Dictionary<int, Sprite> employees = new();
        private static readonly List<string> interactiveBlocks = new();
        
        public static Sprite getBlock(string name) {
            if (blocks.ContainsKey(name)) return blocks[name];
            Sprite block = Resources.Load<Sprite>($"Arts/Blocks/{name}");
            if (block is null) return null;
            blocks.Add(name, block);
            return block;
        }

        public static Sprite getRandomEmployee(Dictionary<int, bool> exclude) {
            int r = Random.Range(0, employees.Count-1);
            if (exclude.ContainsKey(r)) return getRandomEmployee(exclude);
            exclude[r] = true;
            return employees[r];
        }

        public static Sprite getOutline(string name) {
            if (outlines.ContainsKey(name)) return outlines[name];
            return null;
        }
        
        private static readonly List<string> availableBlocks = new();
        public static void addAvailableBlock() {
            string randomBlock = interactiveBlocks[Random.Range(0, interactiveBlocks.Count)];
            interactiveBlocks.Remove(randomBlock);
            availableBlocks.Add(randomBlock);
        }
        public static string getRandomAvailableBlock(Dictionary<string, bool> exclude) {
            string result = availableBlocks[Random.Range(0, availableBlocks.Count)];
            if (exclude.ContainsKey(result)) return getRandomAvailableBlock(exclude);
            exclude[result] = true;
            return result;
        }

        private static Dictionary<string, int> blockCount = new();

        public static int getExistingBlockTypeCount() {
            return blockCount.Count;
        }

        public static (string, int) getRandomBlock(Dictionary<string, bool> exclude) {
            List<string> keys = new List<string>(blockCount.Keys);
            int i = Random.Range(0, keys.Count);
            string key = keys[i];
            if (exclude.ContainsKey(key)) return getRandomBlock(exclude);
            exclude[key] = true;
            return (key, blockCount[key]);
        }

        public static void addBlockCount(string name, int count) {
            if (count < 0) return;
            if (!blockCount.ContainsKey(name)) blockCount[name] = count;
            else blockCount[name] += count;
        }

        public static void minusBlockCount(string name, int count) {
            if (count < 0) return;
            blockCount[name] -= count;
            if (blockCount[name] <= 0) blockCount.Remove(name);
        }
    }
}