import time
import board
import digitalio
import usb_hid
import json
from adafruit_hid.keyboard import Keyboard
from adafruit_hid.keyboard_layout_us import KeyboardLayoutUS
from adafruit_hid.keycode import Keycode

# 外部設定ファイルから設定を読み込む
def load_jis_keymap():
    """JIS配列キーマップを外部ファイルから読み込み"""
    try:
        with open("/jis_keymap.json", "r", encoding="utf-8") as f:
            keymap_data = json.load(f)
            print("[DEBUG] jis_keymap.json 読み込み成功")
            
            # Keycodeオブジェクトに変換
            jis_map = {}
            for category, mappings in keymap_data.items():
                for char, keycode_names in mappings.items():
                    keycodes = []
                    for name in keycode_names:
                        if hasattr(Keycode, name):
                            keycodes.append(getattr(Keycode, name))
                        else:
                            print(f"[WARNING] Unknown keycode: {name}")
                    if keycodes:
                        jis_map[char] = keycodes
            
            print(f"[DEBUG] JIS keymap loaded: {len(jis_map)} mappings")
            return jis_map
    except (OSError, ValueError) as e:
        print(f"[ERROR] jis_keymap.json の読み込み失敗: {e}")
        print("[DEBUG] デフォルトJISマップを使用")
        return get_default_jis_map()

def load_function_keys():
    """機能キー設定を外部ファイルから読み込み"""
    try:
        with open("/function_keys.json", "r", encoding="utf-8") as f:
            func_data = json.load(f)
            print("[DEBUG] function_keys.json 読み込み成功")
            
            # Keycodeオブジェクトに変換
            keycode_map = {}
            valid_commands = set()
            
            for category, mappings in func_data.items():
                if category == "valid_commands":
                    valid_commands.update(mappings)
                else:
                    for command, keycode_name in mappings.items():
                        if hasattr(Keycode, keycode_name):
                            keycode_map[command] = getattr(Keycode, keycode_name)
                        else:
                            print(f"[WARNING] Unknown keycode: {keycode_name}")
            
            print(f"[DEBUG] Function keys loaded: {len(keycode_map)} mappings, {len(valid_commands)} valid commands")
            return keycode_map, valid_commands
    except (OSError, ValueError) as e:
        print(f"[ERROR] function_keys.json の読み込み失敗: {e}")
        print("[DEBUG] デフォルト機能キーを使用")
        return get_default_function_keys()

def get_default_jis_map():
    """デフォルトJISマップ（フォールバック用）"""
    return {
        "@": [Keycode.LEFT_BRACKET],
        "[": [Keycode.RIGHT_BRACKET],
        "]": [Keycode.BACKSLASH],
        "\"": [Keycode.SHIFT, Keycode.TWO],
        ":": [Keycode.QUOTE],
        ";": [Keycode.SEMICOLON]
    }

def get_default_function_keys():
    """デフォルト機能キー（フォールバック用）"""
    keycode_map = {
        'enter': Keycode.ENTER, 'f1': Keycode.F1, 'ctrl': Keycode.CONTROL
    }
    valid_commands = {'enter', 'f1', 'ctrl', 'ctrl_down', 'ctrl_up'}
    return keycode_map, valid_commands

# 設定を読み込み
print("[INIT] Loading external configurations...")
JIS_KEYCODE_MAP = load_jis_keymap()
FUNCTION_KEYCODE_MAP, VALID_COMMANDS = load_function_keys()

# 設定読み込み関数
def load_config():
    try:
        with open("/config.json", "r", encoding="utf-8") as f:
            config = json.load(f)
            print("[DEBUG] config.json 読み込み成功")
            print(f"[DEBUG] 設定内容: {config}")
            return config
    except (OSError, ValueError) as e:
        print(f"[ERROR] config.json の読み込み失敗: {e}")
        raise

# 設定をロード
print("[INIT] Loading configuration...")
config = load_config()
print(f"[INIT] Configuration loaded: {config}")

# キーボード出力の初期化
print("[INIT] Initializing keyboard...")
try:
    keyboard = Keyboard(usb_hid.devices)
    layout = KeyboardLayoutUS(keyboard)
    print("[INIT] Keyboard initialization successful")
except Exception as e:
    print(f"[INIT ERROR] Keyboard initialization failed: {e}")
    raise

print("[INIT] Setting up GPIO pins...")
# ボタン用GPIOの設定
try:
    button_next = digitalio.DigitalInOut(board.GP11)
    button_next.direction = digitalio.Direction.INPUT
    button_next.pull = digitalio.Pull.UP
    print("[INIT] GP11 button configured")

    button_send = digitalio.DigitalInOut(board.GP14)
    button_send.direction = digitalio.Direction.INPUT
    button_send.pull = digitalio.Pull.UP
    print("[INIT] GP14 button configured")
except Exception as e:
    print(f"[INIT ERROR] Button GPIO setup failed: {e}")
    raise

# LED用GPIOの設定
try:
    led_pins = [board.GP0, board.GP1, board.GP2, board.GP3, board.GP4]
    led_pin_numbers = [0, 1, 2, 3, 4]
    leds = []
    for i, (pin, pin_num) in enumerate(zip(led_pins, led_pin_numbers)):
        led = digitalio.DigitalInOut(pin)
        led.direction = digitalio.Direction.OUTPUT
        led.value = False
        leds.append(led)
        print(f"[INIT] LED {i+1} (GP{pin_num}) configured")
    print("[INIT] All LEDs configured")
except Exception as e:
    print(f"[INIT ERROR] LED GPIO setup failed: {e}")
    raise

print("[INIT] Hardware initialization complete")

current_slot = 1

def read_file(filepath):
    try:
        with open(filepath, "r", encoding="utf-8") as f:
            content = f.read()
            if content.startswith('\ufeff'):
                content = content[1:]
            content = content.replace('\r\n', '\n').replace('\r', '\n')
            print(f"[DEBUG] {filepath} 読み込み成功。内容長さ: {len(content)}")
            return content
    except OSError:
        print(f"[DEBUG] {filepath} の読み込み失敗")
        return None

def update_leds(slot):
    for i, led in enumerate(leds, start=1):
        led.value = (i == slot)
    print(f"[DEBUG] LED更新: スロット{slot}のLEDが点灯")

def button_pressed(button, last_state, name=""):
    current_state = button.value
    pressed = (last_state and not current_state)
    if pressed:
        print(f"[DEBUG] ボタン押下検出 ({name})")
    return pressed, current_state

def send_character(char):
    """キーボード種別対応の文字送信"""
    japanese_keyboard = config.get('japanese_keyboard', True)  # falseで英字KB、trueで日本語KB
    
    if japanese_keyboard and char in JIS_KEYCODE_MAP:
        # 日本語キーボードでJIS記号の場合
        keycodes = JIS_KEYCODE_MAP[char]
        keyboard.send(*keycodes)
    else:
        layout.write(char)

def convert_text_symbols(text):
    """英字キーボード用記号変換（そのまま出力）"""
    return text

def find_brace_commands(text):
    """手動で{command}パターンを検索"""
    matches = []
    i = 0
    while i < len(text):
        start = text.find('{', i)
        if start == -1:
            break
        end = text.find('}', start)
        if end == -1:
            break
        command = text[start+1:end]
        matches.append((start, end+1, command))
        i = end + 1
    return matches

def is_valid_key_command(command):
    """有効なキーコマンドかチェック（外部設定対応）"""
    command_lower = command.lower()
    
    # 外部設定から読み込んだ有効コマンドをチェック
    if command_lower in VALID_COMMANDS:
        return True
    
    # delay_n形式のチェック
    if command_lower.startswith('delay_'):
        delay_part = command_lower[6:]
        try:
            delay_ms = int(delay_part)
            return delay_ms >= 0
        except ValueError:
            return False
    
    return False

def convert_for_japanese_keyboard_smart(text):
    """機能キーを保護しながら記号変換"""
    commands = find_brace_commands(text)
    result = ""
    last_end = 0
    
    for start, end, command in commands:
        if start > last_end:
            normal_text = text[last_end:start]
            result += normal_text
        
        if is_valid_key_command(command):
            result += '{' + command + '}'
        else:
            result += '{' + command + '}'
        
        last_end = end
    
    if last_end < len(text):
        remaining_text = text[last_end:]
        result += remaining_text
    
    return result

def get_keycode_from_command(command):
    """コマンドからKeycodeを取得（外部設定対応）"""
    command_lower = command.lower()
    
    # 外部設定から読み込んだ機能キーマップをチェック
    return FUNCTION_KEYCODE_MAP.get(command_lower)

def process_function_keys(text):
    """機能キーを処理してトークンに分解"""
    commands = find_brace_commands(text)
    tokens = []
    last_end = 0
    
    for start, end, command in commands:
        # コマンド前の通常テキスト
        if start > last_end:
            tokens.append(('text', text[last_end:start]))
        
        command_lower = command.lower()
        
        # delay_n形式の処理
        if command_lower.startswith('delay_'):
            delay_part = command_lower[6:]
            try:
                delay_ms = int(delay_part)
                if delay_ms >= 0:
                    tokens.append(('delay', delay_ms))
                else:
                    tokens.append(('text', '{' + command + '}'))
            except ValueError:
                tokens.append(('text', '{' + command + '}'))
        # 修飾キーの処理
        elif command_lower.endswith('_down'):
            base_key = command_lower.replace('_down', '')
            keycode = get_keycode_from_command(base_key)
            if keycode:
                tokens.append(('modifier_down', keycode))
                print(f"[DEBUG] 修飾キー押下認識: {command}")
            else:
                tokens.append(('text', '{' + command + '}'))
                print(f"[DEBUG] 無効修飾キー→文字: {{{command}}}")
        elif command_lower.endswith('_up'):
            base_key = command_lower.replace('_up', '')
            keycode = get_keycode_from_command(base_key)
            if keycode:
                tokens.append(('modifier_up', keycode))
                print(f"[DEBUG] 修飾キー解除認識: {command}")
            else:
                tokens.append(('text', '{' + command + '}'))
                print(f"[DEBUG] 無効修飾キー→文字: {{{command}}}")
        # 単発キーの処理
        else:
            keycode = get_keycode_from_command(command)
            if keycode:
                tokens.append(('single_key', keycode))
                print(f"[DEBUG] 単発キー認識: {command}")
            else:
                tokens.append(('text', '{' + command + '}'))
                print(f"[DEBUG] 無効コマンド→文字: {{{command}}}")
        
        last_end = end
    
    # 残りのテキスト
    if last_end < len(text):
        tokens.append(('text', text[last_end:]))
    
    return tokens

def send_tokens(tokens, typing_delay, add_final_enter=False):
    """トークンを順番に送信"""
    
    for token_type, content in tokens:
        try:
            if token_type == 'text':
                for char in content:
                    if char == '\n':
                        if add_final_enter:
                            keyboard.send(Keycode.ENTER)
                            print(f"[DEBUG] 改行送信")
                        else:
                            continue  # 改行文字を無視
                    else:
                        send_character(char)
                    time.sleep(typing_delay)
            
            elif token_type == 'modifier_down':
                keyboard.press(content)
                print(f"[DEBUG] キー押下: {content}")
            
            elif token_type == 'modifier_up':
                keyboard.release(content)
                print(f"[DEBUG] キー解除: {content}")
            
            elif token_type == 'single_key':
                keyboard.send(content)
                print(f"[DEBUG] 単発キー: {content}")
                time.sleep(typing_delay)
            
            elif token_type == 'delay':
                delay_seconds = content / 1000.0
                time.sleep(delay_seconds)
        
        except Exception as e:
            print(f"[ERROR] Token error: {e}")
            continue

def send_text_with_speed(text):
    """設定された速度でテキストを送信"""
    typing_delay = config['typing_delay']
    enable_modifier_keys = config.get('enable_modifier_keys', False)
    add_final_enter = config.get('add_final_enter', False)
    japanese_keyboard = config.get('japanese_keyboard', True)  # falseで英字KB、trueで日本語KB
    
    # ASCII文字のみを抽出
    ascii_chars = []
    for char in text:
        if char == '\n' or ord(char) <= 127:
            ascii_chars.append(char)
    
    processed_text = ''.join(ascii_chars)
    
    # 英字キーボードの場合のみ記号変換
    if not japanese_keyboard and enable_modifier_keys:
        processed_text = convert_for_japanese_keyboard_smart(processed_text)
    elif not japanese_keyboard:
        processed_text = convert_text_symbols(processed_text)
    
    # 機能キー処理
    if enable_modifier_keys:
        tokens = process_function_keys(processed_text)
        send_tokens(tokens, typing_delay, add_final_enter)
    else:
        print("[DEBUG] 通常モード - シンプル文字送信")
        # シンプルな文字送信
        for char in processed_text:
            try:
                if char == '\n':
                    if add_final_enter:
                        keyboard.send(Keycode.ENTER)
                        print("[DEBUG] 改行処理により Enter送信")
                    else:
                        print("[DEBUG] 改行文字を無視")
                        continue
                else:
                    send_character(char)
                time.sleep(typing_delay)
            except Exception as e:
                print(f"[ERROR] 文字送信エラー: {char} - {e}")
                continue

def main():
    global current_slot
    print("[MAIN] Starting main function...")
    
    try:
        startup_delay = config['startup_delay']
        print(f"[MAIN] Waiting {startup_delay} seconds for USB HID recognition...")
        time.sleep(startup_delay)

        print("[MAIN] Initializing LEDs...")
        update_leds(current_slot)
        print(f"[MAIN] LED initialization complete. Current slot: {current_slot}")

        last_next_state = True
        last_send_state = True
        
        print("[MAIN] Entering main loop...")
        loop_count = 0

        while True:
            loop_count += 1
            if loop_count % 1000 == 0:  # 1000回ごとにハートビート
                print(f"[HEARTBEAT] Loop count: {loop_count}")
            
            pressed_next, last_next_state = button_pressed(button_next, last_next_state, "GP11（次へボタン）")
            pressed_send, last_send_state = button_pressed(button_send, last_send_state, "GP14（送信ボタン）")

            if pressed_next:
                current_slot += 1
                if current_slot > 5:
                    current_slot = 1
                update_leds(current_slot)
                print(f"[MAIN] Selected slot: {current_slot}")
                time.sleep(0.2)

            if pressed_send:
                filename = f"/slot{current_slot}.txt"
                print(f"[MAIN] Attempting to send {filename}...")
                text = read_file(filename)
                if text is None:
                    print(f"[ERROR] {filename} が見つかりません")
                else:
                    print(f"[MAIN] Sending content of {filename}...")
                    send_text_with_speed(text)
                    if not text.endswith('\n'):
                        keyboard.send(Keycode.ENTER)
                    print(f"[MAIN] Send complete for {filename}")
                time.sleep(0.2)

            time.sleep(0.05)
            
    except Exception as e:
        print(f"[MAIN ERROR] Exception in main: {e}")
        import traceback
        traceback.print_exception(type(e), e, e.__traceback__)
        raise

# 起動メッセージ
print("=== portableClipboard Starting ===")
print("CircuitPython Version Check...")

try:
    print("Initializing hardware...")
    # 実行
    main()
except Exception as e:
    print(f"FATAL ERROR: {e}")
    import traceback
    traceback.print_exception(type(e), e, e.__traceback__)
    while True:
        # エラー時は全LEDを点滅させる
        for led in leds:
            led.value = True
        time.sleep(0.5)
        for led in leds:
            led.value = False
        time.sleep(0.5)
