﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Igor.Elixir.Model;
using Igor.Text;

namespace Igor.Elixir.Render
{
    public class ExFileRenderer : ExRenderer
    {
        public string FileHeader =>
            $@"@author Igor compiler
@doc Compiler version: {Version.HeaderVersionString}
DO NOT EDIT THIS FILE - it is machine generated";

        public void WriteFile(ExFile file)
        {
            Comment(FileHeader, "# ");
            ForEach(file.Modules.Values, WriteModule, true);
        }

        public void WriteModule(ExModule mod)
        {
            Line($"defmodule {mod.Name} do");
            Indent();

            if (!string.IsNullOrEmpty(mod.Annotation))
            {
                Line(@"@moduledoc """"""");
                Comment(mod.Annotation, "");
                Line(@"""""""");
            }

            if (mod.Struct != null)
            {
                if (mod.Struct.IsException)
                    WriteException(mod.Struct);
                else
                    WriteStruct(mod.Struct);
            }

            ForEach(mod.Modules.Values, WriteModule, true);
            Blocks(mod.Uses, u => $"use {u}");
            Blocks(mod.Requires, u => $"require {u}");
            Blocks(mod.Imports, u => $"import {u}");
            EmptyLine();
            Blocks(mod.Behaviours, u => $"@behaviour {u}");
            ForEach(mod.Callbacks, WriteBlock, emptyLineDelimiter: true);
            ForEach(mod.Blocks, WriteBlock, emptyLineDelimiter: true);
            Outdent();
            Line("end");
        }

        private void WriteBlock(ExBlock block)
        {
            WriteDoc(block.Annotation);
            Block(block.Text);
        }

        private void WriteStruct(ExStruct rec)
        {
            if (rec.Fields.Any(f => f.Enforce))
                Line($"@enforce_keys [{rec.Fields.Where(f => f.Enforce).JoinStrings(", ", f => ":" + f.Name)}]");
            Line($"defstruct [{rec.Fields.JoinStrings(", ", FormatStructField)}]");
        }

        private void WriteException(ExStruct rec)
        {
            Line($"defexception [{rec.Fields.JoinStrings(", ", FormatStructField)}]");
        }

        private string FormatStructField(ExStructField field)
        {
            if (field.Default != null)
                return $"{field.Name}: {field.Default}";
            else
                return $"{field.Name}: nil";
        }

        public static string Render(ExFile file)
        {
            var renderer = new ExFileRenderer();
            renderer.WriteFile(file);
            return TextHelper.FixEndLine(TextHelper.RemoveDoubleEmptyLines(renderer.Build()));
        }
    }
}
