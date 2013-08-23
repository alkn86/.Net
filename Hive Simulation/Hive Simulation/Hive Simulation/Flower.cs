using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hive_Simulation
{
    [Serializable]
    class Flower
    {
       
        public Flower(Point location, Random random) {

            this.Location = location;
            this.Age = 0;
            this.Alive = true;
            this.Nectar = InitialNectar;
            this.NectarHarvested = 0;
            lifespan = random.Next(LifeSpanMin, LifeSpanMax+1);
        }

        private const int LifeSpanMin=15000;
        private const int LifeSpanMax=30000;
        private const double InitialNectar=1.5;
        private const double MaxNectar = 5.0;
        private const double NectarAddedPerTurn = 0.01;
        private const double NectarGatheredPerTurn = 0.3;



       public Point Location{get;private set;}
       public int Age{get;private set;}
       public bool Alive{get; private set;}
       public double Nectar { get; private set; }
       public double NectarHarvested { get; set; }
       private int lifespan;

        public double HarvestNectar()
        {

            if (NectarHarvested > MaxNectar)
                return 0;
            else
            {
                Nectar -= NectarGatheredPerTurn;
                NectarHarvested += NectarGatheredPerTurn;
                return NectarGatheredPerTurn;
            }
        
                
        
        }

        public void Go()
        {
            Age++;
            if (Age > lifespan)
                Alive = false;
            else
            {
                Nectar += NectarAddedPerTurn;
                if (Nectar > MaxNectar)
                    Nectar = MaxNectar;
            }
        }
    }
}
