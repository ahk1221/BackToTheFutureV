﻿using System;
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

        public Delorean(Vector3 position, float heading = 0)
        {
            Vehicle = World.CreateVehicle(new Model("bttf2"), position, heading);
            Vehicle.IsInvincible = true;
            Vehicle.IsRadioEnabled = false;

            Vehicle.InstallModKit();

            Vehicle.SetMod(VehicleMod.RearBumper, 2, true);
            Vehicle.ToggleMod(VehicleToggleMod.Turbo, true);
            Vehicle.SetMod(VehicleMod.Frame, -1, true);
            Vehicle.SetMod(VehicleMod.Horns, 16, true);
            Vehicle.SetMod(VehicleMod.RearBumper, 0, true);
            Vehicle.SetMod(VehicleMod.RightFender, 0, true);
            Vehicle.SetMod(VehicleMod.Fender, 0, true);
            Vehicle.SetMod(VehicleMod.ArchCover, 0, true);
            Vehicle.SetMod(VehicleMod.Exhaust, 0, true);
            Vehicle.SetMod(VehicleMod.Hood, 0, true);
            Vehicle.SetMod(VehicleMod.Ornaments, 0, true);

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
        public bool IsOn
        {
            get => isOn;
            set
            {
                isOn = value;

                if(isOn)
                    AudioPlayer.PlaySoundFromName("input_on", out AudioPlayer player, false, 2);
                else
                    AudioPlayer.PlaySoundFromName("input_off", out AudioPlayer player, false, 2);
            }
        }

        public DateTime DestinationTime { get; set; }
        public DateTime PreviousTime { get; set; }

        public int MPHSpeed { get => delorean.MPHSpeed; set => delorean.MPHSpeed = value; } 
        public Vehicle Vehicle => delorean.Vehicle;

        public TimeTravelHandler TimeTravelHandler;
        public SparksHandler SparksHandler;

        private bool isOn;

        private string destinationTimeRaw;
        private DateTime nextReset;
        private Delorean delorean;


        private UIText destinationTimeText;
        private UIText currentTimeText;
        private UIText previousTimeText;


        public TimeCircuits(Delorean del)
        {
            delorean = del;
            
            destinationTimeText = new UIText("Oct 21 1994 22:22 PM", new Point(1050, 360), 0.7f, Color.OrangeRed, GTA.Font.HouseScript, false);
            currentTimeText = new UIText("Oct 21 1991 22:11 PM", new Point(1050, 384), 0.7f, Color.OrangeRed, GTA.Font.HouseScript, false);
            previousTimeText = new UIText("Oct 21 1991 22:11 PM", new Point(1050, 408), 0.7f, Color.OrangeRed, GTA.Font.HouseScript, false);

            TimeTravelHandler = new TimeTravelHandler(this);
            SparksHandler = new SparksHandler(this);
        }

        private void ResetEverything()
        {
            SparksHandler?.ForceStop();
        }

        private void Draw()
        {
            destinationTimeText.Caption = DestinationTime.ToString("MMM dd yyyy h:mm tt");
            currentTimeText.Caption = Utils.GetWorldTime().ToString("MMM dd yyyy h:mm tt");
            previousTimeText.Caption = PreviousTime.ToString("MMM dd yyyy h:mm tt");

            destinationTimeText.Draw();
            currentTimeText.Draw();
            previousTimeText.Draw();
        }

        public void Tick()
        {
            if (!IsOn)
            {
                ResetEverything();
                return;
            }

            SparksHandler?.Process();
            TimeTravelHandler?.Process();

            Draw();

            if (DateTime.UtcNow > nextReset)
            {
                destinationTimeRaw = "";
            }

            if(destinationTimeRaw.Length == 12)
            {
                var dateTime = Utils.ParseFromRawString(destinationTimeRaw);

                if(dateTime == DateTime.MinValue)
                    AudioPlayer.PlaySoundFromName("input enter error", out AudioPlayer player);
                else
                    AudioPlayer.PlaySoundFromName("input enter", out AudioPlayer player);

                DestinationTime = dateTime;

                destinationTimeRaw = "";
            }

        }

        public void KeyDown(KeyEventArgs e)
        {
            // Only want to do this if we're currently in the vehicle
            if (Game.Player.Character.CurrentVehicle != Vehicle) return;

            if(e.KeyCode == Keys.Enter)
            {
                IsOn = !IsOn;
            }

            if (!IsOn) return;

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
