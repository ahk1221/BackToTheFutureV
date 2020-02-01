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
using System.Windows.Forms;

namespace BackToTheFutureV.Handlers
{
    public class SparksHandler : Handler
    {
        private AudioPlayer sparksAudio;
        private AudioPlayer diodesGlowingSound;

        private bool hasPlayedDiodeSound;
        private int timeTravelAt;
        private int startSparksAt;

        private readonly string[] wheelNames = new string[4]
        {
            "wheel_lf",
            "wheel_lr",
            "wheel_rr",
            "wheel_rf"
        };

        private List<PtfxEntityPlayer> sparksPtfxs = new List<PtfxEntityPlayer>();
        private List<PtfxEntityPlayer> wheelPtfxes = new List<PtfxEntityPlayer>();

        public SparksHandler(TimeCircuits circuits) : base(circuits)
        {
            sparksAudio = new AudioPlayer($"sparks_{LowerCaseDeloreanType}.wav", false, 0.7f);
            diodesGlowingSound = new AudioPlayer("diodes_glowing.wav", false, 1);

            sparksPtfxs.Add(new PtfxEntityPlayer("des_bigjobdrill", "ent_ray_big_drill_sparks", Vehicle, new Vector3(0, 3f, 0), Vector3.Zero, 3.5f, true, true, 15));
            sparksPtfxs.Add(new PtfxEntityPlayer("scr_paletoscore", "scr_paleto_box_sparks", Vehicle, new Vector3(0, 3f, 0), new Vector3(0, 0, 180), 1.5f, true, true, 300));
            sparksPtfxs.Add(new PtfxEntityPlayer("scr_reconstructionaccident", "sp_sparking_generator", Vehicle, new Vector3(0, 3f, 0), Vector3.Zero, 1.5f, true, true, 400));

            foreach (var wheelName in wheelNames)
            {
                var worldPos = Vehicle.GetBoneCoord(wheelName);
                var offset = Vehicle.GetOffsetFromWorldCoords(worldPos);

                offset = new Vector3(offset.X, offset.Y - 0.3f, offset.Z - 0.15f);

                var ptfx = new PtfxEntityPlayer("scr_carsteal4", "scr_carsteal5_car_muzzle_flash", Vehicle, offset, new Vector3(0f, 0f, -90f), 1f, true, true, 15);

                wheelPtfxes.Add(ptfx);
            }
        }

        public override void Process()
        {
            if (MPHSpeed < 82)
            {
                Stop();
            }

            if (MPHSpeed >= 82)
            {
                if (!hasPlayedDiodeSound)
                {
                    diodesGlowingSound.Play();
                    hasPlayedDiodeSound = true;
                }
            }

            if (MPHSpeed >= 88)
            {
                if (timeTravelAt == -1)
                    timeTravelAt = Game.GameTime + 6000;

                if (!sparksAudio.IsPlaying)
                {
                    sparksAudio.Play();
                    startSparksAt = Game.GameTime + 1000;
                }

                if (Game.GameTime > startSparksAt)
                {
                    sparksPtfxs.ForEach(x =>
                    {
                        x.Play();
                        x.Process();
                    });
                    wheelPtfxes.ForEach(x =>
                    {
                        x.Play();
                        x.Process();
                    });

                    Vehicle.SetMod(VehicleMod.Spoilers, 0, true);

                    if (Game.GameTime > timeTravelAt)
                        SparksEnded();
                }
            }
        }

        public override void Stop()
        {
            sparksAudio.Stop();
            diodesGlowingSound.Stop();
            wheelPtfxes.ForEach(x => x.Stop());
            sparksPtfxs.ForEach(x => x.Stop());
            Vehicle.SetMod(VehicleMod.Spoilers, 1, true);

            hasPlayedDiodeSound = false;
            timeTravelAt = -1;
        }

        public override void KeyPress(Keys key)
        {
        }

        private void SparksEnded()
        {
            Stop();

            Function.Call(Hash.DETACH_VEHICLE_FROM_ANY_TOW_TRUCK, Vehicle.Handle);

            TimeCircuits?.GetHandler<TimeTravelHandler>()?.StartTimeTravelling();
        }
    }
}
