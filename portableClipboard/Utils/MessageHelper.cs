using portableClipboard.Constants;
using portableClipboard.Utils;

namespace portableClipboard.Utils
{
    /// <summary>
    /// 警告メッセージ生成のヘルパークラス
    /// </summary>
    public static class MessageHelper
    {
        /// <summary>
        /// 日本語キーボード非対応文字の保存警告メッセージを生成
        /// </summary>
        /// <param name="slotNumber">スロット番号</param>
        /// <returns>警告メッセージ</returns>
        public static string CreateJISUntypableCharsSaveWarning(string slotNumber)
        {
            return $"スロット{slotNumber}に日本語キーボードで入力できない文字（{TextValidationHelper.GetUntypableCharsDescription()}）が含まれています。\n\n" +
                   $"{WarningMessages.JIS_UNTYPABLE_CHARS_BASE}\n\n" +
                   $"{WarningMessages.SAVE_CONFIRMATION}";
        }

        /// <summary>
        /// 非ASCII文字の保存警告メッセージを生成
        /// </summary>
        /// <param name="slotNumber">スロット番号</param>
        /// <returns>警告メッセージ</returns>
        public static string CreateNonAsciiCharsSaveWarning(string slotNumber)
        {
            return $"スロット{slotNumber}に日本語などの非ASCII文字が含まれています。\n\n" +
                   $"{WarningMessages.NON_ASCII_CHARS_BASE}\n\n" +
                   $"{WarningMessages.SAVE_CONFIRMATION}";
        }

        /// <summary>
        /// プログラム書き込み時の警告メッセージを生成
        /// </summary>
        /// <param name="hasUntypableChars">現在のスロットに入力不可文字があるか</param>
        /// <param name="untypableSlots">入力不可文字を含むスロットのリスト</param>
        /// <param name="currentSlot">現在のスロット番号</param>
        /// <param name="hasNonAscii">現在のスロットに非ASCII文字があるか</param>
        /// <param name="nonAsciiSlots">非ASCII文字を含むスロットのリスト</param>
        /// <returns>警告メッセージ</returns>
        public static string CreateProgramWriteWarning(
            bool hasUntypableChars, 
            System.Collections.Generic.List<string> untypableSlots, 
            string currentSlot,
            bool hasNonAscii, 
            System.Collections.Generic.List<string> nonAsciiSlots)
        {
            string message = $"{WarningMessages.PROGRAM_WRITE_BASE}\n\n";

            // 入力不可文字の警告
            if (hasUntypableChars || untypableSlots.Count > 0)
            {
                message += $"{WarningMessages.WARNING_ICON}";
                
                if (hasUntypableChars)
                {
                    message += $"現在のスロット{currentSlot}";
                    if (untypableSlots.Count > 0)
                    {
                        message += "と" + string.Join(", ", untypableSlots);
                    }
                }
                else
                {
                    message += string.Join(", ", untypableSlots);
                }
                
                message += $"に日本語キーボードで入力できない文字（{TextValidationHelper.GetUntypableCharsDescription()}）が含まれています。\n\n" +
                          $"{WarningMessages.JIS_UNTYPABLE_CHARS_BASE}\n\n";
            }

            // 非ASCII文字の情報
            if (hasNonAscii || nonAsciiSlots.Count > 0)
            {
                message += $"{WarningMessages.INFO_ICON}";
                
                if (hasNonAscii)
                {
                    message += $"現在のスロット{currentSlot}";
                    if (nonAsciiSlots.Count > 0)
                    {
                        message += "と" + string.Join(", ", nonAsciiSlots);
                    }
                }
                else
                {
                    message += string.Join(", ", nonAsciiSlots);
                }
                
                message += $"に日本語などの非ASCII文字が含まれています。\n\n" +
                          $"{WarningMessages.NON_ASCII_CHARS_BASE}\n\n";
            }

            message += $"{WarningMessages.WRITE_CONFIRMATION}";
            return message;
        }
    }
}
