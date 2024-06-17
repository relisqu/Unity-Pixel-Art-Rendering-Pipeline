using System;
using System.Collections.Generic;
using Shaders.PerformanceMeasureForThesisStuff;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Shaders
{
    [ExecuteInEditMode]
    public class ApplyPalletAndDithering : NamedMonobehaviour
    {
        public Material ShaderMaterial;
        public Texture2D Palette;
        private List<Color> finalColors;
        [Tooltip("If you want to have different pixel size for this feature - leave empty")] public UniformPixelSize UniformPixelSize;

        private bool hasUniformPixelProperty => UniformPixelSize != null;
        private void Awake()
        {
            UpdateColorPalette();
        }

        //If you change palette texture and need to update it. Also here you can modify color (like give it a shift/invert them before passing to shader)
        public void UpdateColorPalette()
        {
            finalColors = new List<Color>();
            finalColors.Clear();
            for (var index = 0; index < Palette.width; index++)
            {
                var color = Palette.GetPixelBilinear((float)index / Palette.width, 0);
                if (!finalColors.Contains(color))
                {
                    finalColors.Add(color);
                }
            }
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
#if UNITY_EDITOR
            UpdateColorPalette();
#endif
            if(hasUniformPixelProperty) ShaderMaterial.SetFloat("_PixelSize", UniformPixelSize.PixelSize);
            ShaderMaterial.SetColorArray("_Pallet", finalColors.ToArray());
            ShaderMaterial.SetInt("_PixelPalletCount", finalColors.Count);
            Graphics.Blit(src, dest, ShaderMaterial);
        }
    }
}