using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using Steamworks;

namespace SteamFarmer
{
    static class Program
    {
        private static bool isRunning = false;
        private static uint currentAppId = 0;
        private static readonly string configFile = "steam_appid.txt";

        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool autoStart = args.Contains("-auto");
            uint argAppId = GetAppIdFromArgs(args);
            uint appId = 0;

            if (!autoStart)
            {
                if (!SteamAPI.IsSteamRunning())
                {
                    MessageBox.Show("Steam is not running! Start Steam and try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using var inputForm = new Form
                {
                    Text = "SteamFarmer",
                    Size = new System.Drawing.Size(410, 115),
                    StartPosition = FormStartPosition.CenterScreen,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var label = new Label
                {
                    Text = "Game ID (App ID):",
                    Location = new System.Drawing.Point(15, 20),
                    Size = new System.Drawing.Size(120, 20),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top
                };

                var textBox = new TextBox
                {
                    Location = new System.Drawing.Point(135, 18),
                    Size = new System.Drawing.Size(150, 20),
                    Text = GetDefaultAppId(argAppId).ToString(),
                    Anchor = AnchorStyles.Right | AnchorStyles.Top
                };
                if (textBox.Text == "0") textBox.Text = "";

                textBox.KeyPress += (sender, e) =>
                {
                    if (e.KeyChar == (char)Keys.Enter)
                    {
                        e.Handled = true;
                        inputForm.DialogResult = DialogResult.OK;
                        inputForm.Close();
                    }
                };

                var okButton = new Button
                {
                    Text = "Start",
                    Location = new System.Drawing.Point(295, 16),
                    Size = new System.Drawing.Size(80, 25),
                    Anchor = AnchorStyles.Right | AnchorStyles.Top
                };
                okButton.Click += (sender, e) =>
                {
                    inputForm.DialogResult = DialogResult.OK;
                    inputForm.Close();
                };

                var infoLabel = new Label
                {
                    Text = "The program will run in the background, without a window.",
                    Location = new System.Drawing.Point(15, 48),
                    Size = new System.Drawing.Size(360, 20),
                    ForeColor = System.Drawing.Color.Gray,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top
                };

                inputForm.Controls.AddRange(new Control[] { label, textBox, okButton, infoLabel });

                if (string.IsNullOrEmpty(textBox.Text)) textBox.Focus();
                else { textBox.SelectAll(); okButton.Focus(); }

                if (inputForm.ShowDialog() != DialogResult.OK) return;

                string appIdStr = textBox.Text.Trim();
                if (string.IsNullOrEmpty(appIdStr) || !uint.TryParse(appIdStr, out appId) || appId == 0)
                {
                    MessageBox.Show("Invalid game ID. Enter a numeric ID greater than 0.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                appId = GetDefaultAppId(argAppId);
                if (appId == 0) return;
            }

            currentAppId = appId;

            try
            {
                File.WriteAllText(configFile, appId.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating configuration file: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool initResult = false;
            try
            {
                initResult = SteamAPI.Init();
            }
            catch (DllNotFoundException dllEx)
            {
                MessageBox.Show("Native Steam API libraries not found.\n\n" +
                    "Make sure steam_api64.dll files are in the program folder.\n\n" +
                    $"Error details: {dllEx.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (EntryPointNotFoundException epEx)
            {
                MessageBox.Show("Invalid Steam API library version.\n\n" +
                    "Try updating Steamworks.NET or Steam client.\n\n" +
                    $"Error details: {epEx.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!initResult)
            {
                MessageBox.Show("Steam API initialization error.\n\nPossible reasons:\n" +
                    "- Steam is not running\n" +
                    "- You do not have this game in your library\n" +
                    "- Invalid App ID\n" +
                    "- Steam is running under a different user", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            isRunning = true;

            if (!autoStart)
            {
                MessageBox.Show("Process started!\nTo close, use Steam or Task Manager.", "SteamFarmer", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Task.Run(RunFarmingLoop);
            Application.Run(new HiddenForm());
        }

        private static uint GetAppIdFromArgs(string[] args)
        {
            var idArg = args.FirstOrDefault(arg => arg.StartsWith("-id="));
            return idArg != null && uint.TryParse(idArg.Substring(4), out uint id) && id > 0 ? id : 0;
        }

        private static uint GetDefaultAppId(uint argAppId)
        {
            if (argAppId > 0) return argAppId;

            if (File.Exists(configFile))
            {
                try
                {
                    string content = File.ReadAllText(configFile).Trim();
                    if (uint.TryParse(content, out uint fileAppId) && fileAppId > 0) return fileAppId;
                }
                catch { }
            }

            return 0;
        }

        private static void RunFarmingLoop()
        {
            try
            {
                while (isRunning)
                {
                    if (!SteamAPI.IsSteamRunning()) break;

                    SteamAPI.RunCallbacks();
                    Thread.Sleep(5000);
                }
            }
            catch { }
            finally
            {
                try { SteamAPI.Shutdown(); }
                catch { }
                Application.Exit();
            }
        }

        private class HiddenForm : Form
        {
            public HiddenForm()
            {
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
                Visible = false;
                FormBorderStyle = FormBorderStyle.None;
                Size = new System.Drawing.Size(0, 0);
                
                FormClosing += (sender, e) =>
                {
                    isRunning = false;
                    try { SteamAPI.Shutdown(); }
                    catch { }
                };
            }

            protected override void SetVisibleCore(bool value) => base.SetVisibleCore(false);

            protected override CreateParams CreateParams
            {
                get
                {
                    var cp = base.CreateParams;
                    cp.ExStyle |= 0x80;
                    return cp;
                }
            }
        }
    }
}