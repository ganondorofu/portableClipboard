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
            string filePath = Path.Combine(drivePath, "code.py");

            try
            {
                string codePyContent = GetCodePyContent();
                // BOMなしUTF-8でファイルを保存
                var utf8WithoutBom = new System.Text.UTF8Encoding(false);
                File.WriteAllText(filePath, codePyContent, utf8WithoutBom);
                Console.WriteLine($"code.py を作成しました: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"code.py の作成に失敗しました: {ex.Message}");
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
            string filePath = Path.Combine(drivePath, "config.json");

            try
            {
                string configJsonContent = GetConfigJsonContent();
                // BOMなしUTF-8でファイルを保存
                var utf8WithoutBom = new System.Text.UTF8Encoding(false);
                File.WriteAllText(filePath, configJsonContent, utf8WithoutBom);
                Console.WriteLine($"config.json を作成しました: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"config.json の作成に失敗しました: {ex.Message}");
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
            string libPath = Path.Combine(drivePath, "lib");
            string adafruitHidPath = Path.Combine(libPath, "adafruit_hid");

            try
            {
                // libフォルダを作成（既存の場合は削除して再作成）
                if (Directory.Exists(libPath))
                {
                    Directory.Delete(libPath, true);
                }
                Directory.CreateDirectory(libPath);
                
                // adafruit_hidフォルダを作成
                Directory.CreateDirectory(adafruitHidPath);

                // 必要なライブラリファイルを作成
                CreateAdafruitHidFiles(adafruitHidPath);

                Console.WriteLine($"adafruit_hid ライブラリを作成しました: {adafruitHidPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"adafruit_hid ライブラリの作成に失敗しました: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// adafruit_hidライブラリファイルを作成
        /// </summary>
        /// <param name="adafruitHidPath">adafruit_hidフォルダのパス</param>
        private void CreateAdafruitHidFiles(string adafruitHidPath)
        {
            var files = new[]
            {
                "__init__.py",
                "keyboard.py",
                "keycode.py",
                "keyboard_layout_base.py",
                "keyboard_layout_us.py",
                "mouse.py",
                "consumer_control.py",
                "consumer_control_code.py"
            };

            foreach (var fileName in files)
            {
                try
                {
                    string content = GetEmbeddedResource($"adafruit_hid.{fileName}");
                    string filePath = Path.Combine(adafruitHidPath, fileName);
                    // BOMなしUTF-8でファイルを保存
                    var utf8WithoutBom = new System.Text.UTF8Encoding(false);
                    File.WriteAllText(filePath, content, utf8WithoutBom);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ファイル {fileName} の作成に失敗しました: {ex.Message}");
                    // フォールバック：ハードコーディングされた内容を使用
                    string fallbackContent = GetFallbackAdafruitHidContent(fileName);
                    if (!string.IsNullOrEmpty(fallbackContent))
                    {
                        string filePath = Path.Combine(adafruitHidPath, fileName);
                        // BOMなしUTF-8でファイルを保存
                        var utf8WithoutBom = new System.Text.UTF8Encoding(false);
                        File.WriteAllText(filePath, fallbackContent, utf8WithoutBom);
                    }
                }
            }
        }

        #region リソース読み込みヘルパー

        /// <summary>
        /// 埋め込みリソースからテキストを読み込む
        /// </summary>
        /// <param name="resourceName">リソース名</param>
        /// <returns>リソースの内容</returns>
        private string GetEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fullResourceName = $"portableClipboard.Resources.{resourceName}";
            
            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"埋め込みリソース '{fullResourceName}' が見つかりません。");
                }
                
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        #endregion

        #region ファイル内容取得メソッド

        /// <summary>
        /// code.pyの内容を取得
        /// </summary>
        /// <returns>code.pyの内容</returns>
        private string GetCodePyContent()
        {
            try
            {
                return GetEmbeddedResource("code.py");
            }
            catch (Exception ex)
            {
                // フォールバック：ハードコーディングされた内容
                Console.WriteLine($"リソースからcode.pyを読み込めませんでした: {ex.Message}");
                return GetFallbackCodePyContent();
            }
        }

        /// <summary>
        /// config.jsonの内容を取得
        /// </summary>
        /// <returns>config.jsonの内容</returns>
        private string GetConfigJsonContent()
        {
            try
            {
                return GetEmbeddedResource("config.json");
            }
            catch (Exception ex)
            {
                // フォールバック：ハードコーディングされた内容
                Console.WriteLine($"リソースからconfig.jsonを読み込めませんでした: {ex.Message}");
                return GetFallbackConfigJsonContent();
            }
        }

        #endregion

        #region フォールバック用ファイル内容

        /// <summary>
        /// フォールバック用code.pyの内容を取得
        /// </summary>
        private string GetFallbackCodePyContent()
        {
            return @"import time
import usb_hid
from adafruit_hid.keyboard import Keyboard
from adafruit_hid.keycode import Keycode
from adafruit_hid.keyboard_layout_us import KeyboardLayoutUS
import digitalio
import board
import json

# 設定ファイルを読み込み
try:
    with open('config.json', 'r') as f:
        config = json.load(f)
        typing_delay = config.get('typing_delay', 0.01)
        startup_delay = config.get('startup_delay', 3)
except Exception as e:
    typing_delay = 0.01
    startup_delay = 3

# 初期化
print('Clipboard Pico starting...')
time.sleep(startup_delay)

# キーボード初期化
kbd = Keyboard(usb_hid.devices)
layout = KeyboardLayoutUS(kbd)

# LEDピンの設定（任意）
led = digitalio.DigitalInOut(board.LED)
led.direction = digitalio.Direction.OUTPUT

def type_text(text):
    '''テキストをタイプする'''
    if not text:
        return
    
    try:
        layout.write(text)
        time.sleep(typing_delay)
    except Exception as e:
        print(f'Error typing text: {e}')

def read_slot_file(slot_number):
    '''スロットファイルを読み込む'''
    filename = f'slot{slot_number}.txt'
    try:
        with open(filename, 'r', encoding='utf-8') as f:
            return f.read().strip()
    except Exception as e:
        print(f'Error reading {filename}: {e}')
        return None

def main():
    '''メイン処理'''
    print('Ready! Press buttons to paste clipboard slots.')
    
    while True:
        # ここにボタン処理やファイル監視処理を追加
        # 現在は基本的な初期化のみ
        time.sleep(0.1)

if __name__ == '__main__':
    main()
";
        }

        /// <summary>
        /// フォールバック用config.jsonの内容を取得
        /// </summary>
        private string GetFallbackConfigJsonContent()
        {
            return @"{
  ""startup_delay"": 3,
  ""typing_delay"": 0.01,
  ""slots"": 10,
  ""slot_file_prefix"": ""slot"",
  ""slot_file_extension"": "".txt""
}";
        }

        /// <summary>
        /// フォールバック用adafruit_hidファイル内容を取得
        /// </summary>
        private string GetFallbackAdafruitHidContent(string fileName)
        {
            switch (fileName)
            {
                case "__init__.py":
                    return @"# SPDX-FileCopyrightText: 2017 Dan Halbert for Adafruit Industries
# SPDX-License-Identifier: MIT
import usb_hid

def find_device(devices, *, usage_page, usage):
    for device in devices:
        if device.usage_page == usage_page and device.usage == usage:
            return device
    raise ValueError(f'Could not find matching HID device.')
";

                case "keyboard.py":
                    return @"# SPDX-FileCopyrightText: 2017 Dan Halbert for Adafruit Industries  
# SPDX-License-Identifier: MIT
import time
import usb_hid
from . import find_device

class Keyboard:
    def __init__(self, devices):
        self._keyboard_device = find_device(devices, usage_page=0x1, usage=0x06)
        self.report = bytearray(8)
        
    def send(self, *keycodes):
        for i in range(len(self.report)):
            self.report[i] = 0
            
        modifier = 0
        keycode_indices = []
        
        for keycode in keycodes:
            if isinstance(keycode, int):
                if 0xE0 <= keycode <= 0xE7:
                    modifier |= 1 << (keycode - 0xE0)
                else:
                    keycode_indices.append(keycode)
        
        self.report[0] = modifier
        for i, keycode in enumerate(keycode_indices[:6]):
            self.report[i + 2] = keycode
            
        self._keyboard_device.send_report(self.report)
        
    def release_all(self):
        for i in range(len(self.report)):
            self.report[i] = 0
        self._keyboard_device.send_report(self.report)
";

                case "keycode.py":
                    return @"# SPDX-FileCopyrightText: 2017 Dan Halbert for Adafruit Industries
# SPDX-License-Identifier: MIT

class Keycode:
    LEFT_CONTROL = 0xE0
    LEFT_SHIFT = 0xE1
    LEFT_ALT = 0xE2
    LEFT_GUI = 0xE3
    RIGHT_CONTROL = 0xE4
    RIGHT_SHIFT = 0xE5
    RIGHT_ALT = 0xE6
    RIGHT_GUI = 0xE7
    
    A = 0x04
    B = 0x05
    C = 0x06
    D = 0x07
    E = 0x08
    F = 0x09
    G = 0x0A
    H = 0x0B
    I = 0x0C
    J = 0x0D
    K = 0x0E
    L = 0x0F
    M = 0x10
    N = 0x11
    O = 0x12
    P = 0x13
    Q = 0x14
    R = 0x15
    S = 0x16
    T = 0x17
    U = 0x18
    V = 0x19
    W = 0x1A
    X = 0x1B
    Y = 0x1C
    Z = 0x1D
    
    ONE = 0x1E
    TWO = 0x1F
    THREE = 0x20
    FOUR = 0x21
    FIVE = 0x22
    SIX = 0x23
    SEVEN = 0x24
    EIGHT = 0x25
    NINE = 0x26
    ZERO = 0x27
    
    ENTER = 0x28
    ESCAPE = 0x29
    BACKSPACE = 0x2A
    TAB = 0x2B
    SPACE = 0x2C
    
    SHIFT = LEFT_SHIFT
    CONTROL = LEFT_CONTROL
    ALT = LEFT_ALT
    GUI = LEFT_GUI
";

                case "keyboard_layout_base.py":
                    return @"# SPDX-FileCopyrightText: 2017 Dan Halbert for Adafruit Industries
# SPDX-License-Identifier: MIT

class KeyboardLayoutBase:
    def __init__(self, keyboard):
        self.keyboard = keyboard
        
    def write(self, string):
        for char in string:
            keycode_sequence = self.char_to_keycode(char)
            if keycode_sequence:
                self.keyboard.send(*keycode_sequence)
                self.keyboard.release_all()
                
    def char_to_keycode(self, char):
        char_code = ord(char)
        if char_code < len(self.ASCII_TO_KEYCODE):
            return self.ASCII_TO_KEYCODE[char_code]
        return ()
";

                case "keyboard_layout_us.py":
                    return @"# SPDX-FileCopyrightText: 2017 Dan Halbert for Adafruit Industries
# SPDX-License-Identifier: MIT
from .keyboard_layout_base import KeyboardLayoutBase
from .keycode import Keycode

class KeyboardLayoutUS(KeyboardLayoutBase):
    ASCII_TO_KEYCODE = (
        (), (), (), (), (), (), (), (),
        (Keycode.BACKSPACE,), (Keycode.TAB,), (Keycode.ENTER,), (), (), (Keycode.ENTER,), (), (),
        (), (), (), (), (), (), (), (),
        (), (), (), (Keycode.ESCAPE,), (), (), (), (),
        (Keycode.SPACE,),
        (Keycode.ONE, Keycode.SHIFT),
        (Keycode.A,), (Keycode.B,), (Keycode.C,), (Keycode.D,), (Keycode.E,), (Keycode.F,),
        (Keycode.G,), (Keycode.H,), (Keycode.I,), (Keycode.J,), (Keycode.K,), (Keycode.L,),
        (Keycode.M,), (Keycode.N,), (Keycode.O,), (Keycode.P,), (Keycode.Q,), (Keycode.R,),
        (Keycode.S,), (Keycode.T,), (Keycode.U,), (Keycode.V,), (Keycode.W,), (Keycode.X,),
        (Keycode.Y,), (Keycode.Z,),
    )
";

                case "mouse.py":
                    return @"# SPDX-FileCopyrightText: 2017 Dan Halbert for Adafruit Industries
# SPDX-License-Identifier: MIT
class Mouse:
    def __init__(self, devices):
        pass
";

                case "consumer_control.py":
                    return @"# SPDX-FileCopyrightText: 2017 Dan Halbert for Adafruit Industries
# SPDX-License-Identifier: MIT
class ConsumerControl:
    def __init__(self, devices):
        pass
";

                case "consumer_control_code.py":
                    return @"# SPDX-FileCopyrightText: 2017 Dan Halbert for Adafruit Industries
# SPDX-License-Identifier: MIT
class ConsumerControlCode:
    MUTE = 0xE2
    VOLUME_INCREMENT = 0xE9
    VOLUME_DECREMENT = 0xEA
";

                default:
                    return "";
            }
        }

        #endregion
    }
}
