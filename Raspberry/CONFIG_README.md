# 外部設定ファイル説明書

## 📁 設定ファイル構成

### 🔧 `jis_keymap.json` - JIS配列記号マッピング
JIS配列キーボードでの記号送信用のキーコードマッピングを定義

```json
{
  "basic_symbols": {
    "@": ["LEFT_BRACKET"],     // @マークはLeftBracketキーで送信
    ":": ["QUOTE"]             // コロンはQuoteキーで送信
  },
  "shift_symbols": {
    "\"": ["SHIFT", "TWO"],    // ダブルクォートはShift+2で送信
    "&": ["SHIFT", "SIX"]      // アンパサンドはShift+6で送信
  }
}
```

### 🎯 `function_keys.json` - 機能キー設定
{command}形式で使用できる機能キーとコマンドを定義

```json
{
  "function_keys": {
    "f1": "F1",                // {f1}でF1キー送信
    "f12": "F12"               // {f12}でF12キー送信
  },
  "navigation_keys": {
    "enter": "ENTER",          // {enter}でEnterキー送信
    "tab": "TAB"               // {tab}でTabキー送信
  },
  "valid_commands": [
    "ctrl_down", "ctrl_up",    // {ctrl_down}, {ctrl_up}が有効
    "f1", "enter"              // {f1}, {enter}が有効
  ]
}
```

## ⚙️ カスタマイズ方法

### 1. **新しい記号を追加**
`jis_keymap.json`に記号とキーコードの組み合わせを追加

```json
{
  "basic_symbols": {
    "新しい記号": ["KEYCODE_NAME"]
  }
}
```

### 2. **新しい機能キーを追加**
`function_keys.json`に機能キーを追加

```json
{
  "custom_keys": {
    "print": "PRINT_SCREEN",   // {print}でPrintScreenキー
    "menu": "APPLICATION"      // {menu}でApplicationキー
  },
  "valid_commands": [
    "print", "menu"            // 有効コマンドリストに追加
  ]
}
```

### 3. **キーコード一覧**
使用可能なKeycode名:
- **文字キー**: `A`, `B`, `C`...`Z`, `ONE`, `TWO`...`ZERO`
- **修飾キー**: `SHIFT`, `CONTROL`, `ALT`, `WINDOWS`
- **ファンクション**: `F1`, `F2`...`F12`
- **ナビゲーション**: `ENTER`, `TAB`, `ESCAPE`, `SPACE`, `BACKSPACE`
- **矢印キー**: `UP_ARROW`, `DOWN_ARROW`, `LEFT_ARROW`, `RIGHT_ARROW`
- **記号キー**: `MINUS`, `EQUALS`, `LEFT_BRACKET`, `RIGHT_BRACKET`, `BACKSLASH`, `SEMICOLON`, `QUOTE`, `GRAVE_ACCENT`, `COMMA`, `PERIOD`, `SLASH`

## 🔄 設定の反映
1. ファイルを編集後、Raspberry Pi Picoを再起動
2. デバッグメッセージで読み込み状況を確認
3. エラーがある場合はデフォルト設定にフォールバック

## 🐛 トラブルシューティング
- **設定ファイルが読めない**: JSON形式を確認
- **キーコードが無効**: 使用可能なKeycode名を確認
- **コマンドが動作しない**: `valid_commands`リストに追加されているか確認
