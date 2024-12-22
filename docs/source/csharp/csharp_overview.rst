**********
Overview
**********

C# Target
=========

C# target name is ``csharp``. It must be used for the attribute target for attributes
controlling C# code generation:

.. code-block:: igor

    [csharp namespace="Protocol"]

It is also used as the :ref:`compiler` command line argument for the ``-target`` option:

.. code-block:: batch

    igorc.exe -t csharp *.igor

``-cs`` option is the shortcut ``-t csharp``.

C# Versions
===========

Igor can use C# features of recent versions in generated code. Currently the minimum supported version is C# 4.0. The default is 5.0. 
You can enable features of newer versions with ``-target-version`` command line argument.

Example:

.. code-block:: batch

    igorc.exe -cs -target-version 7.1 *.igor

Here's the list of C# features that Igor uses:

======================= =========== ===========================================================
Feature                 Version     Example
======================= =========== ===========================================================
Async tasks             5.0+        ``Task<RpcResult>``
Index initializers      6.0+        ``new Dictionary<int, string> { [0] = "zero" };``
**nameof**              6.0+        ``throw new ArgumentNullException(nameof(arg))``
Readonly properties     6.0+        ``public bool Value { get; }``
**out var**             7.0+        ``dict.TryGetValue(key, out var value)``
**is null**             7.0+        ``value is null``
Default literals        7.1+        ``async Task ProcessAsync(CancellationToken ct = default)``
======================= =========== ===========================================================
