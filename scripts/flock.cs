using Godot;
using System.Collections.Generic;
public partial class Flock : Node3D{
	List<Boid> flock = new List<Boid>();
	int num_boids = 400;
	Vector2 world_size = new Vector2(50,50);
	QuadTree quad_tree;
	Rectangle tree_boundary;
	
	public override void _Ready(){
		tree_boundary = new Rectangle((int)(-world_size.X * 500), (int)(-world_size.Y * 500),(int)(world_size.X * 500), (int)(world_size.Y * 500));
		for (int i = 0; i < num_boids; i++)
		{
			Boid boid = new Boid();
			AddChild(boid);
			boid.Position = new Vector3((float)GD.RandRange(-world_size.X,world_size.X),0,(float)GD.RandRange(-world_size.Y,world_size.Y));
		 	flock.Add(boid);
		}
	}

	public override void _Process(double _delta){
		quad_tree = new QuadTree(tree_boundary,4);
		foreach(Boid boid in flock){
			var pos = boid.Position;
			var point = new Point((int)(pos.X * 1000), (int)(pos.Z * 1000));
			quad_tree.Insert(point);
		}
		foreach(Boid boid in flock){
			boid.Flock(quad_tree);
		}
	}
}


// func _process(_delta):
// 	# Recreate quadtree each frame
// 	quad_tree = QuadTree.new(tree_boundary, 4)  # capacity of 4
//
// 	# Insert all boid positions
// 	for boid in flock:
// 		var pos = boid.position
// 		var point = Point.new(int(pos.x * 1000), int(pos.z * 1000))  # z for 3D position
// 		quad_tree.Insert(point)
//
// 	# Update boids with quadtree
// 	for boid in flock:
// 		boid.flock(flock, quad_tree)  # Pass quadtree to boid
