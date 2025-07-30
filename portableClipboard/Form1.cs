using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using portableClipboard.Controllers;
using portableClipboard.Models;
using portableClipboard.Utils;

namespace portableClipboard
{
    public partial class Form1 : Form
    {
        private readonly MainFormController _controller;
        private string _previousSlot = "";
        private UsbDrive _currentDrive;
        private bool _isExiting = false;

        public Form1()
        {
            InitializeComponent();
            
            _controller = new MainFormController();
            SetupEventHandlers();
            InitializeControls();
            SetupSystemTray();
        }

        /// <summary>
        /// コントローラーのイベントハンドラーを設定
        /// </summary>
        private void SetupEventHandlers()
        {
            _controller.UsbDrivesUpdated += OnUsbDrivesUpdated;
            _controller.SlotContentChanged += OnSlotContentChanged;
            _controller.ErrorOccurred += OnErrorOccurred;
            _controller.OperationSucceeded += OnOperationSucceeded;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void InitializeControls()
        {
            _controller.RefreshUsbDrives();
            InitializeComboBoxes();
            
            // イベントハンドラーを手動で登録
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
            textBox1.TextChanged += textBox1_TextChanged;
            
            // 設定値を初期読み込み
            LoadSettings();
        }

        /// <summary>
        /// スロット選択変更イベント
        /// </summary>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedSlot = comboBox1.Text;
            string previousContent = textBox1.Text;

            _controller.SelectSlot(selectedSlot, _currentDrive, _previousSlot, previousContent);
            _previousSlot = selectedSlot;
        }

        /// <summary>
        /// コンボボックスの初期化
        /// </summary>
        private void InitializeComboBoxes()
        {
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;

            var slotNumbers = _controller.GetAvailableSlotNumbers();
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(slotNumbers.ToArray());

            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
                _previousSlot = comboBox1.Text;
            }
        }

        /// <summary>
        /// 再読み込みボタンクリックイベント
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            string currentSlot = comboBox1.Text;
            bool isModified = textBox1.Modified;

            _controller.ReloadSlotFromFile(currentSlot, _currentDrive, isModified);
        }

        /// <summary>
        /// 保存ボタンクリックイベント
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            string currentSlot = comboBox1.Text;
            string content = textBox1.Text;

            // 日本語キーボードでの入力不可文字チェック
            bool isJapaneseKeyboard = checkBox3.Checked;
            if (!string.IsNullOrEmpty(content) && isJapaneseKeyboard && TextValidationHelper.ContainsUntypableCharactersForJIS(content))
            {
                string message = MessageHelper.CreateJISUntypableCharsSaveWarning(currentSlot);

                DialogResult result = MessageBox.Show(
                    message,
                    "日本語キーボード非対応文字検出",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    return; // 保存をキャンセル
                }
            }

            // 非ASCII文字チェック
            if (!string.IsNullOrEmpty(content) && TextValidationHelper.ContainsNonAsciiCharacters(content))
            {
                string message = MessageHelper.CreateNonAsciiCharsSaveWarning(currentSlot);

                DialogResult result = MessageBox.Show(
                    message,
                    "非ASCII文字検出",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.No)
                {
                    return; // 保存をキャンセル
                }
            }

            _controller.SaveSlotToFile(currentSlot, content, _currentDrive);
        }

        /// <summary>
        /// USBドライブ更新ボタンクリックイベント
        /// </summary>
        private void button1_Click_1(object sender, EventArgs e)
        {
            _controller.RefreshUsbDrives();
        }

        /// <summary>
        /// 全削除ボタンクリックイベント
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            _controller.ClearAllSlots(_currentDrive);
        }

        /// <summary>
        /// プログラム書き込みボタンクリックイベント
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            if (_currentDrive != null && _currentDrive.IsValid)
            {
                // 現在のテキストボックスの内容をチェック
                bool hasNonAscii = false;
                bool hasUntypableChars = false;
                bool isJapaneseKeyboard = checkBox3.Checked;
                string currentSlot = comboBox2.SelectedItem?.ToString() ?? "1";
                
                if (!string.IsNullOrEmpty(textBox1.Text))
                {
                    hasNonAscii = TextValidationHelper.ContainsNonAsciiCharacters(textBox1.Text);
                    hasUntypableChars = isJapaneseKeyboard && TextValidationHelper.ContainsUntypableCharactersForJIS(textBox1.Text);
                }
                
                // USBドライブ内の既存スロットもチェック
                var nonAsciiSlots = _controller.CheckNonAsciiCharacters(_currentDrive.Path);
                var untypableSlots = isJapaneseKeyboard ? _controller.CheckUntypableCharactersForJIS(_currentDrive.Path) : new List<string>();
                
                string confirmMessage = MessageHelper.CreateProgramWriteWarning(
                    hasUntypableChars, untypableSlots, currentSlot, hasNonAscii, nonAsciiSlots);

                // 確認ダイアログを表示
                DialogResult result = MessageBox.Show(
                    confirmMessage,
                    "プログラム書き込み確認",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    _controller.InitializePicoFiles(_currentDrive.Path);
                }
            }
            else
            {
                MessageBox.Show("有効なUSBドライブが選択されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 起動遅延テキストボックス変更イベント（textBox2）
        /// </summary>
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // 現在選択されているUSBドライブに設定を保存
            if (_currentDrive != null && _currentDrive.IsValid)
            {
                _controller.SaveBootDelaySetting(_currentDrive.Path, textBox2.Text);
            }
        }

        /// <summary>
        /// タイピング間隔テキストボックス変更イベント（textBox3）
        /// </summary>
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // 現在選択されているUSBドライブに設定を保存
            if (_currentDrive != null && _currentDrive.IsValid)
            {
                _controller.SaveTypingDelaySetting(_currentDrive.Path, textBox3.Text);
            }
        }

        /// <summary>
        /// 現在のUSBドライブから設定を読み込み
        /// </summary>
        private void LoadSettings()
        {
            if (_currentDrive != null && _currentDrive.IsValid)
            {
                // 起動遅延設定を読み込み（textBox2）
                string bootDelay = _controller.GetBootDelaySetting(_currentDrive.Path);
                textBox2.Text = bootDelay;

                // タイピング間隔設定を読み込み（textBox3）
                string typingDelay = _controller.GetTypingDelaySetting(_currentDrive.Path);
                textBox3.Text = typingDelay;

                // 改行処理設定を読み込み（checkBox1）
                bool addFinalEnter = _controller.GetAddFinalEnterSetting(_currentDrive.Path);
                checkBox1.Checked = addFinalEnter;

                // 修飾キー機能設定を読み込み（checkBox2）
                bool enableModifierKeys = _controller.GetModifierKeysSetting(_currentDrive.Path);
                checkBox2.Checked = enableModifierKeys;

                // 日本語キーボード設定を読み込み（checkBox3）
                bool japaneseKeyboard = _controller.GetJapaneseKeyboardSetting(_currentDrive.Path);
                checkBox3.Checked = japaneseKeyboard;
            }
            else
            {
                // デフォルト値を設定
                textBox2.Text = "3";      // 起動遅延（秒）
                textBox3.Text = "0.01";   // タイピング間隔（秒）
                checkBox1.Checked = false; // 改行処理（デフォルト：無効）
                checkBox2.Checked = false; // 修飾キー機能（デフォルト：無効）
                checkBox3.Checked = false; // 日本語キーボード（デフォルト：無効=英字キーボード）
            }
        }

        #region コントローラーイベントハンドラー

        /// <summary>
        /// USBドライブ更新時の処理
        /// </summary>
        private void OnUsbDrivesUpdated(List<UsbDrive> drives)
        {
            comboBox2.Items.Clear();

            if (drives.Count == 0)
            {
                comboBox2.Items.Add(UsbDrive.CreateInvalidDrive());
                comboBox2.Enabled = false;
                _currentDrive = null;
            }
            else
            {
                foreach (var drive in drives)
                {
                    comboBox2.Items.Add(drive);
                }
                comboBox2.Enabled = true;
            }

            if (comboBox2.Items.Count > 0)
            {
                comboBox2.SelectedIndex = 0;
                _currentDrive = comboBox2.SelectedItem as UsbDrive;
                
                // USBドライブが更新されたら設定を再読み込み
                LoadSettings();
            }
        }

        /// <summary>
        /// スロット内容変更時の処理
        /// </summary>
        private void OnSlotContentChanged(string content)
        {
            textBox1.Text = content;
            textBox1.Modified = false;
        }

        /// <summary>
        /// エラー発生時の処理
        /// </summary>
        private void OnErrorOccurred(string error)
        {
            MessageBox.Show(error, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 操作成功時の処理
        /// </summary>
        private void OnOperationSucceeded(string message)
        {
            // 必要に応じてステータスバー等で通知
            textBox1.Modified = false;
        }

        #endregion

        /// <summary>
        /// USBドライブ選択変更イベント
        /// </summary>
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentDrive = comboBox2.SelectedItem as UsbDrive;
            
            // USBドライブが変更されたら設定を再読み込み
            LoadSettings();
        }

        /// <summary>
        /// テキストボックス内容変更イベント
        /// </summary>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_previousSlot))
            {
                _controller.UpdateSlotContent(_previousSlot, textBox1.Text);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // 現在選択されているUSBドライブに改行処理設定を保存
            if (_currentDrive != null && _currentDrive.IsValid)
            {
                _controller.SaveAddFinalEnterSetting(_currentDrive.Path, checkBox1.Checked);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            // 現在選択されているUSBドライブに最終改行設定を保存
            if (_currentDrive != null && _currentDrive.IsValid)
            {
                _controller.SaveModifierKeysSetting(_currentDrive.Path, checkBox2.Checked);
            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            // 現在選択されているUSBドライブに日本語キーボード設定を保存
            if (_currentDrive != null && _currentDrive.IsValid)
            {
                _controller.SaveJapaneseKeyboardSetting(_currentDrive.Path, checkBox3.Checked);
            }
        }

        #region タスクトレイ関連

        /// <summary>
        /// システムトレイの設定
        /// </summary>
        private void SetupSystemTray()
        {
            try
            {
                // カスタムアイコンを読み込み
                string iconPath = System.IO.Path.Combine(Application.StartupPath, "icon.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    notifyIcon1.Icon = new System.Drawing.Icon(iconPath);
                    this.Icon = new System.Drawing.Icon(iconPath);
                }
                else
                {
                    // フォールバック: システムアイコンを使用
                    notifyIcon1.Icon = System.Drawing.SystemIcons.Application;
                    this.Icon = System.Drawing.SystemIcons.Application;
                }
            }
            catch (Exception)
            {
                // エラー時はシステムアイコンを使用
                notifyIcon1.Icon = System.Drawing.SystemIcons.Application;
                this.Icon = System.Drawing.SystemIcons.Application;
            }
            notifyIcon1.Text = "Portable Clipboard";
            notifyIcon1.Visible = true;
        }

        /// <summary>
        /// フォームのリサイズイベント - 最小化時にタスクトレイに隠す
        /// </summary>
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.ShowInTaskbar = false;
                notifyIcon1.ShowBalloonTip(2000, "Portable Clipboard", "アプリケーションはタスクトレイに最小化されました。", ToolTipIcon.Info);
            }
        }

        /// <summary>
        /// フォーム終了時の処理 - タスクトレイに隠すか完全終了かを判定
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_isExiting)
            {
                e.Cancel = true;
                this.Hide();
                this.ShowInTaskbar = false;
                notifyIcon1.ShowBalloonTip(2000, "Portable Clipboard", "アプリケーションはタスクトレイで実行中です。", ToolTipIcon.Info);
            }
        }

        /// <summary>
        /// タスクトレイアイコンのダブルクリック - フォームを表示
        /// </summary>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowForm();
        }

        /// <summary>
        /// コンテキストメニュー「表示」項目のクリック
        /// </summary>
        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowForm();
        }

        /// <summary>
        /// コンテキストメニュー「終了」項目のクリック
        /// </summary>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _isExiting = true;
            notifyIcon1.Visible = false;
            Application.Exit();
        }

        /// <summary>
        /// フォームを表示する
        /// </summary>
        private void ShowForm()
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        #endregion
    }
}
