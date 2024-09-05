using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> executionQueue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (!instance)
        {
            instance = FindObjectOfType<UnityMainThreadDispatcher>();
            if (!instance)
            {
                var obj = new GameObject("UnityMainThreadDispatcher");
                instance = obj.AddComponent<UnityMainThreadDispatcher>();
            }
        }
        return instance;
    }

    private static UnityMainThreadDispatcher instance = null;

    void Update()
    {
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                executionQueue.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(IEnumerator action)
    {
        lock (executionQueue)
        {
            executionQueue.Enqueue(() => { StartCoroutine(action); });
        }
    }

    public void Enqueue(Action action)
    {
        Enqueue(ActionWrapper(action));
    }

    IEnumerator ActionWrapper(Action a)
    {
        a();
        yield return null;
    }
}
