// ------------------- 生成左上象限 -------------------
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

    int halfRows = rows / 2 + rows % 2; // 上半部分
    int halfCols = cols / 2 + cols % 2; // 左半部分

    for (int r = 0; r < halfRows; r++)
    {
        for (int c = 0; c < halfCols; c++)
        {
            int code = levelMap[r, c];
            Vector2 pos = new Vector2(c * cellSize, (rows - 1 - r) * cellSize);

            if (BackgroundSprite != null)
                CreateSpriteObject(BackgroundSprite, parent.transform, pos, 0f, $"tile_{r}_{c}_bg");

            Sprite tileSprite = GetSpriteForCode(code, false);
            if (tileSprite != null)
            {
                float rot = DetermineRotationForTile(r, c, code);
                CreateSpriteObject(tileSprite, parent.transform, pos, rot, $"tile_{r}_{c}_code{code}");
            }

            if (code == 5 && EmptyWithPelletSprite != null)
                CreateSpriteObject(EmptyWithPelletSprite, parent.transform, pos, 0f, $"tile_{r}_{c}_pellet");
            if (code == 6 && PowerPelletSprite != null)
                CreateSpriteObject(PowerPelletSprite, parent.transform, pos, 0f, $"tile_{r}_{c}_power");
        }
    }

    Selection.activeGameObject = parent;
    EditorUtility.DisplayDialog("Done", "Top-left quadrant built.", "OK");
}
