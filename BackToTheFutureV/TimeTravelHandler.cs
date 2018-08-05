using System;
using System.Collections.Generic;
using System.Linq;
using BackToTheFutureV.Entities;
using GTA;

namespace BackToTheFutureV
{
    public class TimeTravelHandler
    {
        private AudioPlayer timeTravelAudio;
        private AudioPlayer reentryAudio;

        public TimeCircuits timeCircuits;

        private bool firstTick = true;
        private bool hasFadedScreenOut;
        private bool hasSetupDates;
        private bool hasFadedScreenIn;
        private bool hasPlayedReentryAudio;

        public bool hasEnded;

        private float gameTimer;

        public TimeTravelHandler(TimeCircuits circuits)
        {
            timeTravelAudio = new AudioPlayer("./scripts/BackToTheFutureV/sounds/time_travel.wav", false);
            reentryAudio = new AudioPlayer("./scripts/BackToTheFutureV/sounds/reentry.wav", false);

            timeCircuits = circuits;
        }

        public void Process()
        {
            if(firstTick)
            {
                gameTimer = Game.GameTime + 2000;

                timeTravelAudio.Play();

                timeCircuits.Vehicle.IsVisible = false;
                timeCircuits.Vehicle.HasCollision = false;
                timeCircuits.Vehicle.FreezePosition = true;
                Game.Player.Character.IsVisible = false;

                firstTick = false;
            }

            if(Game.GameTime > gameTimer)
            {
                if(!hasFadedScreenOut)
                {
                    Game.FadeScreenOut(1000);
                    gameTimer = Game.GameTime + 1500;
                    hasFadedScreenOut = true;

                    return;
                }

                if(!hasSetupDates)
                {
                    timeCircuits.PreviousTime = new DateTime(World.CurrentDate.Year, World.CurrentDate.Month, World.CurrentDate.Hour, World.CurrentDayTime.Hours, World.CurrentDayTime.Minutes, World.CurrentDayTime.Seconds);
                    World.CurrentDayTime = new TimeSpan(timeCircuits.DestinationTime.Hour, timeCircuits.DestinationTime.Minute, timeCircuits.DestinationTime.Second);
                    World.CurrentDate = timeCircuits.DestinationTime;

                    gameTimer = Game.GameTime + 1000;
                    hasSetupDates = true;

                    return;
                }

                if(!hasFadedScreenIn)
                {
                    Game.FadeScreenIn(1000);
                    gameTimer = Game.GameTime + 700;
                    hasFadedScreenIn = true;

                    return;
                }

                if(!hasPlayedReentryAudio)
                {
                    reentryAudio.Play();
                    gameTimer = Game.GameTime + 500;
                    hasPlayedReentryAudio = true;

                    return;
                }

                if(!hasEnded)
                {
                    timeCircuits.Vehicle.IsVisible = true;
                    timeCircuits.Vehicle.HasCollision = true;
                    timeCircuits.Vehicle.FreezePosition = false;
                    Game.Player.Character.IsVisible = true;

                    timeCircuits.MPHSpeed = 65;

                    hasEnded = true;
                }
            }
        }
    }
}
