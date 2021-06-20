using System;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using Dalamud.Configuration;
using Num = System.Numerics;
using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
//using 'JobRingWrapper.cs';

namespace PixelPerfect
{
    public class PixelPerfect : IDalamudPlugin
    {
        public string Name => "Pixel Perfect 2.01 BETA";
        private DalamudPluginInterface _pluginInterface;
        private Config _configuration;

        //Activation booleans
        private bool _enabled = true;
        private bool _config;
        private bool _combat = true;
        private bool _circle;
        private bool _instance;
        private bool _ring;

        //Dban Additions
        private bool _ring2;
        private bool _classrings = true;
        private bool _petrings = false;
        private bool _partyrings = false;

        //Colours
        private Num.Vector4 _col = new Num.Vector4(1f, 1f, 1f, 1f);
        private Num.Vector4 _col2 = new Num.Vector4(1f, .5f, .5f, 1f);
        private Num.Vector4 _colRing = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        //Dban additions
        private Num.Vector4 _colRing2 = new Num.Vector4(1f, 1f, 1f, 1f);
        private Num.Vector4 _defaultCol = new Num.Vector4(1f, 1f, 1f, 25f);
        private Num.Vector4[] _classcolors = { new Num.Vector4(1f, 1f, 1f, 1f), new Num.Vector4(1f, 1f, 1f, 1f), new Num.Vector4(1f, 1f, 1f, 1f) };
        private Num.Vector4 _petRingColor = new Num.Vector4(242f / 255f, 206f / 255f, 44f / 255f, 1f);

        //Ring Properties
        private float _radius = 10f;
        private int _segments = 100;
        private float _thickness = 10f;

        //Ring2 Properties
        private float _radius2 = 15f;
        private float _thickness2 = 11f;
        private int _segments2 = 50;

        //ClassRing Properties
        private int _classSegments = 50;
        private float _classOpacity = 100;
        private float _classThickness =10;

        //PetRing Properties
        private int _petSegments = 50;
        private float _petOpacity = 100;
        private float _petThickness = 10;

        
        //Dban classes
        JobRingWrapper Wrapper = new JobRingWrapper();
        PetFinder _petfinder = new PetFinder();
        GetPartyMembers _pmembers;

        public void Initialize(DalamudPluginInterface pI)
        {
            _pluginInterface = pI;
            _configuration = _pluginInterface.GetPluginConfig() as Config ?? new Config();
            _ring = _configuration.Ring;
            _thickness = _configuration.Thickness;
            _colRing = _configuration.ColRing;
            _colRing2 = _configuration.ColRing2;
            _segments = _configuration.Segments;
            _radius = _configuration.Radius;
            _radius2 = _configuration.Radius2;
            _enabled = _configuration.Enabled;
            _classrings = _configuration.ClassRings;
            _combat = _configuration.Combat;
            _circle = _configuration.Circle;
            _ring2 = _configuration.Ring2;
            _instance = _configuration.Instance;
            _col = _configuration.Col;
            _col2 = _configuration.Col2;
            _petrings = _configuration.PetRings;
            _petRingColor = _configuration.PetRingsColor;
            _petThickness = _configuration.PetThickness;
            _petOpacity = _configuration.PetOpacity;
            _classOpacity = _configuration.ClassOpacity;
            _classThickness = _configuration.ClassThickness;
            _classSegments = _configuration.ClassSegments;
            _pluginInterface.UiBuilder.OnBuildUi += DrawWindow;
            _pluginInterface.UiBuilder.OnOpenConfigUi += ConfigWindow;
            _pluginInterface.CommandManager.AddHandler("/pp", new CommandInfo(Command)
            {
                HelpMessage = "Pixel Perfect config."
            });
        }

        public void Dispose()
        {
            _pluginInterface.UiBuilder.OnBuildUi -= DrawWindow;
            _pluginInterface.UiBuilder.OnOpenConfigUi -= ConfigWindow;
            _pluginInterface.CommandManager.RemoveHandler("/pp");
        }

        private void DrawWindow()
        {
            if (_config)
            {
                ImGui.SetNextWindowSize(new Num.Vector2(300, 500), ImGuiCond.FirstUseEver);
                ImGui.Begin("Pixel Perfect Config", ref _config);
                ImGui.Checkbox("Hitbox", ref _enabled);
                ImGui.Checkbox("Outer Ring", ref _circle);
                ImGui.Checkbox("Combat Only", ref _combat);
                ImGui.Checkbox("Instance Only", ref _instance);
                ImGui.ColorEdit4("Colour", ref _col, ImGuiColorEditFlags.NoInputs);
                ImGui.ColorEdit4("Outer Colour", ref _col2, ImGuiColorEditFlags.NoInputs);
                ImGui.Separator();

                ImGui.PushID("Ring1");
                ImGui.Checkbox("Ring", ref _ring);
                ImGui.DragFloat("Radius", ref _radius);
                ImGui.DragFloat("Thickness", ref _thickness);
                ImGui.DragInt("Smoothness", ref _segments);
                ImGui.ColorEdit4("Ring Colour", ref _colRing, ImGuiColorEditFlags.NoInputs);
                ImGui.PopID();
                ImGui.Separator();

                ImGui.PushID("Ring2");
                ImGui.Checkbox("Ring 2", ref _ring2);
                ImGui.DragFloat("Radius", ref _radius2);
                ImGui.DragFloat("Thickness", ref _thickness2);
                ImGui.DragInt("Smoothness", ref _segments2);
                ImGui.ColorEdit4("Ring Colour", ref _colRing2, ImGuiColorEditFlags.NoInputs);


                ImGui.PopID();

                ImGui.Separator();
                ImGui.Checkbox("[Beta] Enable Class Rings?", ref _classrings);

                if (_classrings)
                {
                    ImGui.PushID("Class");
                    ImGui.DragFloat("Thickness", ref _classThickness);
                    ImGui.DragInt("Smoothness", ref _classSegments);
                    ImGui.DragFloat("Opacity", ref _classOpacity);
                    ImGui.PopID();
                }
                ImGui.Separator();

                ImGui.Checkbox("[Beta] Enable Pet Rings?", ref _petrings);
                if(_petrings) 
                {
                    ImGui.PushID("Pets");
                    ImGui.ColorEdit4("Pet Ring Color", ref _petRingColor, ImGuiColorEditFlags.NoInputs);
                    ImGui.DragInt("Smoothness", ref _petSegments);
                    ImGui.DragFloat("Thickness", ref _petThickness);
                    ImGui.DragFloat("Opacity", ref _petOpacity);
                    ImGui.PopID();
                }
                //ImGui.Checkbox("[Beta] Enable Party Rings?", ref _partyrings);

                ImGui.Separator();
                if (ImGui.Button("Save and Close Config"))
                {
                    SaveConfig();
                    _config = false;
                }
                ImGui.SameLine();
                ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);

                if (ImGui.Button("Buy Haplo a Hot Chocolate"))
                {
                    System.Diagnostics.Process.Start("https://ko-fi.com/haplo");
                }
                ImGui.PopStyleColor(3);
                ImGui.End();
            }

            if (!_enabled || _pluginInterface.ClientState.LocalPlayer == null) return;
            if (_combat)
            {
                if (!_pluginInterface.ClientState.Condition[Dalamud.Game.ClientState.ConditionFlag.InCombat]) { return; }
            }

            if (_instance)
            {
                if (!_pluginInterface.ClientState.Condition[Dalamud.Game.ClientState.ConditionFlag.BoundByDuty]) { return; }
            }

            var actor = _pluginInterface.ClientState.LocalPlayer;
            if (!_pluginInterface.Framework.Gui.WorldToScreen(
                new SharpDX.Vector3(actor.Position.X, actor.Position.Z, actor.Position.Y),
                out var pos)) return;
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Num.Vector2(pos.X - 10 - ImGuiHelpers.MainViewport.Pos.X, pos.Y - 10 - ImGuiHelpers.MainViewport.Pos.Y));
            ImGui.Begin("Pixel Perfect", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBackground);
            ImGui.GetWindowDrawList().AddCircleFilled(
                new Num.Vector2(pos.X, pos.Y),
                2f,
                ImGui.GetColorU32(_col),
                100);
            if (_circle)
            {
                ImGui.GetWindowDrawList().AddCircle(
                    new Num.Vector2(pos.X, pos.Y),
                    2.2f,
                    ImGui.GetColorU32(_col2),
                    100);
            }
            ImGui.End();

            if (!_ring && !_classrings) return;
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Num.Vector2(0, 0));
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(new Num.Vector2(0, 0));
            ImGui.Begin("Ring", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground);
            ImGui.SetWindowSize(ImGui.GetIO().DisplaySize);

            //Generate Custom Rings
            if (_ring)
            {
                DrawRingWorld(_pluginInterface.ClientState.LocalPlayer, _radius, _segments, _thickness, ImGui.GetColorU32(_colRing));

                if (_ring2)
                {
                    DrawRingWorld(_pluginInterface.ClientState.LocalPlayer, _radius2, _segments2, _thickness2, ImGui.GetColorU32(_colRing2));
                }
            }

            //Generate Class Rings
            if (_classrings)
            {
                DrawClassRings(_pluginInterface.ClientState.LocalPlayer, _pluginInterface.ClientState.LocalPlayer.ClassJob.Id);
            }

            //Generate Pet Rings [Beta]
            if (_petrings)
            {

                DrawPetRings(_pluginInterface.ClientState.LocalPlayer, _pluginInterface);
            }

            //Generate Party Rings [Not Implemented in 1.6.0.0]
            if (_partyrings)
            {
                //The Class I'm calling will be called 'get party' that returns an array of actors that contains all current party members. 
                //I think that, perhaps, it would be a good idea to ONLY generate the party list if there's a change in the number of party members. 
                //ie, when partylist.length() before and partylist.length() after aren't equal. 
                //That will either be handled here, or in the class itself. 

                _pmembers = new GetPartyMembers(_pluginInterface.ClientState.LocalPlayer, _pluginInterface);
            }

            ImGui.End();
            ImGui.PopStyleVar();
        }
        private void ConfigWindow(object sender, EventArgs args)
        {
            _config = true;
        }

        private void Command(string command, string arguments)
        {
            _config = true;
        }

        private void SaveConfig()
        {
            _configuration.Enabled = _enabled;
            _configuration.Combat = _combat;
            _configuration.Circle = _circle;
            _configuration.Instance = _instance;
            _configuration.Col = _col;
            _configuration.Col2 = _col2;
            _configuration.ColRing = _colRing;
            _configuration.ColRing2 = _colRing2;
            _configuration.Thickness = _thickness;
            _configuration.Segments = _segments;
            _configuration.Segments2 = _segments2;
            _configuration.Ring = _ring;
            _configuration.Ring2 = _ring2;
            _configuration.Radius = _radius;
            _configuration.Radius2 = _radius2;
            _configuration.ClassRings = _classrings;
            _configuration.PetRingsColor = _petRingColor;
            _configuration.PetRings = _petrings;
            _configuration.PetThickness = _petThickness;
            _configuration.PetOpacity = _petOpacity;
            _configuration.ClassOpacity = _classOpacity;
            _configuration.ClassThickness = _classThickness;
            _configuration.ClassSegments = _classSegments;
            _pluginInterface.SavePluginConfig(_configuration);
        }

        private void DrawRingWorld(Dalamud.Game.ClientState.Actors.Types.Actor actor, float radius, int numSegments, float thicc, uint colour)
        {
            var seg = numSegments / 2;
            for (var i = 0; i <= numSegments; i++)
            {
                _pluginInterface.Framework.Gui.WorldToScreen(new SharpDX.Vector3(actor.Position.X + ((radius) * (float)Math.Sin((Math.PI / seg) * i)), actor.Position.Z, actor.Position.Y + (radius * (float)Math.Cos((Math.PI / seg) * i))), out SharpDX.Vector2 pos);
                ImGui.GetWindowDrawList().PathLineTo(new Num.Vector2(pos.X, pos.Y));
            }
            ImGui.GetWindowDrawList().PathStroke(colour, ImDrawFlags.Closed, thicc);
        }

        private void DrawClassRings(Actor actor, UInt32 job)
        {
            //The following (sloppy) implementation is not final.
            //  Expect awkward bugs.


            //Set up opacity, clamp function won't work. 
            if(_classOpacity > 100)
            {
                _classOpacity = 100;
            }

            if( _classOpacity < 0)
            {
                _classOpacity = 0;
            }

            //Because I don't like negative numbers in the UI
            if(_thickness < 0)
            {
                _thickness = 0;
            }

            int[] radii = Wrapper.GetRadii((int)job);

            Num.Vector4[] colors = new Num.Vector4[4];

            colors = Wrapper.GetColors((int)job);

            for (int i = 0; i < 3; i++)
            {
                if (radii[i] != -1 && radii[i] != 0)
                {
                    {
                        //Apply our opacity modifier.
                        colors[i].W *= (_classOpacity/100);
                        DrawRingWorld(actor, radii[i], _classSegments, _classThickness, ImGui.GetColorU32(colors[i]));
                    }
                }
            }
        }

        private void DrawPetRings(PlayerCharacter character, DalamudPluginInterface p_interface)
        {
            //This logic is contained in the PetFinder class. 
            if (_pluginInterface.ClientState.LocalPlayer.ClassJob.Id < 26 || _pluginInterface.ClientState.LocalPlayer.ClassJob.Id > 28) { return; }

            Actor petactor = _petfinder.GetPlayerPet(character, p_interface);
            if (petactor == null)
            {
                return;
            }
            //Set up opacity, clamp function won't work. 
            if (_petOpacity > 100)
            {
                _petOpacity = 100;
            }

            if (_petOpacity < 0)
            {
                _petOpacity = 0;
            }

            _petRingColor.W = (_petOpacity / 100);
           
            DrawRingWorld(petactor, 15, _petSegments, _petThickness, ImGui.GetColorU32(_petRingColor));

            //SCH Fey Blessing is 20y, needs additional ring. 
            if (character.ClassJob.Id == 28)
            {
                DrawRingWorld(petactor, 20, _petSegments, _petThickness, ImGui.GetColorU32(_petRingColor));
            }
        }
    }



    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public bool Combat { get; set; } = true;
        public bool Circle { get; set; }
        public bool Circle2 { get; set; }
        public bool Instance { get; set; }
        public Num.Vector4 Col { get; set; } = new Num.Vector4(1f, 1f, 1f, 1f);
        public Num.Vector4 Col2 { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 1f);
        public Num.Vector4 ColRing { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        public Num.Vector4 ColRing2 { get; set; } = new Num.Vector4(0.4f, 0.4f, 0.4f, 0.5f);
        public Num.Vector4 PetRingsColor { get; set; } = new Num.Vector4(242f / 255f, 206f / 255f, 44f / 255f, 1f);
        public int Segments { get; set; } = 100;
        public int Segments2 { get; set; } = 100;
        public int PetSegments { get; set; } = 100;
        public float Thickness { get; set; } = 10f;
        public float Thickness2 { get; set; } = 10f;
        public float PetThickness { get; set; } = 10f;
        public bool Ring { get; set; }
        public bool Ring2 { get; set; }
        public float Radius { get; set; } = 2f;
        public float Radius2 { get; set; } = 4f;
        public bool ClassRings { get; set; }
        public bool PetRings { get; set; }
        public float ClassOpacity { get; set; }
        public float ClassThickness { get; set; }
        public float PetOpacity { get; set; } = 1f;
        public int ClassSegments { get; set; }
    }

}
