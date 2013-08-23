using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hive_Simulation
{
    [Serializable]
    class World
    {

        public World(MessageDelegate messageSender)
        {
            bees = new List<Bee>();
            flowers = new List<Flower>();
            Random random = new Random();
            hive = new Hive(this,messageSender);
            for (int i = 0; i < 10; i++)
                AddFlower(random);

        }






        private const double NectarHarvestedPerNewFlower = 50.0;
        private const int FieldMinX = 15;
        private const int FieldMinY = 177;
        private const int FieldMaxX = 690;
        private const int FieldMaxY = 290;

        public Hive Hive { get { return hive; } private set { hive = value; } }
        public List<Bee> Bees { get { return bees; } private set { bees = value; } }
        public List<Flower> Flowers { get { return flowers; } private set { Flowers = value; } }

        private Hive hive;
        private List<Bee> bees;
        private List<Flower> flowers;

        public void Go(Random random)
        {
            Hive.Go(random);

            for (int i = Bees.Count-1; i >= 0; i--)
            {
                Bee bee = Bees[i];
                bee.Go(random);
                if (bee.CurrentState == BeeState.Retired)
                    Bees.Remove(bee);
            }

            double totalNectarHarvested = 0;
            for (int i = Flowers.Count-1; i >= 0; i--)
            {
                Flower flower = Flowers[i];
                flower.Go();
                totalNectarHarvested += flower.NectarHarvested;
                if (!flower.Alive)
                    Flowers.Remove(flower);
            }
            if (totalNectarHarvested > NectarHarvestedPerNewFlower)
            {
                foreach (Flower flower in flowers)
                    flower.NectarHarvested = 0;
                AddFlower(random);
            }

                

        }


        private void AddFlower(Random random)
        {
            Point location = new Point(random.Next(FieldMinX, FieldMaxX), random.Next(FieldMinY, FieldMaxY));
            Flower flower = new Flower(location, random);
            Flowers.Add(flower);
        }

        
    }

}


