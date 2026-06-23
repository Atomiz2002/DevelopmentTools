#if DEVELOPMENT_TOOLS_RUNTIME_ODIN_INSPECTOR
using DevelopmentEssentials.Extensions.Unity;
using DevelopmentTools.Editor.AttributeDrawers;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor.ContextMenus {

    [InitializeOnLoad]
    public static class GlobalContextMenu {

        static GlobalContextMenu() => EditorApplication.contextualPropertyMenu += OnContextMenu;

        private static void OnContextMenu(GenericMenu menu, SerializedProperty prop) {
            if (prop.isArray)
                return;

            switch (prop.boxedValue) {
                case Sprite sprite:   menu.AddItem(new("- Preview Sprite -"), false, () => PreviewTexture2DWindow.Create(sprite.ToTexture2D())); break;
                case Texture texture: menu.AddItem(new("- Preview Texture -"), false, () => PreviewTexture2DWindow.Create(texture)); break;
            }
        }

    }

}
#endif