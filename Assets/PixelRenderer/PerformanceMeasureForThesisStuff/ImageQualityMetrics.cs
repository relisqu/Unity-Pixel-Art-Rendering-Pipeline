
using UnityEngine;

public class ImageQualityMetrics : MonoBehaviour
{
    public ComputeShader colourfulnessComputeShader;
    public ComputeShader contrastComputeShader;
    public ComputeShader sharpnessComputeShader;
    public ComputeShader noiseComputeShader;
    private RenderTexture inputTexture;
    private Camera _camera;
    private Camera _camera1;

    void Start()
    {
        _camera1 = Camera.main;
        _camera = Camera.main;
        // Capture the current screen to inputTexture
        CaptureScreen();

        // Colourfulness
        float colourfulness = CalculateColourfulness();
        Debug.Log("Image Colourfulness: " + colourfulness);

        // Contrast
        float contrast = CalculateContrast();
        Debug.Log("Image Contrast: " + contrast);

        // Sharpness
        float sharpness = CalculateSharpness();
        Debug.Log("Image Sharpness: " + sharpness);
        
        // noise
       // float noise = CalculateNoise();
      //  Debug.Log("Image Noise: " + noise);


    }

    public void CaptureScreen()
    {
        // Create a new RenderTexture with the screen dimensions
        inputTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
        inputTexture.Create();

        // Capture the screen into the RenderTexture
        RenderTexture.active = inputTexture;
        GL.Clear(true, true, Color.clear);
        _camera.targetTexture = inputTexture;
        _camera.Render();
        _camera.targetTexture = null;
        RenderTexture.active = null;
    }

    public float CalculateColourfulness()
    {
        RenderTexture resultTexture = CreateResultTexture();
        int kernelHandle = colourfulnessComputeShader.FindKernel("CSMain");
        DispatchComputeShader(colourfulnessComputeShader, kernelHandle, resultTexture);

        Texture2D result = ReadRenderTexture(resultTexture);
        Color[] pixels = result.GetPixels();
        float meanRG = 0, meanYB = 0, stdRG = 0, stdYB = 0;

        foreach (Color pixel in pixels)
        {
            meanRG += pixel.r;
            meanYB += pixel.g;
        }

        meanRG /= pixels.Length;
        meanYB /= pixels.Length;

        foreach (Color pixel in pixels)
        {
            stdRG += Mathf.Pow(pixel.r - meanRG, 2);
            stdYB += Mathf.Pow(pixel.g - meanYB, 2);
        }

        stdRG = Mathf.Sqrt(stdRG / pixels.Length);
        stdYB = Mathf.Sqrt(stdYB / pixels.Length);

        float colorfulness = stdRG + stdYB + 0.3f * (meanRG + meanYB);

        Destroy(resultTexture);
        return colorfulness;
    }

    public float CalculateContrast()
    {
        RenderTexture resultTexture = CreateResultTexture();
        int kernelHandle = contrastComputeShader.FindKernel("CSMain");
        DispatchComputeShader(contrastComputeShader, kernelHandle, resultTexture);

        Texture2D result = ReadRenderTexture(resultTexture);
        Color[] pixels = result.GetPixels();
        float contrast = 0;

        foreach (Color pixel in pixels)
        {
            contrast += pixel.r;
        }

        Destroy(resultTexture);
        return contrast / pixels.Length;
    }

    public float CalculateSharpness()
    {
        RenderTexture resultTexture = CreateResultTexture();
        int kernelHandle = sharpnessComputeShader.FindKernel("CSMain");
        DispatchComputeShader(sharpnessComputeShader, kernelHandle, resultTexture);

        Texture2D result = ReadRenderTexture(resultTexture);
        Color[] pixels = result.GetPixels();
        float sharpness = 0;

        foreach (Color pixel in pixels)
        {
            sharpness += pixel.r;
        }

        Destroy(resultTexture);
        return sharpness / pixels.Length;
    }

    public string CalculateNoise()
    {
        RenderTexture resultTexture = CreateResultTexture();
        int kernelHandle = noiseComputeShader.FindKernel("CSMain");
        DispatchComputeShader(noiseComputeShader, kernelHandle, resultTexture);

        Texture2D result = ReadRenderTexture(resultTexture);
        Color[] pixels = result.GetPixels();
        float noise = 0;

        foreach (Color pixel in pixels)
        {
            noise += pixel.r;
        }

        var mean = noise / pixels.Length;
        var variance = 0f;
        foreach (Color pixel in pixels)
        {
            variance += Mathf.Pow(pixel.r - mean, 2) /  pixels.Length;
        }
        float standardDeviation = Mathf.Sqrt(variance);

        var res= "mean "+ mean;
        res+=" variance "+ variance;
        res+=" standard deviation "+ standardDeviation;
        Destroy(resultTexture);
        return res;
    }
    RenderTexture CreateResultTexture()
    {
        RenderTexture resultTexture = new RenderTexture(inputTexture.width, inputTexture.height, 0, RenderTextureFormat.ARGBFloat);
        resultTexture.enableRandomWrite = true;
        resultTexture.Create();
        return resultTexture;
    }

    void DispatchComputeShader(ComputeShader shader, int kernelHandle, RenderTexture resultTexture)
    {
        shader.SetTexture(kernelHandle, "Input", inputTexture);
        shader.SetTexture(kernelHandle, "Result", resultTexture);
        shader.Dispatch(kernelHandle, inputTexture.width / 8, inputTexture.height / 8, 1);
    }

    Texture2D ReadRenderTexture(RenderTexture renderTexture)
    {
        RenderTexture.active = renderTexture;
        Texture2D result = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBAFloat, false);
        result.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        result.Apply();
        RenderTexture.active = null;
        return result;
    }
}