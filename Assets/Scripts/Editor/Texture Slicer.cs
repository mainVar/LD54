using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Animation2D;
using UnityEditor;
using UnityEditor.U2D;
using UnityEditor.U2D.Aseprite;
using UnityEditor.U2D.Sprites;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.U2D;

public class TextureSlicer : EditorWindow
{
    private SerializedObject serializedObject;
    private ReorderableList textureList;
    private string animationListName = "NewAnimationList";
    private string selectedFolderPath = "Assets";
    private Vector2 sliceGrid = new Vector2(3, 3); // Number of rows and columns in the grid

    [MenuItem("Window/Texture Slicer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<TextureSlicer>("Texture Slicer");
    }

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
       
        
        textureList = new ReorderableList(serializedObject, serializedObject.FindProperty("texturesToSlice"), true, true, true, true);

        textureList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Textures to Slice");
        textureList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = textureList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };
    }

    private void OnGUI()
    {
        serializedObject.Update();

        GUILayout.Label("Texture Slicer", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Select textures to slice into sprite assets.", MessageType.Info);
        GUILayout.Space(10);

        EditorGUILayout.Vector2Field("Slice Grid", sliceGrid);
        selectedFolderPath = EditorGUILayout.TextField("Folder Path", selectedFolderPath);

        if (Event.current.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDrop.paths.Length == 1 && AssetDatabase.IsValidFolder(DragAndDrop.paths[0])
                ? DragAndDropVisualMode.Copy
                : DragAndDropVisualMode.Rejected;
        }
        else if (Event.current.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();

            if (DragAndDrop.paths.Length == 1 && AssetDatabase.IsValidFolder(DragAndDrop.paths[0]))
            {
                selectedFolderPath = DragAndDrop.paths[0];
                GUI.FocusControl(null); // Убираем фокус с текстового поля
            }
        }
        
        animationListName = EditorGUILayout.TextField("AnimationList Name", animationListName);
        GUILayout.Space(10);

        textureList.DoLayoutList();

        GUILayout.Space(10);

        // Drag and drop area
        Event evt = Event.current;
        Rect dropArea = GUILayoutUtility.GetRect(0f, 50f, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drag & Drop Textures Here");

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    texturesToSlice.Clear();
                    
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is Texture2D)
                        {
                            texturesToSlice.Add(draggedObject as Texture2D);
                        }
                        else if (draggedObject is DefaultAsset)
                        {
                            // If a folder is dragged, find all textures inside it
                            string folderPath = AssetDatabase.GetAssetPath(draggedObject);
                            string[] texturePaths = Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories);

                            foreach (string texturePath in texturePaths)
                            {
                                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                                if (texture != null)
                                {
                                    texturesToSlice.Add(texture);
                                }
                            }
                        }
                    }
                }
                break;
        }

        if (GUILayout.Button("Slice Textures"))
        {
            if (textureList.count > 0)
            {
                SliceTextures();
            }
            else
            {
                EditorUtility.DisplayDialog("No Textures Selected", "Please select textures to slice.", "OK");
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    public List<Texture2D> texturesToSlice = new List<Texture2D>();
    private string[] stateNames = {"S", "SW", "W", "NW", "N", "NE", "E", "SE"};
    private void SliceTextures()
    {
        // Create AnimationList asset in the specified folder
        AnimationList animationList = ScriptableObject.CreateInstance<AnimationList>();
        animationList.Name = animationListName;
        string combine = Path.Combine(selectedFolderPath, $"{animationListName}");
        
        
        if (!AssetDatabase.IsValidFolder(combine))
        {
            // Если папка не существует, создаем её
            Directory.CreateDirectory(combine);
            
        }
        string animationListPath = Path.Combine(combine, $"{animationListName}.asset");
        AssetDatabase.CreateAsset(animationList, animationListPath);
        AssetDatabase.SaveAssets();

        int stateIndex = 0;
        foreach (Texture2D texture in texturesToSlice)
        {
            string texturePath = AssetDatabase.GetAssetPath(texture);

            // Calculate the width and height of each sliced sprite
            int spriteWidth = texture.width / (int)sliceGrid.x;
            int spriteHeight = texture.height / (int)sliceGrid.y;

            List<SpriteMetaData> spriteMetaDataList = new List<SpriteMetaData>();
            TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (textureImporter != null) {
                textureImporter.isReadable = true;
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            }

            int index = 0;
            for (int y = (int)sliceGrid.y - 1; y >= 0; y--) // Start from the top
            {
                for (int x = 0; x < sliceGrid.x; x++) // Then from left to right
                {
                    Rect rect = new Rect(x * spriteWidth, y * spriteHeight, spriteWidth, spriteHeight);

                    // Check if the slice contains any non-transparent pixels
                    if (SliceContainsNonTransparentPixels(texture, rect))
                    {
                        // Create sprite metadata for each sliced sprite
                        SpriteMetaData spriteMetaData = new SpriteMetaData
                        {
                            name = "Slice_" + index,
                            rect = rect,
                            pivot = new Vector2(0.5f, 0.5f)
                        };

                        spriteMetaDataList.Add(spriteMetaData);
                        index++;
                    }
                }

                
            }
            // Set the sprite sheet data in the texture importer

            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Multiple;

                textureImporter.spritesheet = spriteMetaDataList.ToArray();
                textureImporter.filterMode = FilterMode.Point;
                textureImporter.isReadable = false;

                // Reimport the texture with new settings
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            }

            // Refresh the asset database to show changes
            AssetDatabase.Refresh();
            string spriteSheet = AssetDatabase.GetAssetPath( texture );
            // Filter and collect the Sprite objects
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath( spriteSheet ).OfType<Sprite>().ToArray();
            CreateAssets(animationList, sprites, stateNames[stateIndex], combine);
            stateIndex++;
        }

        EditorUtility.DisplayDialog("Slicing Complete", "Texture slicing complete.", "OK");
    }
    static void AutoSaveChanges(ISpriteEditorDataProvider dataProvider)
    {
        dataProvider.Apply();
        var assetImporter = dataProvider.targetObject as AssetImporter;
        assetImporter.SaveAndReimport();
    }
    private bool SliceContainsNonTransparentPixels(Texture2D texture, Rect rect)
    {
        Color[] pixels = texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);

        foreach (Color pixel in pixels)
        {
            if (pixel.a > 0)
            {
                return true;
            }
        }

        return false;
    }
    
    private void CreateAssets(AnimationList animationList, Sprite[] sprites, string name, string path)
    {
        if (string.IsNullOrEmpty(animationListName))
        {
            Debug.LogError("AnimationList Name cannot be empty.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(path))
        {
            Debug.LogError($"Invalid folder path. {path}");
            return;
        }
        Animation2DFrames animationFrames = ScriptableObject.CreateInstance<Animation2DFrames>();
        animationFrames.Frames = new Sprite[sprites.Length];
        for (var i = 0; i < sprites.Length; i++) {
            animationFrames.Frames[i] = sprites[i];
        }
        animationFrames.State = name;
        string animationFramesPath = Path.Combine(path, $"{name}.asset");
        AssetDatabase.CreateAsset(animationFrames, animationFramesPath);
        AssetDatabase.SaveAssets();

        // Add Animation2DFrames to AnimationList
        animationList.List = animationList.List ?? new Animation2DFrames[0];
        ArrayUtility.Add(ref animationList.List, animationFrames);

        // Re-import the assets to apply changes
        AssetDatabase.Refresh();
    }
}
