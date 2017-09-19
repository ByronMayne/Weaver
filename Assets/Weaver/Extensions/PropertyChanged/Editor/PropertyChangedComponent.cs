using System;
using UnityEngine;
using Mono.Cecil;

namespace Weaver
{
    [CreateAssetMenu(menuName = "Weaver/Components/Property Changed", fileName = "Property Changed Component")]
    public class PropertyChangedComponent : WeaverComponent
    {
        public override string addinName
        {
            get
            {
                return "Property Changed";
            }
        }
    }
}
