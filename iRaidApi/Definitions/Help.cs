using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidApi.Definitions
{
    public class Help
    {
        public Help(string scope)
        {
            this.Scope = scope;
            this.Methods = new List<Method>();
        }

        public string Scope { get; set; }
        public List<Method> Methods { get; set; }
    }

    public class Method
    {
        public Method(string name, string description, bool authrequired, string returntype, string category)
        {
            this.Name = name;
            this.Description = description;
            this.ReturnType = returntype;
            this.AuthRequired = authrequired;
            this.Category = category;
            this.Parameters = new List<Parameter>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool AuthRequired { get; set; }
        public string ReturnType { get; set; }
        public string Category { get; set; }
        public List<Parameter> Parameters { get; set; }
    }

    public class Parameter
    {
        public Parameter(string name, string type)
        {
            this.Name = name;
            this.Type = type;
        }

        public string Name { get; set; }
        public string Type { get; set; }
    }
}
