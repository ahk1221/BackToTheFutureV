using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using BackToTheFutureV.Handlers;
using System.Collections.Generic;
using GTA.Native;

namespace BackToTheFutureV.Entities
{
    public enum DeloreanType
    {
        BTTF,
        BTTF2,
        BTTF3
    }

    public class Delorean
    {
        public TimeCircuits Circuits { get; private set; }
        public Vehicle Vehicle { get; }

        public DeloreanType DeloreanType { get; set; }

        public string LowerCaseDeloreanType => DeloreanType.ToString().ToLower();

        public bool IsInfoCopy { get; set; }

        public float MPHSpeed
        {
            get
            {
                return (Vehicle.Speed / 0.27777f / 1.60934f);
            }
            set
            {
                Vehicle.Speed = (value * 0.27777f * 1.60934f);
            }
        }

        public Delorean(Vehicle vehicle) : base()
        {
            Vehicle = vehicle;

            Circuits = new TimeCircuits(this);
        }

        public Delorean(Vector3 position, float heading = 0, DeloreanType type = DeloreanType.BTTF2) : base()
        {
            DeloreanType = type;

            Vehicle = World.CreateVehicle(new Model(LowerCaseDeloreanType), position, heading);
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

            Vehicle.DirtLevel = 0;

            Utils.SetDecorator(Vehicle, "IsBTTFTimeMachine", true);

            Circuits = new TimeCircuits(this);
        }

        public Delorean CreateCopy(bool isInfoCopy, bool hide = true)
        {
            var delorean = new Delorean(Vehicle.Position, Vehicle.Heading);

            delorean.IsInfoCopy = isInfoCopy;
            delorean.DeloreanType = DeloreanType;
            delorean.Circuits.DestinationTime = Circuits.DestinationTime;
            delorean.Circuits.PreviousTime = Circuits.PreviousTime;
            delorean.Circuits.isOn = Circuits.IsOn;
            delorean.Circuits.IsFueled = Circuits.IsFueled;
            delorean.Circuits.IsRemoteControlled = Circuits.IsRemoteControlled;

            if(Vehicle.GetPedOnSeat(VehicleSeat.Driver) != null && !Circuits.IsRemoteControlled)
            {
                var ped = delorean.Vehicle.CreatePedOnSeat(VehicleSeat.Driver, Vehicle.GetPedOnSeat(VehicleSeat.Driver).Model);
                Function.Call((Hash)0xE952D6431689AD9A, Vehicle.GetPedOnSeat(VehicleSeat.Driver), ped);
            }

            Utils.HideVehicle(delorean.Vehicle, hide);

            Main.AddDelorean(delorean);

            return delorean;
        }

        public void Tick()
        {
            Circuits?.Tick();
        }

        public void KeyDown(KeyEventArgs e)
        {
            Circuits?.KeyDown(e);
        }

        public void Dispose()
        {
            if (Vehicle?.GetPedOnSeat(VehicleSeat.Driver) != Game.Player.Character)
                Vehicle?.GetPedOnSeat(VehicleSeat.Driver)?.Delete();
            Circuits?.StopAllHandlers();
            Vehicle?.Delete();
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

        public Delorean Delorean { get; }

        public float MPHSpeed { get => Delorean.MPHSpeed; set => Delorean.MPHSpeed = value; } 
        public Vehicle Vehicle => Delorean.Vehicle;

        public DeloreanType DeloreanType => Delorean.DeloreanType;
        public string LowerCaseDeloreanType => Delorean.LowerCaseDeloreanType;

        public bool IsRemoteControlled { get; set; }
        public bool IsFueled { get; set; } = true;

        private Dictionary<Type, Handler> registeredHandlers = new Dictionary<Type, Handler>();

        public bool isOn;

        private string destinationTimeRaw;
        private DateTime nextReset;

        private UIText emptyText;
        private UIText destinationTimeText;
        private UIText currentTimeText;
        private UIText previousTimeText;

        private UIText speedText;

        public TimeCircuits(Delorean del)
        {
            Delorean = del;
            
            emptyText = new UIText("Empty", new Point(1050, 336), 0.5f, Color.Yellow, GTA.Font.HouseScript, false);
            destinationTimeText = new UIText("Oct 21 1994 22:22 PM", new Point(1050, 360), 0.7f, Color.OrangeRed, GTA.Font.HouseScript, false);
            currentTimeText = new UIText("Oct 21 1991 22:11 PM", new Point(1050, 384), 0.7f, Color.OrangeRed, GTA.Font.HouseScript, false);
            previousTimeText = new UIText("Oct 21 1991 22:11 PM", new Point(1050, 408), 0.7f, Color.OrangeRed, GTA.Font.HouseScript, false);

            speedText = new UIText("88", new Point(1200, 645), 2f, Color.Red, GTA.Font.ChaletComprimeCologne, false);

            RegisterHandlers();
        }

        public T GetHandler<T>()
        {
            if(registeredHandlers.TryGetValue(typeof(T), out Handler handler))
            {
                return (T)Convert.ChangeType(handler, typeof(T));
            }

            return default(T);
        }

        private void RegisterHandlers()
        {
            registeredHandlers.Add(typeof(FuelHandler), new FuelHandler(this));
            registeredHandlers.Add(typeof(FreezeHandler), new FreezeHandler(this));
            registeredHandlers.Add(typeof(FireTrailsHandler), new FireTrailsHandler(this));
            registeredHandlers.Add(typeof(ReentryHandler), new ReentryHandler(this));
            registeredHandlers.Add(typeof(TimeTravelHandler), new TimeTravelHandler(this));
            registeredHandlers.Add(typeof(SparksHandler), new SparksHandler(this));
            registeredHandlers.Add(typeof(RCHandler), new RCHandler(this));
        }

        private void Draw()
        {
            emptyText.Caption = IsFueled ? "" : "Empty";
            destinationTimeText.Caption = DestinationTime.ToString("MMM dd yyyy h:mm tt");
            currentTimeText.Caption = Utils.GetWorldTime().ToString("MMM dd yyyy h:mm tt");
            previousTimeText.Caption = PreviousTime.ToString("MMM dd yyyy h:mm tt");
            speedText.Caption = (((int)MPHSpeed) >= 88) ? "88" : ((int)MPHSpeed).ToString();

            emptyText.Draw();
            destinationTimeText.Draw();
            currentTimeText.Draw();
            previousTimeText.Draw();
            speedText.Draw();
        }

        public void StopAllHandlers()
        {
            foreach(var handler in registeredHandlers.Values)
            {
                handler.Stop();
            }
        }

        public void Tick()
        {
            if (Delorean.IsInfoCopy) return;

            if (!IsOn)
            {
                GetHandler<SparksHandler>().Stop();
                return;
            }

            registeredHandlers.Values.ToList().ForEach(x => x.Process());

            if(Game.Player.Character.IsInVehicle(Vehicle))
            {
                Draw();
            }

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
            if (Delorean.IsInfoCopy) return;

            registeredHandlers.Values.ToList().ForEach(x => x.KeyPress(e.KeyCode));

            // Only want to do this if we're currently in the vehicle
            if (Game.Player.Character.CurrentVehicle != Vehicle)
            {
                // If we're not in the vehicle:

                if(e.KeyCode == Keys.K)
                {
                    GetHandler<RCHandler>().StartRC();
                }

                return;
            }

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
