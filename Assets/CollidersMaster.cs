
using System.Linq;
using Roguelike.Physics2D;
using Wargon.ezs.Unity;

namespace LD54.Editor {
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    public class CollidersMaster : MonoBehaviour {
        public Vector2 size;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(CollidersMaster))]
    class ColldersMasterEditor : Editor {
        public override void OnInspectorGUI() {
            
            base.OnInspectorGUI();
            if (GUILayout.Button("Fix Colliders")) {
                Fix(target as CollidersMaster);
            }
        }

        void Fix(CollidersMaster master) {
            var colliderEntities = master.GetComponentsInChildren<MonoEntity>();
            foreach (var colliderEntity in colliderEntities) {
                var components = colliderEntity.Components;
                
                var rectangleObj = components.FirstOrDefault(x => x is Rectangle2D);
                var rectangle = (Rectangle2D)rectangleObj;
                rectangle.w = master.size.x;
                rectangle.h = master.size.y;
                var index = components.IndexOf(rectangleObj);
                components[index] = rectangle;
                EditorUtility.SetDirty(colliderEntity);
            }
            Debug.Log($"{colliderEntities.Length} colliders fixed");
        }
    }
#endif
}

