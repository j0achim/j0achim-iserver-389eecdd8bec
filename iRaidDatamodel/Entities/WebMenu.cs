using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class WebMenu : DomainLogic<WebMenu>
    {
        public WebMenu(long PageId, string Name, string Icon, bool active = false)
        {
            this.PageId = PageId;
            this.name = Name;
            this.icon = Icon;
            this.active = (active) ? "active" : "";
            this.version = 0;
        }

        public long PageId { get; set; }
        public string active { get; set; }
        public string icon { get; set; }
        public string name { get; set; }
        public int version { get; set; }

        public bool SaveMenu(User user)
        {
            this.version++;

            // Actually save page.
            return Save(user);
        }
    }

    [LoadInfo]
    public class WebPage : DomainLogic<WebPage>
    {
        public WebPage(string Title, string Body)
        {
            this.Title = Title;
            this.Body = Body;
            this.version = 0;
        }

        public string Title { get; set; }
        public string Body { get; set; }
        public int version { get; set; }

        public bool SavePage(User user)
        {
            this.version++;

            // Actually save page.
            return Save(user);
        }
    }
}
