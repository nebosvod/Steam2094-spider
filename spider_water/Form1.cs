using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using MySql.Data;
using System.Threading;
using System.IO;
using System.Net.Mail;
using System.Web;
using System.Net;

namespace spider_water
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        public static float GetVolume(string date_req)
        {
            string[] portnames = SerialPort.GetPortNames();
           // SerialPort port = new SerialPort("COM6", 57600, Parity.None, 8, StopBits.One);
            SerialPort port = new SerialPort("COM5", 57600, Parity.None, 8, StopBits.One);

            byte[] data1 = { 0x01, 0x10, 0xD9, 0x71, 0x00, 0x00, 0x02, 0xA5, 0x00, 0x9E, 0xD0 };
            byte[] data2 = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x00, 0x45, 0xCA };  // Чтение конфигурации архивов
            //byte[] data2 = { 0x03, 0x03, 0x0B, 0x01, 0x00, 0x00, 0x17, 0xCC }; // Чтение текущих значений
            byte[] data3 = { 0x01, 0x03, 0x0C, 0x01, 0x00, 0x00, 0x17, 0x5A };
            byte[] data4 = { 0x01, 0x03, 0x0E, 0x01, 0x00, 0x00, 0x16, 0xE2 };


            port.Open();

            port.Write(data1, 0, data1.Length);
            Thread.Sleep(200);

            int byteRecieved = port.BytesToRead;
            byte[] messByte = new byte[byteRecieved];
            port.Read(messByte, 0, byteRecieved);
            Thread.Sleep(200);


            port.Write(data2, 0, data2.Length);
            Thread.Sleep(200);
            int byteRecieved2 = port.BytesToRead;
            byte[] messByte2 = new byte[byteRecieved2];
            port.Read(messByte2, 0, byteRecieved2);
            Thread.Sleep(200);

            //  for (int k = 0; k <= 7; k++)
            //  {
            //      port.Write(data3, 0, data3.Length);
            //      Thread.Sleep(200);
            //
            //    int byteRecieved3 = port.BytesToRead;
            //    byte[] messByte3 = new byte[byteRecieved3];
            //    port.Read(messByte3, 0, byteRecieved3);
            //    Thread.Sleep(200);
            // }



            string ds = "";
            string d1 = "";
            string d2 = "";
            string d3 = "";

            for (int k = 0; k < byteRecieved2; k++)
            {
                if (messByte2[k].ToString("X").Length == 1)
                {
                    ds = ds + "0" + messByte2[k].ToString("X");
                }
                else
                {
                    ds = ds + messByte2[k].ToString("X");
                }
            }


            d1 = d1 + ds[14] + ds[15] + ds[16] + ds[17];
            d2 = d2 + ds[28] + ds[29] + ds[30] + ds[31];
            d3 = d3 + ds[42] + ds[43] + ds[44] + ds[45];

            //----------------- Чтение дескрипторов архива --------------------------------------------------------------------

            byte[] data5 = { 0x01, 0x10, 0xB7, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0xE1, 0xBA };

            port.Write(data5, 0, data5.Length);
            Thread.Sleep(200);
            int byteRecieved5 = port.BytesToRead;
            byte[] messByte5 = new byte[byteRecieved5];
            port.Read(messByte5, 0, byteRecieved5);
            Thread.Sleep(200);

            for (int k = 0; k < 8; k++)
            {
                port.Write(data3, 0, data3.Length);
                Thread.Sleep(200);

                int byteRecieved3 = port.BytesToRead;
                byte[] messByte3 = new byte[byteRecieved3];
                port.Read(messByte3, 0, byteRecieved3);
                Thread.Sleep(200);
            }


            //-----------------------------------------------------------------------------------------------------------------

            //----------------- Чтение дескрипторов файлов --------------------------------------------------------------------
            //Получаем год, месяц, значение ячейки памяти           
            byte[] data6 = { 0x01, 0x10, 0xB7, 0x00, 0x00, 0x00, 0x04, 0x00, 0x04, 0x00, 0x00, 0xA0, 0x7B };
            string[] dfs = new string[6];

            port.Write(data6, 0, data6.Length);
            Thread.Sleep(200);
            int byteRecieved6 = port.BytesToRead;
            byte[] messByte6 = new byte[byteRecieved6];
            port.Read(messByte6, 0, byteRecieved6);
            Thread.Sleep(200);



            for (int k = 0; k < 6; k++)
            {
                port.Write(data3, 0, data3.Length);
                Thread.Sleep(300);
                int byteRecieved3 = port.BytesToRead;
                byte[] messByte3 = new byte[byteRecieved3];
                port.Read(messByte3, 0, byteRecieved3);
                Thread.Sleep(300);


                for (int i = 0; i < byteRecieved3; i++)
                {
                    if (messByte3[i].ToString("X").Length == 1)
                    {
                        dfs[k] = dfs[k] + "0" + messByte3[i].ToString("X");
                    }
                    else
                    {
                        dfs[k] = dfs[k] + messByte3[i].ToString("X");
                    }
                }


            }

            string[] date_year = new string[24];
            string[] date_month = new string[24];
            string[] memory_s = new string[24];
            Thread.Sleep(1000);
            for (int k = 0; k < 6; k++)
            {
                date_year[k * 4] = Convert.ToString(dfs[k][14]) + Convert.ToString(dfs[k][15]);
                date_month[k * 4] = Convert.ToString(dfs[k][16]) + Convert.ToString(dfs[k][17]);
                memory_s[k * 4] = Convert.ToString(dfs[k][22]) + Convert.ToString(dfs[k][23]) + Convert.ToString(dfs[k][24]) + Convert.ToString(dfs[k][25]) + Convert.ToString(dfs[k][26]) + Convert.ToString(dfs[k][27]);

                date_year[4 * k + 1] = Convert.ToString(dfs[k][30]) + Convert.ToString(dfs[k][31]);
                date_month[4 * k + 1] = Convert.ToString(dfs[k][32]) + Convert.ToString(dfs[k][33]);
                memory_s[4 * k + 1] = Convert.ToString(dfs[k][38]) + Convert.ToString(dfs[k][39]) + Convert.ToString(dfs[k][40]) + Convert.ToString(dfs[k][41]) + Convert.ToString(dfs[k][42]) + Convert.ToString(dfs[k][43]);

                date_year[4 * k + 2] = Convert.ToString(dfs[k][46]) + Convert.ToString(dfs[k][47]);
                date_month[4 * k + 2] = Convert.ToString(dfs[k][48]) + Convert.ToString(dfs[k][49]);
                memory_s[4 * k + 2] = Convert.ToString(dfs[k][54]) + Convert.ToString(dfs[k][55]) + Convert.ToString(dfs[k][56]) + Convert.ToString(dfs[k][57]) + Convert.ToString(dfs[k][58]) + Convert.ToString(dfs[k][59]);

                date_year[4 * k + 3] = Convert.ToString(dfs[k][62]) + Convert.ToString(dfs[k][63]);
                date_month[4 * k + 3] = Convert.ToString(dfs[k][64]) + Convert.ToString(dfs[k][65]);
                memory_s[4 * k + 3] = Convert.ToString(dfs[k][70]) + Convert.ToString(dfs[k][71]) + Convert.ToString(dfs[k][72]) + Convert.ToString(dfs[k][73]) + Convert.ToString(dfs[k][74]) + Convert.ToString(dfs[k][75]);

            }

            //-----------------------------------------------------------------------------------------------------------------


            //----------------- Чтение суточных данных за месяц --------------------------------------------------------------------


            byte[] data222 = { };

            string[] md = new string[2];



            port.Write(data4, 0, data4.Length);
            Thread.Sleep(200);
            int byteRecieved4 = port.BytesToRead;
            byte[] messByte4 = new byte[byteRecieved4];
            port.Read(messByte4, 0, byteRecieved4);
            Thread.Sleep(200);

            port.Close();
            //         for (int k = 0; k <= 62; k++)

            int year_req;

            string memory_req = "";

            year_req = Convert.ToInt16(date_req.Substring(0, 4));


            for (int k = 0; k < 24; k++)
            {
                if (date_year[k] == Convert.ToString((year_req - 1972), 16).ToUpper() && date_month[k] == date_req.Substring(4, 2))
                {
                    memory_req = memory_s[k];
                }
            }
            string memory_req_rev = memory_req.Substring(2, 2) + memory_req.Substring(0, 2);

            int num = Int32.Parse(memory_req_rev, System.Globalization.NumberStyles.HexNumber);
            int l = Convert.ToInt16(date_req.Substring(6, 2));
            num = (num - 8)+8*l;
            //if (date_req.Substring(4, 2) == "03" && l == 1)
            //{
            //    num = (num - 64) + 64 * (l - 3);
            //}
            string hexValue = num.ToString("X");



            byte[] data111 = { 0x01, 0x10, 0xB7, 0x00, 0x00, 0x00, 0x04, byte.Parse(hexValue.Substring(2, 2), System.Globalization.NumberStyles.HexNumber), byte.Parse(hexValue.Substring(0, 2), System.Globalization.NumberStyles.HexNumber), 0x00, 0x00 };
            data222 = GetCRC(data111);

            port.Open();

            port.Write(data222, 0, data222.Length);
            Thread.Sleep(200);

            int byteRecieved222 = port.BytesToRead;
            byte[] messByte222 = new byte[byteRecieved222];
            port.Read(messByte222, 0, byteRecieved222);
            Thread.Sleep(200);

            for (int k = 0; k < 1; k++)
            {
                port.Write(data3, 0, data3.Length);
                Thread.Sleep(100);
                int byteRecieved3 = port.BytesToRead;
                byte[] messByte3 = new byte[byteRecieved3];
                port.Read(messByte3, 0, byteRecieved3);
                Thread.Sleep(100);

                for (int i = 7; i <= 10; i++)
                {
                    if (messByte3[i].ToString("X").Length == 1)
                    {
                        md[k] = md[k] + "0" + messByte3[i].ToString("X");
                    }
                    else
                    {
                        md[k] = md[k] + messByte3[i].ToString("X");
                    }
                }
                Thread.Sleep(100);

            }

            port.Write(data4, 0, data4.Length);
            Thread.Sleep(200);
            port.Read(messByte4, 0, byteRecieved4);
            Thread.Sleep(200);


            port.Close();

             UInt32 fl1 = 0;





                string ch1_1 = Convert.ToString(md[0][6]) + Convert.ToString(md[0][7]);
                string ch1_2 = Convert.ToString(md[0][4]) + Convert.ToString(md[0][5]);
                string ch1_3 = Convert.ToString(md[0][2]) + Convert.ToString(md[0][3]);
                string ch1_4 = Convert.ToString(md[0][0]) + Convert.ToString(md[0][1]);

                byte[] data8 = { Convert.ToByte(Convert.ToInt32(ch1_4, 16)), Convert.ToByte(Convert.ToInt32(ch1_3, 16)), Convert.ToByte(Convert.ToInt32(ch1_2, 16)), Convert.ToByte(Convert.ToInt32(ch1_1, 16)) };

               
                fl1 =BitConverter.ToUInt32(data8, 0);

            float fl2 = Convert.ToSingle(fl1) / 1000;
            return fl2;
 
        }
        
        
        
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        //                           Запрос CRC кода к выданным байтам                                      //
        //////////////////////////////////////////////////////////////////////////////////////////////////////
        public static byte[] GetCRC(byte[] btMass)                                                          // 
        {                                                                                                   // 
            byte[] newBtMass;                                                                               //
            newBtMass = new byte[btMass.Length + 2];                                                        // 
            //
            for (int i = 0; i < btMass.Length; i++)                                                         // 
            {                                                                                               //
                newBtMass[i] = btMass[i];                                                                   //
            }                                                                                               //
            //                      
            //
            int Registr = 0xFFFF;                                                                           //
            for (int i = 0; i < btMass.Length; i++)                                                         //
            {                                                                                               //
                Registr = (Registr ^ btMass[i]);                                                            //
                //
                for (int j = 0; j < 8; j++)                                                                 //
                {                                                                                           //
                    if ((Registr & 0x1) == 1)                                                               //
                    {                                                                                       //
                        Registr = Registr >> 1;                                                             //
                        Registr = (Registr ^ 0xA001);                                                       //
                    }                                                                                       //
                    //
                    else                                                                                    //
                    {                                                                                       //
                        Registr = Registr >> 1;                                                             //
                    }                                                                                       //
                }                                                                                           //
            }                                                                                               //
            byte lCRC = (byte)(Registr & 0xff);                                                             //
            byte hCRC = (byte)(Registr >> 8);                                                               //
            newBtMass[newBtMass.Length - 1] = hCRC;                                                         //    
            newBtMass[newBtMass.Length - 2] = lCRC;                                                         //    
            return newBtMass;                                                                               //
        }                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////





        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string conn_str = "Database=resources;Data Source=10.1.1.50;User Id=user;Password=password";
            MySqlLib.MySqlData.MySqlExecute.MyResult result = new MySqlLib.MySqlData.MySqlExecute.MyResult();
            result = MySqlLib.MySqlData.MySqlExecute.SqlScalar("SELECT steam_date FROM steam2094 ORDER BY steam_date DESC LIMIT 0,1", conn_str);
            string date_from_mysql = result.ResultText;

            DateTime date1 = DateTime.Now;
            DateTime date2 = DateTime.Now;


            string year_str, month_str, day_str;

            date1 = new DateTime(Convert.ToInt32(date_from_mysql.Substring(6, 4)), Convert.ToInt32(date_from_mysql.Substring(3, 2)), Convert.ToInt32(date_from_mysql.Substring(0, 2)));
            date1 = date1.AddDays(1);


            TimeSpan interval = date2 - date1;

            int k = 0;
            for (k = 0; k <= (interval.Days - 1); k++)
            {

                string date_req;
                //string date_sql = date_req.Substring(6, 4) + date_req.Substring(3, 2) + date_req.Substring(0, 2);

                year_str = Convert.ToString(date1.Year);
                month_str = Convert.ToString(date1.Month);
                if (month_str.Length < 2) month_str = '0' + month_str;
                day_str = Convert.ToString(date1.Day);
                if (day_str.Length < 2) day_str = '0' + day_str;

                date_req = year_str + month_str + day_str;


                float volume = GetVolume(date_req);

                char[] chars = volume.ToString("R").ToCharArray();
                for (int n = 0; n < volume.ToString("R").Length; n++)
                {
                    if (chars[n] == ',')
                    {
                        chars[n] = '.';
                    }
                }
                string volume_str = new string(chars);



                result = MySqlLib.MySqlData.MySqlExecute.SqlNoneQuery("INSERT INTO `steam2094` (`steam_date`,`steam_ch1`) VALUES (" + "'" + date_req + "'" + "," + "'" + volume_str + "'" + ")", conn_str);



                date1 = date1.AddDays(1);

            }
            Application.Exit();

        }   


    }
}






namespace MySqlLib
{
    /// <summary>
    /// Набор компонент для простой работы с MySQL базой данных.
    /// </summary>
    public class MySqlData
    {

        /// <summary>
        /// Методы реализующие выполнение запросов с возвращением одного параметра либо без параметров вовсе.
        /// </summary>
        public class MySqlExecute
        {

            /// <summary>
            /// Возвращаемый набор данных.
            /// </summary>
            public class MyResult
            {
                /// <summary>
                /// Возвращает результат запроса.
                /// </summary>
                public string ResultText;
                /// <summary>
                /// Возвращает True - если произошла ошибка.
                /// </summary>
                public string ErrorText;
                /// <summary>
                /// Возвращает текст ошибки.
                /// </summary>
                public bool HasError;
            }

            /// <summary>
            /// Для выполнения запросов к MySQL с возвращением 1 параметра.
            /// </summary>
            /// <param name="sql">Текст запроса к базе данных</param>
            /// <param name="connection">Строка подключения к базе данных</param>
            /// <returns>Возвращает значение при успешном выполнении запроса, текст ошибки - при ошибке.</returns>
            public static MyResult SqlScalar(string sql, string connection)
            {
                MyResult result = new MyResult();
                try
                {
                    MySql.Data.MySqlClient.MySqlConnection connRC = new MySql.Data.MySqlClient.MySqlConnection(connection);
                    MySql.Data.MySqlClient.MySqlCommand commRC = new MySql.Data.MySqlClient.MySqlCommand(sql, connRC);
                    connRC.Open();
                    try
                    {
                        result.ResultText = commRC.ExecuteScalar().ToString();
                        result.HasError = false;
                    }
                    catch (Exception ex)
                    {
                        result.ErrorText = ex.Message;
                        result.HasError = true;

                    }
                    connRC.Close();
                }
                catch (Exception ex)//Этот эксепшн на случай отсутствия соединения с сервером.
                {
                    result.ErrorText = ex.Message;
                    result.HasError = true;
                }
                return result;
            }


            /// <summary>
            /// Для выполнения запросов к MySQL без возвращения параметров.
            /// </summary>
            /// <param name="sql">Текст запроса к базе данных</param>
            /// <param name="connection">Строка подключения к базе данных</param>
            /// <returns>Возвращает True - ошибка или False - выполнено успешно.</returns>
            public static MyResult SqlNoneQuery(string sql, string connection)
            {
                MyResult result = new MyResult();
                try
                {
                    MySql.Data.MySqlClient.MySqlConnection connRC = new MySql.Data.MySqlClient.MySqlConnection(connection);
                    MySql.Data.MySqlClient.MySqlCommand commRC = new MySql.Data.MySqlClient.MySqlCommand(sql, connRC);
                    connRC.Open();
                    try
                    {
                        commRC.ExecuteNonQuery();
                        result.HasError = false;
                    }
                    catch (Exception ex)
                    {
                        result.ErrorText = ex.Message;
                        result.HasError = true;
                    }
                    connRC.Close();
                }
                catch (Exception ex)//Этот эксепшн на случай отсутствия соединения с сервером.
                {
                    result.ErrorText = ex.Message;
                    result.HasError = true;
                }
                return result;
            }

        }
        /// <summary>
        /// Методы реализующие выполнение запросов с возвращением набора данных.
        /// </summary>
        public class MySqlExecuteData
        {
            /// <summary>
            /// Возвращаемый набор данных.
            /// </summary>
            public class MyResultData
            {
                /// <summary>
                /// Возвращает результат запроса.
                /// </summary>
                public DataTable ResultData;
                /// <summary>
                /// Возвращает True - если произошла ошибка.
                /// </summary>
                public string ErrorText;
                /// <summary>
                /// Возвращает текст ошибки.
                /// </summary>
                public bool HasError;
            }
            /// <summary>
            /// Выполняет запрос выборки набора строк.
            /// </summary>
            /// <param name="sql">Текст запроса к базе данных</param>
            /// <param name="connection">Строка подключения к базе данных</param>
            /// <returns>Возвращает набор строк в DataSet.</returns>
            public static MyResultData SqlReturnDataset(string sql, string connection)
            {
                MyResultData result = new MyResultData();
                try
                {
                    MySql.Data.MySqlClient.MySqlConnection connRC = new MySql.Data.MySqlClient.MySqlConnection(connection);
                    MySql.Data.MySqlClient.MySqlCommand commRC = new MySql.Data.MySqlClient.MySqlCommand(sql, connRC);
                    connRC.Open();
                    try
                    {
                        MySql.Data.MySqlClient.MySqlDataAdapter AdapterP = new MySql.Data.MySqlClient.MySqlDataAdapter();
                        AdapterP.SelectCommand = commRC;
                        DataSet ds1 = new DataSet();
                        AdapterP.Fill(ds1);
                        result.ResultData = ds1.Tables[0];
                    }
                    catch (Exception ex)
                    {
                        result.HasError = true;
                        result.ErrorText = ex.Message;
                    }
                    connRC.Close();
                }
                catch (Exception ex)//Этот эксепшн на случай отсутствия соединения с сервером.
                {
                    result.ErrorText = ex.Message;
                    result.HasError = true;
                }
                return result;
            }
        }
    }
}