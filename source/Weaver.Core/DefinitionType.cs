using System;

namespace Weaver.Core
{
    /// <summary>
    /// Contains all the definition types supported by Mono.Cecil
    /// </summary>
    [Flags]
    public enum DefinitionType
    {
        None = 0,
        Assembly = 1 << 1,
        Modules = 1  << 2,
        Type = 1 << 3,
        Field = 1 << 4,
        Method = 1 << 5,
        Property = 1 << 6,
        Event = 1 << 7,
    }
}
