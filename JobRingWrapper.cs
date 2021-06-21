using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Num = System.Numerics;
using Dalamud.Plugin;

namespace PixelPerfect
{
    class JobRingWrapper
    {
        private int[,] _radii;
        private Num.Vector4[,] _colors;
        //This cannot be kept indefinitely, unless support for clases / DOH / DOL never added. 
        private int indexoffset = 19;
        private static float _modulo = 255;

        //General Colors, shown in RGB. 
        //  Consider keeping these as r/g/b/a without modulo, and somehow dividing it in the method below?
        private Num.Vector4 aoeDPS = new Num.Vector4(161 / _modulo, 34 / _modulo, 55 / _modulo, 255 / _modulo);
        private Num.Vector4 aoeHeal = new Num.Vector4(19 / _modulo, 117/_modulo, 35 / _modulo, 255 / _modulo);
        private Num.Vector4 aoeMit = new Num.Vector4(66 / _modulo, 207 / _modulo, 200 / _modulo, 255 / _modulo);
        private Num.Vector4 meleeRange = new Num.Vector4(255f / _modulo, 255f / _modulo, 255f / _modulo, 255f / _modulo);

        //I do not wanna display the following no matter what. Easiest is to turn alpha to 0, this is just a step further.
        private Num.Vector4 nullVec = new Num.Vector4(0, 0, 0, 0);

        //Special Colors
        private Num.Vector4 exHeal = new Num.Vector4(195 / _modulo, 54 / _modulo, 164 / _modulo, 255 / _modulo);
        public JobRingWrapper()
        {
            _colors = new Num.Vector4[,]
            {
                //Starts at pld, so index-19. 3 fields for three colors.
                //Will need to add additional fields on expac, and also if I add support for classes (no). 
                {aoeMit, meleeRange, nullVec }, //pld
                {aoeDPS, meleeRange, nullVec}, //mnk
                {aoeMit, meleeRange, nullVec}, //war
                {aoeDPS, aoeDPS, nullVec }, //drg
                {aoeDPS, aoeMit, nullVec }, //brd
                {exHeal, aoeDPS, meleeRange }, // whm
                {aoeMit, nullVec, nullVec}, //blm
                {nullVec, nullVec, nullVec },  //acn
                {nullVec, nullVec, nullVec },  //smn
                {aoeHeal, meleeRange, nullVec }, //sch
                {nullVec, nullVec, nullVec }, //rog
                {meleeRange, nullVec, nullVec }, //nin
                {aoeMit, nullVec, nullVec }, //mch
                {aoeMit, meleeRange, nullVec }, //drk
                {aoeHeal, nullVec, nullVec }, //ast
                {meleeRange, nullVec, nullVec }, //sam
                {aoeDPS, nullVec, nullVec }, //rdm
                {nullVec, nullVec, nullVec }, //blu
                {aoeMit, meleeRange, nullVec }, //gnb
                {exHeal, aoeDPS, meleeRange }, //dnc

            };

            _radii = new int[,]
            {
                {-1,0,0}, //classes and gatherers etc
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                { 15, 5 , 0 },   //pld
                { 15, 5, 0 },   //mnk
                { 15, 5, 0 },   //war
                { 15, 10, 0 },  //drg
                { 25, 20, 0 },  //brd
                { 20, 15, 6 },   //whm
                { 25, 0, 0},    //blm
                { -1, 0, 0 },   //Arcanist    
                { -1, 0, 0},    //Summoner
                { 15, 5, 0 },   //Scholar
                {-1, 0, 0 },    //Rogue
                { 5, 0, 0 },    //Ninja
                { 20, 0, 0 },   //mch
                { 15, 5 , 0 },  //DRK
                { 15, 0, 0 },   //ast
                { 5, 0, 0 },    //sam
                { 15, 0, 0 }, //rdm
                { -1, 0, 0 }, //blu
                { 15, 5 , 0 }, //gnb
                { 20, 15, 5} //dnc
            };


        }

        //Returns an array containing up to three colours associated with the class' rings. 
        public Num.Vector4[]  GetColors(Dalamud.Game.ClientState.Actors.Types.PlayerCharacter character, DalamudPluginInterface pinterface)
        {
            //We shouldn't display 'default' aoes unless it's the player character's. 
            if( character != pinterface.ClientState.LocalPlayer)
            {
                meleeRange.W = -1;
            }

            //We SHOULD display 'default' aoes if it's the local player character. 
            if(character == pinterface.ClientState.LocalPlayer)
            {
                meleeRange.W = 1;
            }
            int index = (int)character.ClassJob.Id;
            //If a class or DoH/DoL, throw it away now. 
            index -= indexoffset;
            if (index  < 0)
                return null;

            //Array to hold our retrieved colors.
            Num.Vector4[] colorlist = new Num.Vector4[3];
            for (int i = 0; i < 3; i++)
            {
                colorlist[i] = _colors[index, i];
            }
            return colorlist;
        }

        //Returns an array containing up to three radii associated with the class' rings. 
        public int[] GetRadii(int index)
        {
            //If a class or DoH/DoL, throw it away now. 
            if (index < 19) return null;

            //Array to hold our retrieved radii.
            int[] templist = new int[3];

            for(int i = 0; i < templist.Length; i++)
            {
                templist[i] = _radii[index, i];
            }
            return templist;
        }
    }
}




/* issues: 
 * Maintainability.
 * It is somewhat annoying that the given values on tooltips don't seem right?
 */