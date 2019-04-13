using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Principal;

namespace StudyLayout
{
    public partial class Form1 : Form
    {
        List<WindowPOS.ProcRect> proc = new List<WindowPOS.ProcRect>();
        public Form1()
        {
            InitializeComponent();

            if(!IsUserAdministrator())
                MessageBox.Show("ERROR", "Please restart application as ADMINISTRATOR", MessageBoxButtons.OK, MessageBoxIcon.Error);

            proc = WindowPOS.GetWindowsInfo();
            foreach (var item in proc)
                listView1.Items.Add(new ListViewItem(new string[] { item.proc.ProcessName, item.rect.Left.ToString(), item.rect.Top.ToString(), item.rect.Right.ToString(), item.rect.Bottom.ToString() }));
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            WindowPOS.SaveWindowStates(WindowPOS.GetWindowsInfo(), listView1.SelectedIndices);
        }

        private void cmdRestore_Click(object sender, EventArgs e)
        {
            WindowPOS.RestoreWindowStates();
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            proc = WindowPOS.GetWindowsInfo();
            foreach (var item in proc)
                listView1.Items.Add(new ListViewItem(new string[] { item.proc.ProcessName, item.rect.Left.ToString(), item.rect.Top.ToString(), item.rect.Right.ToString(), item.rect.Bottom.ToString() }));
        }


        public bool IsUserAdministrator()
        {
            bool isAdmin;
            try
            {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }
    }
}
