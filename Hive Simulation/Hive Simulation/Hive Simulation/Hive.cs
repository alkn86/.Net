using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Hive_Simulation
{
    [Serializable]
    class Hive
    {
        public Hive(World world, MessageDelegate messageSender)
        {
            this.Honey = InitialHoney;
            InitialLocations();
            beeCount = 0;
            Random random = new Random();
            this.world = world;
            this.messageSender = messageSender;
            for (beeCount = 0; beeCount < InitialBees; beeCount++)
            {
                AddBee(random);
            }


        }


        public double Honey { get; private set; }
        private Dictionary<string, Point> locations;
        private int beeCount;
        private World world;

        private const int InitialBees = 6;
        private const int MaximumBees = 8;
        private const double InitialHoney = 3.2;
        private const double MaximumHoney = 15.0;
        private const double NectarHoneyRatio = 0.25;
        private const double MinimumHoneyForCreatingBees = 4;

        [NonSerialized]
        public MessageDelegate messageSender;



        public Point GetLocation(string location)
        {
            if (locations.Keys.Contains(location))
                return locations[location];
            else
                throw new ArgumentException("Unavalible location", location);
        }



        private void InitialLocations()
        {
            locations = new Dictionary<string, Point>();
            locations.Add("Entrance", new Point(651, 119));
            locations.Add("Nursery", new Point(101, 202));
            locations.Add("HoneyFactory", new Point(179, 99));
            locations.Add("Exit", new Point(213, 245));

        }

        private void AddBee(Random random)
        {
            int r1 = random.Next(100) - 50;
            int r2 = random.Next(100) - 50;
            Point startPoint = new Point(locations["Nursery"].X + r1, locations["Nursery"].Y + r2);
            Bee newBee = new Bee(beeCount, startPoint, world, this, messageSender);
            world.Bees.Add(newBee);

        }



        public bool AddHoney(double nectar)
        {
            double honeyToAdd = nectar * NectarHoneyRatio;
            if (Honey + honeyToAdd > MaximumHoney)
                return false;
            Honey += honeyToAdd;
            return true;
        }



        public bool ConsumeHoney(double amount)
        {
            if (amount > Honey)
                return false;
            else
            {
                Honey -= amount;
                return true;
            }
        }

        public void Go(Random random)
        {

            if (MaximumBees > world.Bees.Count && Honey > MinimumHoneyForCreatingBees && random.Next(10) == 1)
                AddBee(random);

        }






    }
}
