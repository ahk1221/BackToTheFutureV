using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using BackToTheFutureV.Entities;
using System.Windows.Forms;

namespace BackToTheFutureV.Handlers
{
    public delegate void OnReentryComplete();

    public class ReentryHandler : Handler
    {
        public bool IsReentering { get; private set; }

        public OnReentryComplete OnReentryComplete { get; set; }

        private AudioPlayer reentryAudio;

        private PtfxEntityPlayer flash;

        private int currentStep;
        private int gameTimer;

        public ReentryHandler(TimeCircuits circuits) : base(circuits)
        {
            reentryAudio = new AudioPlayer($"reentry_{LowerCaseDeloreanType}.wav", false, 2);

            flash = new PtfxEntityPlayer("core", "ent_anim_paparazzi_flash", Vehicle, Vector3.Zero, Vector3.Zero, 50f, false, false);
        }

        public void StartReentering()
        {
            IsReentering = true;
        }

        public override void Process()
        {
            reentryAudio?.Process();

            if (Vehicle == null) return;
            if (!IsReentering) return;
            if (Game.GameTime < gameTimer) return;

            switch(currentStep)
            {
                case 0:
                    reentryAudio.Play(Vehicle);

                    flash.Play(true);

                    int timeToAdd = 500;

                    if (DeloreanType == DeloreanType.BTTF)
                        timeToAdd = 100;
                    else if (DeloreanType == DeloreanType.BTTF2)
                        timeToAdd = 600;
                    else if (DeloreanType == DeloreanType.BTTF3)
                        timeToAdd = 600;

                    gameTimer = Game.GameTime + timeToAdd;
                    currentStep++;
                    break;

                case 1:

                    flash.Play(true);

                    timeToAdd = 500;

                    if (DeloreanType == DeloreanType.BTTF)
                        timeToAdd = 300;
                    else if (DeloreanType == DeloreanType.BTTF2)
                        timeToAdd = 600;
                    else if (DeloreanType == DeloreanType.BTTF3)
                        timeToAdd = 600;

                    gameTimer = Game.GameTime + timeToAdd;
                    currentStep++;
                    break;

                case 2:

                    flash.Play(true);

                    currentStep++;
                    break;

                case 3:
                    Stop();

                    OnReentryComplete?.Invoke();

                    break;
            }
        }

        public override void Stop()
        {
            currentStep = 0;
            gameTimer = 0;
            IsReentering = false;
        }

        public override void KeyPress(Keys key)
        {
        }
    }
}
