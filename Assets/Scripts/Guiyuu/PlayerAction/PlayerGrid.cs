using System.Collections.Generic;
using Guiyuu.GridModule;
using UnityEngine;

namespace Guiyuu.PlayerAction {
    public class PlayerGrid {
        private static PlayerGrid playerGrid = null;

        public static PlayerGrid inst {
            get {
                if (playerGrid == null) playerGrid = new PlayerGrid();
                return playerGrid;
            }
        }

        private CircleCollider2D playerCollider = null;
        private readonly Dictionary<Vector2Int, BoxCollider2D> colliders = new();
        private readonly Dictionary<BoxCollider2D, Vector2Int> collider2Pos = new();
        private readonly Dictionary<Vector2Int, PlayerGridPlaceHolder> placeHolders = new();

        public List<PlayerGridPlaceHolder> getAllPlaceHolders() {
            return new List<PlayerGridPlaceHolder>(placeHolders.Values);
        }

        private void checkAir(Vector2Int position, PlayerGridPlaceHolder placeHolder) {
            bool t = !hasCollider(position + new Vector2Int(0, 1));
            bool b = !hasCollider(position + new Vector2Int(0, -1));
            bool l = !hasCollider(position + new Vector2Int(-1, 0));
            bool r = !hasCollider(position + new Vector2Int(1, 0));
            placeHolder.gridObjectInHold.updateHoldAir(t, b, l, r);
        }

        private void checkAdjacentAir(Vector2Int position) {
            PlayerGridPlaceHolder t = getPlaceHolder(position + new Vector2Int(0, 1));
            PlayerGridPlaceHolder b = getPlaceHolder(position + new Vector2Int(0, -1));
            PlayerGridPlaceHolder l = getPlaceHolder(position + new Vector2Int(-1, 0));
            PlayerGridPlaceHolder r = getPlaceHolder(position + new Vector2Int(1, 0));
            if (t is not null) checkAir(position + new Vector2Int(0, 1), t);
            if (b is not null) checkAir(position + new Vector2Int(0, -1), b);
            if (l is not null) checkAir(position + new Vector2Int(-1, 0), l);
            if (r is not null) checkAir(position + new Vector2Int(1, 0), r);
        }

        public Vector2Int getPlayerGridCoord(BoxCollider2D collider2D) {
            if (collider2Pos.ContainsKey(collider2D)) return collider2Pos[collider2D];
            return new Vector2Int(0, 0);
        }
        
        public void placeGridObject(Vector2Int position, PlayerGridPlaceHolder placeHolder) {
            placeHolder.name = $"@{position.x},{position.y}";
            placeCollider(position);
            if (placeHolders.ContainsKey(position)) return;
            placeHolders[position] = placeHolder;
            
            checkAir(position, placeHolder);
            checkAdjacentAir(position);
        }
        private void placeCollider(Vector2Int position) {
            if (colliders.ContainsKey(position)) return;
            colliders[position] = createCollider(position);
            collider2Pos[colliders[position]] = position;
        }

        public void removeGridObject(Vector2Int position) {
            if (!placeHolders.ContainsKey(position)) return;
            PlayerGridPlaceHolder placeHolder = placeHolders[position];
            placeHolders.Remove(position);
            removeCollider(position);
            
            checkAir(position, placeHolder);
            checkAdjacentAir(position);
        }
        private void removeCollider(Vector2Int position) {
            if (!colliders.ContainsKey(position)) return;
            collider2Pos.Remove(colliders[position]);
            Object.Destroy(colliders[position]);
            colliders.Remove(position);
        }
        public bool hasCollider(Vector2Int position) {
            return colliders.ContainsKey(position);
        }

        public List<Collider2D> getAllColliders(bool includePlayerCollider = false) {
            List<Collider2D> collider2Ds = new List<Collider2D>(colliders.Values);
            if (playerCollider == null) playerCollider = PlayerObject.player.GetComponent<CircleCollider2D>();
            if (includePlayerCollider) collider2Ds.Add(playerCollider);
            return collider2Ds;
        }

        public PlayerGridPlaceHolder getPlaceHolder(Vector2Int position) {
            if (!hasCollider(position)) return null;
            return placeHolders[position];
        }

        public int getSize() {
            return placeHolders.Count;
        }

        private BoxCollider2D createCollider(Vector2Int position) {
            GameObject player = PlayerObject.player.gameObject;
            BoxCollider2D boxCollider2D = player.AddComponent<BoxCollider2D>();
            boxCollider2D.size = new Vector2(0.95f, 0.95f);
            boxCollider2D.offset = new Vector2(position.x * GridModule.GridModule.inst.cellSize,
                position.y * GridModule.GridModule.inst.cellSize);
            return boxCollider2D;
        }
    }
}