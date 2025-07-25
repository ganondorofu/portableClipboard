using System;
using System.Collections.Generic;
using System.Windows.Forms;
using portableClipboard.Models;
using portableClipboard.Services;

namespace portableClipboard.Controllers
{
    /// <summary>
    /// メインフォームのコントローラー
    /// UIの操作とビジネスロジックを仲介する
    /// </summary>
    public class MainFormController
    {
        private readonly UsbDriveService _usbDriveService;
        private readonly ClipboardSlotService _slotService;
        private readonly ConfigurationService _configService;
        private readonly PicoInitializationService _picoInitService;
        
        // イベント
        public event Action<List<UsbDrive>> UsbDrivesUpdated;
        public event Action<string> SlotContentChanged;
        public event Action<string> ErrorOccurred;
        public event Action<string> OperationSucceeded;

        public MainFormController()
        {
            _usbDriveService = new UsbDriveService();
            _slotService = new ClipboardSlotService();
            _configService = new ConfigurationService();
            _picoInitService = new PicoInitializationService();
        }

        /// <summary>
        /// USBドライブリストを更新
        /// </summary>
        public void RefreshUsbDrives()
        {
            try
            {
                var drives = _usbDriveService.GetAvailableUsbDrives();
                UsbDrivesUpdated?.Invoke(drives);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"USBドライブの更新に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// スロットを選択
        /// </summary>
        /// <param name="slotNumber">スロット番号</param>
        /// <param name="currentDrive">現在のドライブ</param>
        /// <param name="previousSlot">前のスロット</param>
        /// <param name="previousContent">前のスロットの内容</param>
        public void SelectSlot(string slotNumber, UsbDrive currentDrive, string previousSlot, string previousContent)
        {
            try
            {
                // 前のスロットの内容を保存
                if (!string.IsNullOrEmpty(previousSlot))
                {
                    _slotService.UpdateSlotContent(previousSlot, previousContent);
                }

                if (!_usbDriveService.IsValidDrive(currentDrive))
                {
                    ErrorOccurred?.Invoke("有効なドライブを選択してください");
                    SlotContentChanged?.Invoke(string.Empty);
                    return;
                }

                // 新しいスロットの内容を取得
                var slot = _slotService.GetSlot(slotNumber, currentDrive);
                SlotContentChanged?.Invoke(slot.Content);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"スロット選択エラー: {ex.Message}");
                SlotContentChanged?.Invoke(string.Empty);
            }
        }

        /// <summary>
        /// スロットの内容をファイルから再読み込み
        /// </summary>
        /// <param name="slotNumber">スロット番号</param>
        /// <param name="currentDrive">現在のドライブ</param>
        /// <param name="isCurrentContentModified">現在の内容が変更されているか</param>
        public void ReloadSlotFromFile(string slotNumber, UsbDrive currentDrive, bool isCurrentContentModified)
        {
            try
            {
                if (!_usbDriveService.IsValidDrive(currentDrive))
                {
                    ErrorOccurred?.Invoke("有効なドライブを選択してください");
                    return;
                }

                if (isCurrentContentModified)
                {
                    DialogResult result = MessageBox.Show(
                        "現在の内容に変更があります。読み込み直すと上書きされます。\n続行しますか？",
                        "確認",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result != DialogResult.Yes)
                    {
                        return;
                    }
                }

                // スロットを強制的に再読み込み
                var slot = new ClipboardSlot(slotNumber);
                var fileService = new SlotFileService();
                string content = fileService.ReadSlotFile(currentDrive.Path, slotNumber);
                slot.UpdateContent(content);
                slot.MarkAsSaved();

                // サービス内のスロットも更新
                _slotService.UpdateSlotContent(slotNumber, content);
                SlotContentChanged?.Invoke(content);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"ファイル読み込みエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// スロットの内容をファイルに保存
        /// </summary>
        /// <param name="slotNumber">スロット番号</param>
        /// <param name="content">保存する内容</param>
        /// <param name="currentDrive">現在のドライブ</param>
        public void SaveSlotToFile(string slotNumber, string content, UsbDrive currentDrive)
        {
            try
            {
                if (!_usbDriveService.IsValidDrive(currentDrive))
                {
                    ErrorOccurred?.Invoke("有効なドライブを選択してください");
                    return;
                }

                _slotService.UpdateSlotContent(slotNumber, content);
                _slotService.SaveSlot(slotNumber, currentDrive);
                OperationSucceeded?.Invoke("保存しました");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"保存エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// 全スロットをクリア
        /// </summary>
        /// <param name="currentDrive">現在のドライブ</param>
        public void ClearAllSlots(UsbDrive currentDrive)
        {
            try
            {
                if (!_usbDriveService.IsValidDrive(currentDrive))
                {
                    ErrorOccurred?.Invoke("有効なドライブを選択してください");
                    return;
                }

                DialogResult result = MessageBox.Show(
                    "全スロットの内容を消去します。続行しますか？",
                    "確認",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                _slotService.ClearAllSlots(currentDrive);
                SlotContentChanged?.Invoke(string.Empty);
                OperationSucceeded?.Invoke("全スロットをクリアしました");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"クリアエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// スロット内容を更新（メモリ上のみ）
        /// </summary>
        /// <param name="slotNumber">スロット番号</param>
        /// <param name="content">新しい内容</param>
        public void UpdateSlotContent(string slotNumber, string content)
        {
            try
            {
                _slotService.UpdateSlotContent(slotNumber, content);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"スロット更新エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// 利用可能なスロット番号を取得
        /// </summary>
        /// <returns>スロット番号のリスト</returns>
        public List<string> GetAvailableSlotNumbers()
        {
            return _slotService.GetAvailableSlotNumbers();
        }

        /// <summary>
        /// 指定スロットの内容を取得
        /// </summary>
        /// <param name="slotNumber">スロット番号</param>
        /// <param name="drive">USBドライブ</param>
        /// <returns>スロット内容</returns>
        public string GetSlotContent(string slotNumber, UsbDrive drive)
        {
            try
            {
                var slot = _slotService.GetSlot(slotNumber, drive);
                return slot?.Content;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// スロットが変更されているかチェック
        /// </summary>
        /// <param name="slotNumber">スロット番号</param>
        /// <returns>変更されているかどうか</returns>
        public bool IsSlotModified(string slotNumber)
        {
            return _slotService.IsSlotModified(slotNumber);
        }

        /// <summary>
        /// 指定されたUSBドライブにPicoファイルを初期化
        /// </summary>
        /// <param name="drivePath">USBドライブのパス</param>
        public void InitializePicoFiles(string drivePath)
        {
            try
            {
                bool result = _picoInitService.InitializePicoFiles(drivePath);
                // Picoファイル初期化後に設定項目を確認・追加
                _configService.EnsureAllSettingsExist(drivePath);
                
                if (result)
                {
                    // 成功時の詳細ダイアログ
                    string message = string.Format("Raspberry Pi Pico用ファイルの書き込みが完了しました！\n\n" +
                        "書き込み先: {0}\n\n" +
                        "作成されたファイル:\n" +
                        "• code.py (メインプログラム)\n" +
                        "• config.json (設定ファイル)\n" +
                        "• lib/adafruit_hid/ (キーボードライブラリ)\n\n" +
                        "次の手順:\n" +
                        "1. USBドライブをRaspberry Pi Picoに接続\n" +
                        "2. Picoを再起動してプログラムを開始\n" +
                        "3. LEDでスロット選択、ボタンで送信", drivePath);
                    
                    System.Windows.Forms.MessageBox.Show(
                        message, 
                        "Picoファイル書き込み完了", 
                        System.Windows.Forms.MessageBoxButtons.OK, 
                        System.Windows.Forms.MessageBoxIcon.Information);
                    
                    OperationSucceeded?.Invoke(string.Format("Picoファイルの初期化が完了しました: {0}", drivePath));
                }
                else
                {
                    ErrorOccurred?.Invoke("Picoファイルの書き込みに失敗しました");
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(string.Format("Picoファイル初期化エラー: {0}", ex.Message));
            }
        }

        /// <summary>
        /// 現在のUSBドライブから起動遅延設定を取得
        /// </summary>
        /// <param name="drivePath">USBドライブのパス</param>
        /// <returns>起動遅延設定値</returns>
        public string GetBootDelaySetting(string drivePath)
        {
            // 設定項目の存在確認と自動追加
            _configService.EnsureAllSettingsExist(drivePath);
            return _configService.GetConfigValueOrDefault(drivePath, "startup_delay", _configService.GetDefaultBootDelay());
        }

        /// <summary>
        /// 現在のUSBドライブからタイピング間隔設定を取得
        /// </summary>
        /// <param name="drivePath">USBドライブのパス</param>
        /// <returns>タイピング間隔設定値</returns>
        public string GetTypingDelaySetting(string drivePath)
        {
            // 設定項目の存在確認と自動追加
            _configService.EnsureAllSettingsExist(drivePath);
            return _configService.GetConfigValueOrDefault(drivePath, "typing_delay", _configService.GetDefaultTypingDelay());
        }

        /// <summary>
        /// 起動遅延設定を保存
        /// </summary>
        /// <param name="drivePath">USBドライブのパス</param>
        /// <param name="value">設定値</param>
        public void SaveBootDelaySetting(string drivePath, string value)
        {
            try
            {
                if (int.TryParse(value, out int delay))
                {
                    _configService.WriteBootDelay(drivePath, delay);
                    OperationSucceeded?.Invoke("起動遅延設定を保存しました");
                }
                else
                {
                    ErrorOccurred?.Invoke("起動遅延は数値で入力してください");
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"起動遅延設定の保存に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// タイピング間隔設定を保存
        /// </summary>
        /// <param name="drivePath">USBドライブのパス</param>
        /// <param name="value">設定値</param>
        public void SaveTypingDelaySetting(string drivePath, string value)
        {
            try
            {
                if (double.TryParse(value, out double delay))
                {
                    _configService.WriteTypingSpeed(drivePath, delay); // 小数値のまま保存
                    OperationSucceeded?.Invoke("タイピング間隔設定を保存しました");
                }
                else
                {
                    ErrorOccurred?.Invoke("タイピング間隔は数値で入力してください");
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"タイピング間隔設定の保存に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 改行処理設定を保存
        /// </summary>
        /// <param name="drivePath">USBドライブのパス</param>
        /// <param name="addFinalEnter">改行処理設定（true: 各行を改行として扱う, false: テキストをそのまま送信）</param>
        public void SaveAddFinalEnterSetting(string drivePath, bool addFinalEnter)
        {
            try
            {
                _configService.WriteAddFinalEnter(drivePath, addFinalEnter);
                OperationSucceeded?.Invoke(string.Format("改行処理設定を保存しました: {0}", addFinalEnter ? "各行を改行として扱う" : "テキストをそのまま送信"));
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(string.Format("改行処理設定の保存に失敗しました: {0}", ex.Message));
            }
        }

        /// <summary>
        /// 改行処理設定を取得
        /// </summary>
        /// <param name="drivePath">USBドライブのパス</param>
        /// <returns>改行処理設定（true: 各行を改行として扱う, false: テキストをそのまま送信）</returns>
        public bool GetAddFinalEnterSetting(string drivePath)
        {
            // 設定項目の存在確認と自動追加
            _configService.EnsureAllSettingsExist(drivePath);
            return _configService.ReadAddFinalEnter(drivePath);
        }

        /// <summary>
        /// 修飾キー機能設定を保存
        /// </summary>
        /// <param name="drivePath">USBドライブのパス</param>
        /// <param name="enableModifierKeys">修飾キー機能設定</param>
        public void SaveModifierKeysSetting(string drivePath, bool enableModifierKeys)
        {
            try
            {
                _configService.WriteModifierKeySupport(drivePath, enableModifierKeys);
                OperationSucceeded?.Invoke($"修飾キー機能設定を保存しました: {(enableModifierKeys ? "有効" : "無効")}");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"修飾キー機能設定の保存に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 修飾キー機能設定を取得
        /// </summary>
        /// <param name="drivePath">USBドライブのパス</param>
        /// <returns>修飾キー機能設定</returns>
        public bool GetModifierKeysSetting(string drivePath)
        {
            // 設定項目の存在確認と自動追加
            _configService.EnsureAllSettingsExist(drivePath);
            return _configService.ReadModifierKeySupport(drivePath);
        }

        /// <summary>
        /// 日本語キーボード設定を保存
        /// </summary>
        /// <param name="drivePath">USBドライブのパス</param>
        /// <param name="japaneseKeyboard">日本語キーボード設定（true: 日本語キーボード, false: 英字キーボード）</param>
        public void SaveJapaneseKeyboardSetting(string drivePath, bool japaneseKeyboard)
        {
            try
            {
                _configService.WriteJapaneseKeyboard(drivePath, japaneseKeyboard);
                OperationSucceeded?.Invoke($"キーボード設定を保存しました: {(japaneseKeyboard ? "日本語キーボード" : "英字キーボード")}");
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"キーボード設定の保存に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 日本語キーボード設定を取得
        /// </summary>
        /// <param name="drivePath">USBドライブのパス</param>
        /// <returns>日本語キーボード設定（true: 日本語キーボード, false: 英字キーボード）</returns>
        public bool GetJapaneseKeyboardSetting(string drivePath)
        {
            // 設定項目の存在確認と自動追加
            _configService.EnsureAllSettingsExist(drivePath);
            return _configService.ReadJapaneseKeyboard(drivePath);
        }

        /// <summary>
        /// 現在のスロット内容に非ASCII文字が含まれているかチェック
        /// 非ASCII文字はRaspberry Pi Picoで自動的にスキップされるため、情報提供として使用
        /// </summary>
        /// <param name="drivePath">USBドライブのパス</param>
        /// <returns>非ASCII文字を含むスロットのリスト</returns>
        public List<string> CheckNonAsciiCharacters(string drivePath)
        {
            var nonAsciiSlots = new List<string>();

            try
            {
                var slotNumbers = GetAvailableSlotNumbers();
                foreach (var slotNumber in slotNumbers)
                {
                    var slot = _slotService.GetSlot(slotNumber, new UsbDrive("Temp Drive", drivePath, true));
                    if (!string.IsNullOrEmpty(slot.Content) && ContainsNonAsciiCharacters(slot.Content))
                    {
                        nonAsciiSlots.Add($"スロット{slotNumber}");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"文字チェック中にエラーが発生しました: {ex.Message}");
            }

            return nonAsciiSlots;
        }

        /// <summary>
        /// 現在のフォームの内容に非ASCII文字が含まれているかチェック
        /// </summary>
        /// <param name="slotContents">スロット番号とその内容の辞書</param>
        /// <returns>非ASCII文字を含むスロットのリスト</returns>
        public List<string> CheckFormNonAsciiCharacters(Dictionary<string, string> slotContents)
        {
            var nonAsciiSlots = new List<string>();

            try
            {
                foreach (var kvp in slotContents)
                {
                    if (!string.IsNullOrEmpty(kvp.Value) && ContainsNonAsciiCharacters(kvp.Value))
                    {
                        nonAsciiSlots.Add($"スロット{kvp.Key}");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"文字チェック中にエラーが発生しました: {ex.Message}");
            }

            return nonAsciiSlots;
        }

        /// <summary>
        /// 文字列に非ASCII文字が含まれているかチェック
        /// </summary>
        /// <param name="text">チェック対象の文字列</param>
        /// <returns>非ASCII文字が含まれている場合はtrue</returns>
        private bool ContainsNonAsciiCharacters(string text)
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
    }
}
