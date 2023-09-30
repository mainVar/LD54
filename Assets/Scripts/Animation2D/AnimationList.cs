using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Animation2D {
    [CreateAssetMenu(fileName = "AnimationList", menuName = "ScriptableObjects/AnimationList", order = 1)]
    public class AnimationList : ScriptableObject {
        public string Name;
        public Animation2DFrames[] List;
        private Dictionary<string, Animation2DFrames> map = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Animation2DFrames GetState(string state) {
#if UNITY_EDITOR
            if (map.TryGetValue(state, out Animation2DFrames frames)) {
                return frames;
            }
            throw new Exception($"No such state [{state}] in {Name}");
#elif !UNITY_EDITOR
            return map[state];
#endif
        }

        private void OnValidate() {
            Name = name;
        }

        public void Init() {
            map.Clear();
            foreach (var animation2D in List) {
                var state = animation2D.State;
                if (map.ContainsKey(state)) {
                    Debug.LogError($"Two animations with same State : {state.ToString()} in {animation2D.name}");
                    continue;
                }
                map.Add(animation2D.State, animation2D);
            }
            Debug.Log($"{Name} inited");
        }
    }
    
    public static partial class Animations
    {
        //public const string IDLE = "Idle";
        //public const string RUN = "Run";
    }
}