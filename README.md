# Weaver
A a code weaver built for use in [Unity Engine](https://unity3d.com/). Based very heavily off of [Fody](https://github.com/Fody/Fody). Implmented using [Mono.Cecil](http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cecil/).



### What is Code Weaving?
Weaving refers to the process of injecting functionality into an existing program. This can be done conceptually at a number of levels:

* Source code weaving would inject source code lines before the code is compiled
* IL weaving (for .NET) adds the code as IL instructions in the assembly


A great example of this is the [Unity Project Updater](https://docs.unity3d.com/Manual/APIUpdater.html). It uses both versions of code weaving on your project. It uses text replacing for unsupported code in your source files and IL weaving for compiled dlls.

Weaver for now only uses IL Weaving which runs every time one of your project dynamic link libraries are compiled. 

### Features
* Hooked into Unity callbacks to run automatically. 
* Filter only the assemblies you want to weave. 
* Easy to extend using [ScriptableObjects](https://docs.unity3d.com/ScriptReference/ScriptableObject.html).

## Current Addins
* [**Method Timer**](./Assets/Weaver/Addins/MethodTimer/) - Any method with the `MethodTimerAttribute` will be timed using [Stopwatch](https://msdn.microsoft.com/en-us/library/system.diagnostics.stopwatch(v=vs.110).aspx) and logged to the console. 

#### In Development
* **Property Changed** : Invoke a callback whenever a property has changed in your class. 


## Meta

Handcrafted by Byron Mayne [[twitter](https://twitter.com/byMayne) &bull; [github](https://github.com/ByronMayne)]

Released under the [MIT License](http://www.opensource.org/licenses/mit-license.php).

If you have any feedback or suggestions for Weaver feel free to contact me. 