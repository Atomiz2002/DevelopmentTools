using DevelopmentEssentials.Extensions.Unity;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DevelopmentTools.Editor.Editor.Extensions {

    public static class TextureExtensions {

        public static void CaptureSnapshot(this Camera camera, out Texture2D texture, int width = 0, int height = 0, bool trim = false, bool pointFilter = false) =>
            texture = camera.CaptureSnapshot(width, height, trim, pointFilter);

        public static Texture2D CaptureSnapshot(this Camera camera, int width = 0, int height = 0, bool trim = false, bool pointFilter = false) {
            float renderScale = UniversalRenderPipeline.asset.renderScale;
            UniversalRenderPipeline.asset.renderScale = 1;
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

            UniversalRenderPipeline.asset.renderScale = renderScale;
            return snapshot;
        }

        // implement a way to keep retain the center if needed (trim equally from all sides)
        public static void Trim(this Texture2D texture) {
            Color[] pixels = texture.GetPixels();

            int width  = texture.width;
            int height = texture.height;

            int xMin = width,  xMax = 0;
            int yMin = height, yMax = 0;

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++) {
                Color pixel = pixels[x + y * width];

                if (pixel.a == 0)
                    continue;

                if (x < xMin) xMin = x;
                if (x > xMax) xMax = x;
                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;
            }

            int newWidth  = xMax - xMin + 1;
            int newHeight = yMax - yMin + 1;

            if (newWidth < 0 || newHeight < 0) {
                newWidth  = Mathf.Clamp(newWidth, 0, width);
                newHeight = Mathf.Clamp(newHeight, 0, height);
                Debug.LogWarning("Trimmed transparent texture to 0x0");
            }

            Texture2D trimmedTexture = new(newWidth, newHeight);
            Color[]   newPixels      = new Color[newWidth * newHeight];

            for (int y = 0; y < newHeight; y++)
            for (int x = 0; x < newWidth; x++)
                newPixels[x + y * newWidth] = pixels[x + xMin + (y + yMin) * width];

            trimmedTexture.SetPixels(newPixels);
            trimmedTexture.Apply();

            texture.Reinitialize(newWidth, newHeight);
            texture.SetPixels(trimmedTexture.GetPixels());
            texture.Apply();
        }

        public static Texture2D ReadGPU(this RenderTexture texture) {
            Texture2D t = new(texture.width, texture.height, TextureFormat.ARGB32, false);
            Graphics.CopyTexture(texture, t);
            return t;
        }

        public static Texture2D Read(this RenderTexture texture) {
            Texture2D tex = new(texture.width, texture.height, TextureFormat.ARGB32, false);
            RenderTexture.active = texture;

            tex.ReadPixels(new(0, 0, texture.width, texture.height), 0, 0);
            tex.Apply();

            RenderTexture.active = null;
            return tex;
        }

        public static Texture2D Read(this Texture texture) {
            Texture2D     tex           = new(texture.width, texture.height, TextureFormat.ARGB32, false);
            RenderTexture renderTexture = RenderTexture.GetTemporary(tex.width, tex.height, 24, RenderTextureFormat.ARGB32);

            Graphics.Blit(texture, renderTexture);
            RenderTexture.active = renderTexture;

            tex.ReadPixels(new(0, 0, texture.width, texture.height), 0, 0);
            tex.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);
            return tex;
        }

        public static Sprite ToSprite(this Texture2D texture) =>
            Sprite.Create(texture, new(0, 0, texture.width, texture.height), new(.5f, .5f));

    }

}