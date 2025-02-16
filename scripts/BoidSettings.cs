using Godot;
[GlobalClass]
public partial class BoidSettings : Resource
{
    [Export]public float Searchradius;
    [Export]public float Alignmentweight;
    [Export]public float Cohesionweight;
    [Export]public float Separationweight;
    [Export]public float Separationradius;
    [Export]public float Fieldofview;
	[Export]public float Maxspeed;
	[Export]public float Maxforce;
}
