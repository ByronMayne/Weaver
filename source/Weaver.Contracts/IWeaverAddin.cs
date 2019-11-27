using Mono.Cecil;
using Weaver.Contracts.Diagnostics;

namespace Weaver.Contracts
{
    /// <summary>Implemented by all addins allowing them to interface with Weaver itself.</summary>
    public interface IWeaverAddin
    {
        /// <summary>
        /// Gets or sets the logger used for writing to output. 
        /// </summary>
        ILogger Logger { get; set; }

        /// <summary>
        /// Invoked before all Addins have started their weaving process.
        /// </summary>
        void BeforeWeave();

        /// <summary>I
        /// nvoked whenver we enter the specified module definition.
        /// </summary>
        /// <param name="moduleDefinition">The module definition.</param>
        void Visit(ModuleDefinition moduleDefinition);

        /// <summary>
        /// Invoked whenver we enter the specified type definition.
        /// </summary>
        /// <param name="typeDefinition">The type definition.</param>
        void Visit(TypeDefinition typeDefinition);

        /// <summary>
        /// Invoked whnever we enter the specified method definition.
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        void Visit(MethodDefinition methodDefinition);

        /// <summary>
        /// Invoked whenver we enter the specified property definition.
        /// </summary>
        /// <param name="propertyDefinition">The property definition.</param>
        void Visit(PropertyDefinition propertyDefinition);

        /// <summary>
        /// Invoked whenver we enter the specified field definition.
        /// </summary>
        /// <param name="fieldDefinition">The field definition.</param>
        void Visit(FieldDefinition fieldDefinition);

        /// <summary>
        /// Invoked whenver we enter the specified event definition.
        /// </summary>
        /// <param name="fieldDefinition">The event definition.</param>
        void Visit(EventDefinition eventDefinition);

        /// <summary>
        /// Invoked after all Addins have finished their weaving process.
        /// </summary>
        void AfterWeave();
    }
}
