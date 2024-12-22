**********
 Overview
**********

Lua Target
==========

Lua target name is ``lua``. It must be used for the attribute target for attributes
controlling Lua code generation:

.. code-block:: igor

    [lua enabled]

It is also used as the :ref:`compiler` command line argument for the ``-target`` option:

.. code-block:: batch

    igorc.exe -t lua *.igor
