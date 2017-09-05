using System;
using UnityEngine;
using Mono.Cecil;

namespace Weaver
{
    [CreateAssetMenu(menuName = "Weaver/Extensions/Property Changed", fileName = "Property Changed Extension")]
    public class PropertyChangedExtension : WeaverExtension
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
