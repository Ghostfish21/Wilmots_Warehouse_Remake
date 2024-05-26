using System.Collections.Generic;
using Guiyuu.DayCirculation;
using Guiyuu.Gameloop;
using ModuleManager;

namespace Guiyuu {
    public class DeliverOutPhaseModule : Module {
        private static DeliverOutPhaseModule dopm;
        public static DeliverOutPhaseModule inst => dopm;
    
        public TimerModule timerModule;
        public SkipCounterModule skipCounterModule;
        private bool isFinishedAheadOfTime = false;

        public override void construct() {
            dopm = this;
            DayCirculationModule.normal.subscribeEventBlock(true, "deliverOut", () => {
                onPhaseBegin();
            });
        }

        private bool isFirstRun = true;
        public bool isInPhase { get; private set; } = false;
        private void onPhaseBegin() {
            isInPhase = true;
        
            timerModule.switchPalette("w");
            timerModule.switchFormat("s");
            Windows.inst.openWindow("serviceHatchOpen");

            void onTimerEnd() {
                isInPhase = false;

                if (!Employee.isAllSatisfied) {
                    Windows.inst.openWindow("cjDisappointed");
                    SfxController.inst.play("Lose Lives");
                }

                CounterCollider.inst.hide();
                DateBar.inst.addDay();
                skipCounterModule.isSkipAllowed = false;
                SkipCounterModule.inst.settleAllStars();
                Employee.stopAll();
                StarCollectionManager.clear();
                GateAnimController.inst.close();
                if (isFinishedAheadOfTime) ClockAnimController.inst.close();
                else ClockAnimController.inst.openAndClose();
            
                timerModule.switchFormat("b");
                Invoke(nameof(a), 10f);
            }

            timerModule.createTimer(90f, onTimerEnd);

            Employee.startAll();
            SkipCounterModule.inst.makeDownStars();
            ClockAnimController.inst.openAndClose();
            GateAnimController.inst.open();
        }
    
        private void a() {
            DayCirculationModule.inst.nextEventBlock();
        }

        public override List<string> getDependencies() {
            return new List<string> { "DayCirculationModule" };
        }

        public override string getName() {
            return "DeliverOutPhaseModule";
        }
    }
}
