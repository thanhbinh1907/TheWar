using UnityEngine;

namespace TowerDefense.Core
{
	public class GridManager : MonoBehaviour
	{
		public static GridManager Instance { get; private set; }

		[Header("Grid Config")]
		[SerializeField] private int _width = 20;
		[SerializeField] private int _height = 10;
		[SerializeField] private float _cellSize = 1f;
		[SerializeField] private Vector3 _originPosition;

		private GridNode[,] _grid;

		private void Awake()
		{
			Instance = this;
			CreateGrid();
		}

		private void CreateGrid()
		{
			_grid = new GridNode[_width, _height];
			for (int x = 0; x < _width; x++)
				for (int y = 0; y < _height; y++)
					_grid[x, y] = new GridNode(x, y, walkable: true);
		}

		public GridNode GetNode(int x, int y)
		{
			if (x < 0 || y < 0 || x >= _width || y >= _height) return null;
			return _grid[x, y];
		}

		public Vector3 GetWorldPosition(int x, int y)
		{
			return new Vector3(x, 0, y) * _cellSize + _originPosition;
		}

		public bool GetXY(Vector3 worldPos, out int x, out int y)
		{
			x = Mathf.FloorToInt((worldPos - _originPosition).x / _cellSize);
			y = Mathf.FloorToInt((worldPos - _originPosition).z / _cellSize);
			return x >= 0 && y >= 0 && x < _width && y < _height;
		}

		public void SetWalkable(int x, int y, bool walkable)
		{
			if (_grid[x, y] != null)
				_grid[x, y].Walkable = walkable;
		}

		// Hiện grid trong Editor để debug
		private void OnDrawGizmos()
		{
			for (int x = 0; x < _width; x++)
			{
				for (int y = 0; y < _height; y++)
				{
					bool isWalkable = true;
					if (_grid != null && _grid[x, y] != null)
					{
						isWalkable = _grid[x, y].Walkable;
					}

					Gizmos.color = isWalkable ? Color.white : Color.red;
					Gizmos.DrawWireCube(
						GetWorldPosition(x, y) + new Vector3(_cellSize, 0, _cellSize) * 0.5f,
						new Vector3(_cellSize, 0.1f, _cellSize) * 0.95f
					);
				}
			}
		}
	}
}