using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Hive_Simulation
{

    public partial class Form1 : Form
    {
        World world;
        Random random = new Random();
        private DateTime start = DateTime.Now;
        private DateTime end;
        private int framesRun = 0;

        HiveForm hiveForm = new HiveForm();
        FieldForm fieldForm = new FieldForm();
        Renderer renderer;

        public Form1()
        {
            InitializeComponent();
            world = new World(new MessageDelegate(SendMessage));

            timer1.Interval = 50;
            timer1.Tick += new EventHandler(RunFrame);
            timer1.Enabled = false;
            UpdateStats(new TimeSpan());
            hiveForm.Show(this);
            fieldForm.Show(this);
            MoveChildForms();
            ResetSimulator();
            
        }



        private void UpdateStats(TimeSpan frameDuration)
        {
            lbBees.Text = world.Bees.Count.ToString();
            lbFlowers.Text = world.Flowers.Count.ToString();
            lbHoneyInHive.Text = String.Format("{0:f3}", world.Hive.Honey);
            double nectar = 0;
            foreach (Flower flower in world.Flowers)
                nectar += flower.Nectar;
            lbNectarInFlowers.Text = String.Format("{0:f3}", nectar);
            lbFramesRun.Text = framesRun.ToString();
            double milliseconds = frameDuration.TotalMilliseconds;
            if (milliseconds != 0.0)
                lbFrameRate.Text = String.Format("{0:f0} ({1:f1})ms", 1000 / milliseconds, milliseconds);
            else
                lbFrameRate.Text = "N/A";
        }


        public void RunFrame(object sender, EventArgs e)
        {
            framesRun++;
            world.Go(random);
            renderer.Render();
            end = DateTime.Now;
            TimeSpan frameDuration = start - end;
            start = end;
            UpdateStats(frameDuration);
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                btStart.Text = "Resume Simulation";
                timer1.Stop();
                statusStrip1.Items[0].Text ="Simulation Paused";
            }
            else
            {
                btStart.Text = "Pause Simulation";
                timer1.Start();
            }





        }

        private void btReset_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            renderer.Reset();
            ResetSimulator();                      
            toolStrip1.Items[0].Text = "Start Simulation";
            listBox1.Items.Clear();
            UpdateStats(new TimeSpan());
        }

        private void SendMessage(int ID, string Message)
        {
            statusStrip1.Items[0].Text = "Bee # " + ID + ": " + Message;
            var beeGroups = from bee in world.Bees
                            group bee by bee.CurrentState into beeGroup
                            orderby beeGroup.Key
                            select beeGroup;
            listBox1.Items.Clear();
            foreach (var group in beeGroups)
            {
                string s;
                if (group.Count() == 1)
                    s = "";
                else
                    s = "s";
                listBox1.Items.Add(group.Key.ToString() + ": "
                                   + group.Count() + " bee" + s);
                if (group.Key == BeeState.Idle && group.Count() == world.Bees.Count && framesRun > 0)
                {
                    listBox1.Items.Add("Simulation ended: all bees are idle");
                    toolStrip1.Items[0].Text = "Simulation ended";
                    timer1.Enabled = false;
                }
            }



        }



        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            World CurrentWorld = this.world;
            int FramesRun = this.framesRun;
            bool enabled = timer1.Enabled;
            if (enabled)
                timer1.Stop();
            openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Simulator File (*.bees)|*.bees";
            openFileDialog1.Title = "Open simulation";
            openFileDialog1.CheckPathExists = true;
            openFileDialog1.CheckFileExists = true;


            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                try
                {
                    using (FileStream stream = File.OpenRead(openFileDialog1.FileName))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        world = (World)formatter.Deserialize(stream);
                        framesRun = (int)formatter.Deserialize(stream);
                        UpdateStats(new TimeSpan());
                    }
                }


                catch (Exception ex)
                {
                    MessageBox.Show("Unnable to open simulation file,ex.Message\r\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.world = CurrentWorld;
                    this.framesRun = FramesRun;
                }

            world.Hive.messageSender = new MessageDelegate(SendMessage);
            foreach (Bee bee in world.Bees)
            {
                bee.MessageSender += world.Hive.messageSender;
            }

            if (enabled)
                timer1.Start();
            else
            {
                toolStrip1.Items[0].Text = "Resume Simulation";
                statusStrip1.Items[0].Text = "Simulation paused";
            }

            renderer.Reset();
            renderer = new Renderer(world, hiveForm, fieldForm);


        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            bool isEnabled = timer1.Enabled;
            saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Simulator File (*.bees)|*.bees";
            saveFileDialog1.Title = "Simulation Save";
            saveFileDialog1.CheckPathExists = true;
            if (isEnabled)
                timer1.Stop();
            try
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    using (FileStream saveStream = File.Create(saveFileDialog1.FileName))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(saveStream, this.world);
                        formatter.Serialize(saveStream, this.framesRun);
                    }
                    MessageBox.Show("Current simulation is saved at "+saveFileDialog1.FileName,"Saving success",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Current simulation is not saved: "+ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }

            if (isEnabled)
                timer1.Start();
        }

        private void MoveChildForms()
        {
            hiveForm.Location = new Point(this.Location.X + this.Width + 10, this.Location.Y);
            fieldForm.Location = new Point(this.Location.X, this.Location.Y + Math.Max(this.Height, hiveForm.Height) + 10);
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            MoveChildForms();
        }

        private void ResetSimulator()
        {
            framesRun = 0;
            world = new World(new MessageDelegate(this.SendMessage));
            renderer = new Renderer(this.world, this.hiveForm, this.fieldForm);
        }





    }
}
