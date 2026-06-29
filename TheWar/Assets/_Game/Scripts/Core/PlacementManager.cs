using UnityEngine;

namespace TowerDefense.Core
{
	public class PlacementManager : MonoBehaviour
	{
		public static PlacementManager Instance { get; private set; }

		[Header("Current Selection")]
		public UnitData selectedUnit;

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
			selectedUnit = unitData;
			Debug.Log($"[Placement] Đang giữ thẻ: {unitData.unitName} ({unitData.unitClass}) | Sẵn sàng đặt tháp.");
		}

		// Xóa trạng thái đang cầm thẻ sau khi đã đặt tháp xong
		public void ClearSelection()
		{
			selectedUnit = null;
		}
	}
}