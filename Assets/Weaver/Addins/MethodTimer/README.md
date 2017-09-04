# Method Timer

Method Timer is an easy to use addin that can be used on any method. It creates a stopwatch in that method and logs the time it takes to execute. 


### Input
```csharp
using Weaver;
using UnityEngine;

public class ExampleClass : MonoBehaviour 
{
    public GameObject hand;

    [MethodTimer]
    private void Example() 
    {
        hand = GameObject.Find("Hand");
        hand = GameObject.Find("/Hand");
        hand = GameObject.Find("/Monster/Arm/Hand");
        hand = GameObject.Find("Monster/Arm/Hand");
    }
}
```

### Output
```csharp
public class ExampleBehaviour : MonoBehaviour
{
    public GameObject hand;

    [MethodTimer]
    private void Example()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        hand = GameObject.Find("Hand");
        hand = GameObject.Find("/Hand");
        hand = GameObject.Find("/Monster/Arm/Hand");
        hand = GameObject.Find("Monster/Arm/Hand");
        stopwatch.Stop();
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        Debug.Log("Example" + elapsedMilliseconds);
    }
}
```

### TODO:
* Right now it creates a new stopwatch in each function we should build a pool.