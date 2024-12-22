.. _compiler_commands:

***************************
Commands
***************************

When Igor compiler executable is run, a command is executed. The default command is **compile**, but can be overridden using ``-m`` cli option.

This section covers built-in commands, but custom commands can be created using extension scripts.

=====================
Built-In Commands
=====================

**compile** 
-------------------

**compile** command compiles Igor source files.

If ``-target`` (``-t``) option is provided, target files are generated.

**compile** command is the default one and may be omitted.

**list-commands** 
-------------------

**list-commands** command outputs the list of available commands.

Example:

.. code-block:: batch

   igorc.exe -m list-commands

Output:

.. code-block:: batch

   compile
   list-commands
   list-targets
   list-attributes

**list-targets** 
-------------------

**list-commands** command outputs the list of available targets.

Example:

.. code-block:: batch

   igorc.exe -m list-targets

**list-attributes** 
-------------------

**list-attributes** command outputs the list of available common attributes. If ``-target`` (``-t``) option is provided, target-specific attributes are also displayed.

Example:

.. code-block:: batch

   igorc.exe -m list-attributes -t csharp
