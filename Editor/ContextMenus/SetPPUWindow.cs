using System;
using UnityEditor;

namespace DevelopmentTools.Editor.ContextMenus {

    public class SetPPUWindow : IntEditorWindow {

        [MenuItem("Assets/Set Texture PPU", false, 2000)]
        public static void ShowWindow() {
            GetWindow<SetPPUWindow>("Set Texture PPU").Show();
        }

        protected override int    DefaultInt => 16;
        protected override string Prompt     => "Set PPU";
        protected override string FieldName  => "Pixels Per Unit";
        protected override string Button     => "Set";

        protected override Action<int> Action =>
            ppu =>
                ContextMenuUtils.BulkEdit(ContextMenuUtils.GetSelectedGUIDsRecursively("t:Texture2D"),
                    guid => {
                        if (AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) is not TextureImporter importer)
                            return;

                        importer.spritePixelsPerUnit = ppu;
                        importer.SaveAndReimport();
                    });

    }

}