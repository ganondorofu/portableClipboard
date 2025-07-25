using System;
using System.Collections.Generic;
using System.Linq;
using portableClipboard.Models;

namespace portableClipboard.Services
{
    /// <summary>
    /// クリップボードスロット管理サービス
    /// </summary>
    public class ClipboardSlotService
    {
        private readonly Dictionary<string, ClipboardSlot> _slots;
        private readonly SlotFileService _fileService;

        public ClipboardSlotService()
        {
            _slots = new Dictionary<string, ClipboardSlot>();
            _fileService = new SlotFileService();
            InitializeSlots();
        }

        /// <summary>
        /// スロットを初期化
        /// </summary>
        private void InitializeSlots()
        {
            for (int i = 1; i <= 5; i++)
            {
                _slots[i.ToString()] = new ClipboardSlot(i.ToString());
            }
        }

        /// <summary>
        /// 指定したスロットの内容を取得
        /// </summary>
        /// <param name="slotNumber">スロット番号</param>
        /// <param name="drive">USBドライブ</param>
        /// <returns>スロットの内容</returns>
        public ClipboardSlot GetSlot(string slotNumber, UsbDrive drive = null)
        {
            if (!_slots.ContainsKey(slotNumber))
            {
                throw new ArgumentException($"無効なスロット番号: {slotNumber}");
            }

            var slot = _slots[slotNumber];

            // まだ読み込まれていない場合、ファイルから読み込み
            if (string.IsNullOrEmpty(slot.Content) && drive != null)
            {
                try
                {
                    string content = _fileService.ReadSlotFile(drive.Path, slotNumber);
                    slot.UpdateContent(content);
                    slot.MarkAsSaved(); // ファイルから読み込んだので変更なしとマーク
                }
                catch (Exception ex)
                {
                    // 読み込みエラーは内容を空にする
                    slot.UpdateContent(string.Empty);
                    slot.MarkAsSaved();
                    throw new InvalidOperationException($"スロット{slotNumber}の読み込みに失敗しました: {ex.Message}");
                }
            }

            return slot;
        }

        /// <summary>
        /// スロットの内容を更新
        /// </summary>
        /// <param name="slotNumber">スロット番号</param>
        /// <param name="content">新しい内容</param>
        public void UpdateSlotContent(string slotNumber, string content)
        {
            if (!_slots.ContainsKey(slotNumber))
            {
                throw new ArgumentException($"無効なスロット番号: {slotNumber}");
            }

            _slots[slotNumber].UpdateContent(content);
        }

        /// <summary>
        /// スロットの内容をファイルに保存
        /// </summary>
        /// <param name="slotNumber">スロット番号</param>
        /// <param name="drive">保存先USBドライブ</param>
        public void SaveSlot(string slotNumber, UsbDrive drive)
        {
            if (!_slots.ContainsKey(slotNumber))
            {
                throw new ArgumentException($"無効なスロット番号: {slotNumber}");
            }

            var slot = _slots[slotNumber];
            string error = _fileService.WriteSlotFile(drive.Path, slotNumber, slot.Content);
            
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException($"スロット{slotNumber}の保存に失敗しました: {error}");
            }

            slot.MarkAsSaved();
        }

        /// <summary>
        /// 全スロットをクリア
        /// </summary>
        /// <param name="drive">USBドライブ</param>
        public void ClearAllSlots(UsbDrive drive)
        {
            _fileService.DeleteAllSlotFiles(drive.Path);
            
            // メモリ上のスロットもクリア
            foreach (var slot in _slots.Values)
            {
                slot.UpdateContent(string.Empty);
                slot.MarkAsSaved();
            }
        }

        /// <summary>
        /// 利用可能なスロット番号を取得
        /// </summary>
        /// <returns>スロット番号のリスト</returns>
        public List<string> GetAvailableSlotNumbers()
        {
            return _slots.Keys.OrderBy(k => int.Parse(k)).ToList();
        }

        /// <summary>
        /// スロットが変更されているかチェック
        /// </summary>
        /// <param name="slotNumber">スロット番号</param>
        /// <returns>変更されているかどうか</returns>
        public bool IsSlotModified(string slotNumber)
        {
            return _slots.ContainsKey(slotNumber) && _slots[slotNumber].IsModified;
        }
    }
}
