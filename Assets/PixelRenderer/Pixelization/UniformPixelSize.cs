using UnityEngine;

namespace Shaders
{
    public class UniformPixelSize : MonoBehaviour //this class could be used for having one consistent var that would change pixel size everywhere if you wish
    {
        [Range(1,100)]public int PixelSize;
    }
}