using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Weaver
{
    [CreateAssetMenu(fileName="Weaver Settings")]
    public class WeaverSettings : ScriptableObject // SerializedWeaver<WeaverSettings>
    {
        [SerializeField]
        private List<WeavedAssembly> m_WeavedAssemblies;
    }
}
