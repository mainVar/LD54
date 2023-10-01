namespace Animation2D {
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    [CreateAssetMenu(fileName = "Animation2D", menuName = "ScriptableObjects/Animation2D", order = 1)]
    public class Animation2DFrames : ScriptableObject {
        public string State;
        public Sprite[] Frames;
        
        private void OnValidate() {
            State = name;
        }
    }
    
    #if UNITY_EDITOR
    
    [CustomEditor(typeof(Animation2DFrames))]
    public class Animation2DFramesEditor : Editor {
        public override void OnInspectorGUI() {
            Animation2DFrames animationFrames = (Animation2DFrames)target;

            // Отображаем поле State и Frames
            animationFrames.State = EditorGUILayout.TextField("State", animationFrames.State);
        
            // Отображаем превью первого спрайта
            if (animationFrames.Frames != null && animationFrames.Frames.Length > 0) {
                Texture2D previewTexture = AssetPreview.GetAssetPreview(animationFrames.Frames[0]);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(previewTexture, GUILayout.Width(256), GUILayout.Height(256));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Frames"), true);

            // Применяем изменения
            serializedObject.ApplyModifiedProperties();
        }
    }
        
    #endif
}