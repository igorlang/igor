.. _diagram_schema:

**************************
    Diagram Schema
**************************

What is Diagram Schema?
=======================

Form editors that tools like :ref:`hercules` can display are not the best fit for data containing graphs and trees,
especially with multiple cross references. :ref:`hercules` is capable to display graph data as diagrams, if *diagram
schema* is provided.

The typical usage examples of diagrams in the data-driven game design approach are:
* AI behavior trees
* Flow diagrams of scripted missions and abilities
* Character progression trees (skills, upgrades)

*Diagram schema* is the extension of *Igor schema* that provides extra information on Igor types that can be used to
display *Hercules* diagram edilor.

*Diagram schema* target is called ``diagram``. Use it as attribute target and CLI option target name.

Diagram Schema Format
=====================
 
Technically Diagram Schema is a JSON document of a certain layout. This layout can be described with Igor language itself.
This description is available here: https://raw.githubusercontent.com/igorlang/igor/refs/heads/main/source/Igor.Schema/diagram_schema.igor .

Diagrams consist of blocks and connectors. Block is described with a record (usually a variant record) which is called a block *prototype*.

Blocks also contain *connectors*. Connectors of different blocks can be linked together, if they are compatible.

See :ref:`diagram_structure` for details.

Generating Diagram Schema
=========================

You can use :ref:`compiler` to generate diagram schema from the set of Igor files using the following command:

.. code-block:: batch

   igorc.exe -diagram_schema *.igor

This command will generate ``diagram_schema.json`` file in the current folder. You can override file name with ``-output-file``
command line option. See more information on available command line options here: :ref:`cli_options`.

.. seealso:: :ref:`upload_schema`
