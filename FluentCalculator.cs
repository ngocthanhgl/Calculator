using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class FluentCalculator : Form
{
    [DllImport("user32.dll")]
    static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    const int WM_NCHITTEST = 0x84;
    const int WM_NCLBUTTONDOWN = 0xA1;
    const int HTCLIENT = 1;
    const int HTCAPTION = 2;
    const int HTLEFT = 10;
    const int HTRIGHT = 11;
    const int HTTOP = 12;
    const int HTTOPLEFT = 13;
    const int HTTOPRIGHT = 14;
    const int HTBOTTOM = 15;
    const int HTBOTTOMLEFT = 16;
    const int HTBOTTOMRIGHT = 17;

    const int HANDLE = 12;
    const int TITLE_H = 22;
    const int BTN_W = 38;
    const int GAP = 16;

    Panel pnlTitle;
    Label lblTitle;
    Button btnSettings, btnPin, btnMin, btnClose;
    Panel pnlSettings;
    NumericUpDown nudExprSize, nudResultSize;
    RichTextBox txtExpr;
    Label lblResult;

    bool pinned;
    bool settingsVisible;
    Settings settings;
    string lastResult = "";
    bool adjusting;
    bool coloring;
    bool layoutPending;

    static readonly Color White = Color.White;
    static readonly Color TitleBg = Color.FromArgb(0xF5, 0xF5, 0xF5);
    static readonly Color TextDark = Color.FromArgb(0x32, 0x31, 0x30);
    static readonly Color Accent = Color.FromArgb(0x00, 0x78, 0xD4);
    static readonly Color ResultRed = Color.Red;
    static readonly Color OpRed = Color.Red;
    static readonly Color Divider = Color.FromArgb(0xE1, 0xDF, 0xDD);
    static readonly Color HoverBg = Color.FromArgb(0xE5, 0xE5, 0xE5);
    static readonly Color PressBg = Color.FromArgb(0xD2, 0xD0, 0xCE);

    public FluentCalculator()
    {
        Text = "Calculator";
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        FormBorderStyle = FormBorderStyle.None;
        BackColor = White;
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f);
        DoubleBuffered = true;

        settings = SettingsStore.Load();
        BuildUI();
        ApplySettings();
        RestoreWindow();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ClassStyle |= 0x00020000;
            return cp;
        }
    }

    void BuildUI()
    {
        pnlTitle = new Panel
        {
            Height = TITLE_H,
            Dock = DockStyle.Top,
            BackColor = TitleBg
        };
        pnlTitle.MouseDown += TitleBarMouseDown;
        pnlTitle.Paint += (s, e) =>
        {
            using (var p = new Pen(Divider))
                e.Graphics.DrawLine(p, 0, TITLE_H - 1, pnlTitle.Width, TITLE_H - 1);
        };

        lblTitle = new Label
        {
            Text = "Calculator",
            Font = new Font("Segoe UI", 9f),
            ForeColor = TextDark,
            BackColor = Color.Transparent,
            Location = new Point(10, 0),
            Size = new Size(120, TITLE_H),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
        lblTitle.MouseDown += TitleBarMouseDown;

        btnSettings = MakeBtn("\u2699");
        btnSettings.Click += (s, e) => ToggleSettings();

        btnPin = MakeBtn("\u25C9");
        btnPin.Click += (s, e) =>
        {
            pinned = !pinned;
            TopMost = pinned;
            btnPin.BackColor = pinned ? HoverBg : Color.Transparent;
        };

        btnMin = MakeBtn("_");
        btnMin.Click += (s, e) => WindowState = FormWindowState.Minimized;

        btnClose = MakeBtn("\u2715");
        btnClose.Click += (s, e) => Close();

        LayoutTitleBtns();
        pnlTitle.Controls.AddRange(new Control[] { lblTitle, btnSettings, btnPin, btnMin, btnClose });
        Controls.Add(pnlTitle);

        BuildSettingsPanel();

        txtExpr = new RichTextBox
        {
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 10f),
            ForeColor = TextDark,
            BackColor = White,
            Location = new Point(GAP, TITLE_H + GAP),
            Width = ClientSize.Width - GAP * 2,
            WordWrap = true,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            TabStop = true,
            Multiline = true
        };
        txtExpr.TextChanged += (s, e) => OnExprChanged();
        txtExpr.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Escape) { txtExpr.Clear(); e.SuppressKeyPress = true; }
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; }
        };
        Controls.Add(txtExpr);

        lblResult = new Label
        {
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            ForeColor = TextDark,
            BackColor = White,
            AutoSize = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(GAP, 0),
            Size = new Size(ClientSize.Width - GAP * 2, 60),
            AutoEllipsis = true
        };
        Controls.Add(lblResult);

        AdjustResultPosition();
        ActiveControl = txtExpr;
    }

    void BuildSettingsPanel()
    {
        pnlSettings = new Panel
        {
            Visible = false,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Width = 190,
            Height = 72,
            Location = new Point(4, TITLE_H + 1)
        };

        var fn = new Font("Segoe UI", 9f);

        var l1 = new Label
        {
            Text = "Expression",
            Font = fn, ForeColor = TextDark,
            Location = new Point(8, 8), Size = new Size(96, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };

        nudExprSize = new NumericUpDown
        {
            Location = new Point(106, 8),
            Size = new Size(68, 22),
            Minimum = 10, Maximum = 72, Increment = 2, Value = 10,
            Font = fn, TextAlign = HorizontalAlignment.Center
        };
        nudExprSize.ValueChanged += (s, e) => ApplyExprFont();

        var l2 = new Label
        {
            Text = "Result",
            Font = fn, ForeColor = TextDark,
            Location = new Point(8, 36), Size = new Size(96, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };

        nudResultSize = new NumericUpDown
        {
            Location = new Point(106, 36),
            Size = new Size(68, 22),
            Minimum = 16, Maximum = 96, Increment = 2, Value = 16,
            Font = fn, TextAlign = HorizontalAlignment.Center
        };
        nudResultSize.ValueChanged += (s, e) => ApplyResultFont();

        pnlSettings.Controls.AddRange(new Control[] { l1, nudExprSize, l2, nudResultSize });
        Controls.Add(pnlSettings);
        Controls.SetChildIndex(pnlSettings, 1);
    }

    Button MakeBtn(string text)
    {
        var b = new Button
        {
            Text = text,
            Width = BTN_W, Height = TITLE_H,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = TextDark,
            Font = new Font("Segoe UI", 9f),
            Cursor = Cursors.Hand,
            TabStop = true
        };
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = HoverBg;
        b.FlatAppearance.MouseDownBackColor = PressBg;
        return b;
    }

    void LayoutTitleBtns()
    {
        if (btnClose == null) return;
        int r = pnlTitle.Width;
        btnClose.Location = new Point(r - BTN_W, 0);
        btnMin.Location = new Point(r - BTN_W * 2, 0);
        btnPin.Location = new Point(r - BTN_W * 3, 0);
        btnSettings.Location = new Point(r - BTN_W * 4, 0);
        lblTitle.Width = Math.Max(20, btnSettings.Left - 10);
    }

    static string FormatNum(object val)
    {
        if (val is double)
        {
            double d = (double)val;
            if (double.IsInfinity(d) || double.IsNaN(d))
                return "Error";
            if (Math.Abs(d) < 1e15 && d == Math.Truncate(d))
                return d.ToString("N0");
            if (Math.Abs(d) < 1e-4 || Math.Abs(d) >= 1e10)
                return d.ToString("G10");
            return d.ToString("#,##0.########");
        }
        if (val is decimal)
        {
            decimal m = (decimal)val;
            if (m == Math.Truncate(m))
            {
                if (Math.Abs(m) < 1e15m) return m.ToString("N0");
                return ((double)m).ToString("G10");
            }
            return m.ToString("#,##0.########");
        }
        if (val is float)
        {
            float f = (float)val;
            if (Math.Abs(f) < 1e7f && f == Math.Truncate(f))
                return f.ToString("N0");
            if (Math.Abs(f) >= 1e7f || (Math.Abs(f) < 1e-4f && f != 0))
                return f.ToString("G7");
            return f.ToString("#,##0.########");
        }
        try
        {
            decimal dec = Convert.ToDecimal(val);
            if (Math.Abs(dec) < 1e15m) return dec.ToString("N0");
            return ((double)dec).ToString("G10");
        }
        catch { return val.ToString(); }
    }

    void TitleBarMouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
        }
    }

    void ToggleSettings()
    {
        settingsVisible = !settingsVisible;
        pnlSettings.Visible = settingsVisible;
        if (settingsVisible) { pnlSettings.BringToFront(); nudExprSize.Focus(); }
        btnSettings.BackColor = settingsVisible ? HoverBg : Color.Transparent;
    }

    void ApplySettings()
    {
        settings.ExprFontSize = Math.Max(10, Math.Min(72, settings.ExprFontSize));
        settings.ResultFontSize = Math.Max(16, Math.Min(96, settings.ResultFontSize));

        txtExpr.Font = new Font("Segoe UI", settings.ExprFontSize);
        lblResult.Font = new Font("Segoe UI", settings.ResultFontSize, FontStyle.Bold);
        lblResult.Height = lblResult.Font.Height + 8;

        ColorOperators();
        AdjustResultPosition();

        nudExprSize.Value = settings.ExprFontSize;
        nudResultSize.Value = settings.ResultFontSize;
    }

    void ApplyExprFont()
    {
        settings.ExprFontSize = (int)nudExprSize.Value;
        var old = txtExpr.Font;
        txtExpr.Font = null;
        if (old != null) old.Dispose();
        txtExpr.Font = new Font("Segoe UI", settings.ExprFontSize);
        ColorOperators();
        AdjustResultPosition();
    }

    void ApplyResultFont()
    {
        settings.ResultFontSize = (int)nudResultSize.Value;
        var old = lblResult.Font;
        lblResult.Font = null;
        if (old != null) old.Dispose();
        lblResult.Font = new Font("Segoe UI", settings.ResultFontSize, FontStyle.Bold);
        lblResult.Height = lblResult.Font.Height + 8;
        AdjustResultPosition();
    }

    void AdjustResultPosition()
    {
        if (adjusting) return;
        adjusting = true;
        lblResult.Top = ClientSize.Height - lblResult.Height - GAP;
        txtExpr.Height = Math.Max(20, lblResult.Top - txtExpr.Top - 4);
        adjusting = false;
    }

    void RestoreWindow()
    {
        MinimumSize = new Size(200, 120);
        Size = new Size(settings.WindowWidth, settings.WindowHeight);
        var wa = Screen.GetWorkingArea(this);
        if (Width > wa.Width) Width = wa.Width;
        if (Height > wa.Height) Height = wa.Height;
        if (settings.WindowLeft >= 0)
        {
            var rc = new Rectangle(settings.WindowLeft, settings.WindowTop,
                Width, Height);
            bool onScreen = false;
            foreach (Screen s in Screen.AllScreens)
                if (s.WorkingArea.IntersectsWith(rc)) { onScreen = true; break; }
            if (onScreen)
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(settings.WindowLeft, settings.WindowTop);
            }
        }
        if (settings.WindowState != FormWindowState.Minimized)
            WindowState = settings.WindowState;
        RefreshTextLayout();
    }

    void OnExprChanged()
    {
        if (coloring) return;
        coloring = true;

        string raw = txtExpr.Text;

        for (int i = 0; i < raw.Length; i++)
        {
            char c = raw[i];
            if (c == '*')
            {
                txtExpr.Select(i, 1);
                txtExpr.SelectedText = "\u00D7";
            }
            else if (c == '/')
            {
                txtExpr.Select(i, 1);
                txtExpr.SelectedText = "\u00F7";
            }
        }

        ColorOperators();
        UpdateResult();

        coloring = false;
    }

    void ColorOperators()
    {
        string text = txtExpr.Text;
        if (text.Length == 0) return;

        int savedStart = txtExpr.SelectionStart;
        int savedLen = txtExpr.SelectionLength;

        txtExpr.SuspendLayout();
        txtExpr.Select(0, text.Length);
        txtExpr.SelectionColor = TextDark;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '+' || c == '-' || c == '\u00D7' || c == '\u00F7' || c == '%')
            {
                txtExpr.Select(i, 1);
                txtExpr.SelectionColor = OpRed;
            }
        }

        txtExpr.SelectionStart = Math.Min(savedStart, text.Length);
        txtExpr.SelectionLength = Math.Min(savedLen, text.Length - txtExpr.SelectionStart);
        txtExpr.ResumeLayout();
    }

    static double EvalSeq(string expr)
    {
        expr = expr.Replace(" ", "");
        int pos = 0;
        double result = EvalSeqExpr(expr, ref pos);
        if (pos < expr.Length)
            throw new InvalidOperationException();
        return result;
    }

    static double EvalSeqExpr(string expr, ref int pos)
    {
        double result = EvalSeqTerm(expr, ref pos);
        while (pos < expr.Length)
        {
            char c = expr[pos];
            if (c != '+' && c != '-' && c != '*' && c != '/' && c != '%')
                break;
            pos++;
            double right = EvalSeqTerm(expr, ref pos);
            switch (c)
            {
                case '+': result += right; break;
                case '-': result -= right; break;
                case '*': result *= right; break;
                case '/': result /= right; break;
                case '%': result %= right; break;
            }
        }
        return result;
    }

    static double EvalSeqTerm(string expr, ref int pos)
    {
        if (pos >= expr.Length)
            throw new InvalidOperationException();

        bool negate = false;
        if (expr[pos] == '-')
        {
            negate = true;
            pos++;
        }
        else if (expr[pos] == '+')
        {
            pos++;
        }

        if (pos >= expr.Length)
            throw new InvalidOperationException();

        if (expr[pos] == '(')
        {
            pos++;
            double val = EvalSeqExpr(expr, ref pos);
            if (pos < expr.Length && expr[pos] == ')')
                pos++;
            else
                throw new InvalidOperationException();
            return negate ? -val : val;
        }

        if ("*/%)".IndexOf(expr[pos]) >= 0)
        {
            throw new InvalidOperationException();
        }

        int start = pos;
        bool hasDot = false;
        while (pos < expr.Length && (char.IsDigit(expr[pos]) || (!hasDot && expr[pos] == '.')))
        {
            if (expr[pos] == '.') hasDot = true;
            pos++;
        }
        if (pos == start)
            throw new InvalidOperationException();

        double num = double.Parse(expr.Substring(start, pos - start),
            System.Globalization.CultureInfo.InvariantCulture);
        return negate ? -num : num;
    }

    void UpdateResult()
    {
        string expr = txtExpr.Text.Replace('\u00D7', '*').Replace('\u00F7', '/');

        if (string.IsNullOrWhiteSpace(expr))
        {
            lblResult.Text = "";
            lastResult = "";
            return;
        }

        string t = expr.TrimEnd();
        if (t.Length > 0 && "+-*/%(".IndexOf(t[t.Length - 1]) >= 0)
        {
            lblResult.Text = lastResult;
            return;
        }

        int open = 0, close = 0;
        foreach (char c in expr) { if (c == '(') open++; if (c == ')') close++; }
        if (open > close)
        {
            lblResult.Text = lastResult;
            return;
        }

        try
        {
            double val = EvalSeq(expr);
            if (double.IsInfinity(val) || double.IsNaN(val))
            { lblResult.ForeColor = ResultRed; lblResult.Text = "Error"; lastResult = "Error"; return; }
            string r = "= " + FormatNum(val);
            lblResult.ForeColor = TextDark;
            lblResult.Text = r;
            lastResult = r;
        }
        catch
        {
            lblResult.Text = lastResult;
        }
    }

    void RefreshTextLayout()
    {
        if (txtExpr == null) return;
        txtExpr.Width = ClientSize.Width - GAP * 2;
        if (lblResult != null)
        {
            lblResult.Width = ClientSize.Width - GAP * 2;
            AdjustResultPosition();
        }
        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        LayoutTitleBtns();
        if (!IsHandleCreated || layoutPending) return;
        layoutPending = true;
        BeginInvoke((MethodInvoker)(() =>
        {
            layoutPending = false;
            if (!IsDisposed) RefreshTextLayout();
        }));
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        settings.WindowWidth = Width;
        settings.WindowHeight = Height;
        if (WindowState == FormWindowState.Normal)
        {
            settings.WindowLeft = Left;
            settings.WindowTop = Top;
        }
        settings.WindowState = WindowState == FormWindowState.Minimized
            ? FormWindowState.Normal : WindowState;
        SettingsStore.Save(settings);
        base.OnFormClosing(e);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_NCHITTEST)
        {
            base.WndProc(ref m);
            if (m.Result == (IntPtr)HTCLIENT)
            {
                Point pt = PointToClient(Cursor.Position);
                int x = pt.X, y = pt.Y, w = ClientSize.Width, h = ClientSize.Height;

                if (y < HANDLE && x < HANDLE) { m.Result = (IntPtr)HTTOPLEFT; return; }
                if (y < HANDLE && x >= w - HANDLE) { m.Result = (IntPtr)HTTOPRIGHT; return; }
                if (y >= h - HANDLE && x < HANDLE) { m.Result = (IntPtr)HTBOTTOMLEFT; return; }
                if (y >= h - HANDLE && x >= w - HANDLE) { m.Result = (IntPtr)HTBOTTOMRIGHT; return; }
                if (y < HANDLE) { m.Result = (IntPtr)HTTOP; return; }
                if (y >= h - HANDLE) { m.Result = (IntPtr)HTBOTTOM; return; }
                if (x < HANDLE) { m.Result = (IntPtr)HTLEFT; return; }
                if (x >= w - HANDLE) { m.Result = (IntPtr)HTRIGHT; return; }
            }
            return;
        }
        base.WndProc(ref m);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using (var p = new Pen(Divider))
        {
            e.Graphics.DrawRectangle(p, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            if (txtExpr != null)
                e.Graphics.DrawLine(p, GAP, txtExpr.Bottom + 2, ClientSize.Width - GAP, txtExpr.Bottom + 2);
        }
    }
}

class Settings
{
    public int ExprFontSize = 10;
    public int ResultFontSize = 16;
    public int WindowWidth = 440;
    public int WindowHeight = 320;
    public int WindowLeft = -1;
    public int WindowTop = -1;
    public FormWindowState WindowState = FormWindowState.Normal;
}

static class SettingsStore
{
    static string Path_
    {
        get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Calculator", "calculator.dat"); }
    }

    public static void Save(Settings s)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path_));
            using (var w = new StreamWriter(Path_))
            {
                w.WriteLine(s.ExprFontSize);
                w.WriteLine(s.ResultFontSize);
                w.WriteLine(s.WindowWidth);
                w.WriteLine(s.WindowHeight);
                w.WriteLine(s.WindowLeft);
                w.WriteLine(s.WindowTop);
                w.WriteLine((int)s.WindowState);
            }
        }
        catch { }
    }

    public static Settings Load()
    {
        var s = new Settings();
        try
        {
            if (!File.Exists(Path_)) return s;
            var lines = File.ReadAllLines(Path_);
            if (lines.Length >= 7)
            {
                s.ExprFontSize = int.Parse(lines[0]);
                s.ResultFontSize = int.Parse(lines[1]);
                s.WindowWidth = int.Parse(lines[2]);
                s.WindowHeight = int.Parse(lines[3]);
                s.WindowLeft = int.Parse(lines[4]);
                s.WindowTop = int.Parse(lines[5]);
                s.WindowState = (FormWindowState)int.Parse(lines[6]);
            }
        }
        catch { }
        return s;
    }
}
