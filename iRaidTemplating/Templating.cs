using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iRaidTools;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngine.Configuration;

namespace iRaidTemplating
{
    public static class TemplateCompiler
    {
        /// <summary>
        /// Our private static Razor engine.
        /// </summary>
        private static IRazorEngineService _engine = LoadEngine();

        /// <summary>
        /// Lets configure a new Razor Engine Service.
        /// </summary>
        /// <returns></returns>
        private static IRazorEngineService LoadEngine()
        {
            var Config = new TemplateServiceConfiguration();

            Config.CachingProvider = new DefaultCachingProvider(r => { });

            return RazorEngineService.Create(Config);
        }

        /// <summary>
        /// Tries to compile a template, from cache or build from scratch.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="model"></param>
        /// <param name="cache"></param>
        /// <returns></returns>
        public static string Compile(this string template, object model, string cache)
        {
            if (template.IsNullOrEmpty())
                throw new Exception("Template cannot be null or empty.");

            return _engine.RunCompile(template, cache, null, model); //Razor.Parse(template, model, cache);
        }
    }
}
