## Property Changed Component

Much as the name implies the property changed component gives you the ability to get a callback whenever your properties get changed. 

## Requirements
1) The `[OnChanged(string callbackName)]` is applied to the property.
2) There is a setter on the property. This is where the callback is injected
3) There is a getter for the property. Because a property can contain complex logic we use to this check the `value` field in the setter. 
4) The `string callbackName` function exists in the same class.
5) The callback function takes zero or one argument. 
6) If the callback function has one argument it must be the same type as the property. 



## How it works
In the following example we have a person class with a single `age` property.
```csharp
using UnityEngine;

public class Person
{
    private int m_Age;
    
    [OnChanged("OnBeforeAgeChanged")]
    public int age
    {
        get { return m_Age; }
        set { m_Age = value; }
    }

    ///<summary>
    /// Invoked before the value of age is set.
    ///</summary>
    private void OnBeforeAgeChanged(int newAge)
    {
        string message = "The person was ";
        message += m_Age.ToString();
        message += " but now is ";
        message += newAge.ToString();
        message += ".";
        Debug.Log(message);
    }
}
```
This will be compiled down and before the assembly is reloaded by Unity Weaver will modify it. For this component we only modify the property.

```csharp
public class Person
{
    private int m_Age;
    
    public int age
    {
        get { return m_Age; }
        set 
        {
            if(age != value)
            {
                OnBeforeAgeChanged(value); 
            }
             m_Age = value;
        }
    }

    ///<summary>
    /// Invoked before the value of age is set.
    ///</summary>
    private void OnBeforeAgeChanged(int newAge)
    {
        string message = "The person was ";
        message += m_Age.ToString();
        message += " but now is ";
        message += newAge.ToString();
        message += ".";
        Debug.Log(message);
    }
}
```
As you can see we removed the `OnChangedAttribute` and injected a `if(age != value)` check. 


## Notes
* The property or it's fields can have any access modifier. 
* The callback function can have any access modifier. 
* The callback is invoked before setting the field. So you can check what the previous value of the property was by using the getter. 
* The callback can be an instance or static function.
* The callback is injected as a direct reference and does not use reflection. 
* The `if( [getter] != value)` is always injected as the first thing in the setter. So if you have any extra logic in your setter this will happen after the callback. 
