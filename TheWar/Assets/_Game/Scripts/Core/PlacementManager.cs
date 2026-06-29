using UnityEngine;

namespace TowerDefense.Core
{
	public class PlacementManager : MonoBehaviour
	{
		public static PlacementManager Instance { get; private set; }

		// Encapsulated property
		public UnitData SelectedUnit { get; private set; }

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
				// Nếu bạn chuyển cảnh (Change Scene), giữ bộ não này không bị xóa
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}

		// Hàm này giả lập việc bạn bấm vào một Thẻ bài nhân vật (PvZ Style)
		public void SelectUnitCard(UnitData unitData)
		{
			SelectedUnit = unitData;
#if UNITY_EDITOR
			Debug.Log($"[Placement] Đang giữ thẻ: {unitData.UnitName} ({unitData.UnitClass}) | Sẵn sàng đặt tháp.");
#endif
		}

		// Xóa trạng thái đang cầm thẻ sau khi đã đặt tháp xong
		public void ClearSelection()
		{
			SelectedUnit = null;
		}
	}
}