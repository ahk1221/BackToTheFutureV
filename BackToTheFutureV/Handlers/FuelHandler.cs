using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BackToTheFutureV.Entities;
using GTA;
using GTA.Math;

namespace BackToTheFutureV.Handlers
{
    public class FuelHandler : Handler
    {
        public bool IsRefueling => refuelSound.IsPlaying;

        private AudioPlayer emptySound;
        private AudioPlayer refuelSound;

        private bool canRefuel;

        public FuelHandler(TimeCircuits circuits) : base(circuits)
        {
            emptySound = new AudioPlayer("empty.wav", false);
            refuelSound = new AudioPlayer("refuel.wav", false);
        }

        public void UpdateFuel()
        {
            IsFeuled = false;
            emptySound.Play(Vehicle);
        }

        public void Refuel()
        {
            refuelSound.Play(Vehicle);
            refuelSound.OnPlaybackStopped = () => IsFeuled = true;
        }

        public override void Process()
        {
            emptySound?.Process();
            refuelSound?.Process();

            if(IsRefueling)
            {
                Game.DisableAllControlsThisFrame(2);
                Game.EnableControlThisFrame(2, GTA.Control.LookUpDown);
                Game.EnableControlThisFrame(2, GTA.Control.LookLeftRight);
                Game.EnableControlThisFrame(2, GTA.Control.NextCamera);
                Game.EnableControlThisFrame(2, GTA.Control.LookBehind);
            }

            var bootPos = Vehicle.GetBoneCoord("boot");
            var dir = bootPos - GameplayCamera.Position;

            var angle = Vector3.Angle(dir, GameplayCamera.Direction);
            var dist = Vector3.Distance(bootPos, Game.Player.Character.Position);

            if (angle < 45 && dist < 1.5f)
            {
                canRefuel = true;
            }
            else
            {
                canRefuel = false;
            }
        }

        public override void KeyPress(Keys key)
        {
            if(key == Keys.Oemplus && canRefuel && !IsRefueling)
            {
                Refuel();
            }
        }

        public override void Stop()
        {
        }
    }
}
