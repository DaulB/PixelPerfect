using System;
using Dalamud.Plugin;
using System.Runtime.InteropServices;
using ImGuiNET;
using ImGuiScene;
using Dalamud.Configuration;
using Num = System.Numerics;
using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;

namespace PixelPerfect
{
    class PartyMembers
    {
        private PlayerCharacter[] partymembers = new PlayerCharacter[8];

        public PlayerCharacter[] GetPartyMembers(DalamudPluginInterface _plugininterface)
        {
            //Open the Actor table...
            int counter = 0;
            int partysize = 8;

            for(int i = 0; i<_plugininterface.ClientState.Actors.Length; i++)
            {
                if(_plugininterface.ClientState.Actors[i] != null)
                {
                    byte flag = Marshal.ReadByte(_plugininterface.ClientState.Actors[i].Address + 0x19A0 + 16);

                    bool isPartyMember = (flag & 16) > 0;
                    if (isPartyMember)
                    {
                        PluginLog.Log("Found individual " + _plugininterface.ClientState.Actors[i].Name + " who is a party member.");
                    }

                }
                
            }

            return null;
            for (int i = 0; i < _plugininterface.ClientState.Actors.Length && counter <partysize; i++)
            {
                //make sure the entry isn't null before we seg fault
                if (_plugininterface.ClientState.Actors[i] != null)
                {
                    //next, check that the actor is a playercharacter
                    if (_plugininterface.ClientState.Actors[i].ObjectKind == ObjectKind.Player)
                    {
                         
                        byte flag = Marshal.ReadByte(_plugininterface.ClientState.Actors[i].Address + 0x19A0, 16);

                        //Lets check that the person is a party member.
                        if (flag == 16)
                        {
                            PluginLog.Log("Found Party Character " + _plugininterface.ClientState.Actors[i].Name);
                            PlayerCharacter tempact = (PlayerCharacter)_plugininterface.ClientState.Actors[i];
                            partymembers[counter] = tempact;                  
                        }
                    }
                }
                counter++;
            }
            /*
            for(int i = counter; i < 8; i++)
            {
                //fill the remainder of the list with null. 
                partymembers[i] = null;
            }
            */
            return null;
        }
    }
}
