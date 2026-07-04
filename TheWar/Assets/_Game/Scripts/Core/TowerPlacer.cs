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
			if (PlacementManager.Instance == null || PlacementManager.Instance.SelectedUnit == null) return;

			Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
			if (!Physics.Raycast(ray, out RaycastHit hit)) return;

			// Nếu click trúng vào một bệ tháp đã có sẵn, để TowerSocket tự xử lý, không spawn tháp mới
			if (hit.collider.GetComponent<TowerSocket>() != null) return;

			// Lấy tọa độ từ vị trí click chuột
			Vector3 spawnPos = hit.point;

			// Tạo Tower tại vị trí click
			GameObject tower = Instantiate(_towerSocketPrefab, spawnPos, Quaternion.identity);

			// Cắm unit vào tower ngay lập tức
			tower.GetComponent<TowerSocket>().PlugUnit(PlacementManager.Instance.SelectedUnit);
		}
	}
}