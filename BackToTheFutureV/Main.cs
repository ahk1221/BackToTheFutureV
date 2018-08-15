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
using BackToTheFutureV.Memory;

namespace BackToTheFutureV
{
    public class Main : Script
    {
        public Delorean delorean;

        public PtfxEntityPlayer ptfx;

        public Main()
        {
            Tick += Main_Tick;
            KeyDown += Main_KeyDown;
            Aborted += Main_Aborted;
        }

        private void Main_Aborted(object sender, EventArgs e)
        {
            delorean?.Vehicle?.Delete();
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            delorean?.KeyDown(e);

            if(e.KeyCode == Keys.L)
            {
                delorean = new Delorean(Game.Player.Character.Position, Game.Player.Character.Heading);
                Game.Player.Character.Task.WarpIntoVehicle(delorean.Vehicle, VehicleSeat.Driver);
            }

            if(e.KeyCode == Keys.H)
            {
                UI.ShowSubtitle(World.CurrentDate.ToString());
            }

            if(e.KeyCode == Keys.J)
            {
                if (Game.Player.Character.CurrentVehicle == null) return;

                if (ptfx == null)
                    ptfx = new PtfxEntityPlayer("core", "ent_amb_barrel_fire", Game.Player.Character.CurrentVehicle, new Vector3(-0.7742f, 1.306987f, -0.255118f), Vector3.Zero, 1f, true, false);

                ptfx.Play();
            }
        }

        private void Main_Tick(object sender, EventArgs e)
        {
            delorean?.Tick();

            ptfx?.Process();
        }
    }
}
