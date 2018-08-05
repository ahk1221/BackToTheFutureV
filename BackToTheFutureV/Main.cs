using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using BackToTheFutureV.Entities;

namespace BackToTheFutureV
{
    public class Main : Script
    {
        public Delorean delorean;

        public Main()
        {
            Tick += Main_Tick;
            KeyDown += Main_KeyDown;
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            delorean?.KeyDown(e);

            if(e.KeyCode == Keys.L)
            {
                delorean = new Delorean(Game.Player.Character.Position);
                Game.Player.Character.Task.WarpIntoVehicle(delorean.Vehicle, VehicleSeat.Driver);
            }

            if(e.KeyCode == Keys.J)
            {
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "scr_mp_house");
                Function.Call(Hash._SET_PTFX_ASSET_NEXT_CALL, "scr_mp_house");
                Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_AT_COORD, "scr_sh_lighter_sparks", GTA.Game.Player.Character.Position.X, GTA.Game.Player.Character.Position.Y, GTA.Game.Player.Character.Position.Z + 2f, 0, 0, 0, 3f, 0, 0, 0);
            }
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            delorean?.Tick();
        }
    }
}
