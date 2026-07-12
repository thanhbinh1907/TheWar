using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Tower;
using TowerDefense.Gameplay.Tower;

namespace TowerDefense.Core
{
	public class TowerSocket : MonoBehaviour
	{
		// Static list tracking all currently occupied/active towers
		public static List<TowerSocket> ActiveTowers { get; } = new List<TowerSocket>();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStatics()
		{
			ActiveTowers.Clear();
		}

		[SerializeField] private Transform _unitSpawnPoint;

		public bool IsOccupied { get; private set; }
		public UnitData CurrentUnitData { get; private set; }
		private MeshRenderer _meshRenderer;
		private TowerHealth _health;
		private GameObject _spawnedUnitInstance;

		private void Awake()
		{
			_meshRenderer = GetComponent<MeshRenderer>();
			_health = GetComponent<TowerHealth>();
		}

		private void Start()
		{
			// Bệ tháp trống thì không nhận sát thương
			if (_health != null)
			{
				_health.enabled = false;
			}
		}

		private void OnDisable()
		{
			ActiveTowers.Remove(this);
			if (_health != null)
			{
				_health.OnTowerDied -= HandleTowerDied;
			}
		}

		private void OnDestroy()
		{
			ActiveTowers.Remove(this);
			if (_health != null)
			{
				_health.OnTowerDied -= HandleTowerDied;
			}
		}

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

			// 2.5 Kiểm tra xem loại thẻ này có cắm được vào tháp không
			if (PlacementManager.Instance.SelectedUnit.DeployMode == DeployMode.FreeDeploy)
			{
#if UNITY_EDITOR
				Debug.Log("Thẻ này dùng để thả tự do, không cắm vào tháp được!");
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

			if (!ActiveTowers.Contains(this))
			{
				ActiveTowers.Add(this);
			}

			// Kích hoạt máu của tháp khi đã cắm Lõi nhân vật
			if (_health != null)
			{
				_health.enabled = true;
				_health.Initialize();
				_health.OnTowerDied += HandleTowerDied;
			}

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

			if (data.DeployMode == DeployMode.SocketRanged || data.DeployMode == DeployMode.SocketSpawner)
			{
				if (data.DeployPrefab != null && _unitSpawnPoint != null)
				{
					// Tạo lính nhưng KHÔNG làm con của tháp để tránh bị giãn (Stretch) do Tháp bị Scale.
					// Đặt lính làm con của transform.parent (ví dụ: thư mục Towers chứa các tháp)
					_spawnedUnitInstance = Instantiate(data.DeployPrefab, _unitSpawnPoint.position, _unitSpawnPoint.rotation);
					_spawnedUnitInstance.transform.SetParent(transform.parent, true);

					var towerUnit = _spawnedUnitInstance.GetComponent<TowerUnit>();
					
					if (towerUnit != null)
					{
						towerUnit.Initialize(data);
					}
					else
					{
						Debug.LogWarning($"[TowerSocket] Prefab {data.DeployPrefab.name} is missing TowerUnit component!");
					}
				}
			}

			// Cắm xong thì xóa thẻ đang cầm trên tay đi
			PlacementManager.Instance.ClearSelection();
		}

		public void UnplugUnit()
		{
			if (CurrentUnitData != null)
			{
				if (_spawnedUnitInstance != null)
				{
					Destroy(_spawnedUnitInstance);
					_spawnedUnitInstance = null;
				}
			}

			IsOccupied = false;
			CurrentUnitData = null;
			ActiveTowers.Remove(this);

			if (_health != null)
			{
				_health.OnTowerDied -= HandleTowerDied;
				_health.enabled = false;
			}

			if (_meshRenderer != null)
			{
				var block = new MaterialPropertyBlock();
				block.SetColor("_BaseColor", Color.white);
				_meshRenderer.SetPropertyBlock(block);
			}
		}

		private void HandleTowerDied()
		{
			UnplugUnit();
		}
	}
}