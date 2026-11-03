using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace NamedPipeExplorer
{
    public static class ListBrowser
    {
        public static void Show(
            IReadOnlyList<string> initialItems,
            Func<IReadOnlyList<string>> refreshProvider,
            Action<string> customAction)
        {
            if (initialItems == null)
                throw new ArgumentNullException(nameof(initialItems));

            if (refreshProvider == null)
                throw new ArgumentNullException(nameof(refreshProvider));

            if (customAction == null)
                throw new ArgumentNullException(nameof(customAction));

            List<string> allItems = new List<string>(initialItems);

            using (Form window = new Form())
            {
                window.Text = "Named Pipes";
                window.StartPosition = FormStartPosition.CenterScreen;
                window.ClientSize = new Size(900, 550);
                window.MinimumSize = new Size(500, 350);

                ToolStrip toolBar = new ToolStrip();
                toolBar.GripStyle = ToolStripGripStyle.Hidden;
                toolBar.Dock = DockStyle.Top;

                ToolStripLabel searchLabel =
                    new ToolStripLabel("Search:");

                ToolStripTextBox searchTextBox =
                    new ToolStripTextBox();

                searchTextBox.AutoSize = false;
                searchTextBox.Width = 250;
                searchTextBox.ToolTipText =
                    "Enter part of a pipe name to filter the list.";

                ToolStripButton clearSearchButton =
                    new ToolStripButton("Clear");

                clearSearchButton.ToolTipText =
                    "Clear the search filter.";

                ToolStripButton refreshButton =
                    new ToolStripButton("Refresh");

                refreshButton.ToolTipText =
                    "Enumerate the named pipes again.";

                ToolStripButton customActionButton =
                    new ToolStripButton("Run custom action");

                customActionButton.ToolTipText =
                    "Run your custom code for the selected pipe.";

                customActionButton.Enabled = false;

                toolBar.Items.Add(searchLabel);
                toolBar.Items.Add(searchTextBox);
                toolBar.Items.Add(clearSearchButton);
                toolBar.Items.Add(new ToolStripSeparator());
                toolBar.Items.Add(refreshButton);
                toolBar.Items.Add(new ToolStripSeparator());
                toolBar.Items.Add(customActionButton);

                ListBox listBox = new ListBox();
                listBox.Dock = DockStyle.Fill;
                listBox.HorizontalScrollbar = true;
                listBox.IntegralHeight = false;
                listBox.Font = new Font(
                    FontFamily.GenericMonospace,
                    10.0f);

                Button detailsButton = new Button();
                detailsButton.Text = "Process details";
                detailsButton.AutoSize = true;
                detailsButton.Enabled = false;

                Button closeButton = new Button();
                closeButton.Text = "Close";
                closeButton.AutoSize = true;
                closeButton.DialogResult = DialogResult.Cancel;

                Label countLabel = new Label();
                countLabel.AutoSize = true;
                countLabel.Margin = new Padding(8, 10, 8, 0);

                FlowLayoutPanel buttonPanel =
                    new FlowLayoutPanel();

                buttonPanel.Dock = DockStyle.Bottom;
                buttonPanel.Height = 45;
                buttonPanel.FlowDirection =
                    FlowDirection.RightToLeft;
                buttonPanel.Padding = new Padding(5);

                buttonPanel.Controls.Add(closeButton);
                buttonPanel.Controls.Add(detailsButton);
                buttonPanel.Controls.Add(countLabel);

                ToolStripContainer container =
                    new ToolStripContainer();

                container.Dock = DockStyle.Fill;
                container.TopToolStripPanel.Controls.Add(toolBar);
                container.ContentPanel.Controls.Add(listBox);
                container.ContentPanel.Controls.Add(buttonPanel);

                Action applyFilter = delegate
                {
                    string selectedPipe =
                        listBox.SelectedItem as string;

                    string filter = searchTextBox.Text.Trim();

                    listBox.BeginUpdate();

                    try
                    {
                        listBox.Items.Clear();

                        foreach (string item in allItems)
                        {
                            if (filter.Length == 0 ||
                                item.IndexOf(
                                    filter,
                                    StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                listBox.Items.Add(item);
                            }
                        }

                        if (selectedPipe != null)
                        {
                            int index =
                                listBox.Items.IndexOf(selectedPipe);

                            if (index >= 0)
                                listBox.SelectedIndex = index;
                        }
                    }
                    finally
                    {
                        listBox.EndUpdate();
                    }

                    countLabel.Text = string.Format(
                        "Showing {0} of {1}",
                        listBox.Items.Count,
                        allItems.Count);

                    window.Text = string.Format(
                        "Named Pipes ({0})",
                        allItems.Count);
                };

                searchTextBox.TextChanged += delegate
                {
                    applyFilter();
                };

                clearSearchButton.Click += delegate
                {
                    searchTextBox.Clear();
                    searchTextBox.Focus();
                };

                refreshButton.Click += delegate
                {
                    refreshButton.Enabled = false;

                    try
                    {
                        IReadOnlyList<string> refreshedItems =
                            refreshProvider();

                        if (refreshedItems == null)
                        {
                            throw new InvalidOperationException(
                                "The refresh provider returned null.");
                        }

                        allItems =
                            new List<string>(refreshedItems);

                        applyFilter();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            window,
                            "Unable to refresh the pipe list:" +
                            Environment.NewLine +
                            Environment.NewLine +
                            ex.Message,
                            "Refresh failed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                    finally
                    {
                        refreshButton.Enabled = true;
                    }
                };

                listBox.SelectedIndexChanged += delegate
                {
                    bool hasSelection =
                        listBox.SelectedItem != null;

                    detailsButton.Enabled = hasSelection;
                    customActionButton.Enabled = hasSelection;
                };

                detailsButton.Click += delegate
                {
                    ShowSelectedPipeProcess(window, listBox);
                };

                listBox.DoubleClick += delegate
                {
                    if (listBox.SelectedItem != null)
                        ShowSelectedPipeProcess(window, listBox);
                };

                customActionButton.Click += delegate
                {
                    string selectedPipe =
                        listBox.SelectedItem as string;

                    if (selectedPipe == null)
                        return;

                    try
                    {
                        customAction(selectedPipe);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            window,
                            "The custom action failed:" +
                            Environment.NewLine +
                            Environment.NewLine +
                            ex.Message,
                            "Custom action",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                };

                window.Controls.Add(container);
                window.CancelButton = closeButton;

                applyFilter();
                window.ShowDialog();
            }
        }

        private static void ShowSelectedPipeProcess(
            Form owner,
            ListBox listBox)
        {
            string pipeName = listBox.SelectedItem as string;

            if (pipeName == null)
                return;

            try
            {
                PipeProcessInfo processInfo =
                    NamedPipeProcessInspector.GetServerProcessInfo(
                        pipeName);

                ProcessDetailsWindow.Show(owner, processInfo);
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show(
                    owner,
                    ex.Message +
                    Environment.NewLine +
                    Environment.NewLine +
                    "The pipe may be busy, may have disappeared, " +
                    "or access may have been denied.",
                    "Unable to inspect pipe",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    owner,
                    ex.Message,
                    "Unable to inspect pipe",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
