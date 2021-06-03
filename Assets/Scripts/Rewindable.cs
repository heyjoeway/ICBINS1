using UnityEngine;
using System.Collections.Generic;

public class Rewindable<T> {
    int currentIndex;
    List<object> variables;
    float[] timestamps;

    public static Rewindable<T> Create(int maxEntries = 60 * 10) {
        Rewindable<T> instance = new Rewindable<T>();
        instance.variables = new List<object>(maxEntries);
        for (int i = 0; i < maxEntries; i++)
            instance.variables.Add(null);
            
        instance.timestamps = new float[maxEntries];
        instance.currentIndex = maxEntries - 1;
        return instance;
    }

    public int RewindToTime(float targetTime, bool apply = true) {
        int iClosest = currentIndex;
        float closestError = Mathf.Infinity;

        int i = currentIndex;
        while (true) {
            float newError = Mathf.Abs(targetTime - timestamps[i]);
            if (newError < closestError) {
                closestError = newError;
                iClosest = i;
                if (newError == 0) break;
            }

            i--;
            if (i == -1) i = variables.Count - 1;
            if (i == currentIndex) break;
        }
        
        if (apply) currentIndex = iClosest;
        return iClosest;
    }

    public int RewindByTime(float offsetTime, bool apply = true) {
        return RewindToTime(CurrentTimeStamp() - offsetTime, apply);
    }

    public int RewindByIndex(int indexOffset, bool apply = true) {
        int newIndex = (currentIndex - indexOffset) % timestamps.Length;
        while (newIndex < 0) newIndex += timestamps.Length;
        if (apply) currentIndex = newIndex;
        return newIndex;
    }

    public T GetFromIndex(int index) { return (T)variables[index]; }

    public float CurrentTimeStamp() { return timestamps[currentIndex]; }
    public T Current() { return (T)variables[currentIndex]; }
    public void Set(T value) {
        float timestamp = Time.realtimeSinceStartup;
        if (timestamp != CurrentTimeStamp()) {
            currentIndex += 1;
            currentIndex %= timestamps.Length;
        }
        timestamps[currentIndex] = timestamp;
        variables[currentIndex] = value;
    }
}