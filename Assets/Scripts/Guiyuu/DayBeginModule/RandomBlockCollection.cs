using System.Collections.Generic;
using Guiyuu.BlockDocument;
using UnityEngine;

namespace Guiyuu.DayBeginModule {
    public class RandomBlockCollection {
        private readonly string[] blockTypes;
        private readonly float[][] collectionNoise;

        public RandomBlockCollection(int collectionLength, int blockTypesCount) {
            blockTypes = new string[blockTypesCount];
            Dictionary<string, bool> exclude = new();
            for (int i = 0; i < blockTypesCount; i++) 
                blockTypes[i] = BlockDocuments.getRandomAvailableBlock(exclude);
            
            collectionNoise = new float[collectionLength][];
            for (int i = 0; i < collectionLength; i++) 
                collectionNoise[i] = new float[collectionLength];

            float randomness = Random.Range(0f, 100f);
            for (int i = 0; i < collectionLength; i++) {
                for (int j = 0; j < collectionLength; j++) {
                    collectionNoise[i][j] = calculateNoiseValue(randomness, i, j, 2);
                }
            }
            fixInScale();
        }

        private void fixInScale() {
            float minimumNoise = float.MaxValue;
            float maximumNoise = float.MinValue;
            
            foreach (var noise in collectionNoise) {
                foreach (var value in noise) {
                    if (value < minimumNoise) minimumNoise = value;
                    if (value > maximumNoise) maximumNoise = value;
                }
            }
            
            float scale = 1f / (maximumNoise - minimumNoise);
            foreach (var t in collectionNoise) {
                for (int j = 0; j < t.Length; j++) {
                    t[j] = (t[j] - minimumNoise) * scale;
                }
            }
        }

        private float random(float seed, int x, int y) {
            return Mathf.PerlinNoise(seed+x, seed+y);
        }

        private float calculateNoiseValue(float randomness, int x, int y, int gridSize) {
            float gridX = x / (float)gridSize;
            float gridY = y / (float)gridSize;
            int intGridX = Mathf.FloorToInt(gridX);
            int intGridY = Mathf.FloorToInt(gridY);
            float currentGridRandom = random(randomness, intGridX, intGridY);
            float fracGridX = gridX - intGridX;
            float fracGridY = gridY - intGridY;
            
            int intGridXRight = intGridX+1;
            int intGridYBottom = intGridY+1;
            float rightGridRandom = random(randomness, intGridXRight, intGridY);
            float bottomGridRandom = random(randomness, intGridX, intGridYBottom);
            float bottomRightGridRandom = random(randomness, intGridXRight, intGridYBottom);
            
            float noise = Mathf.Lerp(
                Mathf.Lerp(currentGridRandom, rightGridRandom, fracGridX),
                Mathf.Lerp(bottomGridRandom, bottomRightGridRandom, fracGridX),
                fracGridY
            );
            return noise;
        }

        public string getBlockAt(int x, int y) {
            float noise = collectionNoise[x][y];
            if (noise < 0.25f) return blockTypes[0];
            if (noise < 0.5f) return blockTypes[1];
            if (noise < 0.75f) return blockTypes[2];
            return blockTypes[3];
        }
    }
}