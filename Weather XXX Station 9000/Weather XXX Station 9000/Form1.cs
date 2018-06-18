using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;

namespace Weather_XXX_Station_9000
{
    public partial class Form1 : Form
    {

        string temperature = "0";
        string humidity = "0"; 
        bool parity = true;
        SerialPort port;
        bool isConnected = false;
        string sensorState = "0";
        bool isSaving = false;

        System.Windows.Forms.Timer timerFreezeButton = new System.Windows.Forms.Timer();


        public Form1()
    {
        InitializeComponent();
        list();

    }


    private void connectButton_Click(object sender, EventArgs e)
    {
        if (!isConnected)
        {
            connectToArduino();
        }
        else
        {
            disconnectFromArduino();
        }
    }

    private void stopSave_Click(object sender, EventArgs e)
    {
        saveButton.Enabled = true;
        isSaving = false;
        stopSave.Enabled = false;
    }


        private void refreshButton_Click(object sender, EventArgs e)
    {
        comboBox1.Items.Clear();
        list();
    }

    void timerFreezeButton_Tick(object sender, System.EventArgs e)
    {
        connectButton.Enabled = true;
        saveButton.Enabled = true;
        timerFreezeButton.Stop();
    }


    public void list()
    {
        string[] ports = SerialPort.GetPortNames();
        foreach(string port in ports)
        {
            comboBox1.Items.Insert(0, port);
        }
        if (ports.Length > 0)
        {
            comboBox1.SelectedIndex = 0;
        }
    }


    private void connectToArduino()
    {
        isConnected = true;
        string selectedPort = comboBox1.GetItemText(comboBox1.SelectedItem);
        port = new SerialPort(selectedPort, 9600, Parity.None, 8, StopBits.One);
       timerFreezeButton.Interval = 1500; // here time in milliseconds
       timerFreezeButton.Tick += timerFreezeButton_Tick;
       timerFreezeButton.Start();
        connectButton.Enabled = false;

       try
       {
           port.Open();
           sensorState = "1";
           port.Write(sensorState);
           status.Text = "Connected to Arduino";
           comboBox1.Enabled = false;
           connectButton.Text = "Disconect";
           port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
       }
       catch (Exception  ex)
       {
           MessageBox.Show(ex.Message);
           MessageBox.Show("Wrong serial port!", "Error",
           MessageBoxButtons.OK, MessageBoxIcon.Error);
           comboBox1.Enabled = true;
           isConnected = false;
           isSaving = false;
           status.Text = "Choose diffrent port!";
           connectButton.Text = "Connect";
       }
    }

    private void disconnectFromArduino()
    {
        try
        {
            isConnected = false;
            sensorState = "0";
            port.WriteLine(sensorState);
            port.Close();
            status.Text = "Disconected from Arduino ";
            comboBox1.Enabled = true;
            saveButton.Enabled = false;
            connectButton.Text = "Connect";
            pole_humm.Text = string.Empty;
            pole_temp.Text = string.Empty;
            isSaving = false;
            }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void DataReceivedHandler(object sender, System.IO.Ports.SerialDataReceivedEventArgs  e) // Instrukcje jakie mają się wykonac w momencie odebrania danych z portu szeregowego.
    {
            while (isConnected == true && sensorState == "1" )
            {
                    try
                    {
                        if (parity == true) // sprawdzenie którą wartośc z kolei odbiera
                        {
                            temperature = port.ReadLine(); // odczyt linii - temperatura
                        
                        this.Invoke(new EventHandler(DisplayText_pole_temp));
                            parity = !parity; // negacja zmiennej boolean
                        }
                        else
                        {
                            humidity = port.ReadLine();// odczyt linii - wilgotnośc
                            this.Invoke(new EventHandler(DisplayText_pole_humm)); // wywołanie metody wyświetlania wilgotności
                            parity = !parity;
                        }
                    }
                    catch(Exception)
                    {
                        parity = false;

                        MessageBox.Show("You have benn disconected succesfully!", "Disconnected",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                        port.Close();
                        isConnected = false;
                        isSaving = false;                      
                    }
            }
            parity = true;
        }
           


            
        


    private void Form1_FormClosed(object sender, FormClosedEventArgs e) // instrukcje jakie mają się wykonac w przypadku wyłączenia okna programu
    {
        port.Close();
    }


    private void DisplayText_pole_temp(object sender, EventArgs e)
    {
        if (isConnected == true && sensorState == "1")
        {
            pole_temp.Text = temperature + " ℃";
        }
        else
        {
            pole_temp.Text = string.Empty;

        }
    }

    private void DisplayText_pole_humm(object sender, EventArgs e)
    {
        if (isConnected == true && sensorState == "1")
        { 
        pole_humm.Text = humidity + " %";
        }
        else
        {
        pole_humm.Text = string.Empty;
        }
    }


    private void saveButton_Click(object sender, EventArgs e)
    {
        saveButton.Enabled = false;
        stopSave.Enabled = true;
        isSaving = true;


        var time = DateTime.Now;
        string formattedTimeFileValue = time.ToString("hh:mm");
        string formattedTimeFileName = time.ToString("yyyy-MM-dd");
        string fileName = formattedTimeFileName +".csv";

        string cvsPath = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName;
        string csvFilePath = Path.Combine(cvsPath, fileName);

        try
        {
            File.WriteAllText(csvFilePath,  "Hours:Minutes" + ";" + "Temperature" + ";" + "Humidity" + "\n\r");

        }
        catch(Exception)
        {
            MessageBox.Show("File ocuppied! Close file first", "Error!",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
            isSaving = false;

        }


        var startTimeSpan = TimeSpan.Zero;
        var periodTimeSpan = TimeSpan.FromMinutes(5);


        this.Activate();
        while (isSaving == true)
        {
            if (isConnected == true && sensorState == "1")
            {
                System.IO.File.AppendAllText(csvFilePath,  formattedTimeFileValue + ";" + temperature + ";"+  humidity + "\n\r");
                Application.DoEvents();
            }
            else
            {
                break;
            }
        }





    }

    private void label2_Click(object sender, EventArgs e)
    {

    }

    private void Form1_Load(object sender, EventArgs e)
    {

    }
    private void status_TextChanged(object sender, EventArgs e)
    {

    }

    private void label1_Click(object sender, EventArgs e)
    {

    }
    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {

    }


    }
}
