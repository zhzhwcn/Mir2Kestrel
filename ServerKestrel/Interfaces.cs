using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerKestrel
{
    internal interface IPlayer
    {
    }

    internal interface IAccount
    {
        public int Index { get; set; }
    }

    internal interface IGameDataService
    {
        void LoadGameData();
    }
}
