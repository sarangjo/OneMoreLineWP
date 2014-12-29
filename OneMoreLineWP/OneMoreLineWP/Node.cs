using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneMoreLineWP
{
    public class Node : Sprite
    {
        public Node(Vector2 newPosition)
            : base("Graphics\\node", newPosition)
        {

        }

        public float DistanceFromPlayer(Player p)
        {
            return Vector2.Distance(p.GlobalCenter, GlobalCenter);
        }
    }
}
