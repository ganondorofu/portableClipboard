import time
import board
import digitalio
import usb_hid
import json
from adafruit_hid.keyboard import Keyboard
from adafruit_hid.keyboard_layout_us import KeyboardLayoutUS
from adafruit_hid.keycode import Keycode

# Load settings from external configuration files
def load_jis_keymap():
    """Load JIS keyboard keymap from external file"""
    try:
        with open("/jis_keymap.json", "r", encoding="utf-8") as f:
            keymap_data = json.load(f)
            print("[DEBUG] jis_keymap.json loaded successfully")
            
            # Convert to Keycode objects
            jis_map = {}
            for category, mappings in keymap_data.items():
                print("[DEBUG] Category processing: " + category)
                for char, keycode_names in mappings.items():
                    keycodes = []
                    for name in keycode_names:
                        if hasattr(Keycode, name):
                            keycodes.append(getattr(Keycode, name))
                        else:
                            print("[WARNING] Unknown keycode: " + name)
                    if keycodes:
                        jis_map[char] = keycodes
                        print("[DEBUG] Mapping added: " + char)
            
            print("[DEBUG] JIS keymap loaded")
            print("[DEBUG] Total mappings loaded: " + str(len(jis_map)))
            print("[DEBUG] Checking colon mapping")
            if ':' in jis_map:
                print("[DEBUG] Colon mapping found: " + str(jis_map[':']))
            else:
                print("[DEBUG] Colon mapping NOT found!")
            print("[DEBUG] All available characters: " + str(list(jis_map.keys())))
            return jis_map
    except (OSError, ValueError) as e:
        print("[ERROR] jis_keymap.json load failed")
        print("[DEBUG] Using default JIS map")
        return get_default_jis_map()

def load_function_keys():
    """Load function key settings from external file"""
    try:
        with open("/function_keys.json", "r", encoding="utf-8") as f:
            func_data = json.load(f)
            print("[DEBUG] function_keys.json loaded successfully")
            
            # Convert to Keycode objects
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
                            print("[WARNING] Unknown keycode: " + keycode_name)
            
            print("[DEBUG] Function keys loaded")
            return keycode_map, valid_commands
    except (OSError, ValueError) as e:
        print("[ERROR] function_keys.json load failed")
        print("[DEBUG] Using default function keys")
        return get_default_function_keys()

def get_default_jis_map():
    """Default JIS map (fallback)"""
    return {
        "@": [Keycode.LEFT_BRACKET],
        "[": [Keycode.RIGHT_BRACKET],
        "]": [Keycode.BACKSLASH],
        "\"": [Keycode.SHIFT, Keycode.TWO],
        ":": [Keycode.QUOTE],
        ";": [Keycode.SEMICOLON]
    }

def get_default_function_keys():
    """Default function keys (fallback)"""
    keycode_map = {
        'enter': Keycode.ENTER, 'f1': Keycode.F1, 'ctrl': Keycode.CONTROL
    }
    valid_commands = {'enter', 'f1', 'ctrl', 'ctrl_down', 'ctrl_up'}
    return keycode_map, valid_commands

# Load settings
print("[INIT] Loading external configurations...")
JIS_KEYCODE_MAP = load_jis_keymap()
FUNCTION_KEYCODE_MAP, VALID_COMMANDS = load_function_keys()

# Configuration loading function
def load_config():
    try:
        with open("/config.json", "r", encoding="utf-8") as f:
            config = json.load(f)
            print("[DEBUG] config.json loaded successfully")
            print("[DEBUG] Configuration loaded")
            return config
    except (OSError, ValueError) as e:
        print("[ERROR] config.json load failed")
        raise

# Load configuration
print("[INIT] Loading configuration...")
config = load_config()
print("[INIT] Configuration loaded")

# Initialize keyboard output
print("[INIT] Initializing keyboard...")
try:
    keyboard = Keyboard(usb_hid.devices)
    layout = KeyboardLayoutUS(keyboard)
    print("[INIT] Keyboard initialization successful")
except Exception as e:
    print("[INIT ERROR] Keyboard initialization failed")
    raise

print("[INIT] Setting up GPIO pins...")
# Button GPIO configuration
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
    print("[INIT ERROR] Button GPIO setup failed")
    raise

# LED GPIO configuration
try:
    led_pins = [board.GP0, board.GP1, board.GP2, board.GP3, board.GP4]
    led_pin_numbers = [0, 1, 2, 3, 4]
    leds = []
    for i, (pin, pin_num) in enumerate(zip(led_pins, led_pin_numbers)):
        led = digitalio.DigitalInOut(pin)
        led.direction = digitalio.Direction.OUTPUT
        led.value = False
        leds.append(led)
        print("[INIT] LED configured")
    print("[INIT] All LEDs configured")
except Exception as e:
    print("[INIT ERROR] LED GPIO setup failed")
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
            print("[DEBUG] File loaded successfully")
            return content
    except OSError:
        print("[DEBUG] File load failed")
        return None

def update_leds(slot):
    for i, led in enumerate(leds, start=1):
        led.value = (i == slot)
    print("[DEBUG] LED update completed")

def button_pressed(button, last_state, name=""):
    current_state = button.value
    pressed = (last_state and not current_state)
    if pressed:
        print("[DEBUG] Button press detected")
    return pressed, current_state

def send_character(char):
    """Send character with keyboard type support"""
    japanese_keyboard = config.get('japanese_keyboard', True)
    
    print("[DEBUG] send_character called for: " + char)
    print("[DEBUG] japanese_keyboard: " + str(japanese_keyboard))
    
    if japanese_keyboard and char in JIS_KEYCODE_MAP:
        # JIS symbol on Japanese keyboard
        keycodes = JIS_KEYCODE_MAP[char]
        print("[DEBUG] Using JIS map for: " + char)
        print("[DEBUG] Keycodes: " + str(keycodes))
        keyboard.send(*keycodes)
    else:
        print("[DEBUG] Using layout.write for: " + char)
        if char == ':':
            print("[DEBUG] COLON not found in JIS_KEYCODE_MAP!")
            print("[DEBUG] Available keys: " + str(list(JIS_KEYCODE_MAP.keys())))
        layout.write(char)

def convert_text_symbols(text):
    """Symbol conversion for English keyboard (direct output)"""
    return text

def find_brace_commands(text):
    """Manually search for {command} patterns"""
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
    """Check if command is valid (external settings support)"""
    command_lower = command.lower()
    
    # Check valid commands from external settings
    if command_lower in VALID_COMMANDS:
        return True
    
    # Check delay_n format
    if command_lower.startswith('delay_'):
        delay_part = command_lower[6:]
        try:
            delay_ms = int(delay_part)
            return delay_ms >= 0
        except ValueError:
            return False
    
    return False

def convert_for_japanese_keyboard_smart(text):
    """Symbol conversion while protecting function keys"""
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
    """Get Keycode from command (external settings support)"""
    command_lower = command.lower()
    
    # Check function key mapping from external settings
    return FUNCTION_KEYCODE_MAP.get(command_lower)

def process_function_keys(text):
    """Process function keys and break into tokens"""
    commands = find_brace_commands(text)
    tokens = []
    last_end = 0
    
    for start, end, command in commands:
        # Normal text before command
        if start > last_end:
            tokens.append(('text', text[last_end:start]))
        
        command_lower = command.lower()
        
        # Process delay_n format
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
        # Modifier key processing
        elif command_lower.endswith('_down'):
            base_key = command_lower.replace('_down', '')
            keycode = get_keycode_from_command(base_key)
            if keycode:
                tokens.append(('modifier_down', keycode))
                print(f"[DEBUG] Modifier key press recognized: {command}")
            else:
                tokens.append(('text', '{' + command + '}'))
                print(f"[DEBUG] Invalid modifier key->text: {{{command}}}")
        elif command_lower.endswith('_up'):
            base_key = command_lower.replace('_up', '')
            keycode = get_keycode_from_command(base_key)
            if keycode:
                tokens.append(('modifier_up', keycode))
                print(f"[DEBUG] Modifier key release recognized: {command}")
            else:
                tokens.append(('text', '{' + command + '}'))
                print(f"[DEBUG] Invalid modifier key->text: {{{command}}}")
        # Single key processing
        else:
            keycode = get_keycode_from_command(command)
            if keycode:
                tokens.append(('single_key', keycode))
                print(f"[DEBUG] Single key recognized: {command}")
            else:
                tokens.append(('text', '{' + command + '}'))
                print(f"[DEBUG] Invalid command->text: {{{command}}}")
        
        last_end = end
    
    # Remaining text
    if last_end < len(text):
        tokens.append(('text', text[last_end:]))
    
    return tokens

def send_tokens(tokens, typing_delay, add_final_enter=False):
    """Send tokens in sequence"""
    
    for token_type, content in tokens:
        try:
            if token_type == 'text':
                for char in content:
                    if char == '\n':
                        if add_final_enter:
                            keyboard.send(Keycode.ENTER)
                            print(f"[DEBUG] Newline sent")
                        else:
                            continue  # Ignore newline character
                    else:
                        send_character(char)
                    time.sleep(typing_delay)
            
            elif token_type == 'modifier_down':
                keyboard.press(content)
                print(f"[DEBUG] Key press: {content}")
            
            elif token_type == 'modifier_up':
                keyboard.release(content)
                print(f"[DEBUG] Key release: {content}")
            
            elif token_type == 'single_key':
                keyboard.send(content)
                print(f"[DEBUG] Single key: {content}")
                time.sleep(typing_delay)
            
            elif token_type == 'delay':
                delay_seconds = content / 1000.0
                time.sleep(delay_seconds)
        
        except Exception as e:
            print(f"[ERROR] Token error: {e}")
            continue

def send_text_with_speed(text):
    """Send text at configured speed"""
    typing_delay = config['typing_delay']
    enable_modifier_keys = config.get('enable_modifier_keys', False)
    add_final_enter = config.get('add_final_enter', False)
    japanese_keyboard = config.get('japanese_keyboard', True)
    
    # Extract ASCII characters only
    ascii_chars = []
    for char in text:
        if char == '\n' or ord(char) <= 127:
            ascii_chars.append(char)
    
    processed_text = ''.join(ascii_chars)
    
    # Symbol conversion only for English keyboard
    if not japanese_keyboard and enable_modifier_keys:
        processed_text = convert_for_japanese_keyboard_smart(processed_text)
    elif not japanese_keyboard:
        processed_text = convert_text_symbols(processed_text)
    
    # Function key processing
    if enable_modifier_keys:
        tokens = process_function_keys(processed_text)
        send_tokens(tokens, typing_delay, add_final_enter)
    else:
        print("[DEBUG] Normal mode - Simple character sending")
        # Simple character sending
        for char in processed_text:
            try:
                if char == '\n':
                    if add_final_enter:
                        keyboard.send(Keycode.ENTER)
                        print("[DEBUG] Enter sent for newline processing")
                    else:
                        print("[DEBUG] Ignoring newline character")
                        continue
                else:
                    send_character(char)
                time.sleep(typing_delay)
            except Exception as e:
                print(f"[ERROR] Character send error: {char} - {e}")
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
            if loop_count % 1000 == 0:
                print(f"[HEARTBEAT] Loop count: {loop_count}")
            
            pressed_next, last_next_state = button_pressed(button_next, last_next_state, "GP11 (Next Button)")
            pressed_send, last_send_state = button_pressed(button_send, last_send_state, "GP14 (Send Button)")

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
                    print(f"[ERROR] {filename} not found")
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

# Startup message
print("=== portableClipboard Starting ===")
print("CircuitPython Version Check...")

try:
    print("Initializing hardware...")
    # Execute
    main()
except Exception as e:
    print(f"FATAL ERROR: {e}")
    import traceback
    traceback.print_exception(type(e), e, e.__traceback__)
    while True:
        # Flash all LEDs on error
        for led in leds:
            led.value = True
        time.sleep(0.5)
        for led in leds:
            led.value = False
        time.sleep(0.5)
