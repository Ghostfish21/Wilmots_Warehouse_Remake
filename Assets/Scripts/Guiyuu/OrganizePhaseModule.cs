using System.Collections.Generic;
using Guiyuu.DayCirculation;
using Guiyuu.Gameloop;
using ModuleManager;

namespace Guiyuu {
    public class OrganizePhaseModule : Module {
        public TimerModule timerModule;
        public SkipCounterModule skipCounterModule;

        public override void construct() {
            DayCirculationModule.normal.subscribeEventBlock(true, "organize", onPhaseBegin);
        }

        private bool isFirstRun = true;

        private void onPhaseBegin() {
            skipCounterModule.isSkipAllowed = true;

            timerModule.switchPalette("b");
            timerModule.switchFormat("d");

            void onTimerEnd() {
                skipCounterModule.isSkipAllowed = false;
                DayCirculationModule.inst.nextEventBlock();
            }

            bool temp = false;
            if (!isFirstRun) {
                if (DateBar.inst.monthCount == 0 || DateBar.inst.monthCount == 3 || DateBar.inst.monthCount == 6 ||
                    DateBar.inst.monthCount == 9) {
                    timerModule.createTimer(9000000f, onTimerEnd);
                    timerModule.switchFormat("a");
                    temp = true;
                }
            }

            if (!temp) timerModule.createTimer(180f, onTimerEnd);
            isFirstRun = false;
        }

        public override List<string> getDependencies() {
            return new List<string>() {
                "DayCirculationModule"
            };
        }

        public override string getName() {
            return "OrganizePhaseModule";
        }

        // Update is called once per frame
        void Update() {
        }
    }
}