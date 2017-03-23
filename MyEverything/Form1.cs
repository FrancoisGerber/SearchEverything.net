using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace MyEverything
{
    public partial class Form1 : Form
    {
        List<string> l;
        DataView dv;

        public Form1()
        {
            try
            {

            InitializeComponent();
            ContextMenu cm = new ContextMenu();
            cm.MenuItems.Add("View", ShowApplication);
            cm.MenuItems.Add("Reload", ReloadApplication);
            cm.MenuItems.Add("Exit", ExitApplication);
            notifyIcon1.ContextMenu = cm;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
        }

        private void ReloadApplication(object sender, EventArgs e)
        {
            try
            {
                LoadDB();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
        }

        private void ShowApplication(object sender, EventArgs e)
        {
            try
            { 
            this.Show();
            this.WindowState = FormWindowState.Normal;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            try
            {
            Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
        }

        private void LoadDB()
        {
            try
            {
                l = new List<string>();
                dv = new DataView();
                this.Text = "My Everything - Searching...";
                notifyIcon1.BalloonTipText = "Searching...";
                notifyIcon1.ShowBalloonTip(500);

                var tokenSource2 = new CancellationTokenSource();
                CancellationToken ct = tokenSource2.Token;

                string[] drives = System.IO.Directory.GetLogicalDrives();
                

                

                Task backgroundDBTask = Task.Factory.StartNew(() =>
                {
                    ct.ThrowIfCancellationRequested();
                    DataGridView.CheckForIllegalCrossThreadCalls = false;

                    foreach (var item in drives)
                    {
                         string dir = item;
                         
                         var fileInfos = new List<FileInfo>();
                         GetFiles(new DirectoryInfo(dir), fileInfos);
                         fileInfos.Sort((x, y) => y.Length.CompareTo(x.Length));
                         this.Text = "My Everything - Compiling List...";
                         notifyIcon1.BalloonTipText = "Compiling List...";
                         notifyIcon1.ShowBalloonTip(500);
                         foreach (var f in fileInfos)
                         {
                             l.Add(f.FullName);
                         }
                    }

                    
                   
                }, tokenSource2.Token);

                backgroundDBTask.ContinueWith((t) =>
                {
                    this.Text = "My Everything - Done!";
                    notifyIcon1.BalloonTipText = "Done!";
                    notifyIcon1.ShowBalloonTip(500);
                    var bindableNames = from item in l
                                        select new
                                        {
                                            Path = item
                                        };

                    DataTable dt = new DataTable();
                    dt = Ultimate.ToDataTable(bindableNames.ToList());
                    dt.TableName = "DataTable1";
                    dv.Table = dt;


                    dataGridView1.DataSource = dv;
                    dataGridView1.Columns[0].Width = dataGridView1.Width;

                    tokenSource2.Cancel();
                },
                TaskScheduler.FromCurrentSynchronizationContext());


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadDB();
            
        }
      
        private static void GetFiles(DirectoryInfo dirInfo, List<FileInfo> files)
        {
            try
            {
                var subDirectories = dirInfo.EnumerateDirectories()
                    .Where(d => (d.Attributes & FileAttributes.System) == 0);
                foreach (DirectoryInfo subdirInfo in subDirectories)
                {
                    GetFiles(subdirInfo, files);
                }
                var filesInCurrentDirectory = dirInfo.EnumerateFiles()
                    .Where(f => (f.Attributes & FileAttributes.System) == 0);
                files.AddRange(filesInCurrentDirectory);
            }
            catch (Exception ex)
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            { 
            DataGridViewCell cell = dataGridView1.SelectedCells[0] as DataGridViewCell;
            string value = cell.Value.ToString();
            if (File.Exists(value))
            {
                Process.Start("explorer.exe", "/select, " + value);
            }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            { 
            if (e.KeyChar == (char)Keys.Return)
            {
                dv.RowFilter = "Path like '%" + textBox1.Text + "%'";
                dataGridView1.DataSource = dv;
            }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
        }

        private void dataGridView1_Resize(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.Columns[0].Width = dataGridView1.Width;
            }
            catch (Exception)
            {
                
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            try
            { 
            if (FormWindowState.Minimized == this.WindowState)
            {
                notifyIcon1.Visible = true;
                notifyIcon1.BalloonTipText = "Running here now!";
                notifyIcon1.ShowBalloonTip(500);
                
                this.Hide();
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
               // notifyIcon1.Visible = false;
            }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            { 
            button1.PerformClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!");
            }
        }
       
    }
}
