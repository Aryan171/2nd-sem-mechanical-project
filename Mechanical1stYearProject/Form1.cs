using System.Net.Mail;

namespace Mechanical1stYearProject
{
    public partial class Form1 : Form
    {
        List<Button> pointLoadButtons, udlButtons;
        List<UdlValue> udlValues;
        List<PointLoadValue> pointLoadValues;
        Label udlLabel, pointLoadLabel;

        float barLength = 1.0f;
        float rfA, rfB; // reaction forces at the two ends of the bar

        float[] fd; // force diagram
        float[] bmd; // bending moment diagram
        float[] sfd; // sheer force diagram

        int numberOfSteps = 1000;

        public class UdlValue
        {
            public UdlValue(float start, float end, float udl, Color color)
            {
                this.start = start;
                this.end = end;
                this.udl = udl;
                this.color = color;
            }

            public UdlValue()
            {
                this.start = 0.0f;
                this.end = 0.0f;
                this.udl = 0.0f;
                this.color = Color.White; ;
            }

            public float start, end, udl;
            public Color color;
        }

        public class PointLoadValue
        {
            public PointLoadValue(float point, float load, Color color)
            {
                this.point = point;
                this.load = load;
                this.color = color;
            }

            public PointLoadValue()
            {
                this.point = 0.0f;
                this.load = 0.0f;
                this.color = Color.White;
            }

            public float point, load;
            public Color color;
        }

        public Form1()
        {
            pointLoadButtons = new List<Button>();
            udlButtons = new List<Button>();

            pointLoadValues = new List<PointLoadValue>();
            udlValues = new List<UdlValue>();

            fd = new float[numberOfSteps];
            bmd = new float[numberOfSteps];
            sfd = new float[numberOfSteps];

            Button addUdl = new Button(),
                addPointLoad = new Button();

            addUdl.Text = "Add UDL";
            addPointLoad.Text = "Add point force";

            addUdl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            addPointLoad.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            addUdl.AutoSize = true;
            addPointLoad.AutoSize = true;

            addUdl.FlatAppearance.BorderSize = 0;
            addPointLoad.FlatAppearance.BorderSize = 0;

            addUdl.FlatStyle = FlatStyle.Flat;
            addPointLoad.FlatStyle = FlatStyle.Flat;

            addUdl.TextAlign = ContentAlignment.MiddleLeft;
            addPointLoad.TextAlign = ContentAlignment.MiddleCenter;

            addUdl.Click += new System.EventHandler(this.addUdl_Click);
            addPointLoad.Click += new System.EventHandler(this.addPointLoad_Click);

            pointLoadButtons.Add(addPointLoad);
            pointLoadValues.Add(new PointLoadValue());
            udlButtons.Add(addUdl);
            udlValues.Add(new UdlValue());

            udlLabel = new Label();
            pointLoadLabel = new Label();

            udlLabel.Text = "UDLs:-";
            pointLoadLabel.Text = "Point Loads:-";

            udlLabel.AutoSize = true;
            pointLoadLabel.AutoSize = true;

            udlLabel.Dock = DockStyle.Left;
            pointLoadLabel.Dock = DockStyle.Left;

            InitializeComponent();
        }

        private void addUdl_Click(object sender, EventArgs e)
        {
            Button b = new Button();

            b.Text = udlButtons.Count + ") UDL";
            b.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            b.AutoSize = true;
            b.Font = new Font("Segoe UI", 8);
            b.TextAlign = ContentAlignment.MiddleLeft;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Click += new System.EventHandler(udlButtons_Click);

            udlButtons.Add(b);
            udlValues.Add(new UdlValue());//*********************************************
            SetLoadsList();
        }

        private void addPointLoad_Click(object sender, EventArgs e)
        {
            Button b = new Button();

            b.Text = pointLoadButtons.Count + ") Point Load";
            b.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            b.AutoSize = true;
            b.Font = new Font("Segoe UI", 8);
            b.TextAlign = ContentAlignment.MiddleLeft;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Click += new System.EventHandler(pointLoadButtons_Click);

            pointLoadButtons.Add(b);
            pointLoadValues.Add(new PointLoadValue()); // *********************************

            SetLoadsList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadsList.Visible = !LoadsList.Visible;
            if (LoadsList.Visible)
            {
                SetLoadsList();
            }
        }

        private void SetLoadsList()
        {
            LoadsList.Controls.Clear();
            LoadsList.RowCount = Math.Max(pointLoadButtons.Count, udlButtons.Count) + 1;
            LoadsList.ColumnCount = 2;

            LoadsList.Controls.Add(pointLoadLabel, 0, 0);

            for (int i = 0; i < pointLoadButtons.Count; i++)
            {
                LoadsList.Controls.Add(pointLoadButtons[i], 0, i + 1);
            }

            LoadsList.Controls.Add(udlLabel, 1, 0);

            for (int i = 0; i < udlButtons.Count; i++)
            {
                LoadsList.Controls.Add(udlButtons[i], 1, i + 1);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox1.Text = "0.0";
                barLength = 0.0f;
            }
            try
            {
                barLength = float.Parse(textBox1.Text);
            }
            catch (Exception ex)
            {
                textBox1.Text = Convert.ToString(barLength);
            }
        }

        private void updateDiagrams()
        {
            // updating the fd here

            // first adding all the point loads
            foreach(PointLoadValue p in pointLoadValues)
            {
                fd[(int)Math.Round(p.point * numberOfSteps / barLength)] += p.point;
            }

            // now adding all the udls
            foreach(UdlValue u in udlValues)
            {
                int istart = (int)Math.Round(u.start * numberOfSteps / barLength);
                int iend = (int)Math.Round(u.end * numberOfSteps / barLength);

                float f = ((u.end - u.start) * u.udl) / (iend - istart);

                for (int i = istart; i < iend; i++)
                {
                    fd[i] += f;
                }
            }

            // updating the sfd here
            sfd[0] = fd[0];

            for(int i = 1; i < numberOfSteps; i++)
            {
                sfd[i] = sfd[i - 1] + fd[i];
            }

            // updating the bmd here
            bmd[0] = 0;
            for(int i = 1; i < numberOfSteps; i++)
            {
                bmd[i] = bmd[i - 1] + ((i * barLength / (float)numberOfSteps) * fd[i]);
            }
        }

        // this function is called whenever a udl button is clicked
        private void udlButtons_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            udlValues.RemoveAt(udlButtons.IndexOf(b));
            udlButtons.Remove(b);
            SetLoadsList();
        }

        // this function is called whenever a point load button is clicked
        private void pointLoadButtons_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            pointLoadValues.RemoveAt(pointLoadButtons.IndexOf(b));
            pointLoadButtons.Remove(b);
            SetLoadsList();
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
