namespace Guiyuu.DayCirculation {
    public class NormalDayCircle : DayCircle {
        protected override void initEventBlocks() {
            addEventBlock("deliverIn");
            addEventBlock("organize");
            addEventBlock("deliverOut");
        }
    }
}