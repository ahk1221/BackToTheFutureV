using System;
using System.Collections.Generic;
using System.Linq;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Memory;
using GTA;
using GTA.Native;
using GTA.Math;

namespace BackToTheFutureV
{
    public class TimeTravelHandler
    {
        private AudioPlayer timeTravelAudio;
        private AudioPlayer reentryAudio;

        public TimeCircuits timeCircuits;

        private bool isTimeTravelling = false;
        private int currentStep;
        private float gameTimer;

        private List<Moment> momentsInTime = new List<Moment>();

        public TimeTravelHandler(TimeCircuits circuits)
        {
            timeTravelAudio = new AudioPlayer("./scripts/BackToTheFutureV/sounds/time_travel.wav", false, 2);
            reentryAudio = new AudioPlayer("./scripts/BackToTheFutureV/sounds/reentry.wav", false, 2);

            timeCircuits = circuits;
        }

        public void StartTimeTravelling()
        {
            isTimeTravelling = true;
            gameTimer = 0;
        }

        public void Process()
        {
            if (!isTimeTravelling) return;
            if (Game.GameTime < gameTimer) return;

            switch(currentStep)
            {
                case 0:
                    gameTimer = Game.GameTime + 2000;

                    timeTravelAudio.Play();

                    timeCircuits.Vehicle.IsVisible = false;
                    timeCircuits.Vehicle.HasCollision = false;
                    timeCircuits.Vehicle.FreezePosition = true;
                    Game.Player.Character.IsVisible = false;

                    currentStep++;
                    break;

                case 1:
                    //Game.FadeScreenOut(1000);
                    gameTimer = Game.GameTime + 1500;

                    currentStep++;
                    break;

                case 2:
                    SetupJump();
                    gameTimer = Game.GameTime + 1000;

                    currentStep++;
                    break;

                case 3:
                    //Game.FadeScreenIn(1000);
                    gameTimer = Game.GameTime + 700;

                    currentStep++;
                    break;

                case 4:
                    reentryAudio.Play();
                    gameTimer = Game.GameTime + 500;

                    World.AddExplosion(timeCircuits.Vehicle.Position, ExplosionType.Rocket, 1f, 0, false, false);

                    currentStep++;
                    break;

                case 5:

                    World.AddExplosion(timeCircuits.Vehicle.Position, ExplosionType.Rocket, 1f, 0, false, false);

                    gameTimer = Game.GameTime + 500;

                    currentStep++;
                    break;

                case 6:
                    World.AddExplosion(timeCircuits.Vehicle.Position, ExplosionType.Rocket, 1f, 0, false, false);

                    timeCircuits.Vehicle.IsVisible = true;
                    timeCircuits.Vehicle.HasCollision = true;
                    timeCircuits.Vehicle.FreezePosition = false;
                    Game.Player.Character.IsVisible = true;

                    timeCircuits.MPHSpeed = 65;

                    currentStep = 0;
                    isTimeTravelling = false;
                    gameTimer = 0;
                    break;
            }
        }

        private void SetupJump()
        {
            // Update the previous time.
            timeCircuits.PreviousTime = Utils.GetWorldTime();

            // Set the new GTA time.
            Utils.SetWorldTime(timeCircuits.DestinationTime);

            // Try to find a stored moment for our time jump
            var moment = GetStoredMoment(timeCircuits.DestinationTime, 4);
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
                moment.CurrentDate = timeCircuits.DestinationTime;

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
            var nearbyVehicles = World.GetNearbyVehicles(timeCircuits.Vehicle.Position, 10f).ToList();

            nearbyVehicles
                .Where(x => x != timeCircuits.Vehicle).ToList()
                .ForEach(x  => x.DeleteCompletely());

            // Far vehicles
            var farVehicles = World.GetNearbyVehicles(timeCircuits.Vehicle.Position, 100f).ToList();

            farVehicles
                .Where(x => x != timeCircuits.Vehicle).ToList()
                .ForEach(v =>
                {
                    if(Utils.Random.NextDouble() < 0.2)
                    {
                        // Change vehicle model

                        var vehicleInfo = new VehicleInfo(v);
                        vehicleInfo.Model = Utils.GetRandomVehicleHash(v.Model, timeCircuits.Vehicle.Position);

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
                .Where(x => x != timeCircuits.Vehicle)
                .Select(x => new VehicleInfo(x));

            return new Moment(currentTime, currentWeather, RainPuddleEditor.Level, infos);
        }

        public void ApplyMoment(Moment moment)
        {
            World.GetNearbyVehicles(Game.Player.Character.Position, 100f)
                .Where(x => x != timeCircuits.Vehicle)
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

    public class Moment
    {
        public Moment(DateTime currentDate, Weather weather, float puddleLevel, IEnumerable<VehicleInfo> infos)
        {
            CurrentDate = currentDate;
            Weather = weather;
            PuddleLevel = puddleLevel;
            Vehicles = infos.ToList();
        }

        public Weather Weather { get; set; }

        public float PuddleLevel { get; set; }

        public DateTime CurrentDate { get; set; }

        public List<VehicleInfo> Vehicles { get; set; }
    }
}
