using UnityEngine;

public class ExampleBehaviour : MonoBehaviour
{
    public void OnEnable()
    {
        new GameObject("OnEnable");
    }

    public void OnDisable()
    {
        GameObject go = new GameObject("OnDisable");
        go.name = "Test";
    }

    public void Start()
    {
    }

    public void Awake()
    {
    }
}
