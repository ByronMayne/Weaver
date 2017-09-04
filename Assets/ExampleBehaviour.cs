using UnityEngine;
using Weaver;

public class ExampleBehaviour : MonoBehaviour
{
    public void OnEnable()
    {
        new GameObject("OnEnable");
    }

    [MethodTimer]
    public void OnDisable()
    {
        GameObject go = new GameObject("OnDisable");
        go.name = "Test";
    }

    [MethodTimer]
    public void Start()
    {
    }

    public void Awake()
    {
    }
}
