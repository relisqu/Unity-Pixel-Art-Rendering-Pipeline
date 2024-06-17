using System;
using System.Collections;
using System.Collections.Generic;
using Shaders;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

// Shoutout to https://lindenreidblog.com/2018/09/13/using-command-buffers-in-unity-selective-bloom/ for command buffer explanation

namespace Shaders
{
    public class OutlineSystem
    {
        static OutlineSystem m_Instance; // singleton

        public static Action ResetObj;

        public static OutlineSystem instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new OutlineSystem();
                return m_Instance;
            }
        }

        internal HashSet<OutlineObj> _outlineObjs = new();

        public void Add(OutlineObj o)
        {
            Remove(o);
            _outlineObjs.Add(o);
            ResetObj?.Invoke();
        }

        public void Remove(OutlineObj o)
        {
            _outlineObjs.Remove(o);
            ResetObj?.Invoke();
        }
    }


    [ExecuteInEditMode]
    public class CustomOutlineRenderer : MonoBehaviour
    {
        private CommandBuffer _outlineBuffer;
        private Dictionary<Camera, CommandBuffer> _cameras = new Dictionary<Camera, CommandBuffer>();

        private void Cleanup()
        {
            foreach (var cam in _cameras)
            {
                if (cam.Key)
                {
                    print(cam.Value);
                    cam.Key.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, cam.Value);
                }

            }
            _cameras.Clear();
            _outlineBuffer?.Clear();
        }

        void Start()
        {
            OutlineSystem.ResetObj += Cleanup;
        }

        private void OnDestroy()
        {
            OutlineSystem.ResetObj -= Cleanup;
        }

        public void OnDisable()
        {
            Cleanup();
        }

        public void OnEnable()
        {
            Cleanup();
        }

 
        public void OnWillRenderObject()
        {
            var render = gameObject.activeInHierarchy && enabled;
            if (!render)
            {
                Cleanup();
                return;
            }

            var cam = Camera.current;
            if (!cam)
                return;

            if (_cameras.ContainsKey(cam))
                return;

            // create new command buffer
            _outlineBuffer = new CommandBuffer();
            _outlineBuffer.name = "Outline map buffer";
            _cameras[cam] = _outlineBuffer;

            var outlineSystem = OutlineSystem.instance;

            // create render texture for glow map
            int tempID = Shader.PropertyToID("_Temp1");
            _outlineBuffer.GetTemporaryRT(tempID, -1, -1, 24, FilterMode.Bilinear);
            _outlineBuffer.SetRenderTarget(tempID);
            _outlineBuffer.ClearRenderTarget(true, true, Color.black);

            foreach (OutlineObj o in outlineSystem._outlineObjs)
            {
                Renderer r = o.GetComponent<Renderer>();
                MeshFilter m = o.GetComponent<MeshFilter>();
                Material glowMat = o.OutlineMaterial;
                if (r && glowMat)
                {
                    for (int i = 0; i <  m.sharedMesh.subMeshCount; i++)
                    {
                        _outlineBuffer.DrawRenderer(r, glowMat, i);
                    }
                }
            }

            _outlineBuffer.SetGlobalTexture("_OutlineBuffer", tempID);

            // add this command buffer to the pipeline
            cam.AddCommandBuffer(CameraEvent.BeforeImageEffects, _outlineBuffer);
        }
    }
}