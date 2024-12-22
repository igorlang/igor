****************
    Equality
****************

Equals
======

Igor is capable of generating Equals and GetHashCode code for your records. Use ``equals`` attribute to enable that behaviour.

*Example:*

.. code-block:: igor

    [csharp struct equals]
    record TestStruct
    {
        int int_value;
        string string_value;
    }

The following C# code is generated:

.. code-block:: C#

    public struct TestStruct : System.IEquatable<TestStruct>
    {
        public int IntValue { get; set; }
        public string StringValue { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + IntValue.GetHashCode();
                hash = hash * 23 + (StringValue == null ? 0 : StringValue.GetHashCode());
                return hash;
            }
        }

        public override bool Equals(object other)
        {
            if (!(other is TestStruct))
                return false;
            return Equals((TestStruct)other);
        }

        public bool Equals(TestStruct other)
        {
            return IntValue == other.IntValue && StringValue == other.StringValue;
        }
    }

Both **object.Equals** and **IEquatable<T>.Equals** are generated.

Igor is smart enough to properly use generated nested fields' Equals and GetHashCode functions.

.. note:: If ``equals`` attribute is set on the variant, all records inherit it.

Equality Operators
==================

You can enable ``==`` and ``!=`` operator generating with ``equality`` attribute:

.. code-block:: igor

    [csharp struct equality]
    record TestStruct
    {
        int int_value;
        string string_value;
    }

The following C# operator overloads are generated:

.. .. code-block:: C#

        public static bool operator ==(TestStruct left, TestStruct right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TestStruct left, TestStruct right)
        {
            return !(left == right);
        }

.. note:: ``equality`` implies ``equals``, so Equals and GetHashCode are also generated.

