using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace portableClipboard
{
    internal class SlotFileManager
    {
        public static String readFile(string drive, string slot)
        {
            string filePath = $@"{drive}slot{slot}.txt";
            try
            {
                // ファイルの内容をすべて文字列として読み込む
                string content = File.ReadAllText(filePath);
                return content;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static string writeFile(string drive, string slot, string content)
        {
            // ファイルパスを構築
            string filePath = Path.Combine(drive, $"slot{slot}.txt");

            try
            {
                // content の内容で既存ファイルを完全に上書き（UTF-8 で書き込み）
                File.WriteAllText(filePath, content, new UTF8Encoding(false));

                return null; // 正常に完了した場合は null を返す
            }
            catch (Exception ex)
            {
                // エラーが発生した場合はその詳細を返す
                return ex.Message;
            }
        }

        public static void deleteFile(string drive)
        {
            string filePath;
            for (int i = 1; i <= 5; i++)
            {
                filePath= drive + "slot" + i + ".txt";
                File.Delete(filePath);
                using (File.Create(filePath)) { }
            }
        }

    }
}
