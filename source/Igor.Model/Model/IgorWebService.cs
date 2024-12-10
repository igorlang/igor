using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Igor.Model
{
    public class IgorWebResponse
    {
        public string Annotation { get; set; }
        public int? Code { get; set; }
        public string Status { get; set; }
        public IgorWebVariable MaybeContent => content;
        IgorWebVariable content;

        public IgorWebVariable Content
        {
            get
            {
                if (content == null)
                    content = new IgorWebVariable();
                return content;
            }
        }

        public Dictionary<string, string> Headers { get; private set; } = new Dictionary<string, string>();
    }

    public class IgorWebVariable
    {
        public string Name { get; set; }
        public string Annotation { get; set; }
        public DataFormat Format { get; set; } = DataFormat.Default;

        public IReadOnlyList<IgorAttribute> Attributes => attributes;

        private readonly List<IgorAttribute> attributes = new List<IgorAttribute>();

        public void Attribute(string name, string value = null, string target = "*")
        {
            attributes.Add(new IgorAttribute(target, name, value));
        }
        public string Type { get; set; }
    }

    public class IgorWebQueryParameter
    {
        public string Name { get; private set; }
        public string Static { get; set; }
        public IgorWebVariable Variable { get; set; }

        public IgorWebQueryParameter(string name)
        {
            Name = name;
        }
    }

    public class IgorWebResource : IgorDeclaration
    {
        public HttpMethod Method { get; set; }
        public string PathTemplate { get; set; }
        public List<IgorWebVariable> PathVariables { get; private set; } = new List<IgorWebVariable>();
        public IgorWebVariable MaybeRequestContent => requestContent;
        public IgorWebVariable RequestContent
        {
            get
            {
                if (requestContent == null)
                    requestContent = new IgorWebVariable();
                return requestContent;
            }
        }

        IgorWebVariable requestContent = null;
        public List<IgorWebQueryParameter> Query { get; private set; } = new List<IgorWebQueryParameter>();
        public Dictionary<string, string> RequestHeaders { get; private set; } = new Dictionary<string, string>();

        public List<IgorWebResponse> Responses { get; private set; } = new List<IgorWebResponse>();

        public IgorWebResponse Response(int? code)
        {
            return Responses.GetOrAdd(code, r => r.Code, () => new IgorWebResponse { Code = code });
        }

        public IgorWebQueryParameter QueryParameter(string name)
        {
            return Query.GetOrAdd(name, p => p.Name, () => new IgorWebQueryParameter(name));
        }

        public void StaticQueryParameter(string name, string value)
        {
            QueryParameter(name).Static = value;
        }

        public IgorWebVariable QueryVariable(string name)
        {
            var param = QueryParameter(name);
            if (param.Variable == null)
                param.Variable = new IgorWebVariable { Name = name };
            return param.Variable;
        }

        public IgorWebVariable PathParameter(string name)
        {
            return PathVariables.GetOrAdd(name, p => p.Name, () => new IgorWebVariable { Name = name });
        }

        public IgorWebResource(string name) : base(name)
        {
        }
    }

    public class IgorWebService : IgorDeclaration
    {
        public IReadOnlyList<IgorWebResource> Resources => resources;

        private readonly List<IgorWebResource> resources = new List<IgorWebResource>();
        public IgorWebService(string name) : base(name)
        {
        }

        public IgorWebResource Resource(string name) => resources.GetOrAdd(name, f => f.Name, () => new IgorWebResource(name));
    }
}
