using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using BackToTheFutureV.Entities;

namespace BackToTheFutureV
{
    public class Main : Script
    {
        public static List<Delorean> deloreans = new List<Delorean>();
        public static List<Disposable> disposableObjects = new List<Disposable>();

        private static List<Delorean> deloreansToBeAdded = new List<Delorean>();

        private int nextAnimate = 0;

        public Main()
        {
            Tick += Main_Tick;
            KeyDown += Main_KeyDown;
            Aborted += Main_Aborted;

            var era = new Era();
            era.EraStart = 1990;
            era.EraEnd = 2000;
            era.SpawnableVehicles = new EraVehicleInfo[1]
            {
                new EraVehicleInfo()
                {
                    Model = "ruiner",
                    Probability = 60,
                    Zone = new string[1] { "all" }
                }
            };

            Utils.SerializeObject(era, "./scripts/testyboi.xml");
        }

        public static void AddDelorean(Delorean delorean)
        {
            deloreansToBeAdded.Add(delorean);
        }

        private void Main_Aborted(object sender, EventArgs e)
        {
            Game.FadeScreenIn(1000);

            foreach (var delorean in deloreans)
            {
                delorean.Dispose();
            }

            foreach(var disposable in disposableObjects)
            {
                disposable.Dispose();
            }
        }

        private unsafe void Main_KeyDown(object sender, KeyEventArgs e)
        {
            deloreans.ForEach(x => x?.KeyDown(e));

            FlyingHandling.KeyDown(e.KeyCode);

            if(e.KeyCode == Keys.L)
            {
                Delorean delorean = new Delorean(Game.Player.Character.Position, Game.Player.Character.Heading);
                Game.Player.Character.Task.WarpIntoVehicle(delorean.Vehicle, VehicleSeat.Driver);

                AddDelorean(delorean);
            }

            if(e.KeyCode == Keys.H)
            {
                var copy = deloreans[0].CreateCopy(false);
                AddDelorean(copy);
            }
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            foreach (var delorean in deloreans)
            {
                delorean.Tick();
            }

            if (deloreansToBeAdded.Count > 0)
            {
                deloreans.AddRange(deloreansToBeAdded);
                deloreansToBeAdded.Clear();
            }

            TimeHandler.Tick();

            //if(delorean != null)
            //{
                //UI.ShowSubtitle("Throttle: " + VehicleControl.GetThrottle(delorean.Vehicle) + " Steer: " + VehicleControl.GetSteeringAngle(delorean.Vehicle));

                //Game.DisableControlThisFrame(2, GTA.Control.MoveUp);
                //Game.DisableControlThisFrame(2, GTA.Control.MoveDown);
                //Game.DisableControlThisFrame(2, GTA.Control.MoveLeft);
                //Game.DisableControlThisFrame(2, GTA.Control.MoveRight);

                //delorean.Vehicle.EngineRunning = true;

                //float actualAngle = VehicleControl.GetLargestSteeringAngle(delorean.Vehicle);
                //float limitRadians = VehicleControl.GetMaxSteeringAngle(delorean.Vehicle);
                //float reduction = VehicleControl.CalculateReduction(delorean.Vehicle);

                //bool handbrake;
                //float throttle;
                //bool brake;
                //float steer;

                //VehicleControl.GetControls(limitRadians, out handbrake, out throttle, out brake, out steer);

                //UI.ShowSubtitle("Throttle: " + throttle + " Steer: " + steer);

                //VehicleControl.SetThrottle(delorean.Vehicle, throttle);
                //VehicleControl.SetBrake(delorean.Vehicle, brake);

                //if (brake)
                //    delorean.Vehicle.BrakeLightsOn = true;
                //else
                //    delorean.Vehicle.BrakeLightsOn = false;

                //VehicleControl.SetSteeringAngle(delorean.Vehicle, steer);

                //if(Math.Abs(throttle) >= 0.1f)
                //{
                //    if (delorean.Vehicle.Speed <= 1)
                //        delorean.Vehicle.ApplyForce(delorean.Vehicle.ForwardVector * (0.1f / delorean.Vehicle.Speed) * Math.Sign(throttle));
                //}
            //}
        }
    }
}
