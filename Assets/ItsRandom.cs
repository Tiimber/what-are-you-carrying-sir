using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class ItsRandom {

    public const string DEFAULT_TYPE = "generic";

    public static Dictionary<String, System.Random> random = new Dictionary<String, System.Random>(){
        {"generic", new System.Random()}
    };

    public static Dictionary<String, List<String>> randomSets = new Dictionary<string, List<string>>();

    public static void setRandomSeeds(List<int> randomSeeds, string parentType = DEFAULT_TYPE) {
        if (ItsRandom.randomSets.ContainsKey(parentType)) {
            ItsRandom.randomSets.Remove(parentType);
        }

        List<System.Random> seededRandoms = new List<System.Random>();
        foreach (int seed in randomSeeds) {
            seededRandoms.Add(new System.Random(seed));
        }

        int i = 1;
        List<String> ids = new List<string>();
        foreach (Random randomObj in seededRandoms) {
            String currentId = parentType + "_" + i.ToString("D3");
            random.Add(currentId, randomObj);
            ids.Add(currentId);
            i++;
        }
        
        ItsRandom.randomSets.Add(parentType, ids);
    }

    public static String popRandomTypeForParentType(string parentType = DEFAULT_TYPE) {
        if (ItsRandom.randomSets.ContainsKey(parentType) && randomSets[parentType].Count > 0) {
            List<String> randomsForType = randomSets[parentType];
            String first = randomsForType.First();
            randomsForType.RemoveAt(0);
            return first;
        }
        
        return "no-seed-found";
    }

    public static void setRandomSeed(int randomSeed, string type = DEFAULT_TYPE) {
        if (ItsRandom.random.ContainsKey(type)) {
            ItsRandom.random.Remove(type);
        }
        ItsRandom.random.Add(type, new System.Random(randomSeed));
    }

    private static System.Random getRandomObj(string type) {
        if (ItsRandom.random.ContainsKey(type)) {
            return ItsRandom.random[type];
        }
        return new Random();
    }

    public static T pickRandom<T>(List<T> list, string type = DEFAULT_TYPE) {
        if (list.Count > 0) {
            return list[ItsRandom.randomRange(0, list.Count, type)];
        }
        return default(T);
    }

    //	public static string pickRandom(List<string> strings) {
    //		return strings[ItsRandom.randomRange(0, strings.Count - 1)];
    //	}

    public static V pickRandomValue<K, V>(Dictionary<K, V> dict, string type = DEFAULT_TYPE) {
        if (dict.Count > 0) {
            return dict.ElementAt(ItsRandom.randomRange(0, dict.Count, type)).Value;
        }
        return default(V);
    }

    public static K pickRandomKey<K, V>(Dictionary<K, V> dict, string type = DEFAULT_TYPE) {
        if (dict.Count > 0) {
            return dict.ElementAt(ItsRandom.randomRange(0, dict.Count, type)).Key;
        }
        return default(K);
    }

    public static GameObject pickRandomWithWeights(List<int> weights, List<GameObject> gameObjects, string type = DEFAULT_TYPE) {
        int weightSum = weights.Sum();
        int random = ItsRandom.randomRange(0, weightSum, type);
        int index;
        int accumulatedSum;
        for (index = 0, accumulatedSum = 0; index <= weights.Count; accumulatedSum += weights[index], index++) {
            if (accumulatedSum + weights[index] > random) {
                break;
            }
        }
        return gameObjects[index];
    }

    public static float randomPlusMinus(float medium, float plusMinus, string type = DEFAULT_TYPE) {
        return ItsRandom.randomRange(medium - plusMinus, medium + plusMinus, type);
    }

    public static float randomRange(float min, float max, string type = DEFAULT_TYPE) {
        double value = getRandomObj(type).NextDouble();
        float randomVal = min + (max - min) * (float)value;
        ItsRandom.PrintStack(randomVal, type);
        return randomVal;
    }

    private static void PrintStack<T>(T randomVal, string type = DEFAULT_TYPE) {
        StackTrace stackTrace = new StackTrace();           // get call stack
        StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

        string originMethodName = "?";
        for (int i = 1; i < stackFrames.Length; i++) {
            StackFrame stackFrame = stackFrames[i];
            if (stackFrame.GetMethod().Name.Contains("random") || stackFrame.GetMethod().Name.Contains("Random")) {
                continue;
            }
            originMethodName = stackFrame.GetMethod().Name;
            break;
        }
        // write call stack method names
        UnityEngine.Debug.Log(type + " - " + originMethodName + ", Val: " + randomVal.ToString());   // write method name
//        UnityEngine.Debug.Log(randomVal.ToString());
    }

    public static int randomRange(int min, int max, string type = DEFAULT_TYPE) {
        int randomVal = getRandomObj(type).Next(min, max);
        ItsRandom.PrintStack(randomVal, type);
        return randomVal;
    }

    public static bool randomBool(string type = DEFAULT_TYPE) {
        bool randomVal = getRandomObj(type).NextDouble() < 0.5d;
        ItsRandom.PrintStack(randomVal, type);
        return randomVal;
    }

    public static object randomTime(string type = DEFAULT_TYPE) {
        return ItsRandom.randomRange(0, 23, type) + ":" + ItsRandom.randomRange(0, 59, type);
    }
}
