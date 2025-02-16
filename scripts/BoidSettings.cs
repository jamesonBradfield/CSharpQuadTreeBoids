using Godot;
public partial class BoidSettings : Resource
{
    private float searchradius;
    private float alignmentweight;
    private float cohesionweight;
    private float separationweight;
    private float separationradius;
    private float fieldofview;
	private float maxspeed;
	private float maxforce;
    public BoidSettings(
        float searchradius, float alignmentweight, float cohesionweight, float separationweight, float separationradius, float fieldofview, float maxspeed, float maxforce)
    {
		this.searchradius = searchradius;
		this.alignmentweight = alignmentweight;
		this.cohesionweight = cohesionweight;
		this.separationweight = separationweight;
		this.separationradius = separationradius;
		this.fieldofview = fieldofview;
		this.maxspeed = maxspeed;
		this.maxforce = maxforce;
    }

    [Export]public float Searchradius { get => searchradius; set => searchradius = value; }
    [Export]public float Alignmentweight { get => alignmentweight; set => alignmentweight = value; }
    [Export]public float Cohesionweight { get => cohesionweight; set => cohesionweight = value; }
    [Export]public float Separationweight { get => separationweight; set => separationweight = value; }
    [Export]public float Separationradius { get => separationradius; set => separationradius = value; }
    [Export]public float Fieldofview { get => fieldofview; set => fieldofview = value; }
    [Export]public float Maxspeed { get => maxspeed; set => maxspeed = value; }
    [Export]public float Maxforce { get => maxforce; set => maxforce = value; }
}
