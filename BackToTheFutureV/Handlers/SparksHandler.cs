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

namespace BackToTheFutureV.Handlers
{
    public class SparksHandler : Handler
    {
        private AudioPlayer sparksAudio;
        private AudioPlayer diodesGlowingSound;

        private bool hasPlayedDiodeSound;

        private readonly string[] wheelNames = new string[4]
        {
            "wheel_lf",
            "wheel_lr",
            "wheel_rr",
            "wheel_rf"
        };

        private PtfxEntityPlayer sparksPtfx;
        private List<PtfxEntityPlayer> wheelPtfxes = new List<PtfxEntityPlayer>();

        public SparksHandler(TimeCircuits circuits) : base(circuits)
        {
            sparksAudio = new AudioPlayer("./scripts/BackToTheFutureV/sounds/sparks.wav", true, 2);
            diodesGlowingSound = new AudioPlayer("./scripts/BackToTheFutureV/sounds/diodes_glowing.wav", false, 1);

            sparksPtfx = new PtfxEntityPlayer("des_bigjobdrill", "ent_ray_big_drill_sparks", Vehicle, new Vector3(0, 3f, 0), Vector3.Zero, 3.5f, true, true, 5);
            foreach(var wheelName in wheelNames)
            {
                var worldPos = Vehicle.GetBoneCoord(wheelName);
                var offset = Vehicle.GetOffsetFromWorldCoords(worldPos);

                offset = new Vector3(offset.X, offset.Y - 0.5f, offset.Z - 0.15f);

                var ptfx = new PtfxEntityPlayer("scr_carsteal4", "scr_carsteal5_car_muzzle_flash", Vehicle, offset, new Vector3(0f, 0f, -90f), 1.5f, true, true, 15);

                wheelPtfxes.Add(ptfx);
            }
        }

        public override void Process()
        {
            if (MPHSpeed < 80)
            {
                sparksAudio.Stop();

                sparksPtfx.Stop();
                diodesGlowingSound.Stop();
                wheelPtfxes.ForEach(x => x.Stop());
            }

            if(MPHSpeed >= 82)
            {
                if(!hasPlayedDiodeSound)
                {
                    diodesGlowingSound.Play();
                    hasPlayedDiodeSound = true;
                }
            }

            if (MPHSpeed >= 84 && MPHSpeed < 88)
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

                var pos = Vehicle.Position + new Vector3(0, 3f, 0);

                World.DrawLightWithRange(pos, Color.SkyBlue, 1f, 1f);
            }

            if (MPHSpeed >= 88)
            {
                sparksAudio.Stop();
                sparksPtfx.Stop();
                diodesGlowingSound.Stop();
                wheelPtfxes.ForEach(x => x.Stop());

                hasPlayedDiodeSound = false;

                TimeCircuits?.GetHandler<TimeTravelHandler>()?.StartTimeTravelling();
            }
        }

        public override void Stop()
        {
            sparksPtfx.Stop();
            sparksAudio.Stop();
            diodesGlowingSound.Stop();
            wheelPtfxes.ForEach(x => x.Stop());

            hasPlayedDiodeSound = false;
        }
    }
}
