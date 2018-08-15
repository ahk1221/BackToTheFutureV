using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using BackToTheFutureV.Entities;
using System.Drawing;

namespace BackToTheFutureV
{
    public class SparksHandler
    {
        public TimeCircuits TimeCircuits { get; }

        private AudioPlayer sparksAudio;

        private readonly string[] wheelNames = new string[4]
        {
            "wheel_lf",
            "wheel_lr",
            "wheel_rr",
            "wheel_rf"
        };

        private PtfxEntityPlayer sparksPtfx;
        private List<PtfxEntityPlayer> wheelPtfxes = new List<PtfxEntityPlayer>();

        public SparksHandler(TimeCircuits circuits)
        {
            TimeCircuits = circuits;

            sparksAudio = new AudioPlayer("./scripts/BackToTheFutureV/sounds/sparks.wav", true, 2);

            sparksPtfx = new PtfxEntityPlayer("des_bigjobdrill", "ent_ray_big_drill_sparks", TimeCircuits.Vehicle, new Vector3(0, 3f, 0), Vector3.Zero, 3.5f, true, true, 15);
            foreach(var wheelName in wheelNames)
            {
                var worldPos = TimeCircuits.Vehicle.GetBoneCoord(wheelName);
                var offset = TimeCircuits.Vehicle.GetOffsetFromWorldCoords(worldPos);

                offset = new Vector3(offset.X, offset.Y - 0.5f, offset.Z - 0.15f);

                var ptfx = new PtfxEntityPlayer("scr_carsteal4", "scr_carsteal5_car_muzzle_flash", TimeCircuits.Vehicle, offset, -TimeCircuits.Vehicle.ForwardVector, 1.5f, true, true, 15);

                wheelPtfxes.Add(ptfx);
            }
        }

        public void Process()
        {
            if (TimeCircuits.MPHSpeed < 80)
            {
                sparksAudio.Stop();
                sparksPtfx.Stop();
                wheelPtfxes.ForEach(x => x.Stop());
            }

            if (TimeCircuits.MPHSpeed >= 80 && TimeCircuits.MPHSpeed < 88)
            {
                if (!sparksAudio.IsPlaying)
                    sparksAudio.Play();

                if (!sparksPtfx.IsPlaying)
                    sparksPtfx.Play();

                sparksPtfx.Process();
                wheelPtfxes.ForEach(x => x.Process());

                for (int i = 0; i < wheelPtfxes.Count; i++)
                {
                    wheelPtfxes[i].Play();
                }

                var pos = TimeCircuits.Vehicle.Position + new Vector3(0, 3f, 0);

                World.DrawLightWithRange(pos, Color.Blue, 1f, 1f);
            }

            if (TimeCircuits.MPHSpeed >= 88)
            {
                sparksAudio.Stop();
                sparksPtfx.Stop();
                wheelPtfxes.ForEach(x => x.Stop());

                TimeCircuits.TimeTravelHandler?.StartTimeTravelling();
            }
        }

        public void ForceStop()
        {
            sparksPtfx.Stop();
            sparksAudio.Stop();
            wheelPtfxes.ForEach(x => x.Stop());
        }
    }
}
