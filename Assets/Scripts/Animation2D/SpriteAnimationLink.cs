using System;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace Animation2D {

    public partial struct AnimationState {
        public const int Idle = 0;
        
    }

    public partial struct AnimationState {
        public const int Run = 1;
    }
    public enum AnimationStateEnum {
        Idle = 0,
        Run = 1,
    }

    [Serializable][EcsComponent]
    public struct SpriteAnimation {
        
        public AnimationList AnimationList;
        public string currentState;
        public string nextState;
        public int frame;
        public int times;
        public Sprite[] currentFrames;
        public bool created;
    }

    public static class SpriteAnimationExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Play(this ref SpriteAnimation animation, string newState, int timesToPlay = Int32.MaxValue) {
            if (animation.currentState != newState) {
                animation.nextState = animation.currentState;
                animation.currentState = newState;
                animation.times = timesToPlay;
                animation.currentFrames = animation.AnimationList.GetState(newState).Frames;
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlayForce(this ref SpriteAnimation animation, string newState, int timesToPlay = Int32.MaxValue) {
            animation.nextState = animation.currentState;
            animation.currentState = newState;
            animation.times = timesToPlay;
            animation.currentFrames = animation.AnimationList.GetState(newState).Frames;
        }
        public static void Sub(this ref SpriteAnimation animation, string state, int frame, Action callback) {
            AnimationEvents.Sub(animation.AnimationList.GetState(animation.currentState), frame, callback);
        }

    }
}
