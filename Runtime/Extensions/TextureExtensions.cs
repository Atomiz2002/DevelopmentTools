using DevelopmentEssentials.Extensions.Unity;
using UnityEngine;
#if DEVELOPMENT_TOOLS_SOFT_URP
using UnityEngine.Rendering.Universal;
#endif

namespace DevelopmentTools.Extensions {

    public static class TextureExtensions {

        public static void CaptureSnapshot(this Camera camera, out Texture2D texture, int width = 0, int height = 0, bool trim = false, bool pointFilter = false) =>
            texture = camera.CaptureSnapshot(width, height, trim, pointFilter);

        public static Texture2D CaptureSnapshot(this Camera camera, int width = 0, int height = 0, bool trim = false, bool pointFilter = false) {
#if DEVELOPMENT_TOOLS_SOFT_URP
            float renderScale = UniversalRenderPipeline.asset.renderScale;
            UniversalRenderPipeline.asset.renderScale = 1;
#endif
            bool cameraInitiallyEnabled = camera.isActiveAndEnabled;

            camera.SetActive(true);

            if (width == 0) width   = Screen.width;
            if (height == 0) height = (int) ((float) Screen.height / Screen.width * width);

            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            Texture2D     snapshot      = new(width, height, TextureFormat.ARGB32, false);

            if (pointFilter)
                snapshot.filterMode = FilterMode.Point;

            camera.targetTexture          = renderTexture;
            camera.forceIntoRenderTexture = true;

            camera.Render();

            RenderTexture.active = renderTexture;

            snapshot.ReadPixels(new(0, 0, width, height), 0, 0); // Reads from RenderTexture.active
            snapshot.Apply();

            if (trim)
                snapshot.Trim();

            RenderTexture.active          = null;
            camera.targetTexture          = null;
            camera.forceIntoRenderTexture = false;
            RenderTexture.ReleaseTemporary(renderTexture);

            camera.SetActive(cameraInitiallyEnabled);

#if DEVELOPMENT_TOOLS_SOFT_URP
            UniversalRenderPipeline.asset.renderScale = renderScale;
#endif
            return snapshot;
        }

    }

}