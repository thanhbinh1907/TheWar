using UnityEngine;

namespace TowerDefense.Gameplay.Combat
{
    /// <summary>
    /// Kéo dãn dây cung bằng cách ép xương dây cung đi theo tay kéo.
    /// Gắn script này vào cùng GameObject chứa Animator để nhận Animation Events.
    /// </summary>
    public class BowStringController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Xương dây cung (String Bone) nằm trong cây cung")]
        [SerializeField] private Transform _stringBone;
        
        [Tooltip("Xương bàn tay kéo cung (Ví dụ: Hand_R hoặc Hand_L)")]
        [SerializeField] private Transform _pullHand;
        
        [Header("Settings")]
        [Tooltip("Tốc độ đàn hồi dây cung về vị trí cũ khi buông tay")]
        [SerializeField] private float _snapBackSpeed = 30f;
        
        [Tooltip("Mô hình mũi tên giả (Gắn vào tay kéo hoặc dây cung)")]
        [SerializeField] private GameObject _dummyArrow;
        
        private Vector3 _restLocalPos;
        private bool _isPulled;

        private void Awake()
        {
            if (_stringBone != null)
            {
                _restLocalPos = _stringBone.localPosition;
            }
        }

        // Gọi từ Animation Event (ví dụ: ở đầu/giữa clip Load)
        public void GrabString()
        {
            _isPulled = true;
        }

        // Gọi từ Animation Event (ví dụ: cùng lúc với frame Hit trong clip Release)
        public void ReleaseString()
        {
            _isPulled = false;
        }

        // Gọi từ Animation Event khi tay thò ra sau lấy mũi tên (clip Reload)
        public void ShowArrow()
        {
            if (_dummyArrow != null) _dummyArrow.SetActive(true);
        }

        // Gọi từ Animation Event cùng lúc với lúc bắn mũi tên bay ra (clip Release)
        public void HideArrow()
        {
            if (_dummyArrow != null) _dummyArrow.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_stringBone == null || _pullHand == null) return;

            if (_isPulled)
            {
                // Ép dây cung bám theo điểm kéo (world space)
                _stringBone.position = _pullHand.position;
            }
            else
            {
                // Khi buông, đàn hồi mượt mà về vị trí ban đầu (local space)
                _stringBone.localPosition = Vector3.Lerp(_stringBone.localPosition, _restLocalPos, Time.deltaTime * _snapBackSpeed);
            }
        }
    }
}
