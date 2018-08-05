using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using NAudio;
using NAudio.Wave;

namespace BackToTheFutureV.Entities
{
    public class Delorean
    {
        public TimeCircuits Circuits { get; private set; }
        public Vehicle Vehicle { get; }

        public int MPHSpeed
        {
            get
            {
                return (int)(Vehicle.Speed / 0.27777 / 1.60934);
            }
            set
            {
                Vehicle.Speed = (float)(value * 0.27777 * 1.60934);
            }
        }

        public Delorean(Vector3 position)
        {
            Vehicle = World.CreateVehicle(VehicleHash.Adder, position);
            Vehicle.IsInvincible = true;
            Circuits = new TimeCircuits(this);

        }

        public void Tick()
        {
            Circuits?.Tick();
        }

        public void KeyDown(KeyEventArgs e)
        {
            Circuits?.KeyDown(e);
        }
    }

    public class TimeCircuits
    {
        public bool IsOn { get; set; }

        public DateTime DestinationTime { get; set; }
        public DateTime PreviousTime { get; set; }

        public int MPHSpeed { get => delorean.MPHSpeed; set => delorean.MPHSpeed = value; } 
        public Vehicle Vehicle => delorean.Vehicle;

        public bool HasTimeTravelled;
        public bool IsTimeTravelling;

        private string destinationTimeRaw;
        private DateTime nextReset;

        private Delorean delorean;
        private TimeTravelHandler timeTravelHandler;
        private AudioPlayer sparksAudio;

        public TimeCircuits(Delorean del)
        {
            delorean = del;
            sparksAudio = new AudioPlayer("./scripts/BackToTheFutureV/sounds/sparks.wav", true, 2);
        }

        public void Tick()
        {
            UI.ShowSubtitle(MPHSpeed.ToString());

            if(timeTravelHandler != null)
            {
                timeTravelHandler.Process();

                if (timeTravelHandler.hasEnded)
                    timeTravelHandler = null;
            }

            if (DateTime.UtcNow > nextReset)
            {
                destinationTimeRaw = "";
            }

            if(destinationTimeRaw.Length == 12)
            {
                var dateTime = Utils.ParseFromRawString(destinationTimeRaw);

                if(dateTime == DateTime.MinValue)
                {
                    AudioPlayer.PlaySoundFromName("input error", out AudioPlayer player);
                }
                else
                {
                    AudioPlayer.PlaySoundFromName("input enter", out AudioPlayer player);
                }

                DestinationTime = dateTime;

                destinationTimeRaw = "";
            }

            if (MPHSpeed < 80)
            {
                sparksAudio.Stop();
            }

            if (MPHSpeed >= 80 && MPHSpeed < 88)
            {
                if (!sparksAudio.IsPlaying)
                {
                    sparksAudio.Play();
                }

                // TODO: Play FX here
            }

            if (MPHSpeed >= 88)
            {
                sparksAudio.Stop();
            
                if(timeTravelHandler == null)
                {
                    timeTravelHandler = new TimeTravelHandler(this);
                }
            }

            UI.ShowSubtitle("this is a test", 1);
        }

        public void KeyDown(KeyEventArgs e)
        {
            // Only want to do this if we're currently in the vehicle
            if (Game.Player.Character.CurrentVehicle != Vehicle) return;

            string keyCode = e.KeyCode.ToString();

            if (keyCode.Contains("NumPad") || keyCode.Contains("D"))
            {
                try
                {
                    string number = new String(keyCode.Where(Char.IsDigit).ToArray());

                    AudioPlayer.PlaySoundFromName(number, out AudioPlayer player);

                    destinationTimeRaw += number;
                    nextReset = DateTime.UtcNow + new TimeSpan(0, 0, 5);
                }
                catch(Exception ex)
                {
                }
            }
        }
    }
}
