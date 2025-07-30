namespace portableClipboard.Constants
{
    /// <summary>
    /// 警告メッセージの定数クラス
    /// </summary>
    public static class WarningMessages
    {
        /// <summary>
        /// 日本語キーボード非対応文字警告のベースメッセージ
        /// </summary>
        public const string JIS_UNTYPABLE_CHARS_BASE = 
            "これらの文字は日本語キーボードでは直接入力できません。\n" +
            "Raspberry Pi Picoでの入力時に問題が発生する可能性があります。";

        /// <summary>
        /// 非ASCII文字警告のベースメッセージ
        /// </summary>
        public const string NON_ASCII_CHARS_BASE = 
            "Raspberry Pi Picoは日本語文字を自動的にスキップして動作します。\n" +
            "日本語文字は入力されませんが、ASCII文字は正常に入力されます。";

        /// <summary>
        /// 保存時の確認メッセージ
        /// </summary>
        public const string SAVE_CONFIRMATION = "保存を続行しますか？";

        /// <summary>
        /// 書き込み時の確認メッセージ
        /// </summary>
        public const string WRITE_CONFIRMATION = "続行しますか？";

        /// <summary>
        /// プログラム書き込み基本メッセージ
        /// </summary>
        public const string PROGRAM_WRITE_BASE = 
            "Raspberry Pi Pico用のプログラムをUSBドライブに書き込みます。\n" +
            "既存のcode.py、config.json、libフォルダは上書きされます。";

        /// <summary>
        /// 警告アイコン
        /// </summary>
        public const string WARNING_ICON = "⚠️ 警告：";

        /// <summary>
        /// 情報アイコン
        /// </summary>
        public const string INFO_ICON = "ℹ️ 情報：";
    }
}
