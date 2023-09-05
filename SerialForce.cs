using System;
using System.Security.Cryptography;
using System.Text;

namespace SerialForce
{

    /*
     * 
     *      SerialForce
     *        by Serhad Technologies  
     *        
     */

    public class SerialForce
    {
        public static readonly string Version = "1.0";
        public static string Info()
        {
            return "SERIALFORCE V" + Version + Environment.NewLine + Encoding.ASCII.GetString(new byte[13 + Version.Length]).Replace('\0', '-');
        }
    }
    public abstract class SFData
    {
        public abstract void Reset();
        public abstract byte[] SerializeDirect();
        public abstract void DeserializeDirect(byte[] input);
    }

    public class SFNull : SFData
    {
        public override void DeserializeDirect(byte[] input) {}

        public override void Reset(){}

        public override byte[] SerializeDirect()
        {
            return new byte[0];
        }
    }

    public class SFByteBuffer : SFData
    {
        private byte[] data = new byte[0];
        public byte[] Data
        {
            get { return data; }
            set { if (value == null) data = new byte[0]; else data = value; }
        }
        public override void DeserializeDirect(byte[] input)
        {
            if(input != null)
            {
                data = new byte[input.Length];
                input.CopyTo(data, 0);
            }
        }

        public override void Reset()
        {
            data = new byte[0];
        }

        public override byte[] SerializeDirect()
        {
            byte[] copyedArray = new byte[data.Length];
            data.CopyTo(copyedArray, 0);
            return data;
        }
    }

    public class SFSafeSerializer
    {
        private static readonly MD5 md5 = MD5.Create();
        public static byte[] SerializeSafe(SFData data)
        {
            if (data == null)
                data = new SFNull();
            MemoryStream stream = new MemoryStream();
            byte[] directSerializedData = data.SerializeDirect();
            byte[] hash = md5.ComputeHash(directSerializedData);
            byte[] typeData = Encoding.Unicode.GetBytes(data.GetType().Name);
            stream.Write(BitConverter.GetBytes(typeData.Length));
            stream.Write(typeData);
            stream.Write(hash);
            stream.Write(BitConverter.GetBytes(directSerializedData.Length));
            stream.Write(directSerializedData);
            byte[] safeSerializedData = stream.ToArray();
            stream.Close();
            return safeSerializedData;
        }

        public static bool TryDeserializeSafe(byte[] serializedData, SFData target)
        {
            if (target == null)
                return false;
            MemoryStream stream = new MemoryStream(serializedData);
            if(stream.Length >= 24)
            {
                byte[] temp4byte = new byte[4];
                stream.Read(temp4byte);
                int typeLength = BitConverter.ToInt32(temp4byte);
                if(stream.Length >= 24 + typeLength)
                {
                    byte[] typeByte = new byte[typeLength];
                    stream.Read(typeByte);
                    string type = Encoding.Unicode.GetString(typeByte);
                    if(type == target.GetType().Name)
                    {
                        byte[] hash = new byte[16];
                        stream.Read(hash);
                        stream.Read(temp4byte);
                        int directSerializedDataLength = BitConverter.ToInt32(temp4byte);
                        if(stream.Length == 24 + typeLength + directSerializedDataLength)
                        {
                            byte[] directSerializedData = new byte[directSerializedDataLength];
                            stream.Read(directSerializedData);
                            stream.Close();
                            byte[] targetHash = md5.ComputeHash(directSerializedData);
                            bool hashCheck = true;
                            for(int i = 0; i < 16; i++)
                            {
                                if (hash[i] != targetHash[i])
                                {
                                    hashCheck = false;
                                    break;
                                }
                            }
                            if (hashCheck)
                            {
                                target.Reset();
                                target.DeserializeDirect(directSerializedData);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static string GetSafeSerializedType(byte[] serializedData)
        {
            MemoryStream stream = new MemoryStream(serializedData);
            if(stream.Length >= 8)
            {
                byte[] temp4byte = new byte[4];
                stream.Read(temp4byte);
                int typeLength = BitConverter.ToInt32(temp4byte);
                if(stream.Length >= 8 + typeLength)
                {
                    byte[] typeByte = new byte[typeLength];
                    stream.Read(typeByte);
                    string type = Encoding.Unicode.GetString(typeByte);
                    stream.Close();
                    return type;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
    }

    public class SFString : SFData
    {
        private string val = "";

        public string Value
        {
            get { return val; }
            set { if (value == null) val = ""; else val = value; }
        }

        public SFString(string initialValue)
        {
            Value = initialValue;
        }

        public SFString() { }

        public override void DeserializeDirect(byte[] input)
        {
            val = Encoding.Unicode.GetString(input);
        }

        public override byte[] SerializeDirect()
        {
            return Encoding.Unicode.GetBytes(val);
        }

        public override void Reset()
        {
            val = "";
        }
    }

    public class SFInteger16 : SFData
    {
        public short Value = 0;

        public SFInteger16(short initialValue)
        {
            Value = initialValue;
        }

        public SFInteger16() { }

        public override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToInt16(input);
        }

        public override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }

        public override void Reset()
        {
            Value = 0;
        }
    }

    public class SFInteger32 : SFData
    {
        public int Value = 0;

        public SFInteger32(int initialValue)
        {
            Value = initialValue;
        }

        public SFInteger32() { }

        public override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToInt32(input);
        }

        public override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }

        public override void Reset()
        {
            Value = 0;
        }
    }

    public class SFInteger64 : SFData
    {
        public long Value = 0;

        public SFInteger64(long initialValue)
        {
            Value = initialValue;
        }

        public SFInteger64() { }

        public override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToInt64(input);
        }

        public override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }

        public override void Reset()
        {
            Value = 0;
        }
    }

    public class SFUnsigned8 : SFData
    {
        public byte Value = 0;

        public SFUnsigned8(byte initialValue)
        {
            Value = initialValue;
        }

        public SFUnsigned8() { }

        public override void DeserializeDirect(byte[] input)
        {
            Value = input[0];
        }

        public override byte[] SerializeDirect()
        {
            return new byte[1] { Value };
        }

        public override void Reset()
        {
            Value = 0;
        }
    }

    public class SFUnsigned16 : SFData
    {
        public ushort Value = 0;

        public SFUnsigned16(ushort initialValue)
        {
            Value = initialValue;
        }

        public SFUnsigned16() { }

        public override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToUInt16(input);
        }

        public override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }

        public override void Reset()
        {
            Value = 0;
        }
    }

    public class SFUnsigned32 : SFData
    {
        public uint Value = 0;

        public SFUnsigned32(uint initialValue)
        {
            Value = initialValue;
        }

        public SFUnsigned32() { }

        public override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToUInt32(input);
        }

        public override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }

        public override void Reset()
        {
            Value = 0;
        }
    }

    public class SFUnsigned64 : SFData
    {
        public ulong Value = 0;

        public SFUnsigned64(ulong initialValue)
        {
            Value = initialValue;
        }

        public SFUnsigned64() { }

        public override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToUInt64(input);
        }

        public override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }

        public override void Reset()
        {
            Value = 0;
        }
    }

    public class SFSerialArray : SFData
    {
        private List<byte[]> datas = new List<byte[]>();

        public int Length
        {
            get { return datas.Count; }
            set { }
        }

        public bool CheckRange(int index) => (index >= 0) && (index < datas.Count);

        public void Add(SFData data)
        {
            datas.Add(SFSafeSerializer.SerializeSafe(data));
        }

        public void SetAt(int index, SFData data)
        {
            if(CheckRange(index))
            datas[index] = SFSafeSerializer.SerializeSafe(data);
        }

        public void RemoveAt(int index)
        {
            if(CheckRange(index))
            datas.RemoveAt(index);
        }

        public void Clear()
        {
            datas.Clear();
        }

        public bool TryGetAt(int index, SFData target)
        {
            if (!CheckRange(index))
                return false;
            return SFSafeSerializer.TryDeserializeSafe(datas[index], target);
        }

        public string GetTypeAt(int index)
        {
            if (!CheckRange(index))
                return "";
            return SFSafeSerializer.GetSafeSerializedType(datas[index]);
        }

        public override void DeserializeDirect(byte[] input)
        {
            MemoryStream stream = new MemoryStream(input);
            if(stream.Length >= 4)
            {
                byte[] temp4byte = new byte[4];
                stream.Read(temp4byte);
                int itemCount = BitConverter.ToInt32(temp4byte);
                if(stream.Length > 4 + itemCount * 4)
                {
                    datas.Clear();
                    for(int i = 0; i < itemCount; i++)
                    {
                        stream.Read(temp4byte);
                        int serializedSize = BitConverter.ToInt32(temp4byte);
                        byte[] current = new byte[serializedSize];
                        stream.Read(current);
                        datas.Add(current);
                    }
                }
            }
        }

        public override byte[] SerializeDirect()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(BitConverter.GetBytes(datas.Count));
            for(int i = 0; i < datas.Count; i++)
            {
                byte[] current = datas[i];
                stream.Write(BitConverter.GetBytes(current.Length));
                stream.Write(current);
            }
            byte[] directSerialized = stream.ToArray();
            stream.Close();
            return directSerialized;
        }

        public override void Reset()
        {
            Clear();
        }
    }

    public class SFSerialDictionary : SFData
    {
        private List<string> keys = new List<string>();

        private SFSerialArray values = new SFSerialArray();

        public int EntryCount
        {
            get { return keys.Count; }
            set { }
        }

        public void SetData(string key, SFData data)
        {
            int index = keys.IndexOf(key);
            if(index == -1)
            {
                keys.Add(key);
                values.Add(data);
            }
            else
            {
                values.SetAt(index, data);
            }
        }

        public bool TryGetData(string key, SFData target)
        {
            int index = keys.IndexOf(key);
            if(index != -1)
                return values.TryGetAt(index, target);
            return false;
        }

        public string GetKeyAt(int index)
        {
            if (values.CheckRange(index))
                return keys[index];
            else
                return "";
        }

        public string GetType(string key)
        {
            int index = keys.IndexOf(key);
            if (index == -1)
                return "";
            else
                return values.GetTypeAt(index);
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        public override void DeserializeDirect(byte[] input)
        {
            MemoryStream stream = new MemoryStream(input);
            if(stream.Length >= 4)
            {
                byte[] temp4byte = new byte[4];
                stream.Read(temp4byte);
                int entryCount = BitConverter.ToInt32(temp4byte);
                if(stream.Length >= 4 + entryCount * 4)
                {
                    Clear();
                    for(int i = 0; i < entryCount; i++)
                    {
                        stream.Read(temp4byte);
                        int currentLength = BitConverter.ToInt32(temp4byte);
                        byte[] current = new byte[currentLength];
                        stream.Read(current);
                        keys.Add(Encoding.Unicode.GetString(current));
                    }
                    byte[] valuesSerialized = new byte[stream.Length - stream.Position];
                    stream.Read(valuesSerialized);
                    stream.Close();
                    if(!SFSafeSerializer.TryDeserializeSafe(valuesSerialized, values))
                        keys.Clear();
                }
            }
        }

        public override byte[] SerializeDirect()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(BitConverter.GetBytes(keys.Count));
            for(int i = 0; i < keys.Count; i++)
            {
                byte[] current = Encoding.Unicode.GetBytes(keys[i]);
                stream.Write(BitConverter.GetBytes(current.Length));
                stream.Write(current);
            }
            stream.Write(SFSafeSerializer.SerializeSafe(values));
            byte[] directSerializedData = stream.ToArray();
            stream.Close();
            return directSerializedData;
        }

        public override void Reset()
        {
            Clear();
        }
    }
}
