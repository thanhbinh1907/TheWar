
# Git Commit & Workflow Guidelines for Antigravity AI

Tài liệu này quy định các nguyên tắc bắt buộc khi thực hiện Commit và quản lý Source Control trên GitHub cho dự án Tower Defense nhằm đảm bảo tính sạch sẽ, dễ theo vết và đồng bộ với kiến trúc hệ thống hiện tại.

---

## I. QUY TẮC ĐẶT TÊN BẢN TRÌNH BÀY (COMMIT MESSAGE FORMAT)

Dự án áp dụng chuẩn **Conventional Commits**. Mỗi dòng thông điệp commit (Commit Message) phải có cấu trúc rạch ròi như sau:

`<type>(<scope>): <short description>`

### 1. Các loại Type bắt buộc dùng:

- `feat`: Khi thêm một tính năng hoặc component mới (Ví dụ: Thêm EnemyMovement, tạo EnemyPool).
- `fix`: Khi sửa lỗi code, lỗi logic hệ thống (Ví dụ: Sửa lỗi NullReference, lỗi lệch ô Grid).
- `refactor`: Sửa đổi cấu trúc code nội bộ nhưng không làm thay đổi hành vi bên ngoài (Ví dụ: Chuyển đổi sang MaterialPropertyBlock để tối ưu GPU Instancing).
- `docs`: Cập nhật tài liệu, file .md (Ví dụ: Cập nhật ANTIGRAVITY_CONTEXT.md).
- `chore`: Cập nhật cấu hình build, dọn dẹp thư mục, meta file, hoặc import asset mà không dính tới code logic.

### 2. Quy định về Scope (Phạm vi ảnh hưởng):

Scope phải tương ứng với cấu trúc thư mục quy hoạch trong `_Game/Scripts/` của `ANTIGRAVITY_CONTEXT.md`:

- `core`: GridManager, GridNode, PlacementManager, TowerSocket, UnitData.
- `enemy`: EnemyHealth, EnemyMovement, EnemyPool.
- `tower`: Các logic liên quan tới tháp canh về sau.
- `ui`: Giao diện bài, máu, vàng.
- `shared`: Các interface dùng chung như IDamageable.

### 3. Quy định về Mô tả (Description):

- Viết bằng **tiếng Anh**, bắt đầu bằng chữ thường.
- Không kết thúc bằng dấu chấm.
- Ngắn gọn dưới 50 ký tự và mô tả đúng bản chất hành động.

**Ví dụ Commit chuẩn:**

- `feat(enemy): add EnemyHealth component with OnDied event`
- `refactor(core): optimize TowerSocket color swap using MaterialPropertyBlock`
- `fix(core): resolve NullReferenceException on empty TowerPlacer prefab slot`
- `docs(git): create GIT_GUIDELINES.md file`

---

## II. QUY TẮC PHÂN CHIA VÀ GOM CỤM COMMIT (ATOMIC COMMITS)

1. **Commit nguyên tử (Atomic Commits):** Mỗi commit chỉ nên giải quyết **đúng 1 phần việc duy nhất**.

   - ❌ *KHÔNG NÊN:* Viết xong toàn bộ hệ thống Enemy (gồm Data, Máu, Di chuyển, Spawner) rồi mới commit một cục lớn với nội dung `feat: làm xong enemy`.
   - *NÊN:* Tách nhỏ ra:
     - Commit 1: `feat(shared): implement IDamageable interface`
     - Commit 2: `feat(enemy): add EnemyDataSO scriptable object`
     - Commit 3: `feat(enemy): implement EnemyHealth component`
2. **Kiểm tra trước khi commit (Compile Check):** Tuyệt đối KHÔNG commit đoạn code đang bị lỗi đỏ (Compile Error) trong Unity lên nhánh chính. Code tại mỗi commit phải ở trạng thái biên dịch thành công.

---

## III. QUY TẮC QUẢN LÝ TÀI NGUYÊN TRONG UNITY (.GITIGNORE)

Vì dự án sử dụng các bộ Asset nặng của Synty Studios nằm trong thư mục `Plugins/`[cite: 1] và các file tạm sinh ra tự động bởi Unity (Library, Temp, Logs), file `.gitignore` của dự án phải được thiết lập chuẩn cho Unity để tránh đẩy các file rác lên GitHub làm nặng repo.

### Các tài nguyên TUYỆT ĐỐI KHÔNG commit:

- Thư mục `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/`.
- Các file có đuôi `.csproj`, `.sln` (File giải pháp của Visual Studio/VS Code tự sinh tự động theo máy cá nhân).
- Bản build chạy thử `.apk`, `.exe`, `.html`.

---

## IV. QUY TRÌNH PHÁT TRIỂN (WORKFLOW RUN)

Khi bắt đầu một Milestone mới (Ví dụ: làm sang Hệ thống A* hay Tower Bắn):

1. **Tạo nhánh mới (Branching):** Không code trực tiếp trên nhánh `main`. Tạo một nhánh phụ từ `main` với tên theo format: `feature/milestone-X-tên-tính-năng` (Ví dụ: `feature/milestone-2-enemy-movement`).
2. **Code & Commit liên tục:** Thực hiện viết code, dọn dẹp và commit theo quy tắc "Atomic Commits" nêu ở Mục II trên nhánh phụ đó.
3. **Review & Merge:** Khi hoàn thành xong hoàn toàn Milestone đó và test chạy mượt mà, tiến hành tạo **Pull Request (PR)** để gộp nhánh phụ đó quay trở về nhánh `main`.

---

## V. ĐIỀU KHOẢN TUÂN THỦ DÀNH CHO AI (ANTIGRAVITY EXTRA RULES)

- Trước khi thực hiện viết code hay gợi ý commit, Antigravity AI phải tự đối chiếu xem code cấu trúc có vi phạm quy tắc file limit (Method < 50 dòng, Class < 200 dòng) quy định tại `ANTIGRAVITY_CONTEXT.md` hay không.
- Khi viết mô tả Commit, bắt buộc phải dùng lệnh `MaterialPropertyBlock` thay cho `material.color` nếu có sửa đổi liên quan đến visual để không phá GPU Instancing của Asset Synty.

