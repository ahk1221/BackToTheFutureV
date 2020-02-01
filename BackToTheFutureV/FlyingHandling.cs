using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace BackToTheFutureV
{
    public class FlyingCar
    {
        public Vehicle MainCar { get; private set; }
        public Vehicle Deluxo { get; private set; }

        public FlyingCar(Vehicle car)
        {
            MainCar = car;
            Deluxo = World.CreateVehicle(VehicleHash.Deluxo, car.Position);
            Deluxo.IsVisible = false;
            Deluxo.HasCollision = false;
            Game.Player.Character.Task.WarpIntoVehicle(Deluxo, VehicleSeat.Driver);
            MainCar.AttachTo(Deluxo, 0);
        }

        public void Dispose()
        {
            Game.Player.Character.Task.WarpIntoVehicle(MainCar, VehicleSeat.Driver);
            MainCar?.Detach();
            Deluxo?.Delete();
            Deluxo = null;
        }
    }

    public class FlyingHandling
    {
        private static List<FlyingCar> flyingCars = new List<FlyingCar>();

        public static void Tick()
        {

        }

        public static void KeyDown(Keys key)
        {
            if(key == Keys.X && Game.Player.Character.CurrentVehicle != null)
            {
                flyingCars.Add(new FlyingCar(Game.Player.Character.CurrentVehicle));
            }
        }
    }
}
