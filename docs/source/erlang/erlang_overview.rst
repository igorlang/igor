**********
 Overview
**********

Erlang Target
=============

Erlang target name is ``erlang``. It must be used for the attribute target for attributes
controlling Erlang code generation:

.. code-block:: igor

    [erlang enabled]

It is also used as the :ref:`compiler` command line argument for the ``-target`` option:

.. code-block:: batch

    igorc.exe -t erlang *.igor

``-erl`` option is the shortcut ``-t erlang``.

Output Files
============

As usual ``-o`` command line flag can be used to define output folder. 

.. code-block:: batch

    igorc.exe - t erlang -o myapp *.igor

By default ``*.hrl`` files are generated in the ``include`` folder and ``*.erl`` files are generated in the ``src`` folder.
In the example above those are ``myapp/include`` and ``myapp/src`` folders.

You can alter this behaviour by using ``include_path`` and ``src_path`` attributes.
They can be specified per module or in the command line, to apply globally.

.. code-block:: batch

    igorc.exe - t erlang -o myapp -set src_path='src/protocol' *.igor

You can also control individual module file names and locations by using attributes like ``file`` (for ``*.erl``) and ``hrl_file`` (for ``*.hrl``).
