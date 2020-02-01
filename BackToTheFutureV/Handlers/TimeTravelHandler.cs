using System;
using System.Collections.Generic;
using System.Linq;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Memory;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Windows.Forms;

namespace BackToTheFutureV.Handlers
{
    public delegate void OnTimeTravelComplete();

    public enum TimeTravelMode
    {
        CutScene, Instant 
    }

    public class TimeTravelHandler : Handler
    {
        public TimeTravelMode CurrentMode { get; set; }
        public string LowerCaseCurrentMode => CurrentMode.ToString().ToLower();

        public OnTimeTravelComplete OnTimeTravelComplete { get; set; }

        private AudioPlayer timeTravelAudio;

        private bool isTimeTravelling = false;
        private int currentStep;
        private float gameTimer;

        private List<Delorean> copies = new List<Delorean>();

        public TimeTravelHandler(TimeCircuits circuits) : base(circuits)
        {
            timeTravelAudio = new AudioPlayer($"{LowerCaseCurrentMode}_timetravel_{LowerCaseDeloreanType}.wav", false, 1.2f);
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

            timeTravelAudio?.Dispose();
            timeTravelAudio = null;

            timeTravelAudio = new AudioPlayer($"{LowerCaseCurrentMode}_timetravel_{LowerCaseDeloreanType}.wav", false, 2);
        }

        public override void Process()
        {
            timeTravelAudio?.Process();

            if (Vehicle == null) return;
            if (!isTimeTravelling) return;
            if (Game.GameTime < gameTimer) return;

            switch(currentStep)
            {
                case 0:
                    timeTravelAudio.Play(Vehicle);

                    // If the current Time Travel mode is Instant
                    if (CurrentMode != TimeTravelMode.CutScene)
                    {
                        // Have to call SetupJump manually here.
                        TimeHandler.TimeTravelTo(TimeCircuits, DestinationTime);
                        
                        // Set MPHSpeed
                        MPHSpeed = 65;

                        // Stop handling
                        Stop();
                        return;
                    }

                    TimeCircuits.GetHandler<FireTrailsHandler>().SpawnFireTrails();

                    Utils.HideVehicle(Vehicle, true);

                    // If the Vehicle is remote controlled or the player is not the one in the driver seat
                    if (IsRemoteControlled || Vehicle.GetPedOnSeat(VehicleSeat.Driver) != Game.Player.Character)
                    {
                        // Stop remote controlling
                        TimeCircuits.GetHandler<RCHandler>().StopRC();

                        // Add to time travelled list
                        TimeHandler.AddToTimeTravelledList(TimeCircuits);

                        // Reset currentStep + other fields.
                        ResetFields();
                        return;
                    }

                    gameTimer = Game.GameTime + 4000;

                    currentStep++;
                    break;

                case 1:
                    Game.FadeScreenOut(1000);
                    gameTimer = Game.GameTime + 1500;

                    currentStep++;
                    break;

                case 2:
                    TimeHandler.TimeTravelTo(TimeCircuits, DestinationTime);
                    TimeCircuits.GetHandler<FireTrailsHandler>().Stop();
                    gameTimer = Game.GameTime + 1000;

                    currentStep++;
                    break;

                case 3:
                    Game.FadeScreenIn(1000);
                    gameTimer = Game.GameTime + 2000;

                    currentStep++;
                    break;

                case 4:
                    Reenter();
                    TimeHandler.AddToTimeTravelledList(TimeCircuits);

                    ResetFields();
                    break;
            }
        }

        public override void Stop()
        {
            ResetFields();

            Utils.HideVehicle(Vehicle, false);
        }

        public override void KeyPress(Keys key)
        {
            if (Game.Player.Character.CurrentVehicle != Vehicle) return;

            if (key == Keys.O)
            {
                ToggleModes();
                Utils.DisplayHelpText("Time Travel Mode now set to " + CurrentMode.ToString() + " Mode.");
            }
        }

        public void ResetFields()
        {
            currentStep = 0;
            isTimeTravelling = false;
            gameTimer = 0;
        }

        public void Reenter()
        {
            var reentryHandler = TimeCircuits.GetHandler<ReentryHandler>();

            if(!reentryHandler.IsReentering)
            {
                reentryHandler.OnReentryComplete = OnReentryComplete;
                reentryHandler.StartReentering();
            }
        }

        private void OnReentryComplete()
        {
            Stop();
            MPHSpeed = 87;

            TimeCircuits.GetHandler<FreezeHandler>().StartFreezeHandling();
        }
    }


}
