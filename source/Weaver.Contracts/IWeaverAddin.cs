using Mono.Cecil;
using Weaver.Contracts.Diagnostics;
using Weaver.Core;

namespace Weaver.Contracts
{
    /// <summary>Implemented by all addins allowing them to interface with Weaver itself.</summary>
    public interface IWeaverAddin
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the logger used for writing to output. 
        /// </summary>
        ILogger Logger { get; set; }

        /// <summary>
        /// Gets or sets the affected definitions. Only the Visit methods of the
        /// types returned by this will be invoked. 
        /// </summary>
        DefinitionType AffectedDefinitions { get; }

        /// <summary>I
        /// nvoked whenver we enter the specified assembly definition.
        /// </summary>
        /// <param name="assemblyDefinition">The assembly definition.</param>
        void VisitAssembly(AssemblyDefinition assemblyDefinition);

        /// <summary>I
        /// nvoked whenver we enter the specified module definition.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        void VisitModule(ModuleDefinition moduleDefinition);

        /// <summary>
        /// Invoked whenver we enter the specified type definition.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        void VisitType(TypeDefinition typeDefinition);

        /// <summary>
        /// Invoked whnever we enter the specified method definition.
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        void VisitMethod(MethodDefinition methodDefinition);

        /// <summary>
        /// Invoked whenver we enter the specified property definition.
        /// </summary>
        /// <param name="propertyDefinition">The property definition.</param>
        void VisitProperty(PropertyDefinition propertyDefinition);

        /// <summary>
        /// Invoked whenver we enter the specified field definition.
        /// </summary>
        /// <param name="fieldDefinition">The field definition.</param>
        void VisitField(FieldDefinition fieldDefinition);

        /// <summary>
        /// Invoked whenver we enter the specified event definition.
        /// </summary>
        /// <param name="fieldDefinition">The event definition.</param>
        void VisitEvent(EventDefinition eventDefinition);
    }
}
