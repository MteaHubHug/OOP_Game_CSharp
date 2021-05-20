using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTTER
{
    class Zmaj :Sprite
    {
        public Zmaj(string path,int X,int Y) : base(path, X, Y)
        {
            
        }

        public override int Y
        {
            get { return y; }
            set
            {
                if (value >= GameOptions.DownEdge-this.Heigth) 
                    value = GameOptions.DownEdge-this.Heigth;
                else
                {
                    this.y = value;
                }


            }
        }

    }
}
