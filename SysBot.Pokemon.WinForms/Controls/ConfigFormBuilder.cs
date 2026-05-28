using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SysBot.Pokemon.WinForms.Controls;

/// <summary>
/// Reflection-driven Configuration form. Three perf principles:
///   1. Lazy — first call to <see cref="BuildFormFor"/> renders only the top level.
///   2. No auto-recursion — nested objects become "Configure X →" drill-down buttons.
///   3. Searchable — top text box filters visible rows in real time.
/// Rounded-corner Regions and Resize handlers are deliberately omitted; they were
/// the main source of lag in the previous version.
/// </summary>
public static class ConfigFormBuilder
{
    private static readonly Color Gold      = Color.FromArgb(0xD4, 0xAF, 0x37);
    private static readonly Color GoldDim   = Color.FromArgb(0x6A, 0x55, 0x22);
    private static readonly Color Text      = Color.FromArgb(0xE8, 0xE8, 0xE8);
    private static readonly Color TextMuted = Color.FromArgb(0x8A, 0x8A, 0x8A);
    private static readonly Color InputBg   = Color.FromArgb(0x12, 0x12, 0x12);
    private static readonly Color CardBg    = Color.FromArgb(0x14, 0x14, 0x14);
    private static readonly Color HoverBg   = Color.FromArgb(0x1F, 0x1F, 0x1F);

    /// <summary>One frame of the drill-down breadcrumb stack.</summary>
    private sealed class CrumbFrame
    {
        public string Label = string.Empty;
        public object Target = null!;
    }

    /// <summary>Per-host state — tracks drill-down navigation and current search filter.</summary>
    private sealed class HostState
    {
        public Panel Host = null!;
        public Panel BreadcrumbBar = null!;
        public TextBox SearchBox = null!;
        public Panel ContentArea = null!;
        public readonly Stack<CrumbFrame> Stack = new();
        public string Filter = string.Empty;
    }

    /// <summary>
    /// Initialize the host with a chrome (search bar + breadcrumb) and render the root level.
    /// Cheap — no nested traversal happens here.
    /// </summary>
    public static void BuildFormFor(object root, Panel host)
    {
        host.SuspendLayout();
        host.Controls.Clear();
        host.BackColor = Color.Transparent;
        host.Padding = new Padding(0);

        var state = new HostState { Host = host };

        // Use a TableLayoutPanel for explicit row sizing — eliminates Dock=Top ordering bugs.
        var chrome = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent,
            Padding = new Padding(0),
            Margin = new Padding(0),
        };
        chrome.ColumnStyles.Clear();
        chrome.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        chrome.RowStyles.Clear();
        chrome.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // Row 0: search bar
        chrome.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));    // Row 1: breadcrumb (height set dynamically)
        chrome.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Row 2: content area

        // === Row 0: Search bar in a visible gold-bordered host ===
        var searchHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = CardBg,
            Padding = new Padding(12, 6, 12, 6),
            Margin = new Padding(0, 0, 0, 12),
        };
        searchHost.Paint += (s, e) =>
        {
            using var pen = new Pen(Color.FromArgb(80, 0xD4, 0xAF, 0x37), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, searchHost.Width - 1, searchHost.Height - 1);
        };
        var searchIcon = new Label
        {
            Text = "⌕",
            ForeColor = Gold,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            Dock = DockStyle.Left,
            Width = 24,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };
        var searchBox = new TextBox
        {
            BackColor = CardBg,
            ForeColor = Text,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 11F),
            PlaceholderText = "Search settings…",
            Dock = DockStyle.Fill,
        };
        state.SearchBox = searchBox;
        searchHost.Controls.Add(searchBox);
        searchHost.Controls.Add(searchIcon);
        chrome.Controls.Add(searchHost, 0, 0);

        // === Row 1: Breadcrumb bar (shown when drilled in) ===
        var crumbBar = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Visible = false,
        };
        state.BreadcrumbBar = crumbBar;
        chrome.Controls.Add(crumbBar, 0, 1);

        // === Row 2: Content area (scrolls if categories overflow) ===
        var contentArea = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            AutoScroll = true,
            Margin = new Padding(0),
        };
        state.ContentArea = contentArea;
        chrome.Controls.Add(contentArea, 0, 2);

        host.Controls.Add(chrome);

        // Push root frame and render
        state.Stack.Push(new CrumbFrame { Label = root.GetType().Name, Target = root });
        RenderCurrent(state);

        // Defensively reset scroll position.
        contentArea.AutoScrollPosition = System.Drawing.Point.Empty;

        // Search wiring — debounce-light: filter on each keystroke since we only enumerate a few rows.
        searchBox.TextChanged += (s, e) =>
        {
            state.Filter = searchBox.Text.Trim();
            RenderCurrent(state);
        };

        host.ResumeLayout(true);
    }

    /// <summary>Render the top of the drill-down stack into ContentArea.</summary>
    private static void RenderCurrent(HostState state)
    {
        var current = state.Stack.Peek();
        state.ContentArea.SuspendLayout();
        state.ContentArea.Controls.Clear();

        // --- Breadcrumb display ---
        // Adjust the breadcrumb row height on the parent TableLayoutPanel
        if (state.BreadcrumbBar.Parent is TableLayoutPanel tlp)
        {
            int rowIdx = tlp.GetRow(state.BreadcrumbBar);
            if (rowIdx >= 0 && rowIdx < tlp.RowStyles.Count)
                tlp.RowStyles[rowIdx].Height = (state.Stack.Count > 1) ? 36 : 0;
        }
        if (state.Stack.Count > 1)
        {
            state.BreadcrumbBar.Visible = true;
            state.BreadcrumbBar.Controls.Clear();
            var back = new Button
            {
                Text = "◀ Back",
                FlatStyle = FlatStyle.Flat,
                ForeColor = Gold,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand,
                Width = 80,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            back.FlatAppearance.BorderSize = 0;
            back.FlatAppearance.MouseOverBackColor = HoverBg;
            back.Click += (s, e) =>
            {
                state.Stack.Pop();
                RenderCurrent(state);
            };
            var crumb = new Label
            {
                Text = string.Join("  ›  ", state.Stack.Reverse().Select(f => f.Label)),
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 9.5F),
                AutoSize = true,
                Location = new Point(90, 6),
                BackColor = Color.Transparent,
            };
            state.BreadcrumbBar.Controls.Add(crumb);
            state.BreadcrumbBar.Controls.Add(back);
        }
        else
        {
            state.BreadcrumbBar.Visible = false;
        }

        // --- Collect direct (non-recursive) properties of the current target, group by category ---
        var props = current.Target.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && IsBrowsable(p))
            .ToList();

        // Apply search filter (matches display name + description + property name)
        if (!string.IsNullOrEmpty(state.Filter))
        {
            var f = state.Filter;
            props = props.Where(p =>
                p.Name.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0 ||
                GetDisplayName(p).IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0 ||
                GetDescription(p).IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0 ||
                GetCategory(p).IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();
        }

        // No results state
        if (props.Count == 0)
        {
            var empty = new Label
            {
                Text = string.IsNullOrEmpty(state.Filter)
                    ? "No settings to display."
                    : $"No settings match \"{state.Filter}\".",
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 11F),
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(8, 24, 0, 0),
                BackColor = Color.Transparent,
            };
            state.ContentArea.Controls.Add(empty);
            state.ContentArea.ResumeLayout(true);
            return;
        }

        var groups = props
            .GroupBy(GetCategory)
            .OrderBy(g => g.Key)
            .ToList();

        // Insert reverse so docked Top stacks in natural reading order
        groups.Reverse();
        foreach (var group in groups)
            state.ContentArea.Controls.Add(BuildCategorySection(group.Key, group.Reverse().ToList(), current.Target, state));

        state.ContentArea.ResumeLayout(true);
    }

    // ===== category section =====
    private sealed class CollapseState { public bool Expanded { get; set; } }

    private static Panel BuildCategorySection(string category, List<PropertyInfo> props, object target, HostState hostState)
    {
        var section = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            // Explicit top padding so the first category's header isn't drawn at Y=0
            // (where it can get clipped by the chrome above it).
            Padding = new Padding(0, 8, 0, 12),
            Margin = new Padding(0),
        };

        // Auto-expand when searching, otherwise collapsed by default for fast initial render.
        var state = new CollapseState { Expanded = !string.IsNullOrEmpty(hostState.Filter) };

        var rowsHost = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = CardBg,
            Padding = new Padding(20, 6, 20, 14),
            Visible = state.Expanded,
        };

        bool rendered = false;
        Action ensureBuilt = () =>
        {
            if (rendered) return;
            rendered = true;
            // Render bottom-up because rows dock Top
            var ordered = props.Select(p => BuildRow(p, target, hostState)).Where(r => r != null).Reverse().ToList();
            foreach (var r in ordered)
                if (r != null) rowsHost.Controls.Add(r);
        };
        if (state.Expanded) ensureBuilt();

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 40,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
        };
        var caret = new Label
        {
            Text = state.Expanded ? "▼" : "▶",
            ForeColor = Gold,
            Font = new Font("Segoe UI", 9F),
            AutoSize = false,
            Width = 20,
            Height = 36,
            Location = new Point(0, 6),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent,
        };
        var title = new Label
        {
            Text = $"{category.ToUpperInvariant()}  ·  {props.Count}",
            ForeColor = Gold,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            AutoSize = false,
            Location = new Point(24, 6),
            Width = 600,
            Height = 28,
            BackColor = Color.Transparent,
        };
        header.Controls.Add(title);
        header.Controls.Add(caret);

        void Toggle()
        {
            state.Expanded = !state.Expanded;
            caret.Text = state.Expanded ? "▼" : "▶";
            if (state.Expanded) ensureBuilt();
            rowsHost.Visible = state.Expanded;
        }
        header.Click += (s, e) => Toggle();
        title.Click  += (s, e) => Toggle();
        caret.Click  += (s, e) => Toggle();

        section.Controls.Add(rowsHost);
        section.Controls.Add(header);
        return section;
    }

    // ===== one property row =====
    private static Panel? BuildRow(PropertyInfo prop, object target, HostState hostState)
    {
        var label = GetDisplayName(prop);
        var help  = GetDescription(prop);
        object? value;
        try { value = prop.GetValue(target); }
        catch { return null; }

        var input = MakeInputFor(prop, target, value, hostState);
        if (input == null)
            return null;

        var row = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 10, 0, 10),
        };

        var titleLbl = new Label
        {
            Text = label,
            ForeColor = Text,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 22,
            BackColor = Color.Transparent,
        };

        Label? helpLbl = null;
        if (!string.IsNullOrWhiteSpace(help))
        {
            helpLbl = new Label
            {
                Text = help,
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 9F),
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = MeasureHelpHeight(help),
                BackColor = Color.Transparent,
            };
        }

        var inputHost = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 4, 0, 0),
        };
        input.Dock = DockStyle.Top;
        inputHost.Controls.Add(input);

        // Reverse insert: titleLbl on top, then helpLbl, then inputHost
        row.Controls.Add(inputHost);
        if (helpLbl != null) row.Controls.Add(helpLbl);
        row.Controls.Add(titleLbl);
        return row;
    }

    private static int MeasureHelpHeight(string text)
    {
        int lines = Math.Max(1, (text.Length + 84) / 85);
        return Math.Min(60, 14 * lines + 4);
    }

    // ===== input control factory (no recursion — nested objects become drill-down buttons) =====
    private static Control? MakeInputFor(PropertyInfo prop, object target, object? value, HostState hostState)
    {
        var t = prop.PropertyType;

        if (t == typeof(bool))
        {
            var cb = new CheckBox
            {
                Checked = (bool)(value ?? false),
                AutoSize = true,
                ForeColor = Text,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 10F),
                FlatStyle = FlatStyle.Flat,
                Text = "Enabled",
            };
            if (!prop.CanWrite) cb.Enabled = false;
            else cb.CheckedChanged += (s, e) => SafeSet(prop, target, cb.Checked);
            return cb;
        }

        if (t.IsEnum)
        {
            var combo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F),
                BackColor = InputBg,
                ForeColor = Text,
                Width = 280,
                Height = 28,
            };
            foreach (var v in Enum.GetValues(t)) combo.Items.Add(v!);
            if (value != null) combo.SelectedItem = value;
            if (!prop.CanWrite) combo.Enabled = false;
            else combo.SelectedIndexChanged += (s, e) => { if (combo.SelectedItem != null) SafeSet(prop, target, combo.SelectedItem); };
            return combo;
        }

        if (t == typeof(int) || t == typeof(uint) || t == typeof(short) || t == typeof(ushort) ||
            t == typeof(long) || t == typeof(byte) || t == typeof(sbyte))
        {
            var nud = new NumericUpDown
            {
                Minimum = (t == typeof(uint) || t == typeof(ushort) || t == typeof(byte)) ? 0 : int.MinValue,
                Maximum = int.MaxValue,
                Font = new Font("Segoe UI", 10F),
                BackColor = InputBg,
                ForeColor = Text,
                BorderStyle = BorderStyle.FixedSingle,
                Width = 200,
                Height = 28,
            };
            if (value != null) { try { nud.Value = Convert.ToDecimal(value); } catch { } }
            if (!prop.CanWrite) nud.Enabled = false;
            else nud.ValueChanged += (s, e) => { try { SafeSet(prop, target, Convert.ChangeType(nud.Value, t)); } catch { } };
            return nud;
        }

        // ulong / decimal / float / double — TextBox with parse-on-change (NumericUpDown can't represent ulong)
        if (t == typeof(ulong) || t == typeof(float) || t == typeof(double) || t == typeof(decimal))
        {
            var tbf = MakeTextBox(value?.ToString() ?? string.Empty, 200);
            if (!prop.CanWrite) tbf.Enabled = false;
            else tbf.TextChanged += (s, e) => { try { SafeSet(prop, target, Convert.ChangeType(tbf.Text, t)); } catch { } };
            return tbf;
        }

        if (t == typeof(string))
        {
            var tb = MakeTextBox((string?)value ?? string.Empty, 480);
            if (prop.Name.IndexOf("token", StringComparison.OrdinalIgnoreCase) >= 0)
                tb.UseSystemPasswordChar = true;
            if (!prop.CanWrite) tb.Enabled = false;
            else tb.TextChanged += (s, e) => SafeSet(prop, target, tb.Text);
            return tb;
        }

        // Collections — Edit list dialog
        if (typeof(IList).IsAssignableFrom(t) && t != typeof(string))
        {
            var btn = MakeActionButton($"Edit list…  ({(value is IList l ? l.Count : 0)} items)");
            btn.Click += (s, e) => OpenListEditor(prop, value as IList);
            return btn;
        }

        // Nested object — drill-down button (no inline recursion → huge perf win)
        if (value != null && !t.IsPrimitive)
        {
            var btn = MakeActionButton($"Configure  →");
            btn.Click += (s, e) =>
            {
                hostState.Stack.Push(new CrumbFrame { Label = GetDisplayName(prop), Target = value });
                RenderCurrent(hostState);
            };
            return btn;
        }

        // Read-only fallback
        var ro = MakeTextBox(value?.ToString() ?? "(not editable)", 480);
        ro.ReadOnly = true;
        return ro;
    }

    private static void SafeSet(PropertyInfo prop, object target, object? value)
    {
        try { prop.SetValue(target, value); } catch { /* swallow — bad input is non-fatal */ }
    }

    private static TextBox MakeTextBox(string text, int width)
    {
        return new TextBox
        {
            Text = text,
            BackColor = InputBg,
            ForeColor = Text,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10.5F),
            Width = width,
            Height = 28,
        };
    }

    private static Button MakeActionButton(string text)
    {
        var btn = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Gold,
            BackColor = Color.Transparent,
            Font = new Font("Segoe UI", 10F),
            Width = 240,
            Height = 32,
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
        };
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.BorderColor = GoldDim;
        btn.FlatAppearance.MouseOverBackColor = HoverBg;
        return btn;
    }

    // ===== list editor dialog (unchanged behavior) =====
    private static void OpenListEditor(PropertyInfo prop, IList? list)
    {
        if (list == null) return;
        var dialog = new Form
        {
            Text = GetDisplayName(prop),
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(560, 480),
            BackColor = Color.FromArgb(0x0A, 0x0A, 0x0A),
            ForeColor = Text,
            Font = new Font("Segoe UI", 10F),
            FormBorderStyle = FormBorderStyle.SizableToolWindow,
        };
        var header = new Label
        {
            Text = GetDisplayName(prop),
            Dock = DockStyle.Top,
            Height = 40,
            ForeColor = Gold,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            Padding = new Padding(16, 10, 0, 0),
        };
        var listBox = new ListBox
        {
            Dock = DockStyle.Fill,
            BackColor = InputBg,
            ForeColor = Text,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 10F),
        };
        foreach (var item in list) listBox.Items.Add(item ?? string.Empty);
        var inputBar = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.Transparent, Padding = new Padding(16, 12, 16, 16) };
        var newEntry = new TextBox { BackColor = InputBg, ForeColor = Text, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10F), Dock = DockStyle.Top, Height = 28 };
        var btnAdd = new Button { Text = "Add", BackColor = Gold, ForeColor = Color.Black, FlatStyle = FlatStyle.Flat, Dock = DockStyle.Right, Width = 90, Cursor = Cursors.Hand };
        var btnRemove = new Button { Text = "Remove selected", BackColor = Color.FromArgb(0x8B, 0x2C, 0x2C), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Dock = DockStyle.Left, Width = 150, Cursor = Cursors.Hand };
        btnAdd.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(newEntry.Text)) return;
            object converted = TryConvertForList(list, newEntry.Text) ?? newEntry.Text;
            list.Add(converted);
            listBox.Items.Add(converted);
            newEntry.Clear();
        };
        btnRemove.Click += (s, e) =>
        {
            if (listBox.SelectedIndex < 0) return;
            list.RemoveAt(listBox.SelectedIndex);
            listBox.Items.RemoveAt(listBox.SelectedIndex);
        };
        inputBar.Controls.Add(newEntry);
        var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 40, BackColor = Color.Transparent };
        bottomBar.Controls.Add(btnRemove);
        bottomBar.Controls.Add(btnAdd);
        inputBar.Controls.Add(bottomBar);
        dialog.Controls.Add(listBox);
        dialog.Controls.Add(inputBar);
        dialog.Controls.Add(header);
        dialog.ShowDialog();
    }

    private static object? TryConvertForList(IList list, string raw)
    {
        var listType = list.GetType();
        var elemType = listType.IsGenericType ? listType.GetGenericArguments().FirstOrDefault() : null;
        if (elemType == null) return raw;
        try { return Convert.ChangeType(raw, elemType); }
        catch { return null; }
    }

    // ===== reflection helpers =====
    private static bool IsBrowsable(PropertyInfo p)
    {
        var attr = p.GetCustomAttribute<BrowsableAttribute>();
        return attr?.Browsable ?? true;
    }

    private static string GetCategory(PropertyInfo p)
    {
        var attr = p.GetCustomAttribute<CategoryAttribute>();
        return string.IsNullOrEmpty(attr?.Category) ? "General" : attr!.Category;
    }

    private static string GetDisplayName(PropertyInfo p)
    {
        var dn = p.GetCustomAttribute<DisplayNameAttribute>();
        return string.IsNullOrEmpty(dn?.DisplayName) ? Humanize(p.Name) : dn!.DisplayName;
    }

    private static string GetDescription(PropertyInfo p)
    {
        var d = p.GetCustomAttribute<DescriptionAttribute>();
        return d?.Description ?? string.Empty;
    }

    private static string Humanize(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
                sb.Append(' ');
            sb.Append(name[i]);
        }
        return sb.ToString();
    }
}
