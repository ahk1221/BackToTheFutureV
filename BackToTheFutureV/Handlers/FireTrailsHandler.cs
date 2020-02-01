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
    public class FireTrailsHandler : Handler
    {
        private List<PtfxEntityPlayer> fireTrailPtfxs = new List<PtfxEntityPlayer>();

        private float currentStrength;

        public FireTrailsHandler(TimeCircuits circuits) : base(circuits)
        {
        }

        public void SpawnFireTrails()
        {
            Stop();

            // World positions
            Vector3 leftWheelOffset = Vehicle.GetOffsetFromWorldCoords(Vehicle.GetBoneCoord("wheel_lf"));
            Vector3 rightWheelOffset = Vehicle.GetOffsetFromWorldCoords(Vehicle.GetBoneCoord("wheel_rf"));

            float baseOffset = 0.3f;

            for (int i = 0; i < 30; i++)
            {
                Vector3 leftPosOffset = leftWheelOffset + new Vector3(0, i * baseOffset, -0.2f);
                Vector3 rightPosOffset = rightWheelOffset + new Vector3(0, i * baseOffset, -0.2f);

                PtfxEntityPlayer leftWheelPtfx = new PtfxEntityPlayer("core", "fire_petrol_one", Vehicle, leftPosOffset, Vector3.Zero, 1.2f, true, false);
                PtfxEntityPlayer rightWheelPtfx = new PtfxEntityPlayer("core", "fire_petrol_one", Vehicle, rightPosOffset, Vector3.Zero, 1.2f, true, false);

                leftWheelPtfx.SetEvolutionParam("strength", 1f);
                leftWheelPtfx.SetEvolutionParam("dist", 1f);
                leftWheelPtfx.SetEvolutionParam("fadein", 0.1f);

                rightWheelPtfx.SetEvolutionParam("strength", 1);
                rightWheelPtfx.SetEvolutionParam("dist", 1f);
                rightWheelPtfx.SetEvolutionParam("fadein", 0.15f);

                currentStrength = 1f;

                fireTrailPtfxs.Add(leftWheelPtfx);
                fireTrailPtfxs.Add(rightWheelPtfx);
            }

            fireTrailPtfxs.ForEach(x => x.Play());
        }

        public override void Process()
        {
            if(fireTrailPtfxs.Count > 0)
            {
                var tParam = Game.LastFrameTime * 0.1f;
                currentStrength = Utils.Lerp(currentStrength, 0, tParam);

                fireTrailPtfxs.ForEach(x => x.SetEvolutionParam("strength", currentStrength));

                if (currentStrength < 0.003)
                    Stop();
            }
        }

        public override void Stop()
        {
            fireTrailPtfxs.ForEach(x => x.Stop());
            fireTrailPtfxs.Clear();
        }

        public override void KeyPress(Keys key)
        {
        }
    }
}
