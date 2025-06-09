using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;

// Alias để phân biệt 2 thư viện đều có PdfDocument
using SpirePdf = Spire.Pdf;
using PdfSharpPdf = PdfSharp.Pdf;
using PdfSharpReader = PdfSharp.Pdf.IO;

namespace PdfToolWinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            cboFeature.SelectedIndex = 0; // Mặc định
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private async void btnRun_Click(object sender, EventArgs e)
        {
            string folder = txtFolder.Text;
            if (!Directory.Exists(folder))
            {
                MessageBox.Show("Thư mục không tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            txtLog.Clear();
            progressBar.Value = 0;

            string feature = cboFeature.SelectedItem.ToString();

            if (feature == "Đếm số trang PDF")
            {
                await Task.Run(() => AnalyzePdfFiles(folder));
            }
            else if (feature == "Gộp PDF")
            {
                await Task.Run(() => MergePdfInFolders(folder));
            }
            else if (feature == "Đổi tên sau Ký số")
            {
                // ✏️ MỚI: Tìm tất cả thư mục Unknown trong thư mục gốc
                var unknownFolders = Directory.GetDirectories(folder, "Unknown", SearchOption.AllDirectories);

                if (unknownFolders.Length == 0)
                {
                    Log("Không tìm thấy thư mục Unknown nào!");
                }
                else
                {
                    int total = unknownFolders.Length;
                    int count = 0;
                    foreach (var unknownPath in unknownFolders)
                    {
                        count++;
                        Log($"[{count}/{total}] Xử lý: {unknownPath}");
                        GroupPdfByPrefix(unknownPath);
                    }
                }
            }
            else if (feature == "Nén PDF >100MB")
            {
                await Task.Run(() => CompressLargePdfFiles(folder));
            }


            MessageBox.Show("Hoàn thành!", "Thông báo");
        }


        private void Log(string message)
        {
            if (txtLog.InvokeRequired)
                txtLog.Invoke(new Action(() => txtLog.AppendText(message + Environment.NewLine)));
            else
                txtLog.AppendText(message + Environment.NewLine);
        }

        // ---------- Phân tích PDF ----------
        private void AnalyzePdfFiles(string rootDirectory)
        {
            var pdfFiles = GetPdfFiles(rootDirectory);
            int totalFiles = pdfFiles.Count;
            int processed = 0;

            ConcurrentBag<(string DirectoryName, string Name, int PageCount, int A0, int A1, int A2, int A3, int A4)> results = new();

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 8 };

            Parallel.ForEach(pdfFiles, parallelOptions, pdfFile =>
            {
                var result = AnalyzePdf(pdfFile.FullName);
                results.Add((pdfFile.DirectoryName, pdfFile.Name, result.pageCount, result.A0, result.A1, result.A2, result.A3, result.A4));

                int done = Interlocked.Increment(ref processed);
                int percent = (int)((double)done / totalFiles * 100);

                progressBar.Invoke(new Action(() => progressBar.Value = percent));
                Log($"[{done}/{totalFiles}] {pdfFile.Name}");
            });

            ExportToExcel(results.ToList());
            Log("Đã lưu file Result.xlsx");
        }

        private List<FileInfo> GetPdfFiles(string directoryPath)
        {
            try
            {
                DirectoryInfo directory = new DirectoryInfo(directoryPath);
                return directory.GetFiles("*.pdf", SearchOption.AllDirectories).ToList();
            }
            catch (Exception ex)
            {
                Log("Lỗi: " + ex.Message);
                return new List<FileInfo>();
            }
        }

        private (int pageCount, int A0, int A1, int A2, int A3, int A4) AnalyzePdf(string pdfFilePath)
        {
            var sizes = new[]
            {
                new { name = "A4", w = 210.0, h = 297.0, tolerance = 5.0 },
                new { name = "A3", w = 297.0, h = 420.0, tolerance = 8.0 },
                new { name = "A2", w = 420.0, h = 594.0, tolerance = 10.0 },
                new { name = "A1", w = 594.0, h = 841.0, tolerance = 12.0 },
                new { name = "A0", w = 841.0, h = 1189.0, tolerance = 15.0 }
            };

            Dictionary<string, int> pageCounts = new() { { "A0", 0 }, { "A1", 0 }, { "A2", 0 }, { "A3", 0 }, { "A4", 0 } };

            try
            {
                using (var pdf = new SpirePdf.PdfDocument())
                {
                    pdf.LoadFromFile(pdfFilePath);
                    int pageCount = pdf.Pages.Count;

                    for (int i = 0; i < pageCount; i++)
                    {
                        var page = pdf.Pages[i];
                        double widthMm = page.Size.Width * 0.3528;
                        double heightMm = page.Size.Height * 0.3528;
                        if (widthMm > heightMm)
                            (widthMm, heightMm) = (heightMm, widthMm);

                        bool found = false;
                        foreach (var size in sizes)
                        {
                            if (widthMm <= size.w + size.tolerance && heightMm <= size.h + size.tolerance)
                            {
                                pageCounts[size.name]++;
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            pageCounts["A0"]++;
                    }
                    return (pageCount, pageCounts["A0"], pageCounts["A1"], pageCounts["A2"], pageCounts["A3"], pageCounts["A4"]);
                }
            }
            catch (Exception ex)
            {
                Log($"Lỗi: {ex.Message}");
                return (0, 0, 0, 0, 0, 0);
            }
        }

        private void ExportToExcel(List<(string DirectoryName, string Name, int PageCount, int A0, int A1, int A2, int A3, int A4)> results)
        {
            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Result.xlsx");
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Results");
                ws.Cell(1, 1).Value = "Đường dẫn";
                ws.Cell(1, 2).Value = "Tên file";
                ws.Cell(1, 3).Value = "Số trang";
                ws.Cell(1, 4).Value = "A0";
                ws.Cell(1, 5).Value = "A1";
                ws.Cell(1, 6).Value = "A2";
                ws.Cell(1, 7).Value = "A3";
                ws.Cell(1, 8).Value = "A4";

                for (int i = 0; i < results.Count; i++)
                {
                    var r = results[i];
                    ws.Cell(i + 2, 1).Value = r.DirectoryName;
                    ws.Cell(i + 2, 2).Value = r.Name;
                    ws.Cell(i + 2, 3).Value = r.PageCount;
                    ws.Cell(i + 2, 4).Value = r.A0;
                    ws.Cell(i + 2, 5).Value = r.A1;
                    ws.Cell(i + 2, 6).Value = r.A2;
                    ws.Cell(i + 2, 7).Value = r.A3;
                    ws.Cell(i + 2, 8).Value = r.A4;
                }

                workbook.SaveAs(outputPath);
            }
        }

        // ---------- Gộp PDF ----------
        private void MergePdfInFolders(string rootPath)
        {
            var foldersWithPdf = new List<string>();
            ScanFolderRecursively(rootPath, foldersWithPdf);

            int totalFolders = foldersWithPdf.Count;
            int folderIndex = 0;

            foreach (var folder in foldersWithPdf)
            {
                folderIndex++;
                Log($"[{folderIndex}/{totalFolders}] {folder}");

                var pdfFiles = Directory.GetFiles(folder, "*.pdf", SearchOption.TopDirectoryOnly)
                                        .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                                        .ToList();

                string outputFilePath = Path.Combine(folder, "FileTong.pdf");
                if (File.Exists(outputFilePath)) File.Delete(outputFilePath);

                MergePdfFiles(pdfFiles, outputFilePath);

                Log($"Đã gộp: {outputFilePath}");
            }
        }

        private void ScanFolderRecursively(string folderPath, List<string> result)
        {
            var pdfFiles = Directory.GetFiles(folderPath, "*.pdf", SearchOption.TopDirectoryOnly);
            if (pdfFiles.Length > 0)
            {
                result.Add(folderPath);
                return;
            }

            foreach (var subDir in Directory.GetDirectories(folderPath))
                ScanFolderRecursively(subDir, result);
        }

        private void MergePdfFiles(List<string> fileList, string outputFilePath)
        {
            using (var outDoc = new PdfSharpPdf.PdfDocument())
            {
                foreach (string pdf in fileList)
                {
                    PdfSharpPdf.PdfDocument inputDoc = null;
                    try
                    {
                        inputDoc = PdfSharpReader.PdfReader.Open(pdf, PdfSharpReader.PdfDocumentOpenMode.Import);
                        for (int idx = 0; idx < inputDoc.PageCount; idx++)
                            outDoc.AddPage(inputDoc.Pages[idx]);
                    }
                    catch (Exception ex)
                    {
                        Log($"Lỗi: {ex.Message}");
                    }
                    finally
                    {
                        inputDoc?.Close();
                    }
                }
                outDoc.Save(outputFilePath);
            }
        }

        // ---------- Đổi tên sau Ký số ----------
        private void GroupPdfByPrefix(string unknownPath)
        {
            // 1️⃣ Di chuyển file PDF vào thư mục prefix
            var pdfFiles = Directory.GetFiles(unknownPath, "*.pdf", SearchOption.AllDirectories);
            int totalFiles = pdfFiles.Length;
            int processed = 0;

            foreach (var file in pdfFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string prefix = GetPrefix(fileName);

                if (string.IsNullOrEmpty(prefix)) continue;

                string fileDirectory = Path.GetDirectoryName(file);
                string targetFolder = Path.Combine(fileDirectory, prefix);

                if (!Directory.Exists(targetFolder))
                    Directory.CreateDirectory(targetFolder);

                string targetFile = Path.Combine(targetFolder, Path.GetFileName(file));

                try
                {
                    File.Move(file, targetFile, overwrite: true);
                    Log($"Đã di chuyển: {file} -> {targetFile}");
                }
                catch (Exception ex)
                {
                    Log($"Lỗi di chuyển file {file}: {ex.Message}");
                }

                processed++;
                int percent = (int)((double)processed / totalFiles * 100);
                progressBar.Invoke(new Action(() => progressBar.Value = percent));
            }

            Log("✅ Đã di chuyển file PDF xong. Bắt đầu di chuyển thư mục con của Unknown ra ngoài...");

            // 2️⃣ LẤY ĐÚNG thư mục cha của Unknown (tuyệt đối)
            string parentOfUnknown = new DirectoryInfo(unknownPath).Parent.FullName;

            // 3️⃣ Di chuyển các thư mục con của Unknown ra ngoài (ngang cấp Unknown)
            var subFolders = Directory.GetDirectories(unknownPath, "*", SearchOption.TopDirectoryOnly);

            foreach (var subFolder in subFolders)
            {
                string folderName = Path.GetFileName(subFolder);
                string targetPath = Path.Combine(parentOfUnknown, folderName);

                try
                {
                    if (Directory.Exists(targetPath))
                        Directory.Delete(targetPath, true); // Xóa nếu tồn tại

                    Directory.Move(subFolder, targetPath);
                    Log($"Đã di chuyển thư mục: {subFolder} -> {targetPath}");
                }
                catch (Exception ex)
                {
                    Log($"Lỗi di chuyển {subFolder}: {ex.Message}");
                }
            }

            // 4️⃣ Xóa thư mục Unknown
            try
            {
                Directory.Delete(unknownPath, true);
                Log($"Đã xóa thư mục Unknown: {unknownPath}");
            }
            catch (Exception ex)
            {
                Log($"Lỗi xóa thư mục Unknown: {ex.Message}");
            }

            Log("🎉 Hoàn thành: Di chuyển thư mục con ra ngoài và xóa thư mục Unknown!");
        }

        // Lấy prefix: ví dụ "001-02-0001-xx" -> "001-02-0001"
        private string GetPrefix(string fileName)
        {
            var parts = fileName.Split('-');
            if (parts.Length < 5) return null;

            return string.Join("-", parts.Take(4));
        }


        // ---------- Nén PDF >100MB ----------
        private void CompressLargePdfFiles(string rootDirectory)
        {
            var pdfFiles = Directory.GetFiles(rootDirectory, "*.pdf", SearchOption.AllDirectories);
            List<(string Path, double SizeMB)> largeFiles = new();

            int processed = 0;
            int totalFiles = pdfFiles.Length;

            foreach (var file in pdfFiles)
            {
                FileInfo fi = new FileInfo(file);
                double sizeMB = fi.Length / (1024.0 * 1024.0);

                if (sizeMB > 100)
                {
                    largeFiles.Add((file, sizeMB));
                    Log($"🔴 File lớn: {file} ({sizeMB:F2} MB)");

                    // Nén bằng Ghostscript
                    string compressedTempFile = file + ".compressed.pdf";
                    CompressPdfWithGhostscript(file, compressedTempFile);

                    // Ghi đè file cũ nếu nén thành công
                    if (File.Exists(compressedTempFile))
                    {
                        File.Delete(file);
                        File.Move(compressedTempFile, file);
                        Log($"✅ Đã nén: {file}");
                    }
                }

                processed++;
                int percent = (int)((double)processed / totalFiles * 100);
                progressBar.Invoke(new Action(() => progressBar.Value = percent));
            }

            // Xuất Excel danh sách file lớn
            ExportCompressedFilesExcel(largeFiles);
            Log("🎉 Hoàn thành: Đã nén các file lớn và lưu danh sách vào CompressedFiles.xlsx");
        }

        private void CompressPdfWithGhostscript(string inputFile, string outputFile)
        {
            string ghostscriptPath = @"C:\Program Files\gs\gs10.05.1\bin\gswin64c.exe"; // Đường dẫn Ghostscript
            string arguments = $"-sDEVICE=pdfwrite -dCompatibilityLevel=1.4 -dPDFSETTINGS=/screen -dNOPAUSE -dQUIET -dBATCH -sOutputFile=\"{outputFile}\" \"{inputFile}\"";

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = ghostscriptPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = System.Diagnostics.Process.Start(psi))
            {
                process.WaitForExit();
                string error = process.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                    Log($"⚠️ Ghostscript: {error}");
            }
        }

        private void ExportCompressedFilesExcel(List<(string Path, double SizeMB)> list)
        {
            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CompressedFiles.xlsx");
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Compressed Files");
                ws.Cell(1, 1).Value = "Đường dẫn";
                ws.Cell(1, 2).Value = "Kích thước (MB)";

                for (int i = 0; i < list.Count; i++)
                {
                    ws.Cell(i + 2, 1).Value = list[i].Path;
                    ws.Cell(i + 2, 2).Value = list[i].SizeMB;
                }

                workbook.SaveAs(outputPath);
            }
        }

    }
}
