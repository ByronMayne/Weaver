using UnityEngine;
using Weaver;

public class ExampleBehaviour : MonoBehaviour
{
    private float m_Height = 0f;

    [OnChanged("OnHeightChanged")]
    public float height
    {
        get { return m_Height; }
        set { m_Height = value; }
    }

    [OnChanged("OnAgeChanged", isValidated = true)]
    public int age { get; set; }

    public int otherAge
    {
        get { return age; }
        set
        {
            if (age != value)
            {
                OnAgeChanged(value);
            }
        }
    }

    public void Other()
    {

    }
    public void Start()
    {
        age = 23;
        height = 6.1f;
    }

    private void OnHeightChanged(float newHeight)
    {
        Debug.Log("Height changed from " + m_Height + " to " + newHeight);
    }

    private void OnAgeChanged(int newAge)
    {
        Debug.Log("Age changed from " + age + " to " + newAge);
    }

    private void OnValidate()
    {

    }
}
