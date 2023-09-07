using System.Collections;
using System.Security.Cryptography;
using System.Text;

namespace SerialForce
{

    /*
     * 
     *      SerialForce
     *        by Serhad Cicekli  
     *        
     */

    public class SerialForce
    {
        public static readonly string Version = "1.01";

        public static string Info()
        {
            return "SERIALFORCE V" + Version + Environment.NewLine + Encoding.ASCII.GetString(new byte[13 + Version.Length]).Replace('\0', '-');
        }
    }

    public abstract class SFObject
    {
        protected static readonly MD5 md5 = MD5.Create();

        private bool deserializationFailed = false;

        public bool DeserializationFailed
        {
            get { return deserializationFailed; }
        }

        protected abstract void Reset();

        protected abstract byte[] SerializeDirect();

        protected abstract void DeserializeDirect(byte[] input);

        public static string GetSerializedType(byte[] data, SFTypeResolver typeResolver)
        {
            MemoryStream stream = new MemoryStream(data);
            if(stream.Length >= 36)
            {
                byte[] typeHash = new byte[16];
                stream.Read(typeHash);
                stream.Close();
                for(int i = 0; i < typeResolver.TypeNameCount; i++)
                {
                    string current = typeResolver.GetTypeNameAt(i);
                    if(StructuralComparisons.StructuralEqualityComparer.Equals(typeHash, md5.ComputeHash(Encoding.Unicode.GetBytes(current))))
                    {
                        return current;
                    }
                }
            }
            return "";
        }

        public byte[] Serialized
        {
            get
            {
                MemoryStream stream = new MemoryStream();
                byte[] directSerializedData = SerializeDirect();
                stream.Write(md5.ComputeHash(Encoding.Unicode.GetBytes(GetType().Name))); // type hash
                stream.Write(md5.ComputeHash(directSerializedData)); // data hash
                stream.Write(BitConverter.GetBytes(directSerializedData.Length)); // data length
                stream.Write(directSerializedData);
                byte[] safeSerializedData = stream.ToArray();
                stream.Close();
                return safeSerializedData;
            }
            set
            {
                deserializationFailed = true;
                MemoryStream stream = new MemoryStream(value);
                /*

                    type hash -> 16 bytes
                    data hash -> 16 bytes
                    data length -> 4 bytes

                */
                if (stream.Length >= 36)
                {
                    byte[] dataLengthBytes = new byte[4];
                    byte[] hashBytes = new byte[16];
                    stream.Read(hashBytes); // type hash
                    if (StructuralComparisons.StructuralEqualityComparer.Equals(hashBytes, md5.ComputeHash(Encoding.Unicode.GetBytes(GetType().Name))))
                    {
                        stream.Read(hashBytes); // data hash
                        stream.Read(dataLengthBytes); // data length
                        int dataLength = BitConverter.ToInt32(dataLengthBytes);
                        if (stream.Length == 36 + dataLength)
                        {
                            byte[] directSerializedData = new byte[dataLength];
                            stream.Read(directSerializedData);
                            if (StructuralComparisons.StructuralEqualityComparer.Equals(hashBytes, md5.ComputeHash(directSerializedData)))
                            {
                                DeserializeDirect(directSerializedData);
                                deserializationFailed = false;
                            }
                        }
                    }
                }
            }
        }
    }

    public class SFNull : SFObject
    {
        protected override void DeserializeDirect(byte[] input) {}

        protected override void Reset(){}

        protected override byte[] SerializeDirect()
        {
            return new byte[0];
        }
    }

    public class SFByteBuffer : SFObject
    {
        private byte[] data;

        public SFByteBuffer()
        {
            data = new byte[0];
        }

        public SFByteBuffer(int size)
        {
            data = new byte[size];
        }

        public byte[] Data
        {
            get { return data; }
            set { if (value == null) data = new byte[0]; else data = value; }
        }

        protected override void DeserializeDirect(byte[] input)
        {
            if(input != null)
            {
                data = new byte[input.Length];
                input.CopyTo(data, 0);
            }
        }

        protected override void Reset()
        {
            data = new byte[0];
        }

        protected override byte[] SerializeDirect()
        {
            byte[] copyedArray = new byte[data.Length];
            data.CopyTo(copyedArray, 0);
            return data;
        }
    }

    public class SFString : SFObject
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

        protected override void DeserializeDirect(byte[] input)
        {
            val = Encoding.Unicode.GetString(input);
        }

        protected override byte[] SerializeDirect()
        {
            return Encoding.Unicode.GetBytes(val);
        }

        protected override void Reset()
        {
            val = "";
        }
    }

    public class SFInteger16 : SFObject
    {
        public short Value = 0;

        public SFInteger16(short initialValue)
        {
            Value = initialValue;
        }

        public SFInteger16() { }

        protected override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToInt16(input);
        }

        protected override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }

        protected override void Reset()
        {
            Value = 0;
        }
    }

    public class SFInteger32 : SFObject
    {
        public int Value = 0;

        public SFInteger32(int initialValue)
        {
            Value = initialValue;
        }

        public SFInteger32() { }

        protected override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToInt32(input);
        }

        protected override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }

        protected override void Reset()
        {
            Value = 0;
        }
    }

    public class SFInteger64 : SFObject
    {
        public long Value = 0;

        public SFInteger64(long initialValue)
        {
            Value = initialValue;
        }

        public SFInteger64() { }

        protected override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToInt64(input);
        }

        protected override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }

        protected override void Reset()
        {
            Value = 0;
        }
    }

    public class SFUnsigned8 : SFObject
    {
        public byte Value = 0;

        public SFUnsigned8(byte initialValue)
        {
            Value = initialValue;
        }

        public SFUnsigned8() { }

        protected override void DeserializeDirect(byte[] input)
        {
            Value = input[0];
        }

        protected override byte[] SerializeDirect()
        {
            return new byte[1] { Value };
        }

        protected override void Reset()
        {
            Value = 0;
        }
    }

    public class SFUnsigned16 : SFObject
    {
        public ushort Value = 0;

        public SFUnsigned16(ushort initialValue)
        {
            Value = initialValue;
        }

        public SFUnsigned16() { }

        protected override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToUInt16(input);
        }

        protected override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }

        protected override void Reset()
        {
            Value = 0;
        }
    }

    public class SFUnsigned32 : SFObject
    {
        public uint Value = 0;

        public SFUnsigned32(uint initialValue)
        {
            Value = initialValue;
        }

        public SFUnsigned32() { }

        protected override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToUInt32(input);
        }

        protected override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }

        protected override void Reset()
        {
            Value = 0;
        }
    }

    public class SFUnsigned64 : SFObject
    {
        public ulong Value = 0;

        public SFUnsigned64(ulong initialValue)
        {
            Value = initialValue;
        }

        public SFUnsigned64() { }

        protected override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToUInt64(input);
        }

        protected override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }

        protected override void Reset()
        {
            Value = 0;
        }
    }

    public class SFFloat16 : SFObject
    {
        public Half Value = Half.Zero;

        public SFFloat16() { }

        public SFFloat16(Half initialValue)
        {
            Value = initialValue;
        }

        protected override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToHalf(input);
        }

        protected override void Reset()
        {
            Value = Half.Zero;
        }

        protected override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }
    }

    public class SFFloat32 : SFObject
    {
        public float Value = 0;

        public SFFloat32() { }

        public SFFloat32(float initialValue)
        {
            Value = initialValue;
        }

        protected override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToSingle(input);
        }

        protected override void Reset()
        {
            Value = 0;
        }

        protected override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }
    }

    public class SFFloat64 : SFObject
    {
        public double Value = 0;

        public SFFloat64() { }

        public SFFloat64(double initialValue)
        {
            Value = initialValue;
        }

        protected override void DeserializeDirect(byte[] input)
        {
            Value = BitConverter.ToDouble(input);
        }

        protected override void Reset()
        {
            Value = 0;
        }

        protected override byte[] SerializeDirect()
        {
            return BitConverter.GetBytes(Value);
        }
    }

    public class SFSerialArray : SFObject
    {
        private List<byte[]> datas = new List<byte[]>();

        public int Length
        {
            get { return datas.Count; }
            set { }
        }

        public bool CheckRange(int index) => (index >= 0) && (index < datas.Count);

        public void Add(SFObject data)
        {
            datas.Add(data.Serialized);
        }

        public void SetAt(int index, SFObject data)
        {
            if (CheckRange(index))
                datas[index] = data.Serialized;
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

        public bool TryGetAt(int index, SFObject target)
        {
            if (!CheckRange(index))
                return false;
            target.Serialized = datas[index];
            return !target.DeserializationFailed;
        }

        public string GetTypeAt(int index, SFTypeResolver typeResolver)
        {
            if (!CheckRange(index))
                return "";
            return GetSerializedType(datas[index], typeResolver);
        }

        protected override void DeserializeDirect(byte[] input)
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

        protected override byte[] SerializeDirect()
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

        protected override void Reset()
        {
            Clear();
        }
    }

    public class SFSerialStringDictionary : SFObject
    {
        private List<string> keys = new List<string>();

        private SFSerialArray values = new SFSerialArray();

        public int EntryCount
        {
            get { return keys.Count; }
        }

        public void SetData(string key, SFObject data)
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

        public bool TryGetData(string key, SFObject target)
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

        public string GetType(string key, SFTypeResolver typeResolver)
        {
            int index = keys.IndexOf(key);
            if (index == -1)
                return "";
            else
                return values.GetTypeAt(index, typeResolver);
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        protected override void DeserializeDirect(byte[] input)
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
                    values.Serialized = valuesSerialized;
                    if(values.DeserializationFailed)
                        keys.Clear();
                }
            }
        }

        protected override byte[] SerializeDirect()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(BitConverter.GetBytes(keys.Count));
            for(int i = 0; i < keys.Count; i++)
            {
                byte[] current = Encoding.Unicode.GetBytes(keys[i]);
                stream.Write(BitConverter.GetBytes(current.Length));
                stream.Write(current);
            }
            stream.Write(values.Serialized);
            byte[] directSerializedData = stream.ToArray();
            stream.Close();
            return directSerializedData;
        }

        protected override void Reset()
        {
            Clear();
        }
    }

    public class SFTypeResolver : SFObject
    {
        private List<string> typeNames = new List<string>();

        public SFTypeResolver()
        {
            AddTypeName("SFTypeResolver");
            AddTypeName("SFObject");
            AddTypeName("SFNull");
            AddTypeName("SFByteBuffer");
            AddTypeName("SFString");
            AddTypeName("SFInteger16");
            AddTypeName("SFInteger32");
            AddTypeName("SFInteger64");
            AddTypeName("SFUnsigned8");
            AddTypeName("SFUnsigned16");
            AddTypeName("SFUnsigned32");
            AddTypeName("SFUnsigned64");
            AddTypeName("SFFloat16");
            AddTypeName("SFFloat32");
            AddTypeName("SFFloat64");
            AddTypeName("SFSerialArray");
            AddTypeName("SFSerialStringDictionary");
        }

        public int TypeNameCount
        {
            get { return typeNames.Count; }
        }

        public string GetTypeNameAt(int index)
        {
            if(index >= 0 && index < typeNames.Count)
            {
                return typeNames[index];
            }
            else
            {
                return "";
            }
        }

        public void AddTypeName(string typeName) {
            typeNames.Add(typeName);
        }

        public void Append(SFTypeResolver resolver)
        {
            for(int i = 0; i < resolver.TypeNameCount; i++)
            {
                typeNames.Add(resolver.GetTypeNameAt(i));
            }
        }

        public void Clear()
        {
            Reset();
        }

        protected override void DeserializeDirect(byte[] input)
        {
            MemoryStream stream = new MemoryStream(input);
            byte[] temp4byte = new byte[4];
            stream.Read(temp4byte);
            int typeNameCount = BitConverter.ToInt32(temp4byte);
            for(int i = 0; i < typeNameCount; i++)
            {
                stream.Read(temp4byte);
                byte[] typeNameBytes = new byte[BitConverter.ToInt32(temp4byte)];
                stream.Read(typeNameBytes);
                typeNames.Add(Encoding.Unicode.GetString(typeNameBytes));
            }
            stream.Close();
        }

        protected override void Reset()
        {
            typeNames.Clear();
        }

        protected override byte[] SerializeDirect()
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(BitConverter.GetBytes(typeNames.Count)); ;
            for(int i = 0; i < typeNames.Count; i++)
            {
                byte[] typeNameBytes = Encoding.Unicode.GetBytes(typeNames[i]);
                stream.Write(BitConverter.GetBytes(typeNameBytes.Length));
                stream.Write(typeNameBytes);
            }
            byte[] directSerializedData = stream.ToArray();
            stream.Close();
            return directSerializedData;
        }
    }
}
