using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AddressAllImporter
{
    /// <summary>
    /// 全国の郵便番号をダウンロード・インポートするバッチクラス
    /// </summary>
    class Program
    {
        /// <summary>
        /// 郵便局のページからダウンロードする際のファイル名
        /// </summary>
        private const string DownloadFileName = "ken_all.zip";

        private const string DirectoryName = "address";

        /// <summary>
        /// Httpファイルダウンロードクライアント
        /// </summary>
        private static HttpClient client = new HttpClient();

        private static async Task Main(string[] args)
        {
            try
            {
                using (var response =
                    await client.GetStreamAsync($"https://www.post.japanpost.jp/zipcode/dl/kogaki/zip/{DownloadFileName}"))
                {
                    if (File.Exists(DownloadFileName))
                    {
                        File.Delete(DownloadFileName);
                    }

                    // ダウンロードした郵便番号ファイルを一時領域に出力する
                    using (var fs = new FileStream(DownloadFileName, FileMode.CreateNew, FileAccess.Write))
                    {
                        // 書き込みバッファ
                        var buffer = new byte[1000];
                        int offset = 0;

                        while ((offset = await response.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await fs.WriteAsync(buffer, 0, offset);

                            await fs.FlushAsync();
                        }
                    }

                    // ダウンロードしたファイルを一時領域に出力
                    if (!Directory.Exists(DirectoryName))
                    {
                        Directory.CreateDirectory(DirectoryName);
                    }

                    ZipFile.ExtractToDirectory(DownloadFileName, DirectoryName);
                }

                // 住所データファイルパス
                var csvPath = Path.Combine(DirectoryName, "KEN_ALL.csv");

                // CSVファイルがShift_JISのため、文字コードを追加
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                using (var reader = new StreamReader(csvPath, Encoding.GetEncoding("Shift_JIS")))
                {
                    string line;

                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        var lineItem = line.Split(',');
                    }
                }
            }
            catch (HttpRequestException requestException)
            {
                // ダウンロード失敗時例外
            }
            catch (IOException ioException)
            {
                // ファイル削除・出力時例外
            }
            catch (UnauthorizedAccessException unauthorizedAccessException)
            {
                // アクセス権例外
            }
        }
    }
}
