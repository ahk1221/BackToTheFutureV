using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Native;
using GTA.Math;
using BackToTheFutureV.Entities;
using System.Windows.Forms;

namespace BackToTheFutureV.Handlers
{
    public enum RCModes
    {
        WithCamera,
        WithoutCamera
    }

    public class RCHandler : Handler
    {
        public Ped Clone { get; private set; }

        public bool IsRemoteControlling { get; private set; }

        public RCHandler(TimeCircuits circuits) : base(circuits)
        {
        }

        public void StartRC()
        {
            if (Vehicle == null) return;

            IsRemoteControlled = true;
            IsRemoteControlling = true;

            // Save player info
            Vector3 position = Game.Player.Character.Position;
            float heading = Game.Player.Character.Heading;
            Model model = Game.Player.Character.Model;

            // Spawn copy of player at player pos
            Clone = World.CreatePed(model, position, heading);
            Clone.Position = position;
            Clone.BlockPermanentEvents = true;
            Clone.AlwaysKeepTask = true;
            Clone.IsPersistent = true;
            // CLONE_PED_TO_TARGET
            Function.Call((Hash)0xE952D6431689AD9A, Game.Player.Character.Handle, Clone.Handle);

            // Warp player into the vehicle
            Game.Player.Character.SetIntoVehicle(Vehicle, VehicleSeat.Driver);

            // Set player invisible
            Game.Player.Character.IsVisible = false;
        }

        public void StopRC()
        {
            // Stop remote controlling
            IsRemoteControlling = false;

            // Set position/rotation back of normal player
            if(Vehicle.GetPedOnSeat(VehicleSeat.Driver) == Game.Player.Character)
            {
                Game.Player.Character.Task.WarpOutOfVehicle(Vehicle);
                Game.Player.Character.Position = Clone.Position;
                Game.Player.Character.Heading = Clone.Heading;
                Game.Player.Character.IsVisible = true;
            }

            // Delete the clone
            Clone?.Delete();
        }

        public override void Process()
        {
            if(IsRemoteControlling)
            {
                // Don't be able to get out
                Game.DisableControlThisFrame(2, GTA.Control.VehicleExit);

                // If the clone dies, you die
                if (Clone != null && Clone.Exists() && Clone.IsDead)
                {
                    Game.Player.Character.Kill();
                    Stop();
                }
            }
        }

        public override void Stop()
        {
            IsRemoteControlling = false;
            Clone?.Delete();
        }

        public override void KeyPress(Keys key)
        {

        }
    }
}
