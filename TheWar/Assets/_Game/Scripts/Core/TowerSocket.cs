using UnityEngine;

namespace TowerDefense.Core
{
	public class TowerSocket : MonoBehaviour
	{
		public bool IsOccupied { get; private set; }
		public UnitData CurrentUnitData { get; private set; }
		private MeshRenderer _meshRenderer;

		private void Awake() => _meshRenderer = GetComponent<MeshRenderer>();

		// Hàm này tự kích hoạt khi người chơi click CHUỘT THẲNG VÀO KHỐI CUBE THÁP này
		private void OnMouseDown()
		{
			// 1. Kiểm tra xem người chơi có đang cầm thẻ quân cờ nào trên tay không
			if (PlacementManager.Instance == null || PlacementManager.Instance.SelectedUnit == null)
			{
#if UNITY_EDITOR
				Debug.Log("Bạn chưa chọn thẻ quân cờ nào trên tay để cắm vào tháp này!");
#endif
				return;
			}

			// 2. Kiểm tra xem tháp này đã bị cắm quân khác chưa
			if (IsOccupied)
			{
#if UNITY_EDITOR
				Debug.Log("Tháp này đã có Lõi nhân vật rồi, không cắm đè được!");
#endif
				return;
			}

			// 3. Nếu thỏa mãn hết, tiến hành hấp thụ quân cờ
			PlugUnit(PlacementManager.Instance.SelectedUnit);
		}

		public void PlugUnit(UnitData data)
		{
			CurrentUnitData = data;
			IsOccupied = true;

			// Đổi màu tháp bằng MaterialPropertyBlock để giữ hiệu năng GPU Instancing
			if (_meshRenderer != null)
			{
				var block = new MaterialPropertyBlock();
				block.SetColor("_BaseColor", data.UnitColor);
				_meshRenderer.SetPropertyBlock(block);
			}

#if UNITY_EDITOR
			Debug.Log($"[Socket] Đã cắm thành công Lõi: {data.UnitName} vào chiếc tháp này!");
#endif

			// Cắm xong thì xóa thẻ đang cầm trên tay đi
			PlacementManager.Instance.ClearSelection();
		}
	}
}