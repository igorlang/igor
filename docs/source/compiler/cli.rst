***************************
Command Line Interface
***************************

.. _cli_options:

Command Line Options
====================

Here is the list of options supported by Igor Compiler.

Source file
-----------

Any non-option string is treated as an Igor source file. You can specify several files (separated by whitespaces) and file masks (with ``*`` and ``?`` wildcards).

See ``-p`` option for description of how files are searched.

Source path
-----------

* -p ``PATH``
* -path ``PATH``

Add ``PATH`` to the list of paths to search for source files. You can specify multiple ``-p`` options, their order will be respected. If source file is not found in any of ``-p`` paths,
it is searched in the working directory.

Import file
-----------

* -i ``FILE``
* -import ``FILE``

Specifies import files with modules used by other source files. Code generation is not performed for import files. You can use file masks (with ``*`` and ``?`` wildcards).

See ``-I`` option for description of how files are searched.

Import path
-----------

* -I ``PATH``
* -import-path ``PATH``

Add ``PATH`` to the additional list of paths to search for import files. You can specify multiple ``-I`` options, their order will be respected. 

Here is the order in which modules are searched when processing **using** statements:

* Source path (see ``-p`` option)

* Import path

* Working directory

Script file
-----------

* -x ``FILE``
* -script ``FILE``

Specifies custom generator C# files. You can use file masks (with ``*`` and ``?`` wildcards).

Script path
-----------

* -X ``PATH``
* -script-path ``PATH``

Add ``PATH`` to the additional list of paths to search for script files provided with ``-script``/``-x`` option. 
You can specify multiple ``-X`` options, their order will be respected. At last attempt the working directory is searched.

Lib file
-----------

* -lib ``FILE``

Specifies custom generator DLL. You can use file masks (with ``*`` and ``?`` wildcards).

Lib path
-----------

* -lib-path ``PATH``

Add ``PATH`` to the additional list of paths to search for DLL files provided with ``-lib`` option. 
You can specify multiple ``-lib-path`` options, their order will be respected. At last attempt the working directory is searched.

Roslyn path
-----------

* -roslyn ``PATH``

Provide path to Roslyn C# compiler folder for script compilation. This folder should contain ``csc.exe`` executable. If this option is not provided,
only C# 4.0 can be used for scripts.

Command
-------

* -m ``COMMAND``
* -command ``COMMAND``

Execute the ``COMMAND`` command. See :ref:`compiler_commands` for details.

Target
------

* -t ``TARGET``
* -target ``TARGET``

Generate files for ``TARGET`` generator, e.g. **csharp**, **erlang**, etc.

C# target
---------

* -cs
* -csharp

Generate C# files at output path. Shortcut for ``-t csharp``.

Erlang target
-------------

* -erl
* -erlang

Generate Erlang files. Shortcut for ``-t erlang``.

Igor Compiler will put ``*.erl`` files into ``PATH/src/`` folder and ``*.hrl`` files into ``PATH/include/`` folder, where ``PATH`` is output path.

Schema target
-------------

Generate ``schema.json`` file with Igor Schema (see :ref:`schema`). Shortcut for ``-t schema``.

Diagram schema target
---------------------

Generate ``diagram_schema.json`` file with Diagram Schema (see :ref:`diagram_schema`). Shortcut for ``-t diagram``.

Standard input
--------------

* -stdin

Input source from standard input.

Standard output
----------------

* -O
* -stdout

Print generated files to standard output.

Output folder
-------------

* -o ``PATH``
* -output ``PATH``

Setup output folder path. If not specified, defaults to working directory.

Output file
-----------

* -output-file ``PATH``

Setup output file path, if the target generates a single file (e.g. *schema* target).

Target version
--------------

* -target-version ``VERSION``

Provides the target version if applicable. For example for C# target this is the C# language version (e.g. 6.0). 
Igor will do its best to use modern features for newer versions and respect version limitations for older versions.

Bom
---

* -bom

Include Unicode BOM (Byte order mark) header in the beginning of output files. Turned off by default.

Global attributes
-----------------

* -set:``boolean_attribute``
* -set:``"attribute=value"``

Sets global attribute value. You can set several attributes by providing ``set`` option several times.

Verbose mode
------------

* -v
* -verbose

Display more verbose output. May be useful for troubleshooting.

Help
----

* -h
* -help

Show usage and exit.

Version
-------

* -V
* -version

Show version and exit.

Delete empty files
------------------

* -d
* -delempty

If Igor Compiler has to generate the file with no useful code (export, definitions, etc.), do not generate such a file but instead try to delete the old file if it exists.

Create output directory
-----------------------

* -mkdir

Allow to create output directory if it's missing.

Overwrite read only files
-------------------------

* -w

Allow to overwrite read only files.

Exit code
=========

``igorc.exe`` exits with code 0 if compilation succeeded, 1 otherwise.
