using System;
using UnityEngine;
using Mono.Cecil;

namespace Weaver
{
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
