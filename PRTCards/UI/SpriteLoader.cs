using System.IO;
using System.Reflection;
using UnityEngine;
using static UnityEngine.ImageConversion;



namespace PRT.UI
{
    public static class SpriteLoader
    {
        public static Sprite LoadEmbeddedSprite(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                {
                    return null;
                }

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);

                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(buffer);

                return Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f
                );
            }
        }
    }
}
