*************************
    Custom Attributes
*************************

Extension scripts usually rely on custom attributes to control their behaviour. A script can define any amount of attributes.

Defining custom attributes
==========================

In order to support custom attributes, extension script should provide an attribute descriptor.

Here is the attribute descriptor definition example:

.. code-block:: C#

    namespace Igor.TypeScript
    {
        [CustomAttributes]
        public class MyCustomAttributes
        {
            public static readonly BoolAttributeDescriptor MyAttribute =
                new BoolAttributeDescriptor("my_attribute", IgorAttributeTargets.Any, AttributeInheritance.Scope);
        }
    }

Attribute descriptors should be defined as **public** **static** **readonly** fields.
The class containing them should be marked with ``[CustomAttributes]`` attribute. 
It can be the same class that implements generator extension interface or a different one.

To define a custom attribute descriptor, you need to know the type, the name, the usage target and the inheritance type.

The following attribute types are supported:

=============== =======================================
Attribute type  AttributeDescriptor type               
=============== =======================================
bool            BoolAttributeDescriptor                
int             IntAttributeDescriptor                 
double          FloatAttributeDescriptor               
string          StringAttributeDescriptor              
enum            EnumAttributeDescriptor<EnumType>    
=============== =======================================

See :ref:`attribute_inheritance` for inheritance types.

Accessing attribute value
=========================

Use Attribute function on an AST type to access attribute value:

.. code-block:: C#

    // Provide the default value
    bool myValue = ast.Attribute(MyCustomAttributes.MyAttribute, false);

    // No default value, returns nullable
    bool? myValue = ast.Attribute(MyCustomAttributes.MyAttribute);

Enum attributes
===============

To support custom enum attributes, extension script should declare **enum** type first and then provide an instance of *EnumAttributeDescriptor*:

.. code-block:: C#

    enum MyAttributeValue
    {
        // by default, Igor attribute value is lowercased: none
        None,

        // provide explicit Igor attribute value
        [IgorEnumValue("my_value")]
        MyValue,
    }

    [CustomAttributes]
    public class MyCustomAttributes
    {
        public static readonly EnumAttributeDescriptor<MyAttributeValue> MyAttribute =
            new EnumAttributeDescriptor<MyAttributeValue>("my_attribute", IgorAttributeTargets.Any);
    }

Usage example:

.. code-block:: igor

    [* my_attribute=my_value]
    module ScriptSample
    {
    }
