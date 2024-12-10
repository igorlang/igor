using System.Collections.Generic;

namespace Igor.JavaScript.Model
{
    public class JsImport
    {
        public string Module { get; }

        public JsImport(string module)
        {
            Module = module;
        }

        public string DefaultExport { get; set; }
        public bool ImportAll { get; set; }
    }

    public class JsDeclaration
    {
        public string Text { get; }
        public string Annotation { get; set; }

        public JsDeclaration(string text)
        {
            Text = text;
        }
    }

    /// <summary>
    /// JavaScript file model
    /// </summary>
    public class JsFile
    {
        /// <summary>
        /// File name with extension, relative to the output folder
        /// </summary>
        public string FileName { get; }

        internal bool IsEmpty => false;

        internal List<JsImport> Imports = new List<JsImport>();
        internal List<JsDeclaration> Declarations = new List<JsDeclaration>();

        internal JsFile(string path)
        {
            this.FileName = path;
        }

        public JsImport Import(string module, string defaultExport = null, bool importAll = false)
        {
            var result = Imports.GetOrAdd(module, import => import.Module, () => new JsImport(module));
            if (defaultExport != null)
                result.DefaultExport = defaultExport;
            if (importAll)
                result.ImportAll = true;
            return result;
        }

        public JsDeclaration Declaration(string text)
        {
            var result = new JsDeclaration(text);
            Declarations.Add(result);
            return result;
        }
    }
}
