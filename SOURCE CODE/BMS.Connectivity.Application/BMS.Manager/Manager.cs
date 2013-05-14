/**********************************************************
 * Demo for Standalone SDK.Created by Darcy on Oct.15 2009*
***********************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;


using Microsoft.Win32;
using BMS.APPLICATION.BUSINESSLOGIC;
using System.Configuration;

namespace BMS.Manager
{
    public partial class Manager : Form
    {
        bool canEntry = false;

        BiometricProcess business;        

        public Manager()
        {
            InitializeComponent();

            this.notifyIcon1.DoubleClick += new EventHandler(notifyIcon1_DoubleClick);

            this.Resize += new EventHandler(RTEventsMain_Resize);

            business = new BiometricProcess();

            this.txtIP.Text = ConfigurationManager.AppSettings["IP"];

            this.txtPort.Text = ConfigurationManager.AppSettings["Port"];

            this.notifyIcon1.Text = string.Format("BMS Connectivity Application {0}{1}{0}{2}", Environment.NewLine, ConfigurationManager.AppSettings["IP"], "DisConnected");
        }


        void RTEventsMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }

        void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.notifyIcon1.Visible = false;
        }

        //Create Standalone SDK class dynamicly.
        public zkemkeeper.CZKEMClass axCZKEM1 = new zkemkeeper.CZKEMClass();

        /********************************************************************************************************************************************
        * Before you refer to this demo,we strongly suggest you read the development manual deeply first.                                           *
        * This part is for demonstrating the communication with your device.There are 3 communication ways: "TCP/IP","Serial Port" and "USB Client".*
        * The communication way which you can use duing to the model of the device.                                                                 *
        * *******************************************************************************************************************************************/
        #region Communication
        private bool bIsConnected = false;//the boolean value identifies whether the device is connected
        private int iMachineNumber = 1;//the serial number of the device.After connecting the device ,this value will be changed.

        //If your device supports the TCP/IP communications, you can refer to this.
        //when you are using the tcp/ip communication,you can distinguish different devices by their IP address.
        private void btnConnect_Click(object sender, EventArgs e)
        {
            List<string> tmp = new List<string>();
            business.CanEntryFunctionary("", out tmp);

            if (txtIP.Text.Trim() == "" || txtPort.Text.Trim() == "")
            {
                MessageBox.Show("IP and Port cannot be null", "Error");
                return;
            }
            int idwErrorCode = 0;

            Cursor = Cursors.WaitCursor;
            if (btnConnect.Text == "DisConnect")
            {

                this.axCZKEM1.Disconnect();
                
                this.lbRTShow.Items.Clear();

                this.axCZKEM1.OnFinger -= new zkemkeeper._IZKEMEvents_OnFingerEventHandler(axCZKEM1_OnFinger);
                this.axCZKEM1.OnVerify -= new zkemkeeper._IZKEMEvents_OnVerifyEventHandler(axCZKEM1_OnVerify);
                this.axCZKEM1.OnAttTransactionEx -= new zkemkeeper._IZKEMEvents_OnAttTransactionExEventHandler(axCZKEM1_OnAttTransactionEx);
                this.axCZKEM1.OnKeyPress -= new zkemkeeper._IZKEMEvents_OnKeyPressEventHandler(axCZKEM1_OnKeyPress);
                this.axCZKEM1.OnEnrollFinger -= new zkemkeeper._IZKEMEvents_OnEnrollFingerEventHandler(axCZKEM1_OnEnrollFinger);
                this.axCZKEM1.OnDoor -= new zkemkeeper._IZKEMEvents_OnDoorEventHandler(axCZKEM1_OnDoor);

                bIsConnected = false;
                btnConnect.Text = "Connect";
                lblState.Text = "Current State: DisConnected";
                
                Cursor = Cursors.Default;
                return;
            }

            bIsConnected = axCZKEM1.Connect_Net(txtIP.Text, Convert.ToInt32(txtPort.Text));

            if (bIsConnected == true)
            {
                btnConnect.Text = "DisConnect";
                btnConnect.Refresh();
                lblState.Text = "Current State: Connected";
                this.notifyIcon1.Text = string.Format("BMS Connectivity Application {0}{1}{0}{2}", Environment.NewLine, ConfigurationManager.AppSettings["IP"], "Connected");
                iMachineNumber = 1;//In fact,when you are using the tcp/ip communication,this parameter will be ignored,that is any integer will all right.Here we use 1.

                if (this.axCZKEM1.RegEvent(iMachineNumber, 65535))//Here you can register the realtime events that you want to be triggered(the parameters 65535 means registering all)
                {
                    this.axCZKEM1.OnFinger+=new zkemkeeper._IZKEMEvents_OnFingerEventHandler(axCZKEM1_OnFinger);
                    this.axCZKEM1.OnVerify+=new zkemkeeper._IZKEMEvents_OnVerifyEventHandler(axCZKEM1_OnVerify);
                    this.axCZKEM1.OnAttTransactionEx+=new zkemkeeper._IZKEMEvents_OnAttTransactionExEventHandler(axCZKEM1_OnAttTransactionEx);
                    this.axCZKEM1.OnKeyPress+=new zkemkeeper._IZKEMEvents_OnKeyPressEventHandler(axCZKEM1_OnKeyPress);
                    this.axCZKEM1.OnEnrollFinger+=new zkemkeeper._IZKEMEvents_OnEnrollFingerEventHandler(axCZKEM1_OnEnrollFinger);
                    this.axCZKEM1.OnDoor+=new zkemkeeper._IZKEMEvents_OnDoorEventHandler(axCZKEM1_OnDoor);
                }
             }
            else
            {
                axCZKEM1.GetLastError(ref idwErrorCode);
                MessageBox.Show("Unable to connect the device,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }
            Cursor = Cursors.Default;
        }

        #endregion

        /*************************************************************************************************
        * Before you refer to this demo,we strongly suggest you read the development manual deeply first.*
        * This part is for demonstrating the RealTime Events that triggered  by your operations          *
        **************************************************************************************************/
        #region RealTime Events

        //When you place your finger on sensor of the device,this event will be triggered
        private void axCZKEM1_OnFinger()
        {
            lbRTShow.Items.Add("BMS OnFinger Has been Triggered");
        }

        //After you have placed your finger on the sensor(or swipe your card to the device),this event will be triggered.
        //If you passes the verification,the returned value userid will be the user enrollnumber,or else the value will be -1;
        private void axCZKEM1_OnVerify(int iUserID)
        {
            lbRTShow.Items.Add("BMS OnVerify Has been Triggered,Verifying...");
            if (iUserID != -1)
            {
                lbRTShow.Items.Add("Verified OK,the UserID is " + iUserID.ToString());
            }
            else
            {
                lbRTShow.Items.Add("Verified Failed... ");
            }
        }

        //If your fingerprint(or your card) passes the verification,this event will be triggered
        private void axCZKEM1_OnAttTransactionEx(string sEnrollNumber, int iIsInValid, int iAttState, int iVerifyMethod, int iYear, int iMonth, int iDay, int iHour, int iMinute, int iSecond,int iWorkCode)
        {
            List<string> messages = null;

            canEntry = business.CanEntryFunctionary(sEnrollNumber, out messages) == true ? true: business.CanEntryPerson(sEnrollNumber, out messages) ;

            lbRTShow.Items.Add("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■");
            lbRTShow.Items.Add("BMS OnAttTrasactionEx Has been Triggered,Verified OK");
            lbRTShow.Items.Add("...Time:" + iYear.ToString() + "-" + iMonth.ToString() + "-" + iDay.ToString() + " " + iHour.ToString() + ":" + iMinute.ToString() + ":" + iSecond.ToString());
            lbRTShow.Items.Add("...UserID:" + sEnrollNumber);
            lbRTShow.Items.AddRange(messages.ToArray());
            lbRTShow.Items.Add("BMS OnAttTrasactionEx Has been End");
            lbRTShow.Items.Add("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■");

            if (canEntry)
            {
                axCZKEM1.ACUnlock(int.Parse(ConfigurationManager.AppSettings["MachineNumber"].ToString()), int.Parse(ConfigurationManager.AppSettings["Delay"].ToString()));
            }
        }

        //When you are enrolling your finger,this event will be triggered.
        private void axCZKEM1_OnEnrollFinger(int iEnrollNumber,int iFingerIndex,int iActionResult,int iTemplateLength)
        {
            if (iActionResult == 0)
            {
                lbRTShow.Items.Add("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■");
                lbRTShow.Items.Add("BMS OnEnrollFiger Has been Triggered....");
                lbRTShow.Items.Add(".....UserID: " + iEnrollNumber + " Index: " + iFingerIndex.ToString() + " tmpLen: " + iTemplateLength.ToString());
                lbRTShow.Items.Add("BMS OnEnrollFiger Has been End....");
                lbRTShow.Items.Add("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■");
            }
            else
            {
                lbRTShow.Items.Add("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■");
                lbRTShow.Items.Add("BMS OnEnrollFiger was Triggered by Error");
                lbRTShow.Items.Add("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■");
            }
        }

        //When you press the keypad,this event will be triggered.
        private void axCZKEM1_OnKeyPress(int iKey)
        {
            lbRTShow.Items.Add("BMS OnKeyPress Has been Triggered, Key: " + iKey.ToString());
        }

        //Door sensor event
        private void axCZKEM1_OnDoor(int iEventType)
        {
            lbRTShow.Items.Add("BMS Ondoor Has been Triggered...");
            lbRTShow.Items.Add("...EventType=" + iEventType.ToString());
        }

        //When you have emptyed the Mifare card,this event will be triggered.
        private void axCZKEM1_OnEmptyCard(int iActionResult)
        {
            lbRTShow.Items.Add("BMS OnEmptyCard Has been Triggered...");
            if (iActionResult == 0)
            {
                lbRTShow.Items.Add("...Empty Mifare Card OK");
            }
            else
            {
                lbRTShow.Items.Add("...Empty Failed");
            }
        }


        //After function GetRTLog() is called ,RealTime Events will be triggered. 
        //When you are using these two functions, it will request data from the device forwardly.
        private void rtTimer_Tick(object sender, EventArgs e)
        {
            while (bIsConnected == true)
            {
                if (axCZKEM1.ReadRTLog(iMachineNumber))
                {
                    while (axCZKEM1.GetRTLog(iMachineNumber))
                    {
                        ;
                    }
                }
            }
        }

        #endregion

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bIsConnected)
            {
                axCZKEM1.Disconnect();
                lbRTShow.Items.Clear();
            }

            this.Dispose();
        }



    }
} 