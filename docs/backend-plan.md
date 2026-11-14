# 後端開發規劃

- 資料表設計：[schema.dbml](schema.dbml)
- 每個階段完成後都要實際測試，確認沒問題再進入下一階段
- 標示【新】的項目不在筆記範圍內，需要另外查資料

## 階段 0：建立專案
對應筆記：Entity Framework

### 已決定
- [x] 專案位置：建在 `pokemon_BE/api/`，專案與 namespace 名稱都是 `api`
- [x] 資料庫：SQL Server（LocalDB），使用本專案專用的新資料庫 `pokemon`

### 工作項目
- [x] 【新】在 `api/` 建立 Web API 專案，並加上 `--use-controllers`（.NET 10 的範本預設不使用 Controller）與 `--framework net10.0`
- [x] 【新】在 `pokemon_BE` 根目錄加入 `.gitignore`（`dotnet new gitignore`），避免 `api/bin/`、`api/obj/` 進入版控
- [x] 安裝 EF Core 套件：SqlServer、Design，版本 `10.0.14`
- [x] 安裝 `System.Security.Cryptography.Pkcs`，版本 `10.0.14`（蓋過 EF Core 帶進的弱點版本）
- [x] 確認 `dotnet ef` 指令可以正常執行（10.0.8）
- [x] 設定連線字串，指向 `pokemon` 資料庫
- [x] 建立 `ApplicationDBContext`，並在 `Program.cs` 註冊
- [x] 範本內建的 `Microsoft.AspNetCore.OpenApi` 升到 `10.0.14`
- [x] 安裝 Scalar，加入 API 測試頁（範本已內建 OpenAPI）
- [x] 移除範本內建的 WeatherForecast 範例：`Controllers/WeatherForecastController.cs`、`WeatherForecast.cs`，以及 `api.http` 裡的範例請求

### 完成標準
建置時沒有套件弱點警告，專案可以正常啟動，並開啟 Scalar 測試頁。

## 階段 1：登入
對應筆記：Identity Setup、Register、Token Service、Login

先實作登入：`Teams.AppUserId` 是必填欄位，沒有使用者就無法建立隊伍。

### 已決定
- [x] 登入方式：使用帳號；註冊時另外收 email，email 不能重複
- [x] 帳號不支援中文，維持 Identity 預設（英文字母、數字與 `-._@+`）
- [x] token 有效期限 1 小時，搭配 refresh token
- [x] 不需要角色，也不做後台
- [x] token 的 claims：使用者 Id 與帳號
- [x] 前端存放：token 放在 Pinia（記憶體）；refresh token 放在 httpOnly cookie（JavaScript 讀不到）

### 工作項目
- [x] 安裝套件：Identity.EntityFrameworkCore、Authentication.JwtBearer，版本 `10.0.14`
- [x] `AppUser` 繼承 `IdentityUser`
- [x] `ApplicationDBContext` 改為繼承 `IdentityDbContext<AppUser>`
- [x] `Program.cs`：註冊 Identity 並設定密碼規則與 email 不可重複（`RequireUniqueEmail`）、改用 JWT 驗證、將 `UseAuthentication` 放在 `UseAuthorization` 之前
- [x] 【新】JWT 簽章金鑰存放在 user-secrets，不寫入 `appsettings.json`。repo 會推送到 GitHub，金鑰一旦外洩，任何人都能偽造 token
- [x] `TokenService`：產生 token
- [x] 註冊 API：`RegisterDto` 驗證、以 `CreateAsync` 建立使用者、回傳錯誤訊息
- [x] 登入 API：查詢使用者、驗證密碼（`lockoutOnFailure: true`，連續錯 5 次鎖定 5 分鐘）、回傳 token
- [x] 從 token 取得目前使用者的 extension method
- [x] `GET /api/account/me`：回傳目前登入者的 Id 與帳號
- [x] 【新】Model：`RefreshToken`；`TokenHash` 不可重複；刪除使用者時連帶刪除
- [x] 【新】登入時一併發出 refresh token：隨機字串（用 `RandomNumberGenerator` 產生，不能用 `Random`），資料庫只存雜湊，7 天後失效，以 cookie 回傳（`HttpOnly`、`Secure`、`SameSite=Strict`、`Path=/api/account`）
- [x] 【新】`POST /api/account/refresh`：從 cookie 讀取 refresh token 並驗證，撤銷舊的並發出新的一組；不要求有效的 token
- [x] 【新】`POST /api/account/logout`：撤銷 refresh token 並清除 cookie；不要求有效的 token
- [x] 【新】拿已撤銷的 refresh token 換新時，視為遭盜用，撤銷該使用者所有的 refresh token；撤銷未滿 30 秒的除外，改回傳 409 讓前端重試
- [x] 【新】換發 refresh token 時，刪除該使用者已過期的舊紀錄（已撤銷但未過期的要保留）
- [x] 執行 migration，建立 Identity 與 `RefreshTokens` 的資料表

### 完成標準
可以註冊並登入取得 token；帶 token 呼叫 `GET /api/account/me` 能取得自己的 Id 與帳號，未帶 token 時請求會被拒絕；用 refresh token 能換到新的 token，登出後原本的 refresh token 無法再使用。

## 階段 2：隊伍與成員
對應筆記：DTOs、POST、PUT、DELETE、Async/Await、Repository Pattern、Include()、1-To-Many CREATE、Data Validation、User Generated Content

### 已決定
- [x] 寶可夢資料由前端向 PokeAPI 取得，後端只儲存 `PokemonId`

### 待決定
- [ ] 存檔方式：整份覆蓋，或每個動作各一支 API（整份覆蓋能一併解決交換位置時格數重複、刪除成員後重新編號的問題）
- [ ] `PokemonId` 允許範圍：前端目前只能搜尋 1–1025，後端是否採用相同範圍
- [ ] 是否記錄建立時間與更新時間（例如讓「我的牌組」依最近編輯排序）

### 工作項目
- [ ] Model：`Team`、`Member`（含 navigation property）
- [ ] `DbSet<Team>`、`DbSet<Member>`
- [ ] 【新】在 `OnModelCreating` 設定資料表規則
  - [ ] `Teams`：`(AppUserId, Name)` 不可重複
  - [ ] `Members`：`(TeamId, Slot)` 不可重複
  - [ ] `Members`：`Slot` 只能是 1–6
  - [ ] 刪除規則：使用者 → 隊伍、隊伍 → 成員都連帶刪除
- [ ] 執行 migration，並到資料庫確認上述規則都已建立
- [ ] DTO（回傳、新增、修改用）與驗證規則
- [ ] Mapper
- [ ] Repository（介面與實作），並在 `Program.cs` 註冊
- [ ] `TeamsController`（所有 API 都需要登入）
  - [ ] `GET /api/teams`：我的牌組（目前使用者的所有隊伍）
  - [ ] `GET /api/teams/{id}`
  - [ ] `POST /api/teams`
  - [ ] `PUT /api/teams/{id}`
  - [ ] `DELETE /api/teams/{id}`
- [ ] 所有查詢都以 token 中的使用者過濾，不能讀取、修改或刪除他人的隊伍
- [ ] 隊伍名稱：存檔前去除頭尾空白；名稱重複時回傳清楚的錯誤訊息
- [ ] 回傳成員時依 `Slot` 排序

### 完成標準
帶 token 建立隊伍後能正確讀回；以兩個帳號測試，確認帳號 A 無法讀取或修改帳號 B 的隊伍。

## 階段 3：群組
對應筆記：Many-To-Many

### 待決定
- [ ] 將隊伍加入群組時，後端需要檢查哪些事項
- [ ] 建立隊伍時，是否同時傳入群組 id 清單
- [ ] 是否禁止使用「我的牌組」作為群組名稱

### 工作項目
- [ ] Model：`Group`、`GroupTeam`
- [ ] 在 `OnModelCreating` 設定資料表規則
  - [ ] `GroupTeams`：以 `(GroupId, TeamId)` 作為主鍵
  - [ ] `Groups`：`(AppUserId, Name)` 不可重複
  - [ ] 刪除規則：群組 → 中間表、隊伍 → 中間表都連帶刪除
  - [ ] 【新】刪除規則：使用者 → 群組**不自動刪除**
- [ ] 執行 migration
- [ ] 群組的 DTO、Mapper、Repository
- [ ] `GroupsController`（所有 API 都需要登入）
  - [ ] `GET /api/groups`
  - [ ] `POST /api/groups`
  - [ ] `PUT /api/groups/{id}`：重新命名
  - [ ] `DELETE /api/groups/{id}`
  - [ ] `GET /api/groups/{groupId}/teams`：列出群組內的隊伍
  - [ ] `POST /api/groups/{groupId}/teams/{teamId}`：將隊伍加入群組
  - [ ] `DELETE /api/groups/{groupId}/teams/{teamId}`：將隊伍移出群組

### 完成標準
可以建立群組、加入隊伍並列出群組內的隊伍；刪除群組後隊伍仍保留，刪除隊伍後群組也仍保留。

## 階段 4：串接前端
對應筆記：無（新內容）

### 工作項目
- [ ] 【新】後端開放 CORS，允許 `http://localhost:3000`，並允許帶 cookie
- [ ] 前端：API 網址集中設定在同一處（比照 `POKEAPI_BASE_URL`）
- [ ] 前端 `types/`：API 回傳資料的型別
- [ ] 前端：安裝 Pinia，存放 token
- [ ] 前端 `services/`：登入、隊伍、群組的 API（使用 `$fetch`，自動帶上 token；refresh、logout 要帶上 cookie）
- [ ] 註冊與登入頁面
- [ ] `/team`：存檔、讀取我的牌組、設定群組（選填）
- [ ] token 過期時自動用 refresh token 換新（同一時間只送出一個 refresh 請求，其他請求等它完成後再重試）；refresh 收到 409 時重試一次，收到 401 才導回登入頁

### 完成標準
可以在網頁上登入並儲存隊伍，重新整理後隊伍仍然存在。

## 階段 5：收尾
對應筆記：Filtering、Sorting、Pagination

- [ ] 刪除帳號：先刪除使用者的所有群組，再刪除使用者
- [ ] 【新】忘記密碼：串接寄信服務，用 Identity 內建功能產生與驗證重設碼
- [ ] 依需求加入篩選、排序、分頁
- [ ] 我的最愛：釐清需求 → 設計資料表 → 實作
- [ ] 【新】部署：決定 API 與資料庫的部署位置，並完成正式環境設定；前後端必須部署在同一個網站底下
