using System;
using UnityEngine;

namespace Shaders
{

    [ExecuteInEditMode]
    public class OutlineObj : MonoBehaviour
    {
        public Material OutlineMaterial;
        public void OnEnable()
        {
            OutlineSystem.instance.Add(this);
        }

        public void Start()
        {
            OutlineSystem.instance.Add(this);
        }

        public void OnDisable()
        {
            OutlineSystem.instance.Remove(this);
        }
    }
}