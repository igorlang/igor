.. _diagram_structure:

**************************
    Diagram Structure
**************************

Glossary
=========

.. glossary::

    Diagram card
        Diagram card is a card that describes a diagram. It should be marked with ``[diagram enabled]`` attribute. It should contain at least one collection of blocks, an optional collection of links, 
        and an arbitrary amount of fields that are not related to diagram representation.

    Block record
        Block record is a record (also called a ptototype) that represents a single shape in the diagram and toolbox. Block records should contain two mandatory fields: **ref** and **layout**,
        usually defined in the block variant ancestor. Block records should also have ``diagram archetype`` attribute defined. Instances (values) of block records
        are stored in collections in the diagram card.
    
    Block archetype
        Block archetype is a prdefined block shape supported by Hercules. There's a limited amount of supported archetypes. Archetype is defined with ``diagram archetype`` attribute. 
        Different block records (prototypes) can have the same or different archetypes.
    
    Block connector
        Connector is a slot on the block shape that can be connected with another connector of another block. Connectors are defined on block records (or block record fields) with a
        ``diagram connector`` attribute. Connectors have names that are unique within the block.
    
    Anchor
        Connector anchor is the unique identifier of block instance connector, which consists of block **ref** and connector name.

    Link
        Link is a directed connection between two block connectors of different blocks. There're *anchor links* and *asset links*, that can be mixed together in the same diagram. 
        But asset connectors cannot be linked to anchor connectors and vice versa.
    
    Anchor links
        Anchor links are stored in the **links** collection in the diagram card. Anchor links are stored as pairs of connector anchors - **from** and **to**. Anchor links allow to create arbitrary
        oriented graphs, when each block can have multiple inputs and outputs.

    Asset links
        Block record can have asset fields. Asset fields store references to target blocks, which are called *asset blocks*. Target connector name is not stored (which means that 
        an asset block can have only one *asset connector* - but still an arbitrary amount of anchor connectors and/or asset fields). Asset fields can be optional or required,
        they can also store multiple values (if they are lists). 
    
    Connector categories
        Connector can have a string category. If connector has a category, it can be linked only with another block connector of the same category. This shapes a simple type system.

Diagram Cards
==============

In order for card (see :ref:`cards`) to be displayed and edited as a diagram, it should be marked with ``diagram enabled`` attribute:

.. code-block:: igor

   [diagram enabled]
   record Card.CardDiagram // provide a meaningful type name instead
   {
       ...
   }

This card now represents the diagram, but toolbox is empty - we have not defined any blocks yet.

Diagram Blocks
==============

Blocks are defined as variant records. Each descendant record defines a single diagram shape (a toolbox entry). 

Block records should contain two mandatory fields: **ref** and **layout**, usually defined in the block variant ancestor.

.. code-block:: igor

    // type name may be altered, but keep content as is
    record Layout
    {
        double x = 0;
        double y = 0;
    }

    // this alias is not required and serves self-documentation purpose. It can be renamed or omitted
    define Ref atom;

    enum BlockType
    {
        example;
        ...
    }

    // provide a meaningful type name instead
    variant Block
    {
        Ref ref;           // Contains an auto-generated unique block id. Do NOT change the field name
        Layout layout;     // Contains block location. Do NOT change the field name.
        tag BlockType type;
    }

    [diagram archetype="box"]   // make it the diagram block and define the basic look
    record Block.BlockExample[example]
    {
        ...
    }

The example above defines an **example** block record. Other variant descendants can be added to add more block types.

.. note::
    Layout field is required for block location persistence, but it can also be useful to order blocks. For example, AI behavior trees
    are supposed to be read and processed left to right, application can use ``layout.x`` to find the proper order.

Now when blocks are defined, they should be stored in the diagram card:

.. code-block:: igor

   [diagram enabled]
   record Card.CardDiagram[diagram]
   {
       list<Block> blocks = []; // provide a meaningful field name instead
       ...
   }

Hercules will automatically detect all **list** fields containing diagram blocks. Most often there is one field, but there can be more,
if several block variants are defined.

Now we've got a diagram with shapes, but they still cannot be linked together. Block connectors should be defined.

.. _diagram_connectors:

Connectors and Links
====================

Each block can have an arbitrary number of connectors, and connectors can be connected with links.

There're two types of links: **anchor** links and **asset** links. They can be mixed together in the same diagram.

Connectors are defined using a ``diagram connector`` attribute. Connector attributes are *object attributes* and allow to specify
nested properties:

.. code-block:: igor

    [diagram connector=(name="OUT" type=out position="0.5,1")]

Position value is the required connector position on the block shape. It's the string with two comma-separated float values - 
x and y in relative coordinates, where "0,0" is the top-left corner of the block, "1,0" is the top-right corner,
"1,1" is the bottom-right corner, and so on.

.. note:: Prior to Igor 2.0.7 *object attributes* used braces instead of parentheses.

Anchor Links
------------

Each block can have an arbitrary number of **in** and **out** connectors.
**out** connector of one block can be linked to the **in** connector of another block. 
**in** connectors can have an arbitrary number of incoming links, and **out** connectors can have an arbitrary amount of outgoing links
(though identical links are prohibited).
Such links are stored as records, and diagram card utilizing anchor links should have a single **links** list field.

The following example defines **in** and **out** connectors on the **example** block (from the code snippet above).

.. code-block:: igor

    [diagram archetype="box"]
    [diagram connector=(name="IN" type=in position="0.5,0")] 
    [diagram connector=(name="OUT" type=out position="0.5,1")]
    record Block.BlockExample[example]
    {
        ...
    }

    // this alias is not required and serves self-documentation purpose. It can be renamed or omitted
    define SlotId atom;

    record Anchor  // type name can be altered, but keep content as is
    {
        Ref block;    // contains the auto-generated block id
        SlotId slot;  // contains the connector name ("IN" or "OUT" in this example)
    }

    record Link   // type name can be altered, but keep content as is
    {
        Anchor from;   // source anchor
        Anchor to;     // target anchor
    }

    [diagram enabled]
    record Card.CardDiagram[diagram]
    {
        list<Block> blocks = []; 
        list<Link> links = [];   // do NOT change the field name
        ...
    }

Now **example** blocks can be connected with anchor links. 

For anchor connector definitions type, name and position are mandatory. Type must be **in** or **out**. Name is an arbitrary string,
that is stored in the **slot** field of **Anchor** record.

Anchor links are the most flexible, cause they allow for an arbitrary amount of connectors and circular graphs. However, they are
also more complex for application processing. Asset links provide more simple but more limited functionality.

Asset Links
-----------

Instead of storing links in a separate array, target block ref can be stored in the source block field directly. Such fields are called
asset fields, and target blocks are called asset blocks.

The following example defines asset fields:

.. code-block:: igor

    [diagram archetype="box"]
    record Block.BlockExample[example]
    {
        [diagram connector=(name="A1" type=property position="1,0.2")]
        Ref required_asset; // required: it's an error not to provide a link
        
        [diagram connector=(name="A2" type=property position="1,0.4")]
        ?Ref optional_asset; // link is optional
        
        [diagram connector=(name="A3" type=property position="1,0.6")]
        list<Ref> list_of_assets = [];  // arbitrary amount of outgoing links
        
        [diagram connector=(name="A4" type=property position="1,0.8")]
        ?list<Ref> optional_list_of_assets;

        ...
     }

    [diagram archetype="barrel"]
    [diagram connector=(name="IN" type=asset position="0,0.5")]
    record Block.BlockAsset[asset]
    {
        ...
    }

Connector types used for asset links are **property** and **asset**. Note that **property** connectors are defined on fields, 
while **asset** connectors are defined on blocks. A block cannot have more than one **asset** connector, but an arbitrary amount of 
**property** connectors.

**property** and **asset** connector types can be omitted, asset links are used by default. Connector names can be omitted as well,
cause they are not stored anywhere.

.. _connector_categories:

Connector Categories
---------------------

A connector can be assigned to a certain category. Connectors from different categories are incompatible. If a connector belongs to
a category, it can be only linked to another connector with the same category.

*Example:*

.. code-block:: igor

    [diagram connector=(name="IN" type=in position="0.5,0" category="action")] 
