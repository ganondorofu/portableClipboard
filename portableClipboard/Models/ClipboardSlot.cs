using System;

namespace portableClipboard.Models
{
    /// <summary>
    /// クリップボードスロットを表すモデル
    /// </summary>
    public class ClipboardSlot
    {
        public string SlotNumber { get; set; }
        public string Content { get; set; }
        public bool IsModified { get; set; }
        public DateTime LastModified { get; set; }

        public ClipboardSlot(string slotNumber)
        {
            SlotNumber = slotNumber;
            Content = string.Empty;
            IsModified = false;
            LastModified = DateTime.Now;
        }

        public void UpdateContent(string newContent)
        {
            if (Content != newContent)
            {
                Content = newContent;
                IsModified = true;
                LastModified = DateTime.Now;
            }
        }

        public void MarkAsSaved()
        {
            IsModified = false;
        }
    }
}