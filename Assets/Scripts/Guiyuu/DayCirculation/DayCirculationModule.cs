using System;
using System.Collections.Generic;
using ModuleManager;

namespace Guiyuu.DayCirculation {
    public class DayCirculationModule : Module {
        public static int dayCount { get; private set; } = 0;
        
        // #################### 各种单例变量 ####################
        // Various singleton variables
        private static DayCirculationModule module; // 本类的单例变量, Singleton variable
        public static DayCirculationModule inst => module; // 本类的单例属性, Singleton property
        private static NormalDayCircle normalDayCircle; // 正常日循环的单例变量, Singleton variable
        public static NormalDayCircle normal { // 正常日循环的单例属性, Singleton property
            get {
                if (normalDayCircle == null) normalDayCircle = new NormalDayCircle();
                return normalDayCircle;
            }
        }

        public void nextEventBlock() {
            if (normal.isLastEventBlock()) nextDayCircle();
            else normal.nextEventBlock();
        }
        
        public void nextDayCircle() {
            dayCount++;
            normal.resetEventBlock();
            normal.nextEventBlock();
        }
        
        public override string getName() {
            return "DayCirculationModule";
        }

        public override List<string> getDependencies() {
            return new List<string>();
        }

        public override void construct() {
            module = this;
        }

        private bool isStarted = false;
        private void Update() {
            if (isStarted) return;
            nextDayCircle();
            isStarted = true;
        }
    }
}