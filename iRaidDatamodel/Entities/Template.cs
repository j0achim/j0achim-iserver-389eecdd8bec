using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Diagnostics;
using iRaidTemplating;
using Newtonsoft.Json;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class Template : DomainLogic<Template>
    {
        public Template(long CommunityId, string Body, string Test, string Title, string Description, bool IsBotTemplate)
        {
            this.CommunityId = CommunityId;
            this.Title = Title;
            this.Test = Test;
            this.Description = Description;
            this.Body = Body;
            this.IsBotTemplate = IsBotTemplate;
            this.TestPassed = false;
        }

        public long CommunityId { get; private set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Body { get; set; }
        public string Test { get; set; }
        public bool IsBotTemplate { get; set; }
        public bool TestPassed { get; set; }
        public int TestRuntime { get; set; }
        public int Version { get; set; }

        public string SelfTest()
        {
            Stopwatch s = Stopwatch.StartNew();
            try
            {
                // Lets get cache string.
                var cache = string.Format("{0}-{1}-{2}", Id, CommunityId, Version);

                // Lets get the dynamic object that we pass to template upon parsing.
                object model = JsonConvert.DeserializeObject<ExpandoObject>(Test);

                string r = Body.Compile((object)model, cache);

                s.Stop();

                TestPassed = true;
                TestRuntime = s.Elapsed.Milliseconds;
                Save();

                return r;
            }
            catch(Exception ex)
            {
                s.Stop();

                TestPassed = false;
                TestRuntime = s.Elapsed.Milliseconds;
                Save();

                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        //Increment version when its saved.
        public bool SaveTemplate(User user)
        {
            this.Version++;
            // Actually save template.
            return Save(user);
        }
    }
}
