using System;
using System.Collections.Generic;

namespace Guiyuu.DayCirculation {
    public abstract class DayCircle {
        // #################### 私有的集合变量 ####################
        // Private collection variables, shouldn't be accessed from outside
        private List<EventBlock> eventBlocks = new(); // 保持顺序的事件块集合，EventBlock collection that maintains order
        private Dictionary<string, EventBlock> eventBlocksId2Inst = new(); // 事件块 id 到实例的映射，EventBlock id to instance mapping
        #region EventBlock 集合暴露的方法
        protected void addEventBlock(string id) {
            EventBlock eventBlock = new EventBlock();
            eventBlocks.Add(eventBlock);
            eventBlocksId2Inst.Add(id, eventBlock);
        }
        public void subscribeEventBlock(bool isStart, string id, Action code) {
            if (isStart) eventBlocksId2Inst[id].subscribeStart(code);
            else eventBlocksId2Inst[id].subscribeEnd(code);
        }
        #endregion
        
        protected DayCircle() {
            initEventBlocks();
        }
        
        // #################### 私有的 Temp 变量 ####################
        // Private Temp variables
        private int currentEventBlockIndex = 0;
        private EventBlock currentEventBlock = null;
        #region 轮换事件块的方法
        public void nextEventBlock() {
            if (eventBlocks.Count == 0) return;
            if (currentEventBlock != null) {
                currentEventBlock.endEvent();
                currentEventBlockIndex++;
            }
            eventBlocks[currentEventBlockIndex].startEvent();
            currentEventBlock = eventBlocks[currentEventBlockIndex];
        }
        public bool isLastEventBlock() {
            return currentEventBlockIndex == eventBlocks.Count - 1;
        }
        public void resetEventBlock() {
            currentEventBlockIndex = 0;
            currentEventBlock = null;
        }
        #endregion
        
        protected abstract void initEventBlocks();
    }
}