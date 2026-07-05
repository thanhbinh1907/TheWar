using UnityEngine;

namespace TowerDefense.Core
{
	public class FreeDeployPlacer : MonoBehaviour
	{
		[SerializeField] private Camera _camera;

		private void Update()
		{
			if (!Input.GetMouseButtonDown(0)) return;
			
			var selectedUnit = PlacementManager.Instance?.SelectedUnit;
			if (selectedUnit == null || selectedUnit.DeployMode != DeployMode.FreeDeploy) return;

			Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
			
			// Chỉ cho phép thả lính FreeDeploy xuống mặt đường (giả sử đường có layer Road)
			// Nếu dự án của bạn dùng layer khác, bạn có thể truyền thêm LayerMask vào đây
			if (!Physics.Raycast(ray, out RaycastHit hit)) return;

			if (hit.collider.GetComponent<TowerSocket>() != null)
			{
#if UNITY_EDITOR
				Debug.Log("Không thể thả lính FreeDeploy đè lên tháp!");
#endif
				return;
			}

			// Spawn unit tại điểm chạm
			if (selectedUnit.DeployPrefab != null)
			{
				Instantiate(selectedUnit.DeployPrefab, hit.point, Quaternion.identity);
#if UNITY_EDITOR
				Debug.Log($"[FreeDeploy] Đã thả {selectedUnit.UnitName} xuống đường!");
#endif
				PlacementManager.Instance.ClearSelection();
			}
			else
			{
				Debug.LogError($"[FreeDeploy] {selectedUnit.UnitName} không có DeployPrefab!");
			}
		}
	}
}