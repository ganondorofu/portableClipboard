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
        public static String readFile(string drive,string slot)
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
    }
}
