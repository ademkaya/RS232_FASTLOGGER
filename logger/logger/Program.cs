using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;

namespace logger
{
    class Program
    {
        static Thread receiveThread;        
        static SerialPort     serPort;

        static bool     keyPressDetected = false;
        static string   keyPressData = null;

        static void Main(string[] args)
        {
            string[] CommArgs = null;
            string valTemp  = "COM17,1000000,None,8,1";
            string val      = null;

            if (args.Length != 0)
            {
                CommArgs = args[0].Split(new char[] { ',' });
                goto directJump;
            }

            Console.Write("Enter the Comm. Port Settings:  \n");
            Console.Write("Default  :  COM17,1000000,None,8,1 \n");
            Console.Write("Example  :  ComPort,BaudRate,Parity,DataBits,StopBits \n");
            Console.Write("     DataBits :  COM2 \n");
            Console.Write("     Parity   :  None,Even,Mark,Odd,Space \n");
            Console.Write("     DataBits :  5,6,7,8 \n");
            Console.Write("     Baudrate :  57600,115200, ... \n");
            Console.Write("     StopBits :  1,1.5,2 \n");
            val = Console.ReadLine();           
            if (val == string.Empty)
            {
                val = valTemp;
            }
       
            CommArgs = val.Split(new char[] { ',' });
directJump:

            if (CommArgs.Length != 5)
                Error("Missing required argument");

            /* init serport*/
            serPort = new SerialPort();
            serPort.ReceivedBytesThreshold = 1;

            /* init threads*/
            receiveThread = new Thread(() => receiveData());

            /* fill up serport*/
            serPort.PortName = CommArgs[0];
            serPort.BaudRate = Convert.ToInt32(CommArgs[1]);
            if (Enum.TryParse(CommArgs[2], out Parity parity))
            {
                serPort.Parity = parity;
            }
            serPort.DataBits = Convert.ToInt32(CommArgs[3]);
            if (Enum.TryParse(CommArgs[4], out StopBits stopbits))
            {
                serPort.StopBits = stopbits;
            }

            if (serPort.IsOpen)
                Error("Port already open");

            Console.Write(val + "\n");
            Console.Write("Port Opened \n");

            serPort.Open();
                clearPort(serPort);

            /* start receive thread*/
            receiveThread.Start();

            Console.Write("> Press any key to stop \n");
            keyPressData = Console.ReadLine();
            keyPressDetected = true;

        }

        static void receiveData()
        {
            string str = string.Empty;
            while (!keyPressDetected)
            {
                string buffer = serPort.ReadExisting();
                str += buffer;
            }
            writeFile(str);
        }


        static void Error(string ptr)
        {
            Console.Write(ptr);
        }

        static void clearPort( SerialPort ser)
        {
            ser.DiscardInBuffer();
            ser.DiscardOutBuffer();
        }

        static void writeFile(List<string> lst)
        {
           string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
           using (System.IO.StreamWriter file = new System.IO.StreamWriter(desktopPath + "\\test.csv"))
            {
                foreach (string line in lst)
                {
                        file.WriteLine(line);                 
                }
            }
        }
        static void writeFile(string lst)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(desktopPath + "\\test.csv"))
            {
                file.WriteLine(lst);                
            }
        }
    }
}
