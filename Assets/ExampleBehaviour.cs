using UnityEngine;
using Weaver;

public class ExampleBehaviour : MonoBehaviour
{
    private string m_PlayerName; 
    private GameObject m_MainPlayer;

    [ProfileSample]
    public GameObject GetPlayByName()
    {
        if (m_PlayerName == "MainPlayer")
        {
            return m_MainPlayer;
        }

        return null; 
    }
}
