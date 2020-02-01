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
    public class FreezeHandler : Handler
    {
        private int gameTimer;
        private int currentStep;
        private bool isFreezing;

        private AudioPlayer coldAudio;
        private AudioPlayer ventAudio;

        private PtfxEntityPlayer leftSmokePtfx;
        private PtfxEntityPlayer rightSmokePtfx;

        private int smokeIndex;

        public FreezeHandler(TimeCircuits circuits) : base(circuits)
        {
            coldAudio = new AudioPlayer("cold.wav", false, 1);
            ventAudio = new AudioPlayer("vent.wav", false);

            leftSmokePtfx = new PtfxEntityPlayer("scr_familyscenem", "scr_meth_pipe_smoke", Vehicle, new Vector3(0.5f, -2f, 0.7f), new Vector3(10f, 0, 180f), 10f, false, false);
            rightSmokePtfx = new PtfxEntityPlayer("scr_familyscenem", "scr_meth_pipe_smoke", Vehicle, new Vector3(-0.5f, -2f, 0.7f), new Vector3(10f, 0, 180f), 10f, false, false);
        }

        public void StartFreezeHandling()
        {
            isFreezing = true;
        }

        public override void Process()
        {
            coldAudio?.Process();
            ventAudio?.Process();

            if(Vehicle.DirtLevel != 0)
            {
                Vehicle.DirtLevel = Utils.Lerp(Vehicle.DirtLevel, 0, Game.LastFrameTime * 0.1f);
                if (Vehicle.DirtLevel < 1)
                    Vehicle.DirtLevel = 0;
            }

            if (Vehicle == null) return;
            if (!isFreezing) return;
            if (Game.GameTime < gameTimer) return;

            switch(currentStep)
            {
                case 0:
                    Vehicle.DirtLevel = 12;

                    gameTimer = Game.GameTime + 2000;
                    currentStep++;
                    break;

                case 1:

                    coldAudio.Play(Vehicle);
                    gameTimer = Game.GameTime + 15000;
                    currentStep++;
                    break;

                case 2:

                    if (DeloreanType == DeloreanType.BTTF)
                    {
                        ventAudio.Play(Vehicle);
                        currentStep++;
                        gameTimer = Game.GameTime + 1000;
                    }
                    else
                    {
                        currentStep = 4;
                        gameTimer = Game.GameTime + 5000;
                    }
                    break;

                case 3:
                    for (; smokeIndex < 7;)
                    {
                        rightSmokePtfx.Play(true);
                        leftSmokePtfx.Play(true);

                        gameTimer = Game.GameTime + 500;

                        smokeIndex++;

                        return;
                    }

                    currentStep++;
                    gameTimer = Game.GameTime + 1000;
                    break;

                case 4:

                    TimeCircuits.GetHandler<FuelHandler>().UpdateFuel();
                    Stop();
                    break;
            }
        }

        public override void KeyPress(Keys key)
        {
        }

        public override void Stop()
        {
            currentStep = 0;
            gameTimer = 0;
            isFreezing = false;
        }
    }
}
