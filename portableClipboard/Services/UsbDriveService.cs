using System;
using System.Collections.Generic;
using System.IO;
using portableClipboard.Models;

namespace portableClipboard.Services
{
    /// <summary>
    /// USBドライブ関連のサービス
    /// </summary>
    public class UsbDriveService
    {
        /// <summary>
        /// 利用可能なUSBドライブを取得
        /// </summary>
        /// <returns>USBドライブのリスト</returns>
        public List<UsbDrive> GetAvailableUsbDrives()
        {
            var usbDrives = new List<UsbDrive>();

            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (DriveInfo drive in drives)
                {
                    if (drive.DriveType == DriveType.Removable && drive.IsReady)
                    {
                        string volumeLabel = string.IsNullOrEmpty(drive.VolumeLabel) 
                            ? "（ラベルなし）" 
                            : drive.VolumeLabel;
                        
                        usbDrives.Add(new UsbDrive(volumeLabel, drive.Name));
                    }
                }
            }
            catch (Exception ex)
            {
                // ログ記録などの処理を追加可能
                Console.WriteLine($"USBドライブ取得エラー: {ex.Message}");
            }

            return usbDrives;
        }

        /// <summary>
        /// USBドライブが有効かどうかを確認
        /// </summary>
        /// <param name="drive">確認するドライブ</param>
        /// <returns>有効かどうか</returns>
        public bool IsValidDrive(UsbDrive drive)
        {
            return drive != null && drive.IsValid && !string.IsNullOrEmpty(drive.Path);
        }
    }
}
