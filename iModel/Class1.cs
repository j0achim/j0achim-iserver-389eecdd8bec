using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iModel
{
    public class Organization
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class Character
    {
        public Guid Id { get; set; }
        public DateTime Time { get; set; }
        public string CharacterId { get; set; }
        public string Name { get; set; }
        public string Profession { get; set; }
        public string Gender { get; set; }
        public string Breed { get; set; }
        public string Title { get; set; }
        public string Faction { get; set; }
        public int Level { get; set; }
        public int DefenderLevel { get; set; }
        public Organization Organization { get; set; }
    }

    public interface IDataRepository
    {
        IList<Character> GetAll();
    }

    public class DataRepository : IDataRepository
    {
        //private readonly IRed
    }
}
