using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMakeGames
{
    public interface IGameState
    {
        void Update();
        void Draw();
        void EnterState();
    }
}
