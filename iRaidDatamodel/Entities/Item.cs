using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class Item : DomainLogic<Item>
    {
        public Item(Game game, string Name)
        {
            this.Name = Name;
            this.GameId = game.Id;
        }

        public long GameId { get; private set; }
        public string Name { get; set; }

        public void AddProperty(User user, string Name, string Value)
        {
            ItemProperty p = new ItemProperty(this, Name, Value);

            if (!Repository<ItemProperty>.Contains(p))
            {
                p.Save(user);
            }
        }

        public IEnumerable<ItemProperty> Properties()
        {
            return Repository<ItemProperty>.FindBy(x => x.ItemId == this.Id);
        }
    }

    [LoadInfo]
    public class ItemProperty : DomainLogic<ItemProperty>
    {
        public ItemProperty(Item item, string PropertyName, string PropertyValue)
        {
            this.ItemId = item.Id;
            this.PropertyName = PropertyName;
            this.PropertyValue = PropertyValue;
        }

        public long ItemId { get; private set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }
}
