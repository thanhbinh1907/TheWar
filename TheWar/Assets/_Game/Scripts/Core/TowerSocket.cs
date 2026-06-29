using UnityEngine;

namespace TowerDefense.Core
{
	public class TowerSocket : MonoBehaviour
	{
		public bool isOccupied = false;
		public UnitData currentUnitData;
		private MeshRenderer meshRenderer;

		private void Awake() => meshRenderer = GetComponent<MeshRenderer>();

		// Hàm này tự kích hoạt khi người chơi click CHUỘT THẲNG VÀO KHỐI CUBE THÁP này
		private void OnMouseDown()
		{
			// 1. Kiểm tra xem người chơi có đang cầm thẻ quân cờ nào trên tay không
			if (PlacementManager.Instance == null || PlacementManager.Instance.selectedUnit == null)
			{
				Debug.Log("Bạn chưa chọn thẻ quân cờ nào trên tay để cắm vào tháp này!");
				return;
			}

			// 2. Kiểm tra xem tháp này đã bị cắm quân khác chưa
			if (isOccupied)
			{
				Debug.Log("Tháp này đã có Lõi nhân vật rồi, không cắm đè được!");
				return;
			}

			// 3. Nếu thỏa mãn hết, tiến hành hấp thụ quân cờ
			PlugUnit(PlacementManager.Instance.selectedUnit);
		}

		public void PlugUnit(UnitData data)
		{
			currentUnitData = data;
			isOccupied = true;

			// Đổi màu tháp bằng MaterialPropertyBlock để giữ hiệu năng GPU Instancing
			if (meshRenderer != null)
			{
				var block = new MaterialPropertyBlock();
				block.SetColor("_BaseColor", data.unitColor);
				meshRenderer.SetPropertyBlock(block);
			}

			Debug.Log($"[Socket] Đã cắm thành công Lõi: {data.unitName} vào chiếc tháp này!");

			// Cắm xong thì xóa thẻ đang cầm trên tay đi
			PlacementManager.Instance.ClearSelection();
		}
	}
}