using Animation2D;

namespace LD54 {
    [EcsComponent]
    public struct AnimationStates {
        public AnimationList run;
        public AnimationList idle;
    }
}