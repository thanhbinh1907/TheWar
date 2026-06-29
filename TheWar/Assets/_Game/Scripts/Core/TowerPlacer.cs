using UnityEngine;

namespace TowerDefense.Core
{
	public class TowerPlacer : MonoBehaviour
	{
		[SerializeField] private GameObject _towerSocketPrefab; // Kéo prefab khối Cube Tower vào đây
		[SerializeField] private Camera _camera;

		private void Update()
		{
			if (!Input.GetMouseButtonDown(0)) return;
			if (PlacementManager.Instance == null || PlacementManager.Instance.selectedUnit == null) return;

			Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
			if (!Physics.Raycast(ray, out RaycastHit hit)) return;

			// Lấy tọa độ grid từ vị trí click chuột
			if (!GridManager.Instance.GetXY(hit.point, out int x, out int y)) return;

			GridNode node = GridManager.Instance.GetNode(x, y);

			// Kiểm tra ô hợp lệ (Phải nằm trong lưới, đi bộ được, và chưa có tháp)
			if (node == null || !node.Walkable || node.HasTower)
			{
				Debug.Log($"Vị trí [{x},{y}] không hợp lệ để đặt tháp!");
				return;
			}

			// Tính toán vị trí Spawn đè khít vào tâm ô grid (Cộng thêm offset nửa ô)
			Vector3 spawnPos = GridManager.Instance.GetWorldPosition(x, y);

			// Tạo Tower tại đúng tâm ô grid
			GameObject tower = Instantiate(_towerSocketPrefab, spawnPos, Quaternion.identity);

			// Cắm unit vào tower ngay lập tức
			tower.GetComponent<TowerSocket>().PlugUnit(PlacementManager.Instance.selectedUnit);

			// Đánh dấu ô đã có tháp — khóa không cho đặt đè
			node.HasTower = true;
			GridManager.Instance.SetWalkable(x, y, false);
		}
	}
}