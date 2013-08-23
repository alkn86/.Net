using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Hive_Simulation
{
    [Serializable]
    class Bee
    {
        public Bee(int id, Point location, World world, Hive hive,MessageDelegate messageSender)
        {
            this.ID = id;
            this.Age = 0;
            this.location = location;
            this.InsideHive = true;
            this.CurrentState = BeeState.Idle;
            this.destinationFlower = null;
            this.NectarCollected = 0;
            this.world = world;
            this.hive = hive;
            this.ExState = CurrentState;
            this.MessageSender += messageSender;

        }

        private const double HoneyConsumed = 0.5;
        private const int MoveRate = 3;
        private const double MinimumFlowerNectar = 1.5;
        private const int CareerSpan = 1000;

        public int Age { get; private set; }
        public bool InsideHive { get; private set; }
        public double NectarCollected { get; private set; }
        public BeeState CurrentState { get; private set; }
        public BeeState ExState { get; private set; }


        private Point location;
        public Point Location { get { return location; } }
        public int ID;
        private Flower destinationFlower;
        private World world;
        private Hive hive;

        [NonSerialized]
        public MessageDelegate MessageSender;


        public void Go(Random random)
        {
            Age++;
            switch (CurrentState)
            {
                case BeeState.Idle:
                    if (Age > CareerSpan)
                        CurrentState = BeeState.Retired;
                    else if (world.Flowers.Count > 0 && hive.ConsumeHoney(HoneyConsumed))
                    {
                        Flower flower = world.Flowers[random.Next(world.Flowers.Count)];
                        if (flower.Nectar >= MinimumFlowerNectar && flower.Alive)
                        {
                            destinationFlower = flower;
                            CurrentState = BeeState.FlyingToFlower;
                        }
                    }
                    break;


                case BeeState.FlyingToFlower:

                    if (!world.Flowers.Contains(destinationFlower))
                        CurrentState = BeeState.ReturningToHive;
                    else
                        if (InsideHive)
                        {
                            if (MoveTowardsLocation(hive.GetLocation("Exit")))
                            {
                                InsideHive = false;
                                location = hive.GetLocation("Entrance");
                            }
                        }
                        else
                            if (MoveTowardsLocation(destinationFlower.Location))
                                CurrentState = BeeState.GatheringNectar;
                    

                    break;


                case BeeState.GatheringNectar:
                    double nectar = destinationFlower.HarvestNectar();
                    if (nectar > 0)
                        NectarCollected += nectar;
                    else
                        CurrentState = BeeState.ReturningToHive;
                    break;


                case BeeState.ReturningToHive:
                    if (!InsideHive)
                    {
                        if(MoveTowardsLocation(hive.GetLocation("Entrance")))
                        {
                            location = hive.GetLocation("Exit");
                            InsideHive = true;         
                        }


                    }
                    else
                    {
                        if (MoveTowardsLocation(hive.GetLocation("HoneyFactory")))
                            CurrentState = BeeState.MakingHoney;
                    }
            
                    break;


                case BeeState.MakingHoney:
                    if (NectarCollected < 0.5)
                    {
                        NectarCollected = 0;
                        CurrentState = BeeState.Idle;
                    }
                    else
                    {
                        if (hive.AddHoney(0.5))
                            NectarCollected -= 0.5;
                        else
                            NectarCollected = 0;
                    }
                    break;


                case BeeState.Retired:
                    // отдыхаем
                    break;
            }
            if (CurrentState != ExState&& MessageSender!=null)
            {
                MessageSender(this.ID, this.CurrentState.ToString());
                this.ExState = CurrentState;
            }


        }



        private bool MoveTowardsLocation(Point destination)
        {

            if (Math.Abs(destination.X - location.X) <= MoveRate && Math.Abs(destination.Y - location.Y) <= MoveRate)
                return true;

            if (destination.X > location.X)
                location.X += MoveRate;
            else if (destination.X < location.X)
                location.X -= MoveRate;

            if (destination.Y > location.Y)
                location.Y += MoveRate;
            else if (destination.Y < location.Y)
                location.Y -= MoveRate;

            return false;
        }












    }
}
