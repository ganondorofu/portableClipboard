using System;
using System.IO;
using System.Reflection;
using portableClipboard.Models;

namespace portableClipboard.Services
{
    /// <summary>
    /// Raspberry Pi Pico用ファイルの初期化サービス
    /// </summary>
    public class PicoInitializationService
    {
        /// <summary>
        /// Raspberryフォルダのパスを取得
        /// </summary>
        /// <returns>Raspberryフォルダの絶対パス</returns>
        private string GetRaspberryFolderPath()
        {
            // 実行ファイルのディレクトリから上位に移動してRaspberryフォルダを探す
            string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string projectRoot = Directory.GetParent(currentDir).Parent.FullName; // bin/Debug から戻る
            string solutionRoot = Directory.GetParent(projectRoot).FullName; // プロジェクトフォルダから戻る
            return Path.Combine(solutionRoot, "Raspberry");
        }

        /// <summary>
        /// USBドライブにPico用のファイルを作成（上書き）
        /// </summary>
        /// <param name="drivePath">対象のUSBドライブのパス</param>
        /// <returns>初期化が実行されたかどうか</returns>
        public bool InitializePicoFiles(string drivePath)
        {
            if (string.IsNullOrEmpty(drivePath) || !Directory.Exists(drivePath))
            {
                return false;
            }

            bool filesCreated = false;

            // code.pyを作成（上書き）
            if (CreateCodePy(drivePath))
            {
                filesCreated = true;
            }

            // config.jsonを作成（上書き）
            if (CreateConfigJson(drivePath))
            {
                filesCreated = true;
            }

            // jis_keymap.jsonを作成（上書き）
            if (CreateJisKeymapJson(drivePath))
            {
                filesCreated = true;
            }

            // function_keys.jsonを作成（上書き）
            if (CreateFunctionKeysJson(drivePath))
            {
                filesCreated = true;
            }

            // libフォルダとadafruit_hidライブラリを作成（上書き）
            if (CreateLibraries(drivePath))
            {
                filesCreated = true;
            }

            return filesCreated;
        }

        /// <summary>
        /// code.pyファイルを作成（上書き）
        /// </summary>
        /// <param name="drivePath">ドライブパス</param>
        /// <returns>作成されたかどうか</returns>
        private bool CreateCodePy(string drivePath)
        {
            string sourceFile = Path.Combine(GetRaspberryFolderPath(), "code.py");
            string targetFile = Path.Combine(drivePath, "code.py");

            try
            {
                if (File.Exists(sourceFile))
                {
                    File.Copy(sourceFile, targetFile, true);
                    Console.WriteLine("code.py をコピーしました: " + targetFile);
                    return true;
                }
                else
                {
                    Console.WriteLine("ソースファイルが見つかりません: " + sourceFile);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("code.py のコピーに失敗しました: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// config.jsonファイルを作成（上書き）
        /// </summary>
        /// <param name="drivePath">ドライブパス</param>
        /// <returns>作成されたかどうか</returns>
        private bool CreateConfigJson(string drivePath)
        {
            string sourceFile = Path.Combine(GetRaspberryFolderPath(), "config.json");
            string targetFile = Path.Combine(drivePath, "config.json");

            try
            {
                if (File.Exists(sourceFile))
                {
                    File.Copy(sourceFile, targetFile, true);
                    Console.WriteLine($"config.json をコピーしました: {targetFile}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"ソースファイルが見つかりません: {sourceFile}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"config.json のコピーに失敗しました: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// jis_keymap.jsonファイルを作成（上書き）
        /// </summary>
        /// <param name="drivePath">ドライブパス</param>
        /// <returns>作成されたかどうか</returns>
        private bool CreateJisKeymapJson(string drivePath)
        {
            string sourceFile = Path.Combine(GetRaspberryFolderPath(), "jis_keymap.json");
            string targetFile = Path.Combine(drivePath, "jis_keymap.json");

            try
            {
                if (File.Exists(sourceFile))
                {
                    File.Copy(sourceFile, targetFile, true);
                    Console.WriteLine($"jis_keymap.json をコピーしました: {targetFile}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"ソースファイルが見つかりません: {sourceFile}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"jis_keymap.json のコピーに失敗しました: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// function_keys.jsonファイルを作成（上書き）
        /// </summary>
        /// <param name="drivePath">ドライブパス</param>
        /// <returns>作成されたかどうか</returns>
        private bool CreateFunctionKeysJson(string drivePath)
        {
            string sourceFile = Path.Combine(GetRaspberryFolderPath(), "function_keys.json");
            string targetFile = Path.Combine(drivePath, "function_keys.json");

            try
            {
                if (File.Exists(sourceFile))
                {
                    File.Copy(sourceFile, targetFile, true);
                    Console.WriteLine($"function_keys.json をコピーしました: {targetFile}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"ソースファイルが見つかりません: {sourceFile}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"function_keys.json のコピーに失敗しました: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ライブラリフォルダとファイルを作成（上書き）
        /// </summary>
        /// <param name="drivePath">ドライブパス</param>
        /// <returns>作成されたかどうか</returns>
        private bool CreateLibraries(string drivePath)
        {
            string sourceLibPath = Path.Combine(GetRaspberryFolderPath(), "lib");
            string targetLibPath = Path.Combine(drivePath, "lib");

            try
            {
                if (Directory.Exists(sourceLibPath))
                {
                    // 既存のlibフォルダがあれば削除
                    if (Directory.Exists(targetLibPath))
                    {
                        Directory.Delete(targetLibPath, true);
                    }

                    // libフォルダを再帰的にコピー
                    CopyDirectory(sourceLibPath, targetLibPath);
                    Console.WriteLine($"lib フォルダをコピーしました: {targetLibPath}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"ソースlibフォルダが見つかりません: {sourceLibPath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"lib フォルダのコピーに失敗しました: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ディレクトリを再帰的にコピー
        /// </summary>
        /// <param name="sourceDir">コピー元ディレクトリ</param>
        /// <param name="targetDir">コピー先ディレクトリ</param>
        private void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            // ファイルをコピー
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string targetFile = Path.Combine(targetDir, fileName);
                File.Copy(file, targetFile, true);
            }

            // サブディレクトリを再帰的にコピー
            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(dir);
                string targetSubDir = Path.Combine(targetDir, dirName);
                CopyDirectory(dir, targetSubDir);
            }
        }

        /// <summary>
        /// adafruit_hidライブラリファイルを作成
        /// </summary>
        /// <param name="adafruitHidPath">adafruit_hidフォルダのパス</param>
        private void CreateAdafruitHidFiles(string adafruitHidPath)
        {
            // libフォルダ全体をコピーするため、このメソッドは不要
            // 保持はするがフォールバック用のみ
        }
    }
}
