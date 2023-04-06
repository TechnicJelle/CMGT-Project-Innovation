// Source: https://github.com/codebude/QRCoder.Unity/blob/master/QRCoder.Unity/UnityQRCode.cs

using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace QRCoder.Unity
{
    public class UnityQRCode : AbstractQRCode
    {
        /// <summary>
        /// Constructor without params to be used in COM Objects connections
        /// </summary>
        public UnityQRCode() { }
        public UnityQRCode(QRCodeData data) : base(data) {}

        public Texture2D GetGraphic(int pixelsPerModule)
        {
            return GetGraphic(pixelsPerModule, Color.black, Color.white);
        }

        public Texture2D GetGraphic(int pixelsPerModule, string darkColorHtmlHex, string lightColorHtmlHex)
        {
            return GetGraphic(pixelsPerModule, HexToColor(darkColorHtmlHex), HexToColor(lightColorHtmlHex));
        }

        private static Color HexToColor(string hexColor)
        {
            hexColor = hexColor.Replace("0x", "").Replace("#", "").Trim();
            byte a = 255;
            byte r = byte.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            if (hexColor.Length == 8)
            {
                a = byte.Parse(hexColor.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return new Color32(r, g, b, a);
        }

        private Texture2D GetGraphic(int pixelsPerModule, Color darkColor, Color lightColor)
        {
            int size = QrCodeData.ModuleMatrix.Count * pixelsPerModule;
            Texture2D gfx = new(size, size, TextureFormat.ARGB32, false);
            Color[] darkBrush = GetBrush(pixelsPerModule, pixelsPerModule, darkColor);
            Color[] lightBrush = GetBrush(pixelsPerModule, pixelsPerModule, lightColor);
            for (int x = 0; x < size; x = x + pixelsPerModule)
            {
                for (int y = 0; y < size; y = y + pixelsPerModule)
                {
                    bool module = QrCodeData.ModuleMatrix[(y + pixelsPerModule) / pixelsPerModule - 1][(x + pixelsPerModule) / pixelsPerModule - 1];
                    gfx.SetPixels(x, y, pixelsPerModule, pixelsPerModule, module ? darkBrush : lightBrush);
                }
            }

            gfx.Apply();
            return gfx;
        }

        private static Color[] GetBrush(int sizeX, int sizeY, Color defaultColor) {
            int len = sizeX * sizeY;
            List<Color> brush = new(len);
            for(int i = 0; i < len; i++){
                brush.Add(defaultColor);
            }

            return brush.ToArray();
        }
    }
}
