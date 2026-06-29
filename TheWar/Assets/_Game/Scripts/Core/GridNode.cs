namespace TowerDefense.Core
{
	public class GridNode
	{
		public int X { get; }
		public int Y { get; }
		public bool Walkable { get; set; }
		public bool HasTower { get; set; } 
		public float GCost { get; set; }
		public float HCost { get; set; }
		public float FCost => GCost + HCost;
		public GridNode Parent { get; set; }

		public GridNode(int x, int y, bool walkable)
		{
			X = x;
			Y = y;
			Walkable = walkable;
			HasTower = false; 
		}
	}
}