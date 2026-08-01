using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

class InstallerForm : Form
{
    TextBox txtPath;
    Button btnBrowse, btnInstall;
    Label lblStatus;
    string defaultDir;

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new InstallerForm());
    }

    InstallerForm()
    {
        Text = "Calculator Installer";
        Size = new Size(480, 200);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        BackColor = Color.White;
        Font = new Font("Segoe UI", 9f);

        defaultDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calculator");

        lblStatus = new Label
        {
            Text = "Choose install location:",
            Location = new Point(16, 16),
            Size = new Size(440, 20)
        };

        txtPath = new TextBox
        {
            Text = defaultDir,
            Location = new Point(16, 42),
            Width = 340,
            Height = 24
        };

        btnBrowse = new Button
        {
            Text = "Browse...",
            Location = new Point(366, 40),
            Size = new Size(84, 28),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnBrowse.Click += BrowseClick;

        btnInstall = new Button
        {
            Text = "Install",
            Location = new Point(16, 80),
            Size = new Size(100, 32),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0x00, 0x78, 0xD4),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold),
            Cursor = Cursors.Hand,
            FlatAppearance = { BorderSize = 0, MouseOverBackColor = Color.FromArgb(0x10, 0x6E, 0xBE) }
        };
        btnInstall.Click += InstallClick;

        Controls.AddRange(new Control[] { lblStatus, txtPath, btnBrowse, btnInstall });
    }

    void BrowseClick(object sender, EventArgs e)
    {
        using (var dlg = new FolderBrowserDialog())
        {
            dlg.SelectedPath = txtPath.Text;
            dlg.Description = "Select install directory";
            if (dlg.ShowDialog() == DialogResult.OK)
                txtPath.Text = dlg.SelectedPath;
        }
    }

    void InstallClick(object sender, EventArgs e)
    {
        string dir = txtPath.Text.Trim();
        if (string.IsNullOrWhiteSpace(dir))
        {
            MessageBox.Show("Please select an install directory.", "Calculator Installer",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnInstall.Enabled = false;
        btnInstall.Text = "Installing...";
        lblStatus.Text = "Extracting files...";
        Refresh();

        try
        {
            foreach (var p in Process.GetProcessesByName("Calculator"))
                try { p.Kill(); p.WaitForExit(2000); } catch { }

            Directory.CreateDirectory(dir);
            string targetExe = Path.Combine(dir, "Calculator.exe");

            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("Calculator.exe"))
            {
                if (stream == null)
                {
                    MessageBox.Show("Internal error: application data missing.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnInstall.Enabled = true;
                    btnInstall.Text = "Install";
                    return;
                }
                using (var fs = new FileStream(targetExe, FileMode.Create, FileAccess.Write))
                    stream.CopyTo(fs);
            }

            string desktop = Environment.GetFolderPath(
                Environment.SpecialFolder.DesktopDirectory);
            File.Copy(targetExe, Path.Combine(desktop, "Calculator.exe"), true);

            lblStatus.Text = "Install complete.";

            var result = MessageBox.Show(
                "Calculator installed successfully.\n\nInstalled to: " + dir +
                "\nCopied to desktop.\n\nLaunch Calculator now?",
                "Calculator Installer",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
                Process.Start(targetExe);

            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Install failed:\n" + ex.Message,
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnInstall.Enabled = true;
            btnInstall.Text = "Install";
            lblStatus.Text = "Install failed.";
        }
    }
}
