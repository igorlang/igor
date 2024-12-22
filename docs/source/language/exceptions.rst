.. _exceptions:

******************
    Exceptions
******************

Exceptions are variants and records that describe errors. Use **exception** keyword instead of **record** to define an exception record:

*Example:*

.. code-block:: igor

    exception NeedMoreMinerals
    {
        int amount;
    }

To define exception variant, use **exception variant** keywords:

*Example:*

.. code-block:: igor

    enum ExceptionType
    {
        name_exists;
        private_message_delivery_failed;
    }

    exception variant ChatException
    {
        tag ExceptionType type;
    }

    exception ChatException.NameExistsException[name_exists]
    {
        string name;
    }

    exception ChatException.PrivateMessageDeliveryFailedException[private_message_delivery_failed]
    {
        string name;
    }

While exceptions may be used as regular records, there's a number of differences:

* Generic exceptions are not supported at the moment
* Target language generators may have special rules for exceptions. For example in C# exceptions have to be derived from ``System.Exception`` class.
* Exceptions are used in services to throw errors. See :ref:`services` for details.
