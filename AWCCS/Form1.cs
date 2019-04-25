using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        /*
         * Defines
         */
        const String VERSION = "V01.00.07";

        /*
         * Grid Layout
         */
        
        const int GRID_COLUMN_IMAGE = 0;
        const int GRID_COLUMN_WINDOW_TEXT = 1;
        const int GRID_COLUMN_PATH_TEXT = 2;
        const int GRID_COLUMN_WINDOW_ID = 3;
        const int GRID_COLUMN_PROCESS_ID = 4;
        const int GRID_COLUMN_ALWAYS_ON_TOP = 5; 

        /*
         * Globals
         */
        bool GridSetGroupAlwaysOnTopFlag = false;
        bool GridSetAllAlwaysOntopFlag = false;
        bool GridSetAllAlwaysOntopProcessFlag = false;
        int SelectedViewMode = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //WindowControl.FindAllWindowst();

            System.Collections.ArrayList windowNames = WindowControl.getAllWindowNames();

            this.dataGridView1.Rows.Clear();
            this.dataGridView1.Columns.Clear();

            DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
            imageColumn.HeaderText = "Icon";       
            dataGridView1.Columns.Add(imageColumn);

            this.dataGridView1.Columns.Add("Window_Title", "Window Title");
            this.dataGridView1.Columns.Add("Image_Path", "Image Path");

            this.dataGridView1.Columns.Add("Window_Handle", "Window Handle");
            this.dataGridView1.Columns.Add("Process_IS", "Process ID");

            DataGridViewCheckBoxColumn chkCol = new DataGridViewCheckBoxColumn();
            chkCol.HeaderText = "Always On Top";

            dataGridView1.Columns.Add(chkCol);

            for (int i = 0; i < windowNames.Count; i++) 
            {
                this.dataGridView1.Rows.Add();

                this.dataGridView1.Rows[i].Cells[GRID_COLUMN_IMAGE].Value = ((WindowData)windowNames[i]).imageData;

                this.dataGridView1.Rows[i].Cells[GRID_COLUMN_WINDOW_TEXT].Value = ((WindowData)windowNames[i]).WindowText;
                this.dataGridView1.Rows[i].Cells[GRID_COLUMN_WINDOW_TEXT].ReadOnly = true;

                this.dataGridView1.Rows[i].Cells[GRID_COLUMN_PATH_TEXT].Value = ((WindowData)windowNames[i]).imageName;
                this.dataGridView1.Rows[i].Cells[GRID_COLUMN_PATH_TEXT].ReadOnly = true;

                this.dataGridView1.Rows[i].Cells[GRID_COLUMN_WINDOW_ID].Value = ((WindowData)windowNames[i]).windowID;
                this.dataGridView1.Rows[i].Cells[GRID_COLUMN_WINDOW_ID].ReadOnly = true;

                this.dataGridView1.Rows[i].Cells[GRID_COLUMN_PROCESS_ID].Value = ((WindowData)windowNames[i]).processID;
                this.dataGridView1.Rows[i].Cells[GRID_COLUMN_PROCESS_ID].ReadOnly = true;

                if (((WindowData)windowNames[i]).isTopMost == 1)
                {
                    this.dataGridView1.Rows[i].Cells[GRID_COLUMN_ALWAYS_ON_TOP].Value = 1;
                }
                else
                {
                    this.dataGridView1.Rows[i].Cells[GRID_COLUMN_ALWAYS_ON_TOP].Value = 0;
                }
            }

            //so it is always in order of window title
            this.dataGridView1.Sort(dataGridView1.Columns[GRID_COLUMN_WINDOW_TEXT], ListSortDirection.Ascending);

            //set widths
            this.dataGridView1.Columns[GRID_COLUMN_IMAGE].Width = System.Convert.ToInt32((double)this.dataGridView1.Width * 0.10);
            this.dataGridView1.Columns[GRID_COLUMN_WINDOW_TEXT].Width = System.Convert.ToInt32(this.dataGridView1.Width * 0.30);
            this.dataGridView1.Columns[GRID_COLUMN_WINDOW_ID].Width = System.Convert.ToInt32(this.dataGridView1.Width * 0.10);
            this.dataGridView1.Columns[GRID_COLUMN_PROCESS_ID].Width = System.Convert.ToInt32(this.dataGridView1.Width * 0.10);
            this.dataGridView1.Columns[GRID_COLUMN_ALWAYS_ON_TOP].Width = System.Convert.ToInt32(this.dataGridView1.Width * 0.10);
            this.dataGridView1.Columns[GRID_COLUMN_PATH_TEXT].Width = System.Convert.ToInt32(this.dataGridView1.Width * 0.30);

            switch (SelectedViewMode) 
            {
                default:
                case 0:

                    this.dataGridView1.Columns[GRID_COLUMN_PROCESS_ID].Visible = false;
                    this.dataGridView1.Columns[GRID_COLUMN_WINDOW_ID].Visible = false;
                    this.dataGridView1.Columns[GRID_COLUMN_PATH_TEXT].Visible = false;

                    break;

                case 1:
                    this.dataGridView1.Columns[GRID_COLUMN_PROCESS_ID].Visible = true;
                    this.dataGridView1.Columns[GRID_COLUMN_WINDOW_ID].Visible = true;
                    this.dataGridView1.Columns[GRID_COLUMN_PATH_TEXT].Visible = true;
                    break;
            }

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (this.GridSetAllAlwaysOntopProcessFlag == true)
            {
                //we are updating the list, this should avoid fliker
                return;
            }

            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {

                bool state = false;
                try
                {
                    int value = System.Convert.ToInt32(this.dataGridView1.Rows[i].Cells[GRID_COLUMN_ALWAYS_ON_TOP].Value);

                    if(value >= 1)
                    {
                        state = true;
                    }
                    else
                    {
                        //stays false
                    }
                }
                catch 
                {
                    state = false;
                }

                System.String windowHandle= (String)this.dataGridView1.Rows[i].Cells[GRID_COLUMN_WINDOW_ID].Value;

                WindowControl.setWindowState(windowHandle, state);

                System.Diagnostics.Debug.Write("#button1_Click_1(), " + windowHandle + "," + state + "\n");

                //set me as top most
                this.TopMost = true;
                
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.TopMost = true;

            this.toolStripStatusLabel2.Text = VERSION;
        }

        private void Form1_Enter(object sender, EventArgs e)
        {
            
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            //scan when ever we enter the application
            this.button1_Click(sender, e);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            //   System.Diagnostics.Debug.Write("#Push Change\n");
            this.button1_Click_1(sender, (EventArgs)e);   
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
 
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            /*
             * http://stackoverflow.com/questions/3820672/cell-value-changing-event-c
             * 
             * we handle this to updated, then it calls cellvaluechanged
             * 
             */
            this.dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dataGridView1_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //if we double click on the header for the the always on top we set them all on or all off

            //disable the action change listener on the cells
            this.GridSetAllAlwaysOntopProcessFlag = true;

            if (e.ColumnIndex == GRID_COLUMN_ALWAYS_ON_TOP) 
            {
                //correct column
                if (GridSetAllAlwaysOntopFlag == true)
                {
                    for (int i = 0; i < this.dataGridView1.Rows.Count; i++) 
                    {
                        this.dataGridView1.Rows[i].Cells[GRID_COLUMN_ALWAYS_ON_TOP].Value = 0;
                    }
                    GridSetAllAlwaysOntopFlag = false;
                }
                else 
                {
                    for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                    {
                        this.dataGridView1.Rows[i].Cells[GRID_COLUMN_ALWAYS_ON_TOP].Value = 1;
                    }
                    GridSetAllAlwaysOntopFlag = true;
                }
                
            }

            this.GridSetAllAlwaysOntopProcessFlag = false;

            this.button1_Click_1(sender, e);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            if (this.SelectedViewMode == 0)
            {
                this.SelectedViewMode = 1;
            }
            else 
            {
                this.SelectedViewMode = 0;
            }

            this.button1_Click(sender, e);
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //check the column, if it is the ICON column we do the group select option


            if (e.ColumnIndex == GRID_COLUMN_IMAGE) 
            {
                this.GridSetAllAlwaysOntopProcessFlag = true;

                //for any cell with the same process ID as this one we set it to on top
                for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
                {
                    String processA = this.dataGridView1.Rows[i].Cells[GRID_COLUMN_PATH_TEXT].Value.ToString();
                    String processB = this.dataGridView1.Rows[e.RowIndex].Cells[GRID_COLUMN_PATH_TEXT].Value.ToString();

                    if (processA == processB) 
                    {
                        //set as ontop as they are multiple instances of the same process
                        if (GridSetGroupAlwaysOnTopFlag == true)
                        {
                            this.dataGridView1.Rows[i].Cells[GRID_COLUMN_ALWAYS_ON_TOP].Value = 0;
                            
                        }
                        else 
                        {
                            this.dataGridView1.Rows[i].Cells[GRID_COLUMN_ALWAYS_ON_TOP].Value = 1;
                            
                        }
                    }
                }

                //clear the toggle
                if (GridSetGroupAlwaysOnTopFlag == true)
                {
                    GridSetGroupAlwaysOnTopFlag = false;
                }
                else
                {
                    GridSetGroupAlwaysOnTopFlag = true;
                }

                this.GridSetAllAlwaysOntopProcessFlag = false;

                this.button1_Click_1(sender, e);
            }


            

            
        }
    }
}
