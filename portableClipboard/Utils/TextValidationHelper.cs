using System;

namespace portableClipboard.Utils
{
    /// <summary>
    /// テキスト検証のヘルパークラス
    /// </summary>
    public static class TextValidationHelper
    {
        /// <summary>
        /// 日本語キーボードで入力できない文字
        /// </summary>
        private static readonly char[] UntypableCharsForJIS = { '_', '|', '\\' };

        /// <summary>
        /// 文字列に非ASCII文字が含まれているかチェック
        /// </summary>
        /// <param name="text">チェック対象の文字列</param>
        /// <returns>非ASCII文字が含まれている場合はtrue</returns>
        public static bool ContainsNonAsciiCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            foreach (char c in text)
            {
                // ASCII文字の範囲は0-127
                if (c > 127)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 文字列に日本語キーボードで入力できない文字（_ または | または \）が含まれているかチェック
        /// </summary>
        /// <param name="text">チェック対象の文字列</param>
        /// <returns>入力できない文字が含まれている場合はtrue</returns>
        public static bool ContainsUntypableCharactersForJIS(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            foreach (char c in text)
            {
                if (Array.IndexOf(UntypableCharsForJIS, c) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 日本語キーボードで入力できない文字のリストを取得
        /// </summary>
        /// <returns>入力できない文字の配列</returns>
        public static char[] GetUntypableCharsForJIS()
        {
            return (char[])UntypableCharsForJIS.Clone();
        }

        /// <summary>
        /// 日本語キーボードで入力できない文字を文字列形式で取得
        /// </summary>
        /// <returns>入力できない文字の説明文字列</returns>
        public static string GetUntypableCharsDescription()
        {
            return "_ または | または \\";
        }
    }
}
