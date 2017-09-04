using UnityEngine;
using Weaver;

public class ExampleBehaviour : MonoBehaviour
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

    private void EmptyMethod()
    {
        // Nothing Here
    }
}
