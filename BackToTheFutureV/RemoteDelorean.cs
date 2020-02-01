using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackToTheFutureV.Entities;
using BackToTheFutureV.Handlers;
using GTA;

namespace BackToTheFutureV
{
    public class RemoteDelorean
    {
        public Delorean InfoCopy { get; }

        private int timer;
        private bool hasPlayedWarningSound;

        private AudioPlayer warningSound = new AudioPlayer("rc_warning.wav", false);

        public RemoteDelorean(Delorean copy)
        {
            InfoCopy = copy;
        }

        public void Process()
        {
            if(Utils.GetWorldTime() > (InfoCopy.Circuits.DestinationTime - new TimeSpan(0, 1, 0)) && Utils.GetWorldTime() < (InfoCopy.Circuits.DestinationTime + new TimeSpan(0, 2, 0)) && !hasPlayedWarningSound)
            {
                warningSound.Play();
                hasPlayedWarningSound = true;
            }

            if (Game.GameTime > timer && Utils.GetWorldTime() > InfoCopy.Circuits.DestinationTime && Utils.GetWorldTime() < (InfoCopy.Circuits.DestinationTime + new TimeSpan(0, 1, 0)))
            {
                UI.ShowSubtitle("Spawning...");

                var del = InfoCopy.CreateCopy(false);

                del.Circuits.GetHandler<TimeTravelHandler>().Reenter();

                timer = Game.GameTime + 3000;
                hasPlayedWarningSound = false;
            }
        }
    }
}
