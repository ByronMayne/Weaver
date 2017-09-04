using System;
using UnityEngine;
using Mono.Cecil;

namespace Weaver
{
    [CreateAssetMenu(menuName = "Weaver/Addins/Property Changed", fileName ="Property Changed Addin")]
    public class PropertyChangedAddin : WeaverPlugin
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
