using MaxArtHomeControl.DataType;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MaxArtHomeControl.Control
{
    public static class SensorWorker
    {
        public static AmbientValue ReadAmbientSensor(string ip, int id, SecureString user, SecureString password)
        {
            if (ip == null || string.IsNullOrWhiteSpace(ip)) throw new ArgumentException("ip");

            using (var tcpclnt = new TcpClient())
            {
                tcpclnt.Connect(ip, 8899); //"192.168.0.112"
                var stm = tcpclnt.GetStream();
                byte[] byteUsername = new ASCIIEncoding().GetBytes(user.ToString());
                byte[] bytePword = new ASCIIEncoding().GetBytes(password.ToString());
                stm.Write(byteUsername, 0, byteUsername.Length);
                stm.Write(bytePword, 0, bytePword.Length);

                byte[] byteMsg = new byte[100];
                int length = stm.Read(byteMsg, 0, 100);

                if (length == 0)
                {
                    throw new Exception("Wrong user or password");
                }

                var humidityHex = ByteArrayToString(byteMsg.Skip(6).Take(2).ToArray());
                decimal humidity = (decimal)int.Parse(humidityHex, System.Globalization.NumberStyles.HexNumber) / 10;


                var temperatureHex = ByteArrayToString(byteMsg.Skip(8).Take(2).ToArray());
                decimal temperature = (decimal)int.Parse(temperatureHex, System.Globalization.NumberStyles.HexNumber) / 10;

                tcpclnt.Close();

                return new AmbientValue
                {
                    Id = id,
                    Temperature = temperature,
                    Humidity = humidity
                };
            }
        }

        public static byte[] DetectSensor(string ip)
        {
            if (ip == null || string.IsNullOrWhiteSpace(ip)) throw new ArgumentException("Wrong ip");

            var udpClient = new UdpClient();

            var endPoint = new IPEndPoint(IPAddress.Parse(ip), 1901);
            udpClient.Connect(endPoint);
            udpClient.Send(new byte[] { 255, 01, 01, 02 }, 4);

            var data = udpClient.Receive(ref endPoint);
            return data;
        }

        private static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
    }
}
