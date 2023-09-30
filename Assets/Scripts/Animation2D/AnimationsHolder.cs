using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Animation2D {
    [CreateAssetMenu(fileName = "AnimationsHolder", menuName = "ScriptableObjects/AnimationsHolder", order = 1)]
    public class AnimationsHolder : ScriptableObject {
        public List<AnimationList> Animations;
        public float FrameTime = 0.1f;

        [SerializeField] private TextAsset AnimationsConstFile;
        [SerializeField] [HideInInspector] private List<string> names;

        private readonly string bot = @"    
    }
}";

        private readonly HashSet<string> toGenerate = new();
        private readonly string top = @"
namespace Animation2D {
    public static partial class Animations
    {
";
#if UNITY_EDITOR
        private void OnEnable() {
            RegenerateFile();
        }
#endif

        public void Init() {
            foreach (var animationList in Animations) animationList.Init();
        }

        //[Button(nameof(RegenerateFile))]
        public void RegenerateFile() {
            var animationsCount = typeof(Animations).GetFields().Length;
            
            foreach (var animationList in Animations)
            foreach (var animation2DFrames in animationList.List)
                toGenerate.Add(animation2DFrames.State);
            
            if(animationsCount == toGenerate.Count) return;
            var source = string.Empty;
            source += top;
            foreach (var s in toGenerate) {
                source += GenerateField(s);
                source += Environment.NewLine;
            }

            source += bot;
#if UNITY_EDITOR
            File.WriteAllText(AssetDatabase.GetAssetPath(AnimationsConstFile), source);
#endif
            Debug.Log("[Animations const Re-Generated]");
            toGenerate.Clear();
        }
        const string quote = "\"";
        private string GenerateField(string state) {
            
            return @$"      public const string {state} = {quote}{state}{quote};";
        }
    }
}