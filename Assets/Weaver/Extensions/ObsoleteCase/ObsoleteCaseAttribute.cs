using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Marks the program elements that are no longer in use.
/// </summary>
[AttributeUsage(
    AttributeTargets.Interface |
    AttributeTargets.Event |
    AttributeTargets.Field |
    AttributeTargets.Property |
    AttributeTargets.Method |
    AttributeTargets.Struct |
    AttributeTargets.Class,
    Inherited = false)]
public class ObsoleteCaseAttribute : Attribute
{
    /// <summary>
    /// The text string that describes alternative workarounds.
    /// </summary>
    public string message { get; set; }

    /// <summary>
    /// An information Version telling the user when the Member will be removed.
    /// </summary>
    /// <remarks>
    /// When the assembly version is equal to or higher than this value the <see cref="ObsoleteAttribute.IsError"/> will be marked to true.
    /// Must be convertible to a <see cref="Version"/>.
    /// </remarks>
    public string treatAsErrorFromVersion { get; set; }

    /// <summary>
    /// An information Version telling the user when the Member will be removed.
    /// </summary>
    /// <remarks>
    /// If the assembly version is equal to or higher than this value then a compile error will thrown since it should not exist in the assembly anymore.
    /// Must be convertible to a <see cref="Version"/>.
    /// </remarks>
    public string removedInVersion { get; set; }

    /// <summary>
    /// A value pointing to the name of the replacement member if available.
    /// </summary>
    public string replacementTypeOrMember { get; set; }
}
