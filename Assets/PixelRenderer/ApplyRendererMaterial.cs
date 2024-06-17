using System;
using Shaders.PerformanceMeasureForThesisStuff;
using UnityEngine;

namespace Shaders
{
    [ExecuteInEditMode]
    public class ApplyRendererMaterial : NamedMonobehaviour
    {
        public bool UseMainCamera = true;
        public Camera Camera;
        public Material ShaderMaterial;

        [Tooltip("If you want to have different pixel size for this feature - leave empty")]
        public UniformPixelSize UniformPixelSize;

        public CustomOutlineRenderer OutlineSystem;
        private bool hasUniformPixelProperty => UniformPixelSize != null;

        private void Start()
        {
        }

        private void OnEnable()
        {
            if (UseMainCamera) Camera = Camera.main;
            Debug.Log("h");
            Camera.depthTextureMode = DepthTextureMode.DepthNormals;
            if (OutlineSystem != null)
                OutlineSystem.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (OutlineSystem != null)
                OutlineSystem.gameObject.SetActive(false);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, ShaderMaterial, 0);
            if (hasUniformPixelProperty) ShaderMaterial.SetFloat("_PixelSize", UniformPixelSize.PixelSize);
        }
    }
}