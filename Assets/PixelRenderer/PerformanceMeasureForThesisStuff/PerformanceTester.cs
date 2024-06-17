using System.Collections;
using System.Collections.Generic;
using System.IO;
using Shaders.PerformanceMeasureForThesisStuff;
using UnityEditor;
using UnityEngine;

public class PerformanceTester : MonoBehaviour
{
    public GameObject objectToSpawn;
    public Transform parentObject;
    public int[] spawnCounts = { 100, 500, 1000 }; // Array of counts to test
    public float spawnInterval = 0.5f; // Interval between spawns in seconds

    private int currentTestIndex = 0;
    private int objectsSpawned = 0;
    private float fpsSum = 0;
    private int frameCount = 0;
    private string resultFileName;

    void Start()
    {
        // Initialize the result file name with a given string and the current date
        string testName = "PerformanceTest"; // Change this to your desired name
        string date = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        resultFileName = $"{testName}_{date}.txt";
        StartCoroutine(TestAllCombinations());
        // Start the performance test coroutine
    }

    public List<NamedMonobehaviour> postProcessingScripts;
    public ImageQualityMetrics Testing;


    IEnumerator TestAllCombinations()
    {
        int combinationCount = 1 << postProcessingScripts.Count;
        for (int i = 0; i < combinationCount; i++)
        {
            EnableScripts(i);
            yield return new WaitForEndOfFrame();

            Testing.CaptureScreen();

            var colorfulness = Testing.CalculateColourfulness();
            var contrast = Testing.CalculateContrast();
            var sharpness = Testing.CalculateSharpness();
            var noise = Testing.CalculateNoise();

            Debug.Log(
                $"Comb {GetNameOfCombination(i)}: Colorfulness={colorfulness}, Contrast={contrast}, Sharpness={sharpness}");
            SaveResults(
                $"For combination {GetNameOfCombination(i)}:\n noise={noise};\n Colorfulness={colorfulness};\n Contrast={contrast};\n Sharpness={sharpness}\n");
            yield return PerformanceTestCoroutine();
        }

        Debug.Log("Performance testing completed!");
    }

    void EnableScripts(int combination)
    {
        if ((combination & (1 << 0)) != 0 && (combination & (1 << 1)) != 0)
        {
            return;
        }
        if ((combination & (1 << 2)) != 0 && (combination & (1 << 3)) != 0)
        {
            return;
        }
        for (int j = 0; j < postProcessingScripts.Count; j++)
        {
            postProcessingScripts[j].enabled = (combination & (1 << j)) != 0;
        }
    }

    string GetNameOfCombination(int combination)
    {
        string names = "";
        for (int j = 0; j < postProcessingScripts.Count; j++)
        {
            if ((combination & (1 << j)) != 0)
            {
                names += postProcessingScripts[j].Name + " ";
            }
        }

        return names;
    }

    IEnumerator PerformanceTestCoroutine()
    {
        currentTestIndex = 0;
        objectsSpawned = 0;
        while (currentTestIndex < spawnCounts.Length)
        {
            int spawnCount = spawnCounts[currentTestIndex];

            // Spawn objects
            for (int i = 0; i < spawnCount; i++)
            {
                Instantiate(objectToSpawn, Random.insideUnitSphere * 5f, Quaternion.identity, parentObject);
                objectsSpawned++;

                // Wait for a short interval to avoid freezing the editor
                yield return new WaitForSeconds(spawnInterval + (currentTestIndex * 0.1f));
            }

            // Measure performance
            yield return StartCoroutine(MeasurePerformance(objectsSpawned));

            // Move to the next test
            currentTestIndex++;
        }

        yield return DestroyObjects(parentObject);
        Debug.Log($"Tested for {currentTestIndex}");
        // All tests completed
    }

    public IEnumerator DestroyObjects(Transform t)
    {
        Debug.Log("AAAAAAAAAAA");
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            yield return new WaitForSeconds(0.02f);
            Destroy(t.GetChild(i).gameObject);
        }

        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator MeasurePerformance(int spawnCount)
    {
        // Reset performance counters
        fpsSum = 0;
        frameCount = 0;

        // Measure performance over a period of time (e.g., 5 seconds)
        float measureDuration = 5f;
        float startTime = Time.time;

        while (Time.time - startTime < measureDuration)
        {
            // Calculate FPS
            float currentFPS = 1.0f / Time.deltaTime;
            fpsSum += currentFPS;
            frameCount++;

            yield return null;
        }

        float averageFPS = fpsSum / frameCount;
        int batchCount = UnityStats.batches;

        // Save results to a file
        SaveResults(spawnCount, averageFPS, batchCount);
    }

    void SaveResults(string str)
    {
        string result = $"{str}\n";

        string path = Path.Combine(Application.dataPath, resultFileName);
        using (StreamWriter writer = new StreamWriter(path, true))
        {
            writer.WriteLine(result);
        }

        Debug.Log($"Image results saved to {path}");
    }

    void SaveResults(int spawnCount, float averageFPS, int batchCount)
    {
        string result = $"Spawn Count: {spawnCount}\nAverage FPS: {averageFPS}\nBatch Count: {batchCount}\n";

        string path = Path.Combine(Application.dataPath, resultFileName);
        using (StreamWriter writer = new StreamWriter(path, true))
        {
            writer.WriteLine(result);
        }

        Debug.Log($"Results saved to {path}");
    }
}