using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weaver
{
    public interface ILogable
    {
        Object context { get; }
        string label { get; }
    }
}
