using System;

namespace Guiyuu.DayCirculation {
    public class EventBlock {
        private event Action start;
        private event Action end;
        
        public void startEvent() {
            start?.Invoke();
        }
        
        public void endEvent() {
            end?.Invoke();
        }
        
        public void subscribeStart(Action code) {
            start += code;
        }
        
        public void subscribeEnd(Action code) {
            end += code;
        }
    }
}