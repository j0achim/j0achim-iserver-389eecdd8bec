using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using iRaidDatamodel;
using iRaidDatamodel.Entities;
using iRaidApi.Definitions;
using iRaidTools;

namespace iRaidApi
{
    public class CoreApi
    {
        public static HashSet<Connection> ConnectionPool = new HashSet<Connection>();

        private Connection sender;

        public CoreApi(Connection connection)
        {
            sender = connection;
        }

        private bool TryGetCommand(MethodInfo method, out Command command)
        {
            command = null;

            foreach(var attribute in method.GetCustomAttributes(true))
            {
                if (attribute.GetType() == typeof(Command))
                {
                    try
                    {
                        command = attribute as Command;
                        return true;
                    }
                    catch
                    {
                        // Was unable to cast attribute as Command.
                    }
                }
            }

            return false;
        }

        public Help Help(Type type)
        {
            Help help = new Help(type.ToString());
            List<Method> methods = new List<Method>();

            foreach (var m in type.GetMethods().OrderBy(x=>x.Name))
            {
                Command Command = null;

                if(TryGetCommand(m, out Command))
                {
                    Method method = new Method(m.Name, Command.Description, Command.AuthRequired, m.ReturnType.Name.ToString(), Command.Category);

                    foreach (var p in m.GetParameters())
                    {
                        method.Parameters.Add(new Parameter(p.Name, p.ParameterType.Name.ToString()));
                    }

                    methods.Add(method);
                }
            }

            // Adding methods and ordering them by cateogry then by Name.
            help.Methods = methods.OrderBy(x=>x.Category).ThenBy(x=>x.Name).ToList();

            return help;
        }

        public bool Execute(string jsonstring, Type type, object ctx, out string msg, out string Action)
        {
            // Return string.
            msg = string.Empty;
            Action = string.Empty;

            try
            {
                //Type type = typeof(WebApi);
                JObject Json = new JObject();
                MethodInfo method = null;
                Command Command = null;
                object[] parameters = null;

                try
                {
                    Json = (JObject)JsonConvert.DeserializeObject(jsonstring);
                }
                catch
                {
                    throw new Exception(string.Format("Unable to parse string to valid JSON Object: {0}", jsonstring));
                }

                try
                {
                    Action = (string)Json["action"];

                    if (Action == null || Action == "")
                    {
                        // Action cannot be null or empty.
                        throw new Exception();
                    }
                }
                catch
                {
                    throw new Exception(string.Format("No action was found in JSON string: {0}", jsonstring));
                }

                Console.WriteLine("Trying to execute: {0}", Action);

                try
                {
                    method = type.GetMethod(Action);
                }
                catch
                {
                    throw new Exception(string.Format("No method for action {0} found.", Action));
                }

                try
                {
                    if(!TryGetCommand(method, out Command))
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    throw new Exception(string.Format("No method for action {0} found.", Action));
                }

                if (Command.AuthRequired && !sender.Auth)
                {
                    throw new Exception("You have to login to use this method.");
                }

                try
                {
                    parameters = method.GetParameters().Select(x => Convert.ChangeType((string)Json["params"][x.Name], x.ParameterType)).ToArray();
                }
                catch
                {
                    throw new Exception(string.Format("Failed to execute {0} parameters does not conform the parameter schema.", Action));
                }

                try
                {
                    msg = JsonConvert.SerializeObject(method.Invoke(ctx, parameters));
                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format("{0}", e.InnerException.Message));
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }
    }
}
