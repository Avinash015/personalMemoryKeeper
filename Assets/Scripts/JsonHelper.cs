using UnityEngine;

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        // Ensure it's wrapped correctly for Unity's JsonUtility
        string wrappedJson = "{ \"items\": " + json + " }";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
        return wrapper.items ?? new T[0]; // Ensure it never returns null
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}
