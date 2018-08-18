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
using BackToTheFutureV.Memory;

namespace BackToTheFutureV
{
    public class Main : Script
    {
        public Delorean delorean;

        public PtfxEntityPlayer ptfx;

        public Main()
        {
            Tick += Main_Tick;
            KeyDown += Main_KeyDown;
            Aborted += Main_Aborted;
        }

        private void Main_Aborted(object sender, EventArgs e)
        {
            delorean?.Vehicle?.Delete();
        }

        private unsafe void Main_KeyDown(object sender, KeyEventArgs e)
        {
            delorean?.KeyDown(e);

            if(e.KeyCode == Keys.L)
            {
                delorean = new Delorean(Game.Player.Character.Position, Game.Player.Character.Heading);
                Game.Player.Character.Task.WarpIntoVehicle(delorean.Vehicle, VehicleSeat.Driver);
            }

            if(e.KeyCode == Keys.H)
            {
                if (delorean.Vehicle == null) return;

                delorean.Vehicle.CreatePedOnSeat(VehicleSeat.Driver, PedHash.Business01AMM);
            }
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            delorean?.Tick();

            ptfx?.Process();

            if(delorean != null)
            {
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
            }
        }
    }
}
