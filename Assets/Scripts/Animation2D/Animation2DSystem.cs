using Wargon.ezs;
using Wargon.ezs.Unity;

namespace Animation2D {

    public sealed partial class Animation2DSystem : UpdateSystem {
        [Inject] private AnimationsHolder AnimationsHolder;
        private float frameTime => AnimationsHolder.FrameTime;
        private float time;
        protected override void OnCreate() {
            AnimationsHolder.Init();
        }

        public override void Update() {
            if ((time += UnityEngine.Time.deltaTime) < frameTime) return;
            time -= frameTime;
            
            entities.Without<Inactive>().Each((SpriteAnimation animation, SpriteRender render) => {
                if (!animation.created) {
                    animation.currentFrames = animation.AnimationList.GetState(animation.currentState).Frames;
                    animation.created = true;
                }
                //ref var frames = ref animation.AnimationList.GetState(animation.currentState).Frames;
                if (animation.frame >= animation.currentFrames.Length) {
                    if (--animation.times <= 0) {
                        if (animation.currentState != animation.nextState) {
                            animation.Play(animation.nextState);
                        }
                    }
                    animation.frame = 0;
                }
                
                //AnimationEvents.Invoke(animation.currentState, animation.frame);
                render.value.sprite = animation.currentFrames[animation.frame++];
            });
        }
    }
    [EcsComponent]
    public struct SpriteRender {
        public UnityEngine.SpriteRenderer value;
    }
}