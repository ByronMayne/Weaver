using System;
using UnityEngine;
using Mono.Cecil;

namespace Weaver
{
    public abstract class WeaverPlugin : ScriptableObject
    {
        public abstract string addinName { get; }

        public abstract Type[] GetAttributes();

        public abstract void Initialize(ModuleDefinition module);

        public abstract void OnMatch(Type attributeType, MemberReference effectedMemeber);
    }
}
