#if UNITY_EDITOR
using System;
using DevelopmentEssentials.Extensions.CS;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Editor_ {

    public class CustomPopup : PopupWindowContent {

        private readonly Action drawGUI;

        private Vector2 size;

        public CustomPopup(Action drawGUI, float width = 400, float height = 0f) {
            this.drawGUI = drawGUI;
            size.x       = width;

            if (height != 0f)
                size.y = height;
        }

        public override void OnGUI(Rect rect) {
            GUILayout.BeginVertical();
            drawGUI.SafeInvoke();
            GUILayout.EndVertical();

            Rect lastRect = GUILayoutUtility.GetLastRect();

            if (lastRect.height > size.y) // sometimes lastRect.height is near 0
                size.y = lastRect.height + 5;
        }

        public override Vector2 GetWindowSize() => size;

    }

}
#endif