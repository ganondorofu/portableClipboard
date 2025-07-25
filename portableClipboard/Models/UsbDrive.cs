namespace portableClipboard.Models
{
    /// <summary>
    /// USBドライブを表すモデル
    /// </summary>
    public class UsbDrive
    {
        public string Label { get; set; }
        public string Path { get; set; }
        public bool IsValid { get; set; }

        public UsbDrive(string label, string path, bool isValid = true)
        {
            Label = label;
            Path = path;
            IsValid = isValid;
        }

        public override string ToString()
        {
            return $"{Label} ({Path})";
        }

        public static UsbDrive CreateInvalidDrive()
        {
            return new UsbDrive("デバイスが見つかりませんでした", string.Empty, false);
        }
    }
}
