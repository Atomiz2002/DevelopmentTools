#if DEVELOPMENT_TOOLS_SOFT_U2D_SPRITES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using _DevelopmentEssentials.Extensions.CS;
using _DevelopmentEssentials.Extensions.Unity;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace DevelopmentTools.Editor.ContextMenus {

    public static class Texture2DSlicer {

        private const int tilesetTileSize = 16;
        private const int maxSpritesPerRow = 10;

        [MenuItem("Assets/Slice Tilesets x16")]
        private static void SliceSelectedTilesets() {
            ProcessTextures(ContextMenuUtils.GetSelectedGUIDsRecursively("t:Texture2D"), tex => tex.name.EndsWith("Tileset") ? CreateTilesetSlices(tex) : null);
        }

        [MenuItem("Assets/Slice Textures 10 columns max")] // for sunnyside asset pack
        private static void SliceSelectedTextures() {
            ProcessTextures(ContextMenuUtils.GetSelectedGUIDsRecursively("t:Texture2D"),
                tex => {
                    Match match = Regex.Match(tex.name, @"(\d+)(?!.*\d)");

                    if (!match.Success || !int.TryParse(match.Value, out int sliceCount))
                        return null;

                    return CreateLeftToRightBottomToTopSlices(tex, sliceCount);
                });
        }

        private static void ProcessTextures(List<string> guids, Func<Texture2D, SpriteRect[]> slices) {
            ContextMenuUtils.BulkEdit(guids,
                () => {
                    foreach (Texture2D tex in guids.Select(guid => guid.GUIDToPath().LoadAsset<Texture2D>()).Existing())
                        ApplySpriteEditorSettings(tex, slices(tex));
                });
        }

        private static void ApplySpriteEditorSettings(Texture2D texture, SpriteRect[] referenceSlices) {
            if (referenceSlices == null) return;

            string          path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (!importer) return;

            importer.spriteImportMode = SpriteImportMode.Multiple;

            ISpriteEditorDataProvider dataProvider = new SpriteDataProviderFactories().GetSpriteEditorDataProviderFromObject(importer);
            if (dataProvider == null) return;

            Undo.RegisterImporterUndo(path, $"Auto Slice {texture.name}");

            dataProvider.InitSpriteEditorDataProvider();
            dataProvider.SetSpriteRects(referenceSlices);
            dataProvider.Apply();
        }

        private static SpriteRect[] CreateTilesetSlices(Texture2D texture) {
            List<SpriteRect> newRects = new();

            int i = 0;

            for (int y = texture.height / tilesetTileSize - 1; y >= 0; y--)
            for (int x = 0; x < texture.width / tilesetTileSize; x++)
                newRects.Add(new() {
                    name = $"{i++}",
                    rect = new(x * tilesetTileSize, y * tilesetTileSize, tilesetTileSize, tilesetTileSize)
                });

            return newRects.ToArray();
        }

        private static SpriteRect[] CreateLeftToRightBottomToTopSlices(Texture2D texture, int sliceCount) {
            List<SpriteRect> newRects = new();

            // Loop through the slices left to right, then bottom to top
            for (int i = 0; i < sliceCount; i++) {
                // Calculate the current row and column based on slice index
                int col = i % maxSpritesPerRow; // Column will increase first (left to right)
                int row = Mathf.FloorToInt(i / (float) maxSpritesPerRow); // Row will increase after every 10 slices

                // Calculate the slice width and height
                float sliceWidth = texture.width / Mathf.Min(sliceCount, (float) maxSpritesPerRow); // Calculate width based on sprites per row
                float sliceHeight = texture.height / Mathf.Ceil(sliceCount / (float) maxSpritesPerRow); // Divide height into rows

                // For bottom to top row, adjust row calculation
                int   flippedRow = Mathf.CeilToInt(sliceCount / (float) maxSpritesPerRow) - row - 1;
                float x = col * sliceWidth;
                float y = flippedRow * sliceHeight;

                // Create a rectangle for each slice based on the current row and column

                newRects.Add(new() {
                    name = $"{i}",
                    rect = new(x, y, sliceWidth, sliceHeight)
                });
            }

            return newRects.ToArray();
        }

    }

}
#endif