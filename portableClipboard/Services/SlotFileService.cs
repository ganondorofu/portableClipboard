using System;
using System.IO;
using System.Text;

namespace portableClipboard.Services
{
    /// <summary>
    /// スロットファイルの読み書きを担当するサービス
    /// </summary>
    public class SlotFileService
    {
        /// <summary>
        /// スロットファイルを読み込む
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="slot">スロット番号</param>
        /// <returns>ファイルの内容</returns>
        public string ReadSlotFile(string drive, string slot)
        {
            if (string.IsNullOrEmpty(drive) || string.IsNullOrEmpty(slot))
            {
                throw new ArgumentException("ドライブまたはスロット番号が無効です");
            }

            string filePath = Path.Combine(drive, $"slot{slot}.txt");
            
            try
            {
                return File.ReadAllText(filePath, Encoding.UTF8);
            }
            catch (FileNotFoundException)
            {
                // ファイルが存在しない場合は空文字を返す
                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"ファイル読み込みエラー: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// スロットファイルに書き込む
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="slot">スロット番号</param>
        /// <param name="content">書き込む内容</param>
        /// <returns>エラーメッセージ（成功時はnull）</returns>
        public string WriteSlotFile(string drive, string slot, string content)
        {
            if (string.IsNullOrEmpty(drive) || string.IsNullOrEmpty(slot))
            {
                return "ドライブまたはスロット番号が無効です";
            }

            string filePath = Path.Combine(drive, $"slot{slot}.txt");

            try
            {
                File.WriteAllText(filePath, content ?? string.Empty, new UTF8Encoding(false));
                return null; // 成功
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 全スロットファイルを削除し、空ファイルを作成
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        public void DeleteAllSlotFiles(string drive)
        {
            if (string.IsNullOrEmpty(drive))
            {
                throw new ArgumentException("ドライブパスが無効です");
            }

            for (int i = 1; i <= 5; i++)
            {
                string filePath = Path.Combine(drive, $"slot{i}.txt");
                
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    
                    // 空ファイルを作成
                    using (File.Create(filePath)) { }
                }
                catch (Exception ex)
                {
                    // 個別のファイル削除エラーは継続（他のファイルも処理する）
                    Console.WriteLine($"ファイル削除エラー ({filePath}): {ex.Message}");
                }
            }
        }

        /// <summary>
        /// スロットファイルが存在するかチェック
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="slot">スロット番号</param>
        /// <returns>存在するかどうか</returns>
        public bool SlotFileExists(string drive, string slot)
        {
            if (string.IsNullOrEmpty(drive) || string.IsNullOrEmpty(slot))
            {
                return false;
            }

            string filePath = Path.Combine(drive, $"slot{slot}.txt");
            return File.Exists(filePath);
        }
    }
}
