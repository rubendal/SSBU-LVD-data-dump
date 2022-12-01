using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultimate_LVD_data
{
    public class StageId : IEquatable<StageId>
    {
        public string name { get; set; }
        public string gameName { get; set; }
        public int Type { get; set; }

        public StageId()
        {

        }

        public bool Equals(StageId other)
        {

            return name == other.name &&
                gameName == other.gameName && Type == other.Type;
        }
    }
}
