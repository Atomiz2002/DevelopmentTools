using System;
using System.Collections.Generic;
using DevelopmentEssentials.Extensions.CS;
using UnityEditor;
using UnityEngine;

namespace DevelopmentTools.Debugging.VisualDebugging {

    public class VisualDebugger : MonoBehaviour {

        internal static readonly Dictionary<string, Action> drawRequests = new();

        private void OnDrawGizmos() {
            foreach ((string _, Action draw) in drawRequests) {
                Handles.color = Color.white;
                draw.SafeInvoke();
            }

            drawRequests.Clear();
        }

    }

}