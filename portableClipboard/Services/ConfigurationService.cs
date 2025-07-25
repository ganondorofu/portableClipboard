using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace portableClipboard.Services
{
    /// <summary>
    /// 設定ファイル管理サービス
    /// </summary>
    public class ConfigurationService
    {
        private const string CONFIG_FILE_NAME = "config.json";

        /// <summary>
        /// デフォルトのタイピング遅延を取得
        /// </summary>
        /// <returns>デフォルト値（文字列）</returns>
        public string GetDefaultTypingDelay()
        {
            return "0.01";
        }

        /// <summary>
        /// デフォルトの起動遅延を取得
        /// </summary>
        /// <returns>デフォルト値（文字列）</returns>
        public string GetDefaultBootDelay()
        {
            return "3";
        }

        /// <summary>
        /// 起動遅延時間を読み取り
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <returns>起動遅延時間（文字列）</returns>
        public string ReadBootDelay(string drive)
        {
            return ReadIntegerProperty(drive, "startup_delay")?.ToString();
        }

        /// <summary>
        /// タイピング速度を読み取り
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <returns>タイピング速度（文字列）</returns>
        public string ReadTypingSpeed(string drive)
        {
            return ReadDoubleProperty(drive, "typing_delay")?.ToString();
        }

        /// <summary>
        /// 起動遅延時間を書き込み
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="delay">遅延時間</param>
        public void WriteBootDelay(string drive, int delay)
        {
            WriteIntegerProperty(drive, "startup_delay", delay);
        }

        /// <summary>
        /// タイピング速度を書き込み（小数値対応）
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="speed">タイピング速度（小数値）</param>
        public void WriteTypingSpeed(string drive, double speed)
        {
            WriteDoubleProperty(drive, "typing_delay", speed);
        }

        /// <summary>
        /// 改行処理設定を読み取り
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <returns>改行処理設定（true: 各行を改行として扱う, false: テキストをそのまま送信）</returns>
        public bool ReadAddFinalEnter(string drive)
        {
            return ReadBooleanProperty(drive, "add_final_enter") ?? false;
        }

        /// <summary>
        /// 改行処理設定を書き込み
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="addFinalEnter">改行処理設定（true: 各行を改行として扱う, false: テキストをそのまま送信）</param>
        public void WriteAddFinalEnter(string drive, bool addFinalEnter)
        {
            WriteBooleanProperty(drive, "add_final_enter", addFinalEnter);
        }

        /// <summary>
        /// 修飾キー機能設定を読み取り
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <returns>修飾キー機能設定</returns>
        public bool ReadModifierKeySupport(string drive)
        {
            return ReadBooleanProperty(drive, "enable_modifier_keys") ?? false;
        }

        /// <summary>
        /// 修飾キー機能設定を書き込み
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="enableModifierKeys">修飾キー機能設定</param>
        public void WriteModifierKeySupport(string drive, bool enableModifierKeys)
        {
            WriteBooleanProperty(drive, "enable_modifier_keys", enableModifierKeys);
        }

        /// <summary>
        /// 日本語キーボード設定を読み取り
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <returns>日本語キーボード設定（true: 日本語キーボード, false: 英字キーボード）</returns>
        public bool ReadJapaneseKeyboard(string drive)
        {
            return ReadBooleanProperty(drive, "japanese_keyboard") ?? true;
        }

        /// <summary>
        /// 日本語キーボード設定を書き込み
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="japaneseKeyboard">日本語キーボード設定（true: 日本語キーボード, false: 英字キーボード）</param>
        public void WriteJapaneseKeyboard(string drive, bool japaneseKeyboard)
        {
            WriteBooleanProperty(drive, "japanese_keyboard", japaneseKeyboard);
        }

        /// <summary>
        /// デフォルト設定でconfig.jsonを初期化
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        public void InitializeDefaultConfig(string drive)
        {
            if (string.IsNullOrEmpty(drive))
            {
                return;
            }

            var defaultConfig = new Dictionary<string, object>
            {
                ["startup_delay"] = 3,
                ["typing_delay"] = 0.01,
                ["add_final_enter"] = false,
                ["enable_modifier_keys"] = false,
                ["japanese_keyboard"] = true
            };

            WriteConfigJson(drive, defaultConfig);
        }

        /// <summary>
        /// 設定項目が存在しない場合に追加
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        public void EnsureAllSettingsExist(string drive)
        {
            if (string.IsNullOrEmpty(drive))
            {
                return;
            }

            string jsonContent = ReadConfigJson(drive);
            if (string.IsNullOrEmpty(jsonContent))
            {
                InitializeDefaultConfig(drive);
                return;
            }

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;
                    var configDict = new Dictionary<string, object>();
                    bool needsUpdate = false;

                    // 既存のプロパティを保持
                    foreach (JsonProperty prop in root.EnumerateObject())
                    {
                        configDict[prop.Name] = ConvertJsonElement(prop.Value);
                    }

                    // 必要な設定項目が存在しない場合は追加
                    if (!configDict.ContainsKey("startup_delay"))
                    {
                        configDict["startup_delay"] = 3;
                        needsUpdate = true;
                    }

                    if (!configDict.ContainsKey("typing_delay"))
                    {
                        configDict["typing_delay"] = 0.01;
                        needsUpdate = true;
                    }

                    if (!configDict.ContainsKey("add_final_enter"))
                    {
                        configDict["add_final_enter"] = false;
                        needsUpdate = true;
                    }

                    if (!configDict.ContainsKey("enable_modifier_keys"))
                    {
                        configDict["enable_modifier_keys"] = false;
                        needsUpdate = true;
                    }

                    if (!configDict.ContainsKey("japanese_keyboard"))
                    {
                        configDict["japanese_keyboard"] = true;
                        needsUpdate = true;
                    }

                    // 更新が必要な場合のみファイルを書き込み
                    if (needsUpdate)
                    {
                        WriteConfigJson(drive, configDict);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"設定項目の確認・追加エラー: {ex.Message}");
                // エラーが発生した場合は初期設定で上書き
                InitializeDefaultConfig(drive);
            }
        }

        /// <summary>
        /// 設定値を取得、存在しない場合はデフォルト値を返す
        /// </summary>
        /// <param name="drive">USBドライブのパス</param>
        /// <param name="propertyName">設定項目名</param>
        /// <param name="defaultValue">デフォルト値</param>
        /// <returns>設定値または デフォルト値</returns>
        public string GetConfigValueOrDefault(string drive, string propertyName, string defaultValue)
        {
            if (string.IsNullOrEmpty(drive))
            {
                return defaultValue;
            }

            try
            {
                switch (propertyName)
                {
                    case "startup_delay":
                        return ReadBootDelay(drive) ?? defaultValue;
                    case "typing_delay":
                        return ReadTypingSpeed(drive) ?? defaultValue;
                    case "add_final_enter":
                        return ReadAddFinalEnter(drive).ToString();
                    case "enable_modifier_keys":
                        return ReadModifierKeySupport(drive).ToString();
                    case "japanese_keyboard":
                        return ReadJapaneseKeyboard(drive).ToString();
                    default:
                        return defaultValue;
                }
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// 小数値プロパティを読み取り
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>設定値</returns>
        private double? ReadDoubleProperty(string drive, string propertyName)
        {
            if (string.IsNullOrEmpty(drive) || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            string jsonContent = ReadConfigJson(drive);
            if (string.IsNullOrEmpty(jsonContent))
            {
                return null;
            }

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty(propertyName, out JsonElement element))
                    {
                        return element.GetDouble();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON解析エラー ({propertyName}): {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 小数値プロパティを書き込み
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <param name="value">設定値</param>
        private void WriteDoubleProperty(string drive, string propertyName, double value)
        {
            if (string.IsNullOrEmpty(drive) || string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            string jsonContent = ReadConfigJson(drive);
            if (string.IsNullOrEmpty(jsonContent))
            {
                // 設定ファイルが存在しない場合は新規作成
                var newConfig = new Dictionary<string, object>
                {
                    [propertyName] = value
                };
                WriteConfigJson(drive, newConfig);
                return;
            }

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;
                    var configDict = new Dictionary<string, object>();

                    // 既存のプロパティを保持
                    foreach (JsonProperty prop in root.EnumerateObject())
                    {
                        if (prop.Name == propertyName)
                        {
                            configDict[propertyName] = value; // 値を更新
                        }
                        else
                        {
                            configDict[prop.Name] = ConvertJsonElement(prop.Value);
                        }
                    }

                    // プロパティが存在しなかった場合は追加
                    if (!configDict.ContainsKey(propertyName))
                    {
                        configDict[propertyName] = value;
                    }

                    WriteConfigJson(drive, configDict);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON書き込みエラー ({propertyName}): {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 整数プロパティを読み取り
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>設定値</returns>
        private int? ReadIntegerProperty(string drive, string propertyName)
        {
            if (string.IsNullOrEmpty(drive) || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            string jsonContent = ReadConfigJson(drive);
            if (string.IsNullOrEmpty(jsonContent))
            {
                return null;
            }

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty(propertyName, out JsonElement element))
                    {
                        return element.GetInt32();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON解析エラー ({propertyName}): {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 整数プロパティを書き込み
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <param name="value">設定値</param>
        private void WriteIntegerProperty(string drive, string propertyName, int value)
        {
            if (string.IsNullOrEmpty(drive) || string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            string jsonContent = ReadConfigJson(drive);
            if (string.IsNullOrEmpty(jsonContent))
            {
                // 設定ファイルが存在しない場合は新規作成
                var newConfig = new Dictionary<string, object>
                {
                    [propertyName] = value
                };
                WriteConfigJson(drive, newConfig);
                return;
            }

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;
                    var configDict = new Dictionary<string, object>();

                    // 既存のプロパティを保持
                    foreach (JsonProperty prop in root.EnumerateObject())
                    {
                        if (prop.Name == propertyName)
                        {
                            configDict[propertyName] = value; // 値を更新
                        }
                        else
                        {
                            configDict[prop.Name] = ConvertJsonElement(prop.Value);
                        }
                    }

                    // プロパティが存在しなかった場合は追加
                    if (!configDict.ContainsKey(propertyName))
                    {
                        configDict[propertyName] = value;
                    }

                    WriteConfigJson(drive, configDict);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON書き込みエラー ({propertyName}): {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Boolean値プロパティを読み取り
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>設定値</returns>
        private bool? ReadBooleanProperty(string drive, string propertyName)
        {
            if (string.IsNullOrEmpty(drive) || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            string jsonContent = ReadConfigJson(drive);
            if (string.IsNullOrEmpty(jsonContent))
            {
                return null;
            }

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty(propertyName, out JsonElement element))
                    {
                        return element.GetBoolean();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON解析エラー ({propertyName}): {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Boolean値プロパティを書き込み
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <param name="value">設定値</param>
        private void WriteBooleanProperty(string drive, string propertyName, bool value)
        {
            if (string.IsNullOrEmpty(drive) || string.IsNullOrEmpty(propertyName))
            {
                return;
            }

            string jsonContent = ReadConfigJson(drive);
            if (string.IsNullOrEmpty(jsonContent))
            {
                // 設定ファイルが存在しない場合は新規作成
                var newConfig = new Dictionary<string, object>
                {
                    [propertyName] = value
                };
                WriteConfigJson(drive, newConfig);
                return;
            }

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;
                    var configDict = new Dictionary<string, object>();

                    // 既存のプロパティを保持
                    foreach (JsonProperty prop in root.EnumerateObject())
                    {
                        if (prop.Name == propertyName)
                        {
                            configDict[propertyName] = value; // 値を更新
                        }
                        else
                        {
                            configDict[prop.Name] = ConvertJsonElement(prop.Value);
                        }
                    }

                    // プロパティが存在しなかった場合は追加
                    if (!configDict.ContainsKey(propertyName))
                    {
                        configDict[propertyName] = value;
                    }

                    WriteConfigJson(drive, configDict);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON書き込みエラー ({propertyName}): {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// JsonElementをオブジェクトに変換
        /// </summary>
        /// <param name="element">JsonElement</param>
        /// <returns>変換されたオブジェクト</returns>
        private object ConvertJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    // 小数値も対応
                    if (element.TryGetDouble(out double doubleValue))
                    {
                        return doubleValue;
                    }
                    return element.GetInt32();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();
                default:
                    return element.ToString();
            }
        }

        /// <summary>
        /// 設定JSONファイルを読み取り
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <returns>JSON文字列</returns>
        private string ReadConfigJson(string drive)
        {
            string filePath = Path.Combine(drive, CONFIG_FILE_NAME);
            
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"設定ファイル読み込みエラー: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 設定JSONファイルを書き込み
        /// </summary>
        /// <param name="drive">ドライブパス</param>
        /// <param name="config">設定辞書</param>
        private void WriteConfigJson(string drive, Dictionary<string, object> config)
        {
            string filePath = Path.Combine(drive, CONFIG_FILE_NAME);
            
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                
                string jsonString = JsonSerializer.Serialize(config, options);
                // BOMなしUTF-8でファイルを保存
                var utf8WithoutBom = new System.Text.UTF8Encoding(false);
                File.WriteAllText(filePath, jsonString, utf8WithoutBom);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"設定ファイル書き込みエラー: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}