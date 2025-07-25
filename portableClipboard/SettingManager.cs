using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace portableClipboard
{
    internal class SettingManager
    {
        public static string readBootDelay(string drive)
        {
            if (string.IsNullOrEmpty(drive)) return null;

            string jsonContent = readJson(drive);
            if (string.IsNullOrEmpty(jsonContent)) return null;

            try
            {
                // C# 7.3 対応の using 文（スコープ付き）
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("startup_delay", out JsonElement bootDelayElement))
                    {
                        //Console.WriteLine(bootDelayElement.GetInt32().ToString());
                        return bootDelayElement.GetInt32().ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON解析エラー: {ex.Message}");
            }

            return null;
        }
        public static string readSpeed(string drive)
        {
            if (string.IsNullOrEmpty(drive)) return null;

            string jsonContent = readJson(drive);
            if (string.IsNullOrEmpty(jsonContent)) return null;

            try
            {
                // C# 7.3 対応の using 文（スコープ付き）
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("typing_delay", out JsonElement bootDelayElement))
                    {
                        //Console.WriteLine(bootDelayElement.GetInt32().ToString());
                        return bootDelayElement.GetInt32().ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON解析エラー: {ex.Message}");
            }

            return null;
        }

        private static string readJson(string drive)
        {
            string filePath = $@"{drive}\config.json";
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ファイル読み込みエラー: {ex.Message}");
                return null;
            }
        }
        public static void writeBootDelay(string drive, int content)
        {
            if (string.IsNullOrEmpty(drive)) return;

            string filePath = $@"{drive}\config.json";
            string jsonContent = readJson(drive);
            if (string.IsNullOrEmpty(jsonContent)) return;

            try
            {
                // JSONを解析
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;

                    // JsonElement は不変（読み取り専用）なので、一旦 Dictionary に変換して再構築する
                    var dict = new Dictionary<string, object>();

                    foreach (JsonProperty prop in root.EnumerateObject())
                    {
                        if (prop.Name == "startup_delay")
                        {
                            dict["startup_delay"] = content;  // 値を更新
                        }
                        else
                        {
                            switch (prop.Value.ValueKind)
                            {
                                case JsonValueKind.String:
                                    dict[prop.Name] = prop.Value.GetString();
                                    break;
                                case JsonValueKind.Number:
                                    dict[prop.Name] = prop.Value.GetInt32();
                                    break;
                                case JsonValueKind.True:
                                case JsonValueKind.False:
                                    dict[prop.Name] = prop.Value.GetBoolean();
                                    break;
                                default:
                                    // 必要に応じて他の型も対応
                                    dict[prop.Name] = prop.Value.ToString();
                                    break;
                            }
                        }
                    }

                    // startup_delay がなかったら追加
                    if (!dict.ContainsKey("startup_delay"))
                    {
                        dict["startup_delay"] = content;
                    }

                    // 書き戻す
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string updatedJson = JsonSerializer.Serialize(dict, options);
                    File.WriteAllText(filePath, updatedJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON書き込みエラー: {ex.Message}");
            }
        }
        public static void writeSpeed(string drive, int content)
        {
            if (string.IsNullOrEmpty(drive)) return;

            string filePath = $@"{drive}\config.json";
            string jsonContent = readJson(drive);
            if (string.IsNullOrEmpty(jsonContent)) return;

            try
            {
                // JSONを解析
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    JsonElement root = doc.RootElement;

                    // JsonElement は不変（読み取り専用）なので、一旦 Dictionary に変換して再構築する
                    var dict = new Dictionary<string, object>();

                    foreach (JsonProperty prop in root.EnumerateObject())
                    {
                        if (prop.Name == "typing_delay")
                        {
                            dict["typing_delay"] = content;  // 値を更新
                        }
                        else
                        {
                            switch (prop.Value.ValueKind)
                            {
                                case JsonValueKind.String:
                                    dict[prop.Name] = prop.Value.GetString();
                                    break;
                                case JsonValueKind.Number:
                                    dict[prop.Name] = prop.Value.GetInt32();
                                    break;
                                case JsonValueKind.True:
                                case JsonValueKind.False:
                                    dict[prop.Name] = prop.Value.GetBoolean();
                                    break;
                                default:
                                    // 必要に応じて他の型も対応
                                    dict[prop.Name] = prop.Value.ToString();
                                    break;
                            }
                        }
                    }

                    // startup_delay がなかったら追加
                    if (!dict.ContainsKey("typing_delay"))
                    {
                        dict["typing_delay"] = content;
                    }

                    // 書き戻す
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string updatedJson = JsonSerializer.Serialize(dict, options);
                    File.WriteAllText(filePath, updatedJson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JSON書き込みエラー: {ex.Message}");
            }
        }
    }
}
