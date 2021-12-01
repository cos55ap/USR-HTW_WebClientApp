using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxArtHomeControl.DataType
{
    public class SensorControl
    {
        public SensorControl()
        {

        }
        public int Id { get; set; }
        public string Name { get; set; }
        public DeviceType Type { get; set; }

        public static SensorControl FromBinary(byte[] data)
        {
            if (data == null || data.Length < 35)
            {
                return null;
            }
            return new SensorControl
            {
                Type = (DeviceType)data[3],
                Id = data[4],
                Name = Encoding.ASCII.GetString(data.Skip(19).Take(15).ToArray()).Replace("\0", "")
            };
        }
    }
}
