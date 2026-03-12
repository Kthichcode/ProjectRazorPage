# 🏨 Mường Thanh Hotel – Hệ Thống Quản Lý Khách Sạn

Ứng dụng web quản lý khách sạn được xây dựng bằng **ASP.NET Core 8 Razor Pages**, tích hợp đặt phòng trực tuyến, thanh toán VnPay, chat AI (Gemini), và thông báo thời gian thực qua SignalR.

---

## 📋 Mục Lục

- [Tính Năng](#-tính-năng)
- [Kiến Trúc Dự Án](#-kiến-trúc-dự-án)
- [Công Nghệ Sử Dụng](#-công-nghệ-sử-dụng)
- [Cài Đặt & Chạy](#-cài-đặt--chạy)
- [Tài Khoản Mặc Định](#-tài-khoản-mặc-định)
- [Cấu Hình](#️-cấu-hình)

---

## ✨ Tính Năng

### 👤 Khách Hàng
- Xem danh sách phòng, lọc theo loại phòng và tình trạng
- Đặt phòng trực tuyến với nhiều phòng trong một lần
- Thanh toán qua **VnPay** hoặc **Ví điện tử** nội bộ
- Xem lịch sử đặt phòng & chi tiết hóa đơn
- Để lại đánh giá (review) sau khi sử dụng dịch vụ
- Chat với **AI trợ lý** (Gemini API) để hỏi về phòng, giá cả, tiện ích

### 🛡️ Admin
- Dashboard thống kê doanh thu, số lượng đặt phòng
- Quản lý phòng: thêm, sửa, xóa, upload ảnh (thumbnail + gallery)
- Quản lý loại phòng (RoomType) với giá và tiện ích
- Xem & duyệt tất cả các đặt phòng

### 👔 Manager
- Dashboard theo dõi tình trạng phòng và đặt phòng
- Xem đánh giá của khách hàng
- Quản lý phòng trong phạm vi quyền hạn

### 👷 Staff
- Bảng điều khiển trạng thái phòng theo thời gian thực (SignalR)
- Check-in / Check-out khách hàng
- Xem danh sách các đặt phòng trong ngày

---

## 🏗️ Kiến Trúc Dự Án

```
HotelManagementRazorPage/
│
├── BussinessObjects/          # Tầng thực thể (Entity Layer)
│   ├── Entities/              # Các model: Room, Booking, Payment, Review...
│   ├── Enums/                 # Enum: RoomStatus, BookingStatus...
│   └── ApplicationUser.cs     # Identity User mở rộng
│
├── Repositories/              # Tầng truy cập dữ liệu (Data Access Layer)
│   ├── Interfaces/            # Interface cho từng repository
│   ├── AppDbContext.cs        # EF Core DbContext
│   └── *Repository.cs         # Triển khai: Room, Booking, Payment, Review...
│
├── Services/                  # Tầng nghiệp vụ (Business Logic Layer)
│   ├── Interfaces/            # Interface cho từng service
│   ├── DTOs/                  # Data Transfer Objects
│   └── *Service.cs            # Triển khai: Booking, Room, VnPay, AI Chat...
│
└── HotelManagementRazorPage/  # Tầng trình bày (Presentation Layer)
    ├── Pages/
    │   ├── Index.cshtml        # Trang chủ hiển thị phòng
    │   ├── Admin/             # Quản trị viên
    │   ├── Manager/           # Quản lý
    │   ├── Staff/             # Nhân viên
    │   ├── Bookings/          # Đặt phòng, lịch sử, thanh toán
    │   ├── Rooms/             # Chi tiết phòng
    │   └── Chat/              # AI Chatbot
    ├── Hubs/                  # SignalR Hub (RoomHub)
    ├── SignalR/               # SignalR Service
    ├── wwwroot/               # Static files (CSS, JS, Images)
    └── Program.cs             # Entry point & DI configuration
```

### Sơ Đồ Các Entity Chính

```
ApplicationUser  ──── Booking ──── BookingRoom ──── Room ──── RoomType
                  |                                   └── RoomImage
                  ├── Payment
                  ├── Wallet ──── WalletTransaction
                  └── Review
```

---

## 🛠️ Công Nghệ Sử Dụng

| Thành phần        | Công nghệ                            |
|-------------------|--------------------------------------|
| Framework         | ASP.NET Core 8 – Razor Pages         |
| ORM               | Entity Framework Core 8              |
| Cơ sở dữ liệu     | SQL Server (LocalDB / SQL Express)   |
| Xác thực          | ASP.NET Core Identity                |
| Thanh toán        | VnPay Sandbox API                    |
| AI Chatbot        | Google Gemini API                    |
| Real-time         | SignalR                              |
| Upload ảnh        | IFormFile (lưu local wwwroot)        |

---

## 🚀 Cài Đặt & Chạy

### Yêu Cầu Hệ Thống

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server hoặc SQL Server Express
- Visual Studio 2022+ hoặc VS Code

### Các Bước Cài Đặt

**1. Clone dự án**
```bash
git clone <repository-url>
cd ProjectRazorPage
```

**2. Cấu hình chuỗi kết nối CSDL**

Mở file `HotelManagementRazorPage/appsettings.json` và chỉnh sửa:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(local)\\SQLEXPRESS;Database=HotelManagementRazorPageDb;User Id=sa;Password=<your-password>;TrustServerCertificate=True;"
}
```

**3. Áp dụng Migration**
```bash
cd HotelManagementRazorPage/HotelManagementRazorPage
dotnet ef database update
```

**4. Chạy ứng dụng**
```bash
dotnet run
```

Hoặc mở file `.slnx` bằng Visual Studio và nhấn **F5**.

Ứng dụng sẽ chạy tại: `https://localhost:7130`

---

## 🔑 Tài Khoản Mặc Định

Ứng dụng tự động seed các tài khoản khi khởi động lần đầu:

| Vai trò  | Tên đăng nhập | Mật khẩu     | Email                         |
|----------|---------------|--------------|-------------------------------|
| Admin    | `Admin`       | `Admin123@`  | admin@muongthanh.com          |
| Manager  | `Manager`     | `Manager123@`| manager@muongthanh.com        |
| Staff    | `Staff1`      | `Staff123@`  | staff1@muongthanh.com         |
| Staff    | `Staff2`      | `Staff123@`  | staff2@muongthanh.com         |
| Staff    | `Staff3`      | `Staff123@`  | staff3@muongthanh.com         |

> ⚠️ **Lưu ý bảo mật**: Hãy đổi mật khẩu mặc định trước khi triển khai lên môi trường production.

---

## ⚙️ Cấu Hình

Tất cả cấu hình nằm trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "<chuỗi kết nối SQL Server>"
  },
  "VnPay": {
    "TmnCode": "<mã merchant VnPay>",
    "HashSecret": "<secret key>",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "https://localhost:7130/Bookings/PaymentCallback"
  },
  "Gemini": {
    "ApiKey": "<Google Gemini API Key>"
  }
}
```

> 💡 **Lưu ý**: Hiện tại dự án đang dùng VnPay **Sandbox** (môi trường test). Để dùng thực tế, cần thay bằng thông tin tài khoản VnPay production.

---

## 📁 Cấu Trúc Solution

```
ProjectRazorPage/
└── HotelManagementRazorPage/
    ├── HotelManagementRazorPage.slnx      # Solution file
    ├── BussinessObjects/                  # Class Library
    ├── Repositories/                      # Class Library
    ├── Services/                          # Class Library
    └── HotelManagementRazorPage/          # Web App (Startup Project)
```

---

## 👥 Nhóm Phát Triển

Dự án được phát triển như một bài tập học phần **PRN222** – Lập trình Web với ASP.NET Core.
