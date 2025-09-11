// PacLevelBuilderWindow.cs
// Put this file in Assets/Editor

using UnityEngine;
using UnityEditor;

public class PacLevelBuilderWindow : EditorWindow
{
    public Sprite OutsideCornerSprite;   // 1
    public Sprite OutsideWallSprite;     // 2
    public Sprite InsideCornerSprite;    // 3
    public Sprite InsideWallSprite;      // 4
    public Sprite EmptyWithPelletSprite; // 5
    public Sprite PowerPelletSprite;     // 6
    public Sprite TJunctionSprite;       // 7
    public Sprite GhostExitSprite;       // 8
    public Sprite BackgroundSprite;      // 0

    public float cellSize = 1f;
    public string parentName = "Level_TopLeft";
    public bool clearExisting = true;

    static readonly int[,] levelMap = new int[,]
    {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,8},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };

    [MenuItem("Window/Pac Level Builder")]
    public static void ShowWindow()
    {
        GetWindow<PacLevelBuilderWindow>("Pac Level Builder");
    }

    void OnGUI()
    {
        GUILayout.Label("Assign sprites for codes 0-8", EditorStyles.boldLabel);
        OutsideCornerSprite = (Sprite)EditorGUILayout.ObjectField("Outside Corner (1)", OutsideCornerSprite, typeof(Sprite), false);
        OutsideWallSprite = (Sprite)EditorGUILayout.ObjectField("Outside Wall (2)", OutsideWallSprite, typeof(Sprite), false);
        InsideCornerSprite = (Sprite)EditorGUILayout.ObjectField("Inside Corner (3)", InsideCornerSprite, typeof(Sprite), false);
        InsideWallSprite = (Sprite)EditorGUILayout.ObjectField("Inside Wall (4)", InsideWallSprite, typeof(Sprite), false);
        EmptyWithPelletSprite = (Sprite)EditorGUILayout.ObjectField("Empty w/ Pellet (5)", EmptyWithPelletSprite, typeof(Sprite), false);
        PowerPelletSprite = (Sprite)EditorGUILayout.ObjectField("Power Pellet (6)", PowerPelletSprite, typeof(Sprite), false);
        TJunctionSprite = (Sprite)EditorGUILayout.ObjectField("T-Junction (7)", TJunctionSprite, typeof(Sprite), false);
        GhostExitSprite = (Sprite)EditorGUILayout.ObjectField("Ghost Exit (8)", GhostExitSprite, typeof(Sprite), false);
        BackgroundSprite = (Sprite)EditorGUILayout.ObjectField("Background / Empty (0) optional", BackgroundSprite, typeof(Sprite), false);

        cellSize = EditorGUILayout.FloatField("Cell Size", cellSize);
        parentName = EditorGUILayout.TextField("Parent Name", parentName);
        clearExisting = EditorGUILayout.Toggle("Clear Existing Parent", clearExisting);

        GUILayout.Space(8);

        if (GUILayout.Button("Build Top-Left Quadrant"))
        {
            if (!ValidateSprites())
            {
                if (!EditorUtility.DisplayDialog("Missing sprites", "Some sprites are unassigned. Continue?", "Yes", "Cancel"))
                    return;
            }
            BuildTopLeft();
        }

        if (GUILayout.Button("Clear Parent GameObject"))
        {
            var existing = GameObject.Find(parentName);
            if (existing)
            {
                if (EditorUtility.DisplayDialog("Delete", $"Delete '{parentName}' and all children?", "Delete", "Cancel"))
                    DestroyImmediate(existing);
            }
            else EditorUtility.DisplayDialog("Not Found", $"No GameObject named {parentName} found.", "OK");
        }
    }

    bool ValidateSprites()
    {
        return OutsideCornerSprite && OutsideWallSprite && InsideCornerSprite && InsideWallSprite && TJunctionSprite && GhostExitSprite;
    }

    void BuildTopLeft()
    {
        GameObject parent = GameObject.Find(parentName);
        if (parent != null && clearExisting)
        {
            DestroyImmediate(parent);
            parent = null;
        }
        if (parent == null) parent = new GameObject(parentName);

        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int code = levelMap[r, c];
                // 左上象限生成：Y方向翻转
                Vector2 pos = new Vector2(c * cellSize, (rows - 1 - r) * cellSize);

                // 背景
                if (BackgroundSprite != null)
                    CreateSpriteObject(BackgroundSprite, parent.transform, pos, 0f, $"tile_{r}_{c}_bg");

                // 主贴图
                Sprite tileSprite = GetSpriteForCode(code, false);
                if (tileSprite != null)
                {
                    float rot = DetermineRotationForTile(r, c, code);
                    bool flipX = false;
                    bool flipY = false;

                    // 指定翻转
                    if ((r == 2 && (c == 2 || c == 5 || c == 7 || c == 11))) flipY = true; // 第三排3向下翻转
                    if ((r == 4 && (c == 2 || c == 5))) flipY = true; // 第五排3向上翻转

                    CreateSpriteObject(tileSprite, parent.transform, pos, rot, $"tile_{r}_{c}_code{code}", flipX, flipY);
                }

                // Pellet / PowerPellet
                if (code == 5 && EmptyWithPelletSprite != null)
                    CreateSpriteObject(EmptyWithPelletSprite, parent.transform, pos, 0f, $"tile_{r}_{c}_pellet");
                if (code == 6 && PowerPelletSprite != null)
                    CreateSpriteObject(PowerPelletSprite, parent.transform, pos, 0f, $"tile_{r}_{c}_power");
            }
        }

        Selection.activeGameObject = parent;
        EditorUtility.DisplayDialog("Done", "Top-left quadrant built.", "OK");
    }

    Sprite GetSpriteForCode(int code, bool baseOnly)
    {
        return code switch
        {
            0 => BackgroundSprite,
            1 => OutsideCornerSprite,
            2 => OutsideWallSprite,
            3 => InsideCornerSprite,
            4 => InsideWallSprite,
            5 => baseOnly ? EmptyWithPelletSprite : null,
            6 => baseOnly ? PowerPelletSprite : null,
            7 => TJunctionSprite,
            8 => GhostExitSprite,
            _ => null
        };
    }

    GameObject CreateSpriteObject(Sprite sprite, Transform parent, Vector2 pos, float rot, string name, bool flipX = false, bool flipY = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0f);
        go.transform.localEulerAngles = new Vector3(0, 0, rot);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = name.Contains("_pellet") ? 2 : (name.Contains("_code") ? 1 : 0);
        sr.flipX = flipX;
        sr.flipY = flipY;

        return go;
    }

    bool IsWallLike(int code) => code == 1 || code == 2 || code == 3 || code == 4 || code == 7 || code == 8;

    float DetermineRotationForTile(int r, int c, int code)
    {
        int rows = levelMap.GetLength(0);
        int cols = levelMap.GetLength(1);

        bool up = r > 0 && IsWallLike(levelMap[r - 1, c]);
        bool down = r < rows - 1 && IsWallLike(levelMap[r + 1, c]);
        bool left = c > 0 && IsWallLike(levelMap[r, c - 1]);
        bool right = c < cols - 1 && IsWallLike(levelMap[r, c + 1]);

        switch (code)
        {
            case 1: // Outside corner
                if (up && left) return 180f;
                if (up && right) return 90f;
                if (down && left) return 270f;
                return 0f; // 默认右下

            case 3: // Inside corner
                if (up && right) return 0f;
                if (right && down) return 90f;
                if (down && left) return 180f;
                if (left && up) return 270f;
                return 0f;

            case 2: // Outside wall
            case 4: // Inside wall
                if (up && down) return 90f;
                return 0f; // 默认水平

            case 7: // T-junction
                if (!up) return 0f;
                if (!right) return 90f;
                if (!down) return 180f;
                if (!left) return 270f;
                return 0f;

            case 8: // Ghost exit
                if (!up && down) return 0f;
                if (!right && left) return 90f;
                if (!down && up) return 180f;
                if (!left && right) return 270f;
                return 0f;
        }

        return 0f;
    }
}