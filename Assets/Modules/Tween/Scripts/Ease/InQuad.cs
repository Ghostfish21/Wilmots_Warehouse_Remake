public class InQuad : Ease {
    
    public override float func(float x) {
        return x * x;
    }

    public override string name() {
        return "InQuad";
    }
}
