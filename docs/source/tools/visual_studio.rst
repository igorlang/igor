******************
Visual Studio 
******************

Add Igor Compiler step to Visual Studio project
===============================================

You can add running Igor Compiler as a prebuild event to your Visual Studio (MSBuild) project. 

Perform the following steps:

* Place igorc.exe somewhere to your Visual Studio solution folder.

* In the project root folder create a cmd file called ``prebuild.exe`` with the following content:

.. code-block:: batch

   @cd %~dp0
   igorc.exe **your options**

Use relative path to igorc.exe from your project folder.

* Add *PreBuildEvent* with ``prebuild.cmd`` call to the end of your project (msbuild) file:

.. code-block:: xml

    <PropertyGroup>
      <PreBuildEvent>$(ProjectDir)prebuild.cmd</PreBuildEvent>
    </PropertyGroup>
  </Project>

* Add target generated files to your project.

* Optionally add source Igor files to your project, so that ``prebuild.cmd`` is called when files are modified.

Now every time the project is built, target files are regenerated.

.. _vs_scripts:

Use Visual Studio to edit extension scripts
===========================================

It's convenient to use Visual Studio with *IntelliSense* to edit your extension scripts. To do so, perform the following steps:

* In your C# scripts folder create *IgorScripts.csproj* file (the name is arbitrary) with the following content:

.. code-block:: xml

    <Project Sdk="Microsoft.NET.Sdk">
        <PropertyGroup>
            <TargetFrameworks>net472</TargetFrameworks>
        </PropertyGroup>
        <ItemGroup>
          <Reference Include="igorc">
            <HintPath>igorc.exe</HintPath>
          </Reference>
        </ItemGroup>
    </Project>

* Edit HintPath to a valid relative path to *igorc.exe*.

* Now just open the *csproj* file with Visual Studio 2017+ and edit your scripts with the full *IntelliSense* support.

.. tip::

   When editing extension scripts in Visual Studio you can get advantage of using Igor API context help, by putting
   XML documentation file ``igorc.xml`` to the same folder as referenced ``igorc.exe``.

   The stable version of ``igorc.xml`` can be downloaded here: https://github.com/igorlang/igor/releases/latest/download/igorc.xml .
   
Debugging extension scripts
---------------------------

You can use Visual Studio to debug your extensions. For this purpose, create the ``Properties\launchSettings.json`` 
file in your project directory with the following content:

.. code-block:: json

    {
      "profiles": {
        "dump": {
          "commandName": "Executable",
          "executablePath": "$(TargetDir)\\igorc.exe",
          "commandLineArgs": "-lib $(TargetPath) -t TARGET_NAME -p $(ProjectDir) *.igor"
        }
      }
    }

Using ``-lib`` option allows to run and debug the compiled dll.
Adjust your ``TARGET_NAME`` and path to the source files (``-p`` option) and provide other command line arguments if needed.


Now you can set breakpoints for your scripts in Visual Studio and hit them in debugging session.



