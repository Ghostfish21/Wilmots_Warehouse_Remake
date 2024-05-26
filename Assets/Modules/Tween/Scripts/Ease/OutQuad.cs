public class OutQuad : Ease {
    public override float func(float x) {
        return 1 - (1 - x) * (1 - x);
    }

    public override string name() {
        return "OutQuad";
    }
}
