using System;
using System.Collections.Generic;
using System.Linq;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Memory;
using GTA;
using GTA.Native;
using GTA.Math;

namespace BackToTheFutureV.Handlers
{
    public enum TimeTravelMode
    {
        CutScene, Instant 
    }

    public class TimeTravelHandler : Handler
    {
        public TimeTravelMode CurrentMode { get; set; }

        private AudioPlayer timeTravelAudio;
        private AudioPlayer reentryAudio;

        private bool isTimeTravelling = false;
        private int currentStep;
        private float gameTimer;

        private List<Moment> momentsInTime = new List<Moment>();

        public TimeTravelHandler(TimeCircuits circuits) : base(circuits)
        {
            timeTravelAudio = new AudioPlayer("./scripts/BackToTheFutureV/sounds/time_travel.wav", false, 2);
            reentryAudio = new AudioPlayer("./scripts/BackToTheFutureV/sounds/reentry.wav", false, 2);
        }

        public void StartTimeTravelling()
        {
            isTimeTravelling = true;
            gameTimer = 0;
        }

        public void ToggleModes()
        {
            int newMode = (int)CurrentMode + 1;

            if (newMode > 1)
                newMode = 0;

            CurrentMode = (TimeTravelMode)newMode;
        }

        public override void Process()
        {
            if (!isTimeTravelling) return;
            if (Game.GameTime < gameTimer) return;

            switch(currentStep)
            {
                case 0:
                    timeTravelAudio.Play();

                    if (CurrentMode != TimeTravelMode.CutScene)
                    {
                        // Have to call SetupJump manually here.
                        SetupJump();

                        // Jump to the end
                        currentStep = 7;
                        break;
                    }

                    gameTimer = Game.GameTime + 2000;

                    Vehicle.IsVisible = false;
                    Vehicle.HasCollision = false;
                    Vehicle.FreezePosition = true;
                    Vehicle.EngineRunning = false;
                    Game.Player.Character.IsVisible = false;

                    currentStep++;
                    break;

                case 1:
                    Game.FadeScreenOut(1000);
                    gameTimer = Game.GameTime + 1500;

                    currentStep++;
                    break;

                case 2:
                    SetupJump();
                    gameTimer = Game.GameTime + 1000;

                    currentStep++;
                    break;

                case 3:
                    Game.FadeScreenIn(1000);
                    gameTimer = Game.GameTime + 700;

                    currentStep++;
                    break;

                case 4:
                    reentryAudio.Play();
                    gameTimer = Game.GameTime + 500;

                    World.AddExplosion(Vehicle.Position, ExplosionType.Rocket, 1f, 0, false, false);

                    currentStep++;
                    break;

                case 5:

                    World.AddExplosion(Vehicle.Position, ExplosionType.Rocket, 1f, 0, false, false);

                    gameTimer = Game.GameTime + 500;

                    currentStep++;
                    break;

                case 6:

                    World.AddExplosion(Vehicle.Position, ExplosionType.Rocket, 1f, 0, false, false);

                    currentStep++;
                    break;

                case 7:
                    Vehicle.IsVisible = true;
                    Vehicle.HasCollision = true;
                    Vehicle.FreezePosition = false;
                    Vehicle.EngineRunning = true;
                    Game.Player.Character.IsVisible = true;

                    MPHSpeed = 65f;

                    currentStep = 0;
                    isTimeTravelling = false;
                    gameTimer = 0;
                    break;
            }
        }

        public override void Stop()
        {
            currentStep = 0;
            isTimeTravelling = false;
            gameTimer = 0;

            Vehicle.IsVisible = true;
            Vehicle.HasCollision = true;
            Vehicle.FreezePosition = false;
            Vehicle.EngineRunning = true;
            Game.Player.Character.IsVisible = true;
        }

        private void SetupJump()
        {
            // Update the previous time.
            PreviousTime = Utils.GetWorldTime();

            // Set the new GTA time.
            Utils.SetWorldTime(DestinationTime);

            // Try to find a stored moment for our time jump
            var moment = GetStoredMoment(DestinationTime, 4);
            if (moment != null)
            {
                // We found a moment.
                // Apply it.
                ApplyMoment(moment);

                UI.ShowSubtitle("Found: " + World.Weather + " Puddle Level: " + RainPuddleEditor.Level);
            }
            else
            {
                // We didn't find a moment.
                // Randomise it.
                Randomize();

                UI.ShowSubtitle("Randomizin: " + World.Weather + " Puddle Level: " + RainPuddleEditor.Level);

                // Get the current Moment object for current situation.
                moment = GetMomentForNow();
                moment.CurrentDate = DestinationTime;

                // Add to stored Moments list.
                momentsInTime.Add(moment);
            }
        }

        public void Randomize()
        {
            World.Weather = Utils.GetRandomWeather();

            float puddleLevel = 0;

            if (World.Weather == Weather.Raining)
            {
                puddleLevel = (float)Utils.Random.NextDouble(0.4, 0.8);
            }
            else if(World.Weather == Weather.Clearing)
            {
                puddleLevel = 0.2f;
            }
            else if(World.Weather == Weather.ThunderStorm)
            {
                puddleLevel = 0.9f;
            }

            RainPuddleEditor.Level = puddleLevel;

            // Delete all close-by Vehicles
            var nearbyVehicles = World.GetNearbyVehicles(Vehicle.Position, 10f).ToList();

            nearbyVehicles
                .Where(x => x != Vehicle).ToList()
                .ForEach(x  => x.DeleteCompletely());

            // Far vehicles
            var farVehicles = World.GetNearbyVehicles(Vehicle.Position, 100f).ToList();

            farVehicles
                .Where(x => x != Vehicle).ToList()
                .ForEach(v =>
                {
                    if(Utils.Random.NextDouble() < 0.2)
                    {
                        // Change vehicle model

                        var vehicleInfo = new VehicleInfo(v);
                        vehicleInfo.Model = Utils.GetRandomVehicleHash(v.Model, Vehicle.Position);

                        v.DeleteCompletely();

                        Utils.SpawnFromVehicleInfo(vehicleInfo);
                    }
                    else
                    {
                        // Just delete the vehicle
                        v.DeleteCompletely();
                    }
                });
        }

        public Moment GetMomentForNow()
        {
            var currentTime = Utils.GetWorldTime();
            var currentWeather = World.Weather;
            var currentPuddleLevel = RainPuddleEditor.Level;
            var nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, 300f);

            var infos = nearbyVehicles
                .Where(x => x != Vehicle)
                .Select(x => new VehicleInfo(x));

            return new Moment(currentTime, currentWeather, RainPuddleEditor.Level, infos);
        }

        public void ApplyMoment(Moment moment)
        {
            World.GetNearbyVehicles(Game.Player.Character.Position, 100f)
                .Where(x => x != Vehicle)
                .ToList()
                .ForEach(x => x.DeleteCompletely());

            foreach (var vehicleInfo in moment.Vehicles)
            {
                Utils.SpawnFromVehicleInfo(vehicleInfo);
            }

            if (World.Weather != moment.Weather)
                World.Weather = moment.Weather;

            RainPuddleEditor.Level = moment.PuddleLevel;
        }

        public Moment GetStoredMoment(DateTime currentTime, int maxHoursRange)
        {
            Moment foundMoment = null;

            foreach (var moment in momentsInTime)
            {
                var momentDate = moment.CurrentDate;
                UI.Notify(momentDate.ToString());

                // Let's advance time temporarily to see if the two times still match up.
                var currentTimeAdvanced = currentTime.AddHours(maxHoursRange);
                var momentDateAdvanced = momentDate.AddHours(maxHoursRange);
                if (momentDateAdvanced.Year != currentTimeAdvanced.Year || momentDateAdvanced.Month != currentTimeAdvanced.Month ||
                    momentDateAdvanced.Day != currentTimeAdvanced.Day) continue;

                if (currentTime.Hour >= momentDate.Hour && currentTime.Hour <= momentDate.Hour + maxHoursRange)
                {
                    foundMoment = moment;
                    break;
                }
            }

            return foundMoment;
        }

    }
}
