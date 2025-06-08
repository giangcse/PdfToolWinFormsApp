# 📁 PdfToolWinFormsApp

Đây là một ứng dụng **WinForms** hỗ trợ làm việc với các file PDF, gồm 3 tính năng chính:

✅ **1. Phân tích PDF**
- Quét toàn bộ file PDF trong thư mục (và thư mục con).
- Đếm số trang và phân loại khổ giấy (A0, A1, A2, A3, A4).
- Xuất kết quả ra file **Result.xlsx**.

✅ **2. Gộp các file PDF trong thư mục con**
- Tìm các thư mục con có chứa file PDF.
- Trong mỗi thư mục, gộp tất cả file PDF thành file **FileTong.pdf**.

✅ **3. Tạo folder và di chuyển file PDF theo prefix**
- Quét toàn bộ file PDF.
- Dựa trên tên file PDF (vd: `001-02-0001-01.pdf`), tạo thư mục mới dựa vào prefix (vd: `001-02-0001`).
- Di chuyển toàn bộ file có cùng prefix vào thư mục vừa tạo.

---

## 💻 Yêu cầu

- Visual Studio (bất kỳ phiên bản hỗ trợ WinForms)
- .NET Framework
- Các thư viện NuGet:
  - **PdfSharp**
  - **Spire.PDF**
  - **ClosedXML**

---

## 🚀 Hướng dẫn sử dụng

1️⃣ **Clone project**:  
```bash
git clone https://github.com/tenban/PdfToolWinFormsApp.git
````

2️⃣ **Mở project bằng Visual Studio**.

3️⃣ **Cài đặt các thư viện NuGet**:

* Vào **Tools > NuGet Package Manager > Manage NuGet Packages for Solution…**
* Tìm và cài:

  * **PdfSharp**
  * **Spire.PDF**
  * **ClosedXML**

4️⃣ **Build và chạy** (F5 hoặc Build Solution).

---

## 🏷️ Giao diện

* **Chọn thư mục gốc** (nơi chứa file PDF hoặc thư mục con có file PDF).
* **Chọn tính năng**:

  * Phân tích PDF
  * Gộp PDF
  * Tạo folder & di chuyển PDF theo prefix
* **Nhấn Chạy** để bắt đầu.
* **ProgressBar** và **log** sẽ hiển thị tiến trình.

---

## 💡 Lưu ý

* File kết quả (Result.xlsx hoặc FileTong.pdf) sẽ nằm trong thư mục gốc hoặc thư mục con tương ứng.
* File log hiển thị chi tiết quá trình xử lý.
* Hãy đảm bảo **file PDF không đang được mở** trong ứng dụng khác để tránh lỗi khi di chuyển hoặc gộp.

---

## 📫 Liên hệ

Nếu có vấn đề, vui lòng mở issue hoặc liên hệ trực tiếp với mình.

---

Chúc bạn sử dụng app hiệu quả! 🚀
